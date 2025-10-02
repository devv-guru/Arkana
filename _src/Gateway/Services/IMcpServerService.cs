using Data.Entities.Mcp;
using Data.Entities;
using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services;

public interface IMcpServerService
{
    Task<IEnumerable<McpServer>> GetAllServersAsync();
    Task<McpServer?> GetServerByIdAsync(Guid id);
    Task<McpServer?> GetServerByNameAsync(string name);
    Task<McpServer> CreateServerAsync(McpServer server);
    Task<McpServer?> UpdateServerAsync(Guid id, McpServer server);
    Task<bool> DeleteServerAsync(Guid id);
    Task<bool> IsUserAuthorizedAsync(string userId, Guid serverId);
}

public class McpServerService : IMcpServerService
{
    private readonly IWriteOnlyContext _context;
    private readonly ILogger<McpServerService> _logger;
    private readonly DynamicProxyConfigProvider _proxyConfigProvider;

    public McpServerService(
        IWriteOnlyContext context, 
        ILogger<McpServerService> logger,
        DynamicProxyConfigProvider proxyConfigProvider)
    {
        _context = context;
        _logger = logger;
        _proxyConfigProvider = proxyConfigProvider;
    }

    public async Task<IEnumerable<McpServer>> GetAllServersAsync()
    {
        return await _context.McpServers
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<McpServer?> GetServerByIdAsync(Guid id)
    {
        return await _context.McpServers
            .Include(s => s.UserAssignments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<McpServer?> GetServerByNameAsync(string name)
    {
        return await _context.McpServers
            .FirstOrDefaultAsync(s => s.Name == name && s.IsEnabled);
    }

    public async Task<McpServer> CreateServerAsync(McpServer server)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Create the MCP server
            _context.McpServers.Add(server);
            await _context.SaveChangesAsync();
            
            // Create corresponding YARP proxy configuration
            await CreateProxyConfigurationAsync(server);
            
            await transaction.CommitAsync();
            
            _logger.LogInformation("Created MCP server with proxy configuration: {Name} (ID: {Id})", server.Name, server.Id);
            return server;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<McpServer?> UpdateServerAsync(Guid id, McpServer server)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var existing = await _context.McpServers.FindAsync(id);
            if (existing == null)
                return null;

            existing.Name = server.Name;
            existing.Description = server.Description;
            existing.Endpoint = server.Endpoint;
            existing.ProtocolType = server.ProtocolType;
            existing.IsEnabled = server.IsEnabled;
            existing.Priority = server.Priority;

            await _context.SaveChangesAsync();
            
            // Update corresponding YARP proxy configuration
            await UpdateProxyConfigurationAsync(existing);
            
            await transaction.CommitAsync();
            
            _logger.LogInformation("Updated MCP server with proxy configuration: {Name} (ID: {Id})", existing.Name, existing.Id);
            return existing;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteServerAsync(Guid id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var server = await _context.McpServers.FindAsync(id);
            if (server == null)
                return false;

            // Delete corresponding YARP proxy configuration first
            await DeleteProxyConfigurationAsync(server);
            
            _context.McpServers.Remove(server);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            _logger.LogInformation("Deleted MCP server and proxy configuration: {Name} (ID: {Id})", server.Name, server.Id);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> IsUserAuthorizedAsync(string userId, Guid serverId)
    {
        return await _context.McpUserAssignments
            .AnyAsync(ua => ua.UserId == userId && 
                           ua.McpServerId == serverId && 
                           ua.IsEnabled &&
                           (ua.ExpiresAt == null || ua.ExpiresAt > DateTime.UtcNow));
    }

    private async Task CreateProxyConfigurationAsync(McpServer server)
    {
        // Create a default web host for the server
        var webHost = new WebHost
        {
            Name = $"host-{server.Name}",
            HostName = ExtractHostFromEndpoint(server.Endpoint),
            IsDefault = false
        };
        _context.WebHosts.Add(webHost);
        await _context.SaveChangesAsync();

        // Create cluster for the server
        var cluster = new Cluster
        {
            Name = $"cluster-{server.Name}",
            LoadBalancingPolicy = "RoundRobin",
            HostId = webHost.Id
        };
        _context.Clusters.Add(cluster);
        await _context.SaveChangesAsync();

        // Create destination pointing to the MCP server endpoint
        var destination = new Destination
        {
            Name = $"destination-{server.Name}",
            Address = server.Endpoint,
            Health = "healthy",
            ClusterConfigId = cluster.Id
        };
        _context.Destinations.Add(destination);

        // Create route first
        var route = new Route
        {
            ClusterId = cluster.Id,
            Order = 1,
            HostId = webHost.Id
        };
        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        // Create Match for the route with proper foreign key
        var match = new Match
        {
            Path = $"/mcp/{server.Name}/{{**catch-all}}",
            Hosts = new List<string>(),
            Methods = new List<string>(),
            Headers = new List<HeaderMatch>(),
            QueryParameters = new List<QueryParameterMatch>(),
            RouteConfigId = route.Id
        };
        _context.Matches.Add(match);

        // Create transforms for path rewriting - replace /mcp/{server-name} with /api
        var transforms = new List<Transform>
        {
            new Transform
            {
                RequestHeader = "PathPattern",
                Set = "/api/{**remainder}",
                RouteConfigId = route.Id
            }
        };
        _context.Transforms.AddRange(transforms);

        await _context.SaveChangesAsync();

        // Reload proxy configuration
        await _proxyConfigProvider.ReloadConfigurationAsync();
        
        _logger.LogInformation("Created proxy configuration for MCP server: {ServerName}", server.Name);
    }

    private async Task UpdateProxyConfigurationAsync(McpServer server)
    {
        // For now, delete and recreate the configuration
        // This could be optimized to update in place
        await DeleteProxyConfigurationAsync(server);
        await CreateProxyConfigurationAsync(server);
    }

    private async Task DeleteProxyConfigurationAsync(McpServer server)
    {
        try
        {
            // Find and delete related proxy configuration entities
            var routes = await _context.Routes
                .Include(r => r.Match)
                .Include(r => r.Transforms)
                .Where(r => r.Match != null && r.Match.Path != null && 
                           r.Match.Path.Contains($"/mcp/{server.Name}/"))
                .ToListAsync();

            var clusters = await _context.Clusters
                .Include(c => c.Destinations)
                .Where(c => c.Name == $"cluster-{server.Name}")
                .ToListAsync();

            var webHosts = await _context.WebHosts
                .Where(h => h.Name == $"host-{server.Name}")
                .ToListAsync();

            // Delete in reverse dependency order
            foreach (var route in routes)
            {
                if (route.Transforms != null)
                {
                    _context.Transforms.RemoveRange(route.Transforms);
                }
                if (route.Match != null)
                {
                    _context.Matches.Remove(route.Match);
                }
                _context.Routes.Remove(route);
            }

            foreach (var cluster in clusters)
            {
                if (cluster.Destinations != null)
                {
                    _context.Destinations.RemoveRange(cluster.Destinations);
                }
                _context.Clusters.Remove(cluster);
            }

            foreach (var webHost in webHosts)
            {
                _context.WebHosts.Remove(webHost);
            }

            await _context.SaveChangesAsync();

            // Reload proxy configuration
            await _proxyConfigProvider.ReloadConfigurationAsync();
            
            _logger.LogInformation("Deleted proxy configuration for MCP server: {ServerName}", server.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting proxy configuration for MCP server: {ServerName}", server.Name);
            throw;
        }
    }

    private static string ExtractHostFromEndpoint(string endpoint)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }
        return "localhost";
    }
}
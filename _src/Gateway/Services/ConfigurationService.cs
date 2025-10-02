using Microsoft.EntityFrameworkCore;
using Data.Contexts.Base;
using Data.Entities;
using Gateway.Models;

namespace Gateway.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IWriteOnlyContext _context;
    private readonly DynamicProxyConfigProvider _proxyConfigProvider;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IWriteOnlyContext context,
        DynamicProxyConfigProvider proxyConfigProvider,
        ILogger<ConfigurationService> logger)
    {
        _context = context;
        _proxyConfigProvider = proxyConfigProvider;
        _logger = logger;
    }

    #region Route Management

    public async Task<IEnumerable<Route>> GetRoutesAsync()
    {
        return await _context.Routes
            .Include(r => r.Host)
            .Include(r => r.Match)
                .ThenInclude(m => m.Headers)
            .Include(r => r.Match)
                .ThenInclude(m => m.QueryParameters)
            .Include(r => r.Transforms)
            .Include(r => r.Metadata)
            .Where(r => !r.IsDeleted)
            .ToListAsync();
    }

    public async Task<Route?> GetRouteAsync(Guid id)
    {
        return await _context.Routes
            .Include(r => r.Host)
            .Include(r => r.Match)
                .ThenInclude(m => m.Headers)
            .Include(r => r.Match)
                .ThenInclude(m => m.QueryParameters)
            .Include(r => r.Transforms)
            .Include(r => r.Metadata)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<Route> CreateRouteAsync(Route route)
    {
        route.Id = Guid.NewGuid();
        route.CreatedAt = DateTime.UtcNow;
        route.UpdatedAt = DateTime.UtcNow;
        route.IsDeleted = false;

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created route {RouteId} with path {Path}", route.Id, route.Match?.Path);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();

        return route;
    }

    public async Task<Route> UpdateRouteAsync(Route route)
    {
        var existingRoute = await _context.Routes.FindAsync(route.Id);
        if (existingRoute == null)
            throw new InvalidOperationException($"Route {route.Id} not found");

        route.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingRoute).CurrentValues.SetValues(route);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated route {RouteId}", route.Id);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();

        return route;
    }

    public async Task DeleteRouteAsync(Guid id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
            throw new InvalidOperationException($"Route {id} not found");

        route.IsDeleted = true;
        route.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted route {RouteId}", id);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();
    }

    #endregion

    #region Cluster Management

    public async Task<IEnumerable<Cluster>> GetClustersAsync()
    {
        return await _context.Clusters
            .Include(c => c.Host)
            .Include(c => c.Destinations)
            .Include(c => c.HealthCheck)
            .Include(c => c.SessionAffinity)
            .Include(c => c.HttpClient)
            .Include(c => c.HttpRequest)
            .Include(c => c.Metadata)
            .Where(c => !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<Cluster?> GetClusterAsync(Guid id)
    {
        return await _context.Clusters
            .Include(c => c.Host)
            .Include(c => c.Destinations)
            .Include(c => c.HealthCheck)
                .ThenInclude(h => h.Active)
            .Include(c => c.HealthCheck)
                .ThenInclude(h => h.Passive)
            .Include(c => c.SessionAffinity)
            .Include(c => c.HttpClient)
            .Include(c => c.HttpRequest)
            .Include(c => c.Metadata)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Cluster> CreateClusterAsync(Cluster cluster)
    {
        cluster.Id = Guid.NewGuid();
        cluster.CreatedAt = DateTime.UtcNow;
        cluster.UpdatedAt = DateTime.UtcNow;
        cluster.IsDeleted = false;

        _context.Clusters.Add(cluster);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created cluster {ClusterId} with name {Name}", cluster.Id, cluster.Name);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();

        return cluster;
    }

    public async Task<Cluster> UpdateClusterAsync(Cluster cluster)
    {
        var existingCluster = await _context.Clusters.FindAsync(cluster.Id);
        if (existingCluster == null)
            throw new InvalidOperationException($"Cluster {cluster.Id} not found");

        cluster.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingCluster).CurrentValues.SetValues(cluster);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated cluster {ClusterId}", cluster.Id);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();

        return cluster;
    }

    public async Task DeleteClusterAsync(Guid id)
    {
        var cluster = await _context.Clusters.FindAsync(id);
        if (cluster == null)
            throw new InvalidOperationException($"Cluster {id} not found");

        cluster.IsDeleted = true;
        cluster.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted cluster {ClusterId}", id);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();
    }

    #endregion

    #region Destination Management

    public async Task<IEnumerable<Destination>> GetDestinationsAsync(Guid clusterId)
    {
        return await _context.Destinations
            .Where(d => d.ClusterConfigId == clusterId && !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<Destination?> GetDestinationAsync(Guid id)
    {
        return await _context.Destinations
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<Destination> CreateDestinationAsync(Destination destination)
    {
        destination.Id = Guid.NewGuid();
        destination.CreatedAt = DateTime.UtcNow;
        destination.UpdatedAt = DateTime.UtcNow;
        destination.IsDeleted = false;

        _context.Destinations.Add(destination);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created destination {DestinationId} with address {Address}", 
            destination.Id, destination.Address);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();

        return destination;
    }

    public async Task<Destination> UpdateDestinationAsync(Destination destination)
    {
        var existingDestination = await _context.Destinations.FindAsync(destination.Id);
        if (existingDestination == null)
            throw new InvalidOperationException($"Destination {destination.Id} not found");

        destination.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingDestination).CurrentValues.SetValues(destination);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated destination {DestinationId}", destination.Id);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();

        return destination;
    }

    public async Task DeleteDestinationAsync(Guid id)
    {
        var destination = await _context.Destinations.FindAsync(id);
        if (destination == null)
            throw new InvalidOperationException($"Destination {id} not found");

        destination.IsDeleted = true;
        destination.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted destination {DestinationId}", id);
        
        // Trigger configuration reload
        await ReloadProxyConfigurationAsync();
    }

    #endregion

    #region WebHost Management

    public async Task<IEnumerable<WebHost>> GetWebHostsAsync()
    {
        return await _context.WebHosts
            .Include(h => h.Routes)
            .Include(h => h.Cluster)
            .Where(h => !h.IsDeleted)
            .ToListAsync();
    }

    public async Task<WebHost?> GetWebHostAsync(Guid id)
    {
        return await _context.WebHosts
            .Include(h => h.Routes)
            .Include(h => h.Cluster)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
    }

    public async Task<WebHost> CreateWebHostAsync(WebHost webHost)
    {
        webHost.Id = Guid.NewGuid();
        webHost.CreatedAt = DateTime.UtcNow;
        webHost.UpdatedAt = DateTime.UtcNow;
        webHost.IsDeleted = false;

        _context.WebHosts.Add(webHost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created web host {WebHostId} with name {Name}", webHost.Id, webHost.Name);

        return webHost;
    }

    public async Task<WebHost> UpdateWebHostAsync(WebHost webHost)
    {
        var existingWebHost = await _context.WebHosts.FindAsync(webHost.Id);
        if (existingWebHost == null)
            throw new InvalidOperationException($"WebHost {webHost.Id} not found");

        webHost.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingWebHost).CurrentValues.SetValues(webHost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated web host {WebHostId}", webHost.Id);

        return webHost;
    }

    public async Task DeleteWebHostAsync(Guid id)
    {
        var webHost = await _context.WebHosts.FindAsync(id);
        if (webHost == null)
            throw new InvalidOperationException($"WebHost {id} not found");

        webHost.IsDeleted = true;
        webHost.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted web host {WebHostId}", id);
    }

    #endregion

    #region Configuration Operations

    public async Task ReloadProxyConfigurationAsync()
    {
        _logger.LogInformation("Triggering proxy configuration reload");
        await _proxyConfigProvider.ReloadConfigurationAsync();
    }

    public async Task<bool> ValidateConfigurationAsync()
    {
        try
        {
            var routes = await GetRoutesAsync();
            var clusters = await GetClustersAsync();

            // Validate that all routes reference valid clusters
            var clusterIds = clusters.Select(c => c.Id).ToHashSet();
            var invalidRoutes = routes.Where(r => !clusterIds.Contains(r.ClusterId)).ToList();
            
            if (invalidRoutes.Any())
            {
                _logger.LogWarning("Found {Count} routes with invalid cluster references", invalidRoutes.Count);
                return false;
            }

            // Validate that all clusters have at least one active destination
            foreach (var cluster in clusters)
            {
                var destinations = await GetDestinationsAsync(cluster.Id);
                if (!destinations.Any())
                {
                    _logger.LogWarning("Cluster {ClusterId} has no active destinations", cluster.Id);
                    return false;
                }
            }

            _logger.LogInformation("Configuration validation passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed");
            return false;
        }
    }

    public async Task ImportConfigurationAsync(GatewayConfigurationModel configuration)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Import implementation would go here
            // This is a complex operation that would:
            // 1. Validate the incoming configuration
            // 2. Create/update hosts, clusters, routes, destinations
            // 3. Handle relationships and dependencies
            
            await transaction.CommitAsync();
            await ReloadProxyConfigurationAsync();
            
            _logger.LogInformation("Successfully imported configuration");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to import configuration");
            throw;
        }
    }

    public async Task<GatewayConfigurationModel> ExportConfigurationAsync()
    {
        var hosts = await GetWebHostsAsync();
        var routes = await GetRoutesAsync();
        var clusters = await GetClustersAsync();

        // Build export model
        return new GatewayConfigurationModel
        {
            // Export implementation would build the full configuration model
            // This would serialize the current database state to the 
            // gateway-config.json format
        };
    }

    public async Task<ConfigurationStatus> GetConfigurationStatusAsync()
    {
        var routeCount = await _context.Routes.CountAsync(r => !r.IsDeleted);
        var clusterCount = await _context.Clusters.CountAsync(c => !c.IsDeleted);
        var destinationCount = await _context.Destinations.CountAsync(d => !d.IsDeleted);
        var webHostCount = await _context.WebHosts.CountAsync(h => !h.IsDeleted);

        var isValid = await ValidateConfigurationAsync();
        
        return new ConfigurationStatus
        {
            IsValid = isValid,
            RouteCount = routeCount,
            ClusterCount = clusterCount,
            DestinationCount = destinationCount,
            WebHostCount = webHostCount,
            LastUpdated = DateTime.UtcNow
        };
    }

    #endregion
}
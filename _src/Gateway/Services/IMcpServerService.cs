using Gateway.Models;
using Gateway.Data;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services;

public interface IMcpServerService
{
    Task<IEnumerable<McpServer>> GetAllServersAsync();
    Task<McpServer?> GetServerByIdAsync(int id);
    Task<McpServer?> GetServerByNameAsync(string name);
    Task<McpServer> CreateServerAsync(McpServer server);
    Task<McpServer?> UpdateServerAsync(int id, McpServer server);
    Task<bool> DeleteServerAsync(int id);
    Task<bool> IsUserAuthorizedAsync(string userId, int serverId);
}

public class McpServerService : IMcpServerService
{
    private readonly GatewayDbContext _context;
    private readonly ILogger<McpServerService> _logger;

    public McpServerService(GatewayDbContext context, ILogger<McpServerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<McpServer>> GetAllServersAsync()
    {
        return await _context.McpServers
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<McpServer?> GetServerByIdAsync(int id)
    {
        return await _context.McpServers
            .Include(s => s.UserAccess)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<McpServer?> GetServerByNameAsync(string name)
    {
        return await _context.McpServers
            .FirstOrDefaultAsync(s => s.Name == name && s.IsEnabled);
    }

    public async Task<McpServer> CreateServerAsync(McpServer server)
    {
        server.CreatedAt = DateTime.UtcNow;
        _context.McpServers.Add(server);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created MCP server: {Name} (ID: {Id})", server.Name, server.Id);
        return server;
    }

    public async Task<McpServer?> UpdateServerAsync(int id, McpServer server)
    {
        var existing = await _context.McpServers.FindAsync(id);
        if (existing == null)
            return null;

        existing.Name = server.Name;
        existing.Description = server.Description;
        existing.Endpoint = server.Endpoint;
        existing.Protocol = server.Protocol;
        existing.AuthType = server.AuthType;
        existing.AuthSettings = server.AuthSettings;
        existing.IsEnabled = server.IsEnabled;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated MCP server: {Name} (ID: {Id})", existing.Name, existing.Id);
        return existing;
    }

    public async Task<bool> DeleteServerAsync(int id)
    {
        var server = await _context.McpServers.FindAsync(id);
        if (server == null)
            return false;

        _context.McpServers.Remove(server);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted MCP server: {Name} (ID: {Id})", server.Name, server.Id);
        return true;
    }

    public async Task<bool> IsUserAuthorizedAsync(string userId, int serverId)
    {
        return await _context.McpUserAccess
            .AnyAsync(ua => ua.UserId == userId && 
                           ua.McpServerId == serverId && 
                           ua.IsEnabled &&
                           (ua.ExpiresAt == null || ua.ExpiresAt > DateTime.UtcNow));
    }
}
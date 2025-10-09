using Gateway.Models;
using Gateway.Data;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services;

public interface IUserService
{
    Task<bool> GrantAccessAsync(string userId, int serverId, string? userEmail = null, string? roles = null);
    Task<bool> RevokeAccessAsync(string userId, int serverId);
    Task<IEnumerable<McpServer>> GetUserServersAsync(string userId);
    Task LogAuditEventAsync(string userId, int? serverId, string action, string? details = null, string? ipAddress = null, string? userAgent = null);
}

public class UserService : IUserService
{
    private readonly GatewayDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(GatewayDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> GrantAccessAsync(string userId, int serverId, string? userEmail = null, string? roles = null)
    {
        var existing = await _context.McpUserAccess
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.McpServerId == serverId);

        if (existing != null)
        {
            existing.IsEnabled = true;
            existing.UserEmail = userEmail ?? existing.UserEmail;
            existing.Roles = roles ?? existing.Roles;
        }
        else
        {
            _context.McpUserAccess.Add(new McpUserAccess
            {
                UserId = userId,
                McpServerId = serverId,
                UserEmail = userEmail,
                Roles = roles,
                IsEnabled = true
            });
        }

        await _context.SaveChangesAsync();
        await LogAuditEventAsync(userId, serverId, "ACCESS_GRANTED");
        
        _logger.LogInformation("Granted access to user {UserId} for server {ServerId}", userId, serverId);
        return true;
    }

    public async Task<bool> RevokeAccessAsync(string userId, int serverId)
    {
        var access = await _context.McpUserAccess
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.McpServerId == serverId);

        if (access == null)
            return false;

        access.IsEnabled = false;
        await _context.SaveChangesAsync();
        await LogAuditEventAsync(userId, serverId, "ACCESS_REVOKED");
        
        _logger.LogInformation("Revoked access for user {UserId} from server {ServerId}", userId, serverId);
        return true;
    }

    public async Task<IEnumerable<McpServer>> GetUserServersAsync(string userId)
    {
        return await _context.McpUserAccess
            .Where(ua => ua.UserId == userId && 
                        ua.IsEnabled &&
                        (ua.ExpiresAt == null || ua.ExpiresAt > DateTime.UtcNow))
            .Include(ua => ua.McpServer)
            .Select(ua => ua.McpServer)
            .Where(s => s.IsEnabled)
            .ToListAsync();
    }

    public async Task LogAuditEventAsync(string userId, int? serverId, string action, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new McpAuditLog
        {
            UserId = userId,
            McpServerId = serverId,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _context.McpAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
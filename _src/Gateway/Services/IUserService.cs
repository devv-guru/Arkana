using Data.Entities.Mcp;
using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services;

public interface IUserService
{
    Task<bool> GrantAccessAsync(string userId, Guid serverId, string? userEmail = null, string? userDisplayName = null);
    Task<bool> RevokeAccessAsync(string userId, Guid serverId);
    Task<IEnumerable<McpServer>> GetUserServersAsync(string userId);
    Task LogAuditEventAsync(string userId, Guid? serverId, string action, string? details = null, string? ipAddress = null, string? userAgent = null);
}

public class UserService : IUserService
{
    private readonly IWriteOnlyContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(IWriteOnlyContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> GrantAccessAsync(string userId, Guid serverId, string? userEmail = null, string? userDisplayName = null)
    {
        var existing = await _context.McpUserAssignments
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.McpServerId == serverId);

        if (existing != null)
        {
            existing.IsEnabled = true;
            existing.UserEmail = userEmail ?? existing.UserEmail;
            existing.UserDisplayName = userDisplayName ?? existing.UserDisplayName;
        }
        else
        {
            _context.McpUserAssignments.Add(new McpUserAssignment
            {
                UserId = userId,
                McpServerId = serverId,
                UserEmail = userEmail ?? string.Empty,
                UserDisplayName = userDisplayName ?? string.Empty,
                IsEnabled = true
            });
        }

        await _context.SaveChangesAsync();
        await LogAuditEventAsync(userId, serverId, "ACCESS_GRANTED");
        
        _logger.LogInformation("Granted access to user {UserId} for server {ServerId}", userId, serverId);
        return true;
    }

    public async Task<bool> RevokeAccessAsync(string userId, Guid serverId)
    {
        var access = await _context.McpUserAssignments
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
        return await _context.McpUserAssignments
            .Where(ua => ua.UserId == userId && 
                        ua.IsEnabled &&
                        (ua.ExpiresAt == null || ua.ExpiresAt > DateTime.UtcNow))
            .Include(ua => ua.McpServer)
            .Select(ua => ua.McpServer)
            .Where(s => s.IsEnabled)
            .ToListAsync();
    }

    public async Task LogAuditEventAsync(string userId, Guid? serverId, string action, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        // Map action string to enum - this is a simple mapping, you might want more sophisticated logic
        var eventType = action switch
        {
            "ACCESS_GRANTED" => Data.Enums.McpAuditEventType.ConfigurationChanged,
            "ACCESS_REVOKED" => Data.Enums.McpAuditEventType.AccessDenied,
            _ => Data.Enums.McpAuditEventType.ConnectionAttempt
        };

        var auditLog = new McpAuditLog
        {
            UserId = userId,
            McpServerId = serverId,
            EventType = eventType,
            EventDescription = action,
            AdditionalData = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccess = true
        };

        _context.McpAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
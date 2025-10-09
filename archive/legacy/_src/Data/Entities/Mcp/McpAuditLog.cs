using Data.Enums;

namespace Data.Entities.Mcp;

public class McpAuditLog : EntityBase
{
    public Guid? McpServerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public McpAuditEventType EventType { get; set; }
    public string EventDescription { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string? AdditionalData { get; set; } // JSON serialized data
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public TimeSpan? Duration { get; set; }

    // Navigation properties
    public virtual McpServer? McpServer { get; set; }
}
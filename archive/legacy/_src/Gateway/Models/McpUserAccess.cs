using System.ComponentModel.DataAnnotations;

namespace Gateway.Models;

public class McpUserAccess
{
    public int Id { get; set; }
    
    public int McpServerId { get; set; }
    
    [Required, MaxLength(200)]
    public string UserId { get; set; } = string.Empty; // From JWT sub claim
    
    [MaxLength(200)]
    public string? UserEmail { get; set; }
    
    [MaxLength(1000)]
    public string? Roles { get; set; } // Comma-separated for MVP
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public McpServer McpServer { get; set; } = null!;
}

public class McpAuditLog
{
    public int Id { get; set; }
    
    public int? McpServerId { get; set; }
    
    [Required, MaxLength(200)]
    public string UserId { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty; // "ACCESS", "BLOCKED", "ERROR"
    
    [MaxLength(1000)]
    public string? Details { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public McpServer? McpServer { get; set; }
}
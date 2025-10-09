using System.ComponentModel.DataAnnotations;

namespace Gateway.Models;

public class McpServer
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required, MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;
    
    public McpProtocolType Protocol { get; set; } = McpProtocolType.Http;
    
    public McpAuthType AuthType { get; set; } = McpAuthType.None;
    
    [MaxLength(200)]
    public string? AuthSettings { get; set; } // JSON string for MVP
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public List<McpUserAccess> UserAccess { get; set; } = [];
}

public enum McpProtocolType
{
    Http = 0,
    WebSocket = 1,
    ServerSentEvents = 2,
    Webhook = 3
}

public enum McpAuthType
{
    None = 0,
    Bearer = 1,
    ApiKey = 2,
    OAuth2 = 3
}
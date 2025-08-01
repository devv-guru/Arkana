namespace Data.Entities.Mcp;

public class McpUserApiKey : EntityBase
{
    public Guid McpBackendAuthId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty; // Encrypted
    public bool IsEnabled { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    // Navigation properties
    public virtual McpBackendAuth McpBackendAuth { get; set; } = null!;
}
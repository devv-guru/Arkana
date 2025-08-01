namespace Data.Entities.Mcp;

public class McpUserAssignment : EntityBase
{
    public Guid McpServerId { get; set; }
    public string UserId { get; set; } = string.Empty; // OIDC subject/user identifier
    public string UserEmail { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual McpServer McpServer { get; set; } = null!;
}
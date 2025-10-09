namespace Data.Entities.Mcp;

public class McpRoleAssignment : EntityBase
{
    public Guid McpServerId { get; set; }
    public string RoleName { get; set; } = string.Empty; // OIDC role/group name
    public string RoleDisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual McpServer McpServer { get; set; } = null!;
}
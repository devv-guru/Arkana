using Data.Enums;

namespace Data.Entities.Mcp;

public class McpServer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public McpProtocolType ProtocolType { get; set; } = McpProtocolType.WebSocket;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 0;

    // Navigation properties
    public virtual McpBackendAuth? BackendAuth { get; set; }
    public virtual ICollection<McpUserAssignment> UserAssignments { get; set; } = new List<McpUserAssignment>();
    public virtual ICollection<McpRoleAssignment> RoleAssignments { get; set; } = new List<McpRoleAssignment>();
    public virtual ICollection<McpAuditLog> AuditLogs { get; set; } = new List<McpAuditLog>();
}
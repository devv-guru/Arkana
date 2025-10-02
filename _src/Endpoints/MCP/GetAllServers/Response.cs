namespace Endpoints.MCP.GetAllServers;

public class Response
{
    public List<McpServerDto> Servers { get; set; } = new();
}

public class McpServerDto
{
    public string Id { get; set; } = string.Empty; // ULID string
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public int ProtocolType { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
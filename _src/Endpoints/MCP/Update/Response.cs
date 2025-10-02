namespace Endpoints.MCP.Update;

public class Response
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
using Data.Enums;
using Mediator;

namespace Endpoints.MCP.Update;

public class Request : IRequest<Response?>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public McpProtocolType Protocol { get; set; }
    public bool IsEnabled { get; set; }
}
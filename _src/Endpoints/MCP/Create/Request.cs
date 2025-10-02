using Data.Enums;
using Mediator;

namespace Endpoints.MCP.Create;

public class Request : IRequest<Response>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public McpProtocolType Protocol { get; set; } = McpProtocolType.Http;
    public bool IsEnabled { get; set; } = true;
}
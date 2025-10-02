using Mediator;

namespace Endpoints.MCP.Delete;

public class Request : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}
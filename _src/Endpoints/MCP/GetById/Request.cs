using Mediator;

namespace Endpoints.MCP.GetById;

public class Request : IRequest<Response?>
{
    public string Id { get; set; } = string.Empty;
}
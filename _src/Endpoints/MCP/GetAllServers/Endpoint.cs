using FastEndpoints;
using Mediator;

namespace Endpoints.MCP.GetAllServers;

public class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/servers");
        AllowAnonymous();
        Group<MCPGroup>();
        Summary(s => {
            s.Summary = "Get all MCP servers";
            s.Description = "Retrieves all active MCP servers";
            s.Response<Response>(200, "MCP servers retrieved successfully");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new Request(), ct);
        await Send.OkAsync(result, ct);
    }
}
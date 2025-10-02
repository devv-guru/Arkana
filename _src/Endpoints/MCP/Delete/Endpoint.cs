using FastEndpoints;
using Mediator;

namespace Endpoints.MCP.Delete;

public class Endpoint : Endpoint<Request, object>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Delete("/servers/{id}");
        AllowAnonymous();
        Group<MCPGroup>();
        Summary(s => {
            s.Summary = "Delete MCP server by ULID";
            s.Description = "Soft deletes an MCP server configuration";
            s.Response(204, "MCP server deleted successfully");
            s.Response(404, "MCP server not found");
            s.Response(400, "Invalid ULID format");
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var deleted = await _mediator.Send(req, ct);
        
        if (!deleted)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
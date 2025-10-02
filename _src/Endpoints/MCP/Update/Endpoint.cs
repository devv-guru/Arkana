using FastEndpoints;
using Mediator;

namespace Endpoints.MCP.Update;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/servers/{id}");
        AllowAnonymous();
        Group<MCPGroup>();
        Summary(s => {
            s.Summary = "Update MCP server by ULID";
            s.Description = "Updates an existing MCP server configuration";
            s.Response<Response>(200, "MCP server updated successfully");
            s.Response(404, "MCP server not found");
            s.Response(400, "Invalid request data or ULID format");
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}
using FastEndpoints;
using Mediator;

namespace Endpoints.MCP.GetById;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/servers/{id}");
        AllowAnonymous();
        Group<MCPGroup>();
        Summary(s => {
            s.Summary = "Get MCP server by ULID";
            s.Description = "Retrieves a specific MCP server by its ULID identifier";
            s.Response<Response>(200, "MCP server retrieved successfully");
            s.Response(404, "MCP server not found");
            s.Response(400, "Invalid ULID format");
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
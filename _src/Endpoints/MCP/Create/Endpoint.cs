using FastEndpoints;
using Mediator;

namespace Endpoints.MCP.Create;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/servers");
        AllowAnonymous();
        Group<MCPGroup>();
        Summary(s => {
            s.Summary = "Create new MCP server";
            s.Description = "Creates a new MCP server configuration";
            s.Response<Response>(201, "MCP server created successfully");
            s.Response(400, "Invalid request data");
        });
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        await Send.CreatedAtAsync<GetById.Endpoint>(new { id = result.Id }, result, cancellation: ct);
    }
}
using FastEndpoints;
using Mediator;

namespace Endpoints.Configuration.Update;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("");
        AllowAnonymous();
        Group<ConfigurationGroup>();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        
        if (result.Success)
        {
            await Send.OkAsync(result, ct);
        }
        else
        {
            await Send.ResponseAsync(result, 400, ct);
        }
    }
}

using FastEndpoints;
using Mediator;

namespace Endpoints.Configuration.Reload;

public class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("reload");
        AllowAnonymous();
        Group<ConfigurationGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new Request(), ct);
        
        if (result.Success)
        {
            await SendOkAsync(result, ct);
        }
        else
        {
            await SendAsync(result, 500, ct);
        }
    }
}

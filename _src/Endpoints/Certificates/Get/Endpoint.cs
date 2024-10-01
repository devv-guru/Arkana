using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Certificates.Get;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();
        Group<CertificateGroup>();
        Description(x => x
                .Produces(404)
                .Produces<Response>(200),
            clearDefaults: true
        );
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        if (result is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(result, ct);
    }
}
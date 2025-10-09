using Endpoints.Common.Paging;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Certificates.Find;

public class Endpoint : Endpoint<Request, PagedResult<Response, Response[]>>
{
    private readonly IMediator _mediator;

    public Endpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("find");
        AllowAnonymous();
        Group<CertificateGroup>();
        Description(x => x
                .Produces<PagedResult<Response, Response[]>>(200)
                .AllowAnonymous(),
            clearDefaults: true
        );
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        await Send.OkAsync(result, ct);
    }
}
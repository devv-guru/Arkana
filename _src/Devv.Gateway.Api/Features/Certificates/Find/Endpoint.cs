using Devv.Gateway.Api.Common.Paging;
using FastEndpoints;
using Mediator;

namespace Devv.Gateway.Api.Features.Certificates.Find;

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
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        await SendOkAsync(result, ct);
    }
}
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Certificates.Create;

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
        Group<CertificateGroup>();
        Description(x => x
                .Accepts<Request>()
                .Produces<Response>(201)
                .ProducesProblemFE(400)
                .ProducesProblemFE<InternalErrorResponse>(500),
            clearDefaults: true
        );
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        await Send.CreatedAtAsync<Get.Endpoint>(result.Id, result, cancellation: ct);
    }
}
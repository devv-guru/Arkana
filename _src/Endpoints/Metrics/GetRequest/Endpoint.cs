using FastEndpoints;

namespace Endpoints.Metrics.GetRequest;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/request");
        Group<MetricsGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get request metrics";
            s.Description = "Returns the latest request metrics";
            s.Response<Response>(200, "Success");
        });
    }

    public override Task HandleAsync(Request req, CancellationToken ct)
    {
        return SendOkAsync(Response, ct);
    }
}

using FastEndpoints;

namespace Endpoints.Metrics.GetSystem;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/system");
        Group<MetricsGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get system metrics";
            s.Description = "Returns the latest system metrics";
            s.Response<Response>(200, "Success");
        });
    }

    public override Task HandleAsync(Request req, CancellationToken ct)
    {
        return SendOkAsync(Response, ct);
    }
}

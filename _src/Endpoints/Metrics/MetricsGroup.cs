using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Metrics;

public class MetricsGroup : SubGroup<GatewayGroup>
{
    public MetricsGroup()
    {
        Configure("/metrics", ep =>
        {
            ep.Description(x => x
                .WithTags("Metrics"));
        });
    }
}

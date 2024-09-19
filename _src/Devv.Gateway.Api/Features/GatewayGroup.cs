using FastEndpoints;

namespace Devv.Gateway.Api.Features;

public class GatewayGroup : Group
{
    public GatewayGroup()
    {
        Configure("/gateway",
            ep =>
            {
                ep.Description(
                    x => x.Produces(401)
                        .WithTags("gateway"));
            });
    }
}
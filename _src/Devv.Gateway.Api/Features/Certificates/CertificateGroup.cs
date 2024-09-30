using FastEndpoints;

namespace Devv.Gateway.Api.Features.Certificates;

public class CertificateGroup : SubGroup<GatewayGroup>
{
    public CertificateGroup()
    {
        Configure("/certificates",
            ep =>
            {
                ep.Description(
                    x => x.Produces(401)
                        .WithTags("certificates"));
            });
    }
}
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Certificates;

public class CertificateGroup : SubGroup<GatewayGroup>
{
    public CertificateGroup()
    {
        Configure("/certificates",
            ep =>
            {
                ep.Description(
                    x => x
                        .WithTags("Certificates"));
            });
    }
}
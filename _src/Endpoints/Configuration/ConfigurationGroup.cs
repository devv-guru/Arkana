using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Configuration;

public class ConfigurationGroup : SubGroup<GatewayGroup>
{
    public ConfigurationGroup()
    {
        Configure("/configuration",
            ep =>
            {
                ep.Description(
                    x => x
                        .WithTags("Configuration"));
            });
    }
}

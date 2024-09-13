using FastEndpoints;

namespace Devv.WebServer.Api.Features;

public class ApiGroup : Group
{
    public ApiGroup()
    {
        Configure("/api",
            ep =>
            {
                ep.Description(
                    x => x.Produces(401)
                        .WithTags("api"));
            });
    }
}
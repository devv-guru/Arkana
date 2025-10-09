using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Endpoints;

public class GatewayGroup : Group
{
    public GatewayGroup()
    {
        Configure("/gateway",
            ep =>
            { });
    }
}
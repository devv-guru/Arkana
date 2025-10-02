using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Endpoints.MCP;

public class MCPGroup : SubGroup<GatewayGroup>
{
    public MCPGroup()
    {
        Configure("/mcp",
            ep =>
            {
                ep.Description(
                    x => x
                        .WithTags("MCP Servers"));
            });
    }
}
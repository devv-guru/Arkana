using Gateway.Services;
using System.Security.Claims;

namespace Gateway.Middleware;

/// <summary>
/// Middleware that intercepts MCP requests and exchanges user OIDC tokens for Microsoft Graph access tokens
/// </summary>
public class GraphTokenExchangeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GraphTokenExchangeMiddleware> _logger;

    public GraphTokenExchangeMiddleware(RequestDelegate next, ILogger<GraphTokenExchangeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IGraphTokenService graphTokenService)
    {
        // Only process requests to Microsoft Graph MCP endpoints
        if (!IsGraphMcpRequest(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Allow anonymous access to health check and info endpoints
        if (IsAnonymousHealthCheckRequest(context.Request.Path))
        {
            _logger.LogInformation("Allowing anonymous access to health check endpoint: {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        _logger.LogInformation("Processing Graph MCP request: {Path}", context.Request.Path);

        try
        {
            // Check if user is authenticated
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("Unauthenticated request to Graph MCP endpoint");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication required");
                return;
            }

            // Get user's OIDC token from Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("No Bearer token found in Authorization header");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Bearer token required");
                return;
            }

            var userOidcToken = authHeader.Substring("Bearer ".Length);
            _logger.LogInformation("Exchanging OIDC token for Graph access token");

            // Exchange user's OIDC token for Microsoft Graph access token
            var graphScopes = GetRequiredGraphScopes(context.Request.Path);
            var graphAccessToken = await graphTokenService.ExchangeTokenForGraphAsync(userOidcToken, graphScopes);

            if (string.IsNullOrEmpty(graphAccessToken))
            {
                _logger.LogError("Failed to exchange token for Graph access token");
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Failed to obtain Graph access token");
                return;
            }

            // Inject the Graph access token into the request headers for the downstream MCP server
            context.Request.Headers["X-ARK-TOKEN"] = graphAccessToken;

            // Add security validation header to indicate this request comes from the gateway
            context.Request.Headers["X-MS-Graph-Proxy"] = "true";

            _logger.LogInformation("Successfully injected Graph access token into request headers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token exchange for Graph MCP request");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error during token exchange");
            return;
        }

        // Continue to the next middleware (YARP proxy)
        await _next(context);
    }

    private static bool IsGraphMcpRequest(PathString path)
    {
        // Check if this is a request to Microsoft Graph MCP endpoints
        return path.StartsWithSegments("/mcp/msgraph", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAnonymousHealthCheckRequest(PathString path)
    {
        // Allow anonymous access to health check and info endpoints
        return path.StartsWithSegments("/mcp/msgraph/health", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWithSegments("/mcp/msgraph/info", StringComparison.OrdinalIgnoreCase);
    }

    private static string[] GetRequiredGraphScopes(PathString path)
    {
        // Return appropriate Microsoft Graph scopes based on the request path
        // In a production system, this could be more sophisticated based on the specific MCP tool being called
        return new[]
        {
            "https://graph.microsoft.com/User.Read",
            "https://graph.microsoft.com/Mail.Read",
            "https://graph.microsoft.com/Calendars.Read", 
            "https://graph.microsoft.com/Directory.Read.All",
            "https://graph.microsoft.com/Group.Read.All"
        };
    }
}
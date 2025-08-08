using Microsoft.AspNetCore.Http;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Common utilities for MCP tools
/// </summary>
public static class McpToolsHelper
{
    /// <summary>
    /// Extract access token and correlation ID from HTTP headers with security validation
    /// </summary>
    public static (string accessToken, string? correlationId) ExtractTokenAndCorrelationId(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null)
            throw new InvalidOperationException("HTTP context not available");
            
        // Security: Validate that request came through Arkana Gateway proxy
        var proxyHeader = context.Request.Headers["X-MS-Graph-Proxy"].FirstOrDefault();
        if (string.IsNullOrEmpty(proxyHeader) || proxyHeader != "true")
        {
            throw new UnauthorizedAccessException("Direct calls to MCP server are not allowed. All requests must go through Arkana Gateway.");
        }
        
        // Extract token from standardized header injected by Arkana Gateway
        var arkToken = context.Request.Headers["X-ARK-TOKEN"].FirstOrDefault();
        if (string.IsNullOrEmpty(arkToken))
        {
            throw new UnauthorizedAccessException("No access token provided by Arkana Gateway.");
        }

        // Extract correlation ID for tracing
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        
        return (arkToken, correlationId);
    }
}
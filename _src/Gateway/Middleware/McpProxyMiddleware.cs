using Gateway.Services;
using System.Text.RegularExpressions;

namespace Gateway.Middleware;

public class McpProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<McpProxyMiddleware> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public McpProxyMiddleware(
        RequestDelegate next,
        ILogger<McpProxyMiddleware> logger,
        IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        
        // Check if this is an MCP route
        if (path != null && path.StartsWith("/mcp/", StringComparison.OrdinalIgnoreCase))
        {
            // Extract server name and remaining path
            var mcpPattern = @"^/mcp/([^/]+)/?(.*)$";
            var match = Regex.Match(path, mcpPattern);
            
            if (match.Success)
            {
                var serverName = match.Groups[1].Value;
                var remainingPath = match.Groups[2].Value;
                
                _logger.LogInformation("MCP request for server: {ServerName}, path: {RemainingPath}", 
                    serverName, remainingPath);
                
                // Get server from service
                using var scope = context.RequestServices.CreateScope();
                var mcpService = scope.ServiceProvider.GetRequiredService<IMcpServerService>();
                var server = await mcpService.GetServerByNameAsync(serverName);
                
                if (server != null && server.IsEnabled)
                {
                    // Build target URL - map to /api/ prefix
                    var targetPath = string.IsNullOrEmpty(remainingPath) ? "/" : $"/api/{remainingPath}";
                    var targetUrl = $"{server.Endpoint.TrimEnd('/')}{targetPath}";
                    
                    _logger.LogInformation("Proxying {OriginalPath} to {TargetUrl}", path, targetUrl);
                    
                    // Proxy the request
                    await ProxyRequestAsync(context, targetUrl);
                    return;
                }
                else
                {
                    _logger.LogWarning("MCP server not found or disabled: {ServerName}", serverName);
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync($"MCP server '{serverName}' not found");
                    return;
                }
            }
        }
        
        // Not an MCP route, continue to next middleware
        await _next(context);
    }
    
    private async Task ProxyRequestAsync(HttpContext context, string targetUrl)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            
            // Create proxy request
            using var proxyRequest = new HttpRequestMessage(
                new HttpMethod(context.Request.Method), 
                targetUrl);
            
            // Copy headers (excluding host)
            foreach (var header in context.Request.Headers)
            {
                if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    proxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
            
            // Copy request body if present
            if (context.Request.ContentLength > 0)
            {
                proxyRequest.Content = new StreamContent(context.Request.Body);
                if (context.Request.ContentType != null)
                {
                    proxyRequest.Content.Headers.TryAddWithoutValidation("Content-Type", context.Request.ContentType);
                }
            }
            
            // Send request
            using var response = await httpClient.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead);
            
            // Copy response status
            context.Response.StatusCode = (int)response.StatusCode;
            
            // Copy response headers
            foreach (var header in response.Headers)
            {
                context.Response.Headers.TryAdd(header.Key, header.Value.ToArray());
            }
            
            foreach (var header in response.Content.Headers)
            {
                context.Response.Headers.TryAdd(header.Key, header.Value.ToArray());
            }
            
            // Copy response body
            await response.Content.CopyToAsync(context.Response.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying request to {TargetUrl}", targetUrl);
            context.Response.StatusCode = 502;
            await context.Response.WriteAsync("Bad Gateway");
        }
    }
}
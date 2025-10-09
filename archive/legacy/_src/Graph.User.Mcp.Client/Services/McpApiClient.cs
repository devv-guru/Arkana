using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Services;

/// <summary>
/// Client for making real MCP API calls through the Arkana Gateway
/// </summary>
public class McpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpApiClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _gatewayBaseUrl;
    private readonly string _mcpEndpoint;

    public McpApiClient(HttpClient httpClient, ILogger<McpApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        _gatewayBaseUrl = _configuration["Gateway:BaseUrl"] ?? "http://localhost:5000";
        _mcpEndpoint = _configuration["Gateway:McpEndpoint"] ?? "/mcp/msgraph";
        
        _logger.LogInformation("MCP API Client initialized for Gateway: {GatewayUrl}", _gatewayBaseUrl + _mcpEndpoint);
    }

    /// <summary>
    /// List available MCP tools through the Gateway
    /// </summary>
    public async Task<string> ListMcpToolsAsync(string accessToken)
    {
        try
        {
            var requestUrl = $"{_gatewayBaseUrl}{_mcpEndpoint}";
            _logger.LogInformation("Listing MCP tools via {Url}", requestUrl);

            // Create MCP tools/list request
            var mcpRequest = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/list",
                @params = new { }
            };

            var jsonContent = JsonSerializer.Serialize(mcpRequest, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Set MCP protocol headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");

            var response = await _httpClient.PostAsync(requestUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("MCP tools list succeeded");
                return responseContent;
            }
            else
            {
                _logger.LogError("MCP tools list failed with status {StatusCode}: {Response}", 
                    response.StatusCode, responseContent);
                
                return JsonSerializer.Serialize(new 
                { 
                    Success = false, 
                    Error = $"HTTP {response.StatusCode}", 
                    Details = responseContent 
                }, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing MCP tools");
            return JsonSerializer.Serialize(new 
            { 
                Success = false, 
                Error = ex.Message, 
                Details = ex.ToString() 
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Make an MCP tool call through the Gateway
    /// </summary>
    public async Task<string> CallMcpToolAsync(string toolName, object? parameters, string accessToken)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var toolScope = LogContext.PushProperty("McpTool", toolName);
        
        try
        {
            var requestUrl = $"{_gatewayBaseUrl}{_mcpEndpoint}";
            _logger.LogInformation("Making MCP call to {Tool} via {Url} with correlation {CorrelationId}", 
                toolName, requestUrl, correlationId);

            // Create MCP request payload
            var mcpRequest = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/call",
                @params = new
                {
                    name = toolName,
                    arguments = parameters ?? new { }
                }
            };

            var jsonContent = JsonSerializer.Serialize(mcpRequest, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Set MCP protocol headers (Gateway will exchange for Graph token)
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            _logger.LogDebug("Sending MCP request: {RequestBody}", jsonContent);
            var response = await _httpClient.PostAsync(requestUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("MCP call to {Tool} succeeded with correlation {CorrelationId}", toolName, correlationId);
                _logger.LogDebug("MCP response: {ResponseBody}", responseContent);
                return responseContent;
            }
            else
            {
                _logger.LogError("MCP call to {Tool} failed with status {StatusCode} and correlation {CorrelationId}: {Response}", 
                    toolName, response.StatusCode, correlationId, responseContent);
                
                return JsonSerializer.Serialize(new 
                { 
                    Success = false, 
                    Error = $"HTTP {response.StatusCode}", 
                    Details = responseContent 
                }, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making MCP call to {Tool}", toolName);
            return JsonSerializer.Serialize(new 
            { 
                Success = false, 
                Error = ex.Message, 
                Details = ex.ToString() 
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Test connectivity to the Gateway
    /// </summary>
    public async Task<bool> TestConnectivityAsync()
    {
        try
        {
            var healthUrl = $"{_gatewayBaseUrl}/health";
            _logger.LogInformation("Testing connectivity to Gateway at {Url}", healthUrl);
            
            var response = await _httpClient.GetAsync(healthUrl);
            var isHealthy = response.IsSuccessStatusCode;
            
            _logger.LogInformation("Gateway connectivity test: {Status}", isHealthy ? "Success" : "Failed");
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Gateway connectivity");
            return false;
        }
    }

    /// <summary>
    /// Test MCP server connectivity via Gateway
    /// </summary>
    public async Task<bool> TestMcpConnectivityAsync()
    {
        try
        {
            var mcpHealthUrl = $"{_gatewayBaseUrl}{_mcpEndpoint}/health";
            _logger.LogInformation("Testing MCP server connectivity via Gateway at {Url}", mcpHealthUrl);
            
            var response = await _httpClient.GetAsync(mcpHealthUrl);
            var isHealthy = response.IsSuccessStatusCode;
            
            if (isHealthy)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("MCP server connectivity test successful. Response: {Response}", content);
            }
            else
            {
                _logger.LogWarning("MCP server connectivity test failed with status {StatusCode}", response.StatusCode);
            }
            
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing MCP server connectivity");
            return false;
        }
    }
}
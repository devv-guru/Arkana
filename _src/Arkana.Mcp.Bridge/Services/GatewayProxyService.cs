using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arkana.Mcp.Bridge.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arkana.Mcp.Bridge.Services;

public interface IGatewayProxyService
{
    Task<Tool[]> GetToolsAsync(CancellationToken cancellationToken = default);
    Task<CallToolResult> CallToolAsync(string toolName, Dictionary<string, object>? arguments, CancellationToken cancellationToken = default);
}

public class GatewayProxyService : IGatewayProxyService
{
    private readonly ILogger<GatewayProxyService> _logger;
    private readonly IAuthenticationService _authService;
    private readonly HttpClient _httpClient;
    private readonly string _gatewayUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public GatewayProxyService(
        ILogger<GatewayProxyService> logger,
        IAuthenticationService authService,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _authService = authService;
        _httpClient = httpClient;
        
        _gatewayUrl = configuration["Gateway:Url"] 
            ?? throw new InvalidOperationException("Gateway:Url is required");

        var timeout = configuration["Gateway:Timeout"];
        if (timeout != null && TimeSpan.TryParse(timeout, out var timeoutSpan))
        {
            _httpClient.Timeout = timeoutSpan;
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<Tool[]> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching tools from gateway: {GatewayUrl}", _gatewayUrl);
            
            await SetAuthorizationHeaderAsync(cancellationToken);
            
            var response = await _httpClient.GetAsync($"{_gatewayUrl}/tools", cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Gateway tools response: {Content}", content);
            
            var gatewayTools = JsonSerializer.Deserialize<GatewayTool[]>(content, _jsonOptions);
            
            if (gatewayTools == null)
            {
                _logger.LogWarning("Gateway returned null tools list");
                return [];
            }

            var mcpTools = gatewayTools.Select(ConvertToMcpTool).ToArray();
            _logger.LogInformation("Successfully retrieved {ToolCount} tools from gateway", mcpTools.Length);
            
            return mcpTools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tools from gateway");
            throw;
        }
    }

    public async Task<CallToolResult> CallToolAsync(string toolName, Dictionary<string, object>? arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling tool '{ToolName}' via gateway", toolName);
            
            await SetAuthorizationHeaderAsync(cancellationToken);
            
            var request = new GatewayToolCallRequest
            {
                ToolId = toolName,
                Input = arguments ?? new Dictionary<string, object>()
            };
            
            var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            
            _logger.LogDebug("Gateway tool call request: {Request}", requestJson);
            
            var response = await _httpClient.PostAsync($"{_gatewayUrl}/tools/call", requestContent, cancellationToken);
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Gateway tool call response: {Response}", responseContent);
            
            if (!response.IsSuccessStatusCode)
            {
                return new CallToolResult(
                    Content: [new ToolResponseContent(
                        Type: "text",
                        Text: $"Gateway error ({response.StatusCode}): {responseContent}")],
                    IsError: true
                );
            }
            
            var gatewayResponse = JsonSerializer.Deserialize<GatewayToolCallResponse>(responseContent, _jsonOptions);
            
            return ConvertToMcpResult(gatewayResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call tool '{ToolName}' via gateway", toolName);
            
            return new CallToolResult(
                Content: [new ToolResponseContent(
                    Type: "text",
                    Text: $"Bridge error: {ex.Message}")],
                IsError: true
            );
        }
    }

    private async Task SetAuthorizationHeaderAsync(CancellationToken cancellationToken)
    {
        var token = await _authService.GetAccessTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static Tool ConvertToMcpTool(GatewayTool gatewayTool)
    {
        return new Tool(
            Name: gatewayTool.ToolId ?? gatewayTool.Name ?? "unknown",
            Description: gatewayTool.Description ?? "No description available",
            InputSchema: gatewayTool.InputSchema ?? new { type = "object", properties = new { } }
        );
    }

    private static CallToolResult ConvertToMcpResult(GatewayToolCallResponse? gatewayResponse)
    {
        if (gatewayResponse?.Output == null)
        {
            return new CallToolResult(
                Content: [new ToolResponseContent(Type: "text", Text: "No response from gateway")],
                IsError: false
            );
        }

        var content = new List<ToolResponseContent>();

        if (gatewayResponse.Output is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                content.Add(new ToolResponseContent(Type: "text", Text: element.GetString()));
            }
            else
            {
                content.Add(new ToolResponseContent(
                    Type: "text", 
                    Text: JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true })
                ));
            }
        }
        else
        {
            content.Add(new ToolResponseContent(
                Type: "text",
                Text: JsonSerializer.Serialize(gatewayResponse.Output, new JsonSerializerOptions { WriteIndented = true })
            ));
        }

        return new CallToolResult(
            Content: content.ToArray(),
            IsError: false
        );
    }
}

public class GatewayTool
{
    public string? ToolId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public object? InputSchema { get; set; }
    public string[]? Tags { get; set; }
}

public class GatewayToolCallRequest
{
    public string ToolId { get; set; } = string.Empty;
    public Dictionary<string, object> Input { get; set; } = new();
}

public class GatewayToolCallResponse
{
    public object? Output { get; set; }
}
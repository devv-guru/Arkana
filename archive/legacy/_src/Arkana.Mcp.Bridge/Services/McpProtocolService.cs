using System.Text.Json;
using System.Text.Json.Serialization;
using Arkana.Mcp.Bridge.Models;
using Microsoft.Extensions.Logging;

namespace Arkana.Mcp.Bridge.Services;

public interface IMcpProtocolService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public class McpProtocolService : IMcpProtocolService
{
    private readonly ILogger<McpProtocolService> _logger;
    private readonly IGatewayProxyService _gatewayProxy;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _initialized;

    public McpProtocolService(
        ILogger<McpProtocolService> logger,
        IGatewayProxyService gatewayProxy)
    {
        _logger = logger;
        _gatewayProxy = gatewayProxy;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MCP Protocol Service");

        try
        {
            await ProcessStdioAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP Protocol Service");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MCP Protocol Service");
        return Task.CompletedTask;
    }

    private async Task ProcessStdioAsync(CancellationToken cancellationToken)
    {
        using var stdin = Console.OpenStandardInput();
        using var stdout = Console.OpenStandardOutput();
        using var reader = new StreamReader(stdin);
        using var writer = new StreamWriter(stdout) { AutoFlush = true };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                _logger.LogDebug("Received: {Line}", line);

                var response = await ProcessMessageAsync(line, cancellationToken);
                if (response != null)
                {
                    var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                    _logger.LogDebug("Sending: {Response}", responseJson);
                    await writer.WriteLineAsync(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                var errorResponse = new McpResponse<object>(
                    Id: "unknown",
                    Error: new McpError(-1, ex.Message)
                );
                var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                await writer.WriteLineAsync(errorJson);
            }
        }
    }

    private async Task<object?> ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;

            if (!root.TryGetProperty("method", out var methodProperty))
            {
                return new McpResponse<object>(
                    Id: root.TryGetProperty("id", out var id) ? id.GetString() ?? "unknown" : "unknown",
                    Error: new McpError(-32600, "Invalid Request: missing method")
                );
            }

            var method = methodProperty.GetString();
            var requestId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "unknown" : "unknown";

            return method switch
            {
                "initialize" => await HandleInitializeAsync(root, requestId, cancellationToken),
                "tools/list" => await HandleListToolsAsync(requestId, cancellationToken),
                "tools/call" => await HandleCallToolAsync(root, requestId, cancellationToken),
                _ => new McpResponse<object>(
                    Id: requestId,
                    Error: new McpError(-32601, $"Method not found: {method}")
                )
            };
        }
        catch (JsonException ex)
        {
            return new McpResponse<object>(
                Id: "unknown",
                Error: new McpError(-32700, $"Parse error: {ex.Message}")
            );
        }
    }

    private Task<McpResponse<InitializeResult>> HandleInitializeAsync(
        JsonElement root, 
        string requestId, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling initialize request");

        try
        {
            var initializeResult = new InitializeResult(
                ProtocolVersion: "2024-11-05",
                Capabilities: new ServerCapabilities(
                    Tools: new ToolsCapability(ListChanged: true)
                ),
                ServerInfo: new ServerInfo(
                    Name: "Arkana MCP Bridge",
                    Version: "1.0.0"
                )
            );

            _initialized = true;
            return Task.FromResult(new McpResponse<InitializeResult>(requestId, initializeResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling initialize");
            return Task.FromResult(new McpResponse<InitializeResult>(
                requestId,
                Error: new McpError(-32603, $"Internal error: {ex.Message}")
            ));
        }
    }

    private async Task<McpResponse<ListToolsResult>> HandleListToolsAsync(
        string requestId, 
        CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            return new McpResponse<ListToolsResult>(
                requestId,
                Error: new McpError(-32002, "Server not initialized")
            );
        }

        _logger.LogInformation("Handling tools/list request");

        try
        {
            var tools = await _gatewayProxy.GetToolsAsync(cancellationToken);
            var result = new ListToolsResult(tools);
            
            return new McpResponse<ListToolsResult>(requestId, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tools");
            return new McpResponse<ListToolsResult>(
                requestId,
                Error: new McpError(-32603, $"Internal error: {ex.Message}")
            );
        }
    }

    private async Task<McpResponse<CallToolResult>> HandleCallToolAsync(
        JsonElement root, 
        string requestId, 
        CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            return new McpResponse<CallToolResult>(
                requestId,
                Error: new McpError(-32002, "Server not initialized")
            );
        }

        try
        {
            if (!root.TryGetProperty("params", out var paramsElement))
            {
                return new McpResponse<CallToolResult>(
                    requestId,
                    Error: new McpError(-32602, "Invalid params: missing params")
                );
            }

            var callParams = JsonSerializer.Deserialize<CallToolParams>(paramsElement.GetRawText(), _jsonOptions);
            if (callParams == null)
            {
                return new McpResponse<CallToolResult>(
                    requestId,
                    Error: new McpError(-32602, "Invalid params: cannot deserialize")
                );
            }

            _logger.LogInformation("Handling tools/call request for tool: {ToolName}", callParams.Name);

            var result = await _gatewayProxy.CallToolAsync(callParams.Name, callParams.Arguments, cancellationToken);
            return new McpResponse<CallToolResult>(requestId, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling tool");
            return new McpResponse<CallToolResult>(
                requestId,
                Error: new McpError(-32603, $"Internal error: {ex.Message}")
            );
        }
    }
}
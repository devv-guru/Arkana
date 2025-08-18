using System.Text.Json.Serialization;

namespace Arkana.Mcp.Bridge.Models;

public record McpRequest<T>(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] T? Params
);

public record McpResponse<T>(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("result")] T? Result = default,
    [property: JsonPropertyName("error")] McpError? Error = null
);

public record McpError(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] object? Data = null
);

public record McpNotification<T>(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] T? Params
);

public record InitializeParams(
    [property: JsonPropertyName("protocolVersion")] string ProtocolVersion,
    [property: JsonPropertyName("capabilities")] ClientCapabilities Capabilities,
    [property: JsonPropertyName("clientInfo")] ClientInfo ClientInfo
);

public record ClientCapabilities(
    [property: JsonPropertyName("experimental")] Dictionary<string, object>? Experimental = null,
    [property: JsonPropertyName("sampling")] object? Sampling = null
);

public record ClientInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version
);

public record InitializeResult(
    [property: JsonPropertyName("protocolVersion")] string ProtocolVersion,
    [property: JsonPropertyName("capabilities")] ServerCapabilities Capabilities,
    [property: JsonPropertyName("serverInfo")] ServerInfo ServerInfo
);

public record ServerCapabilities(
    [property: JsonPropertyName("tools")] ToolsCapability? Tools = null,
    [property: JsonPropertyName("experimental")] Dictionary<string, object>? Experimental = null
);

public record ToolsCapability(
    [property: JsonPropertyName("listChanged")] bool ListChanged = true
);

public record ServerInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version
);

public record ListToolsParams();

public record ListToolsResult(
    [property: JsonPropertyName("tools")] Tool[] Tools
);

public record Tool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("inputSchema")] object InputSchema
);

public record CallToolParams(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("arguments")] Dictionary<string, object>? Arguments = null
);

public record CallToolResult(
    [property: JsonPropertyName("content")] ToolResponseContent[] Content,
    [property: JsonPropertyName("isError")] bool IsError = false
);

public record ToolResponseContent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string? Text = null,
    [property: JsonPropertyName("data")] object? Data = null
);
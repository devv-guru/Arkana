using Gateway.Services;
using System.Text.Json;

namespace Gateway.Middleware;

public class PromptInjectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPromptSafetyService _safetyService;
    private readonly IUserService _userService;
    private readonly ILogger<PromptInjectionMiddleware> _logger;

    public PromptInjectionMiddleware(
        RequestDelegate next,
        IPromptSafetyService safetyService,
        IUserService userService,
        ILogger<PromptInjectionMiddleware> logger)
    {
        _next = next;
        _safetyService = safetyService;
        _userService = userService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only check MCP proxy requests with bodies
        if (!IsMcpProxyRequest(context) || !HasRequestBody(context))
        {
            await _next(context);
            return;
        }

        try
        {
            // Enable buffering to read body multiple times
            context.Request.EnableBuffering();
            
            // Read and analyze request body
            var body = await ReadRequestBodyAsync(context.Request);
            var textContent = ExtractTextFromMcpPayload(body);
            
            if (!string.IsNullOrWhiteSpace(textContent))
            {
                var safetyResult = _safetyService.AnalyzeContent(textContent);
                
                if (!safetyResult.IsSafe)
                {
                    await LogSecurityViolation(context, safetyResult);
                    
                    // Block the request
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        error = "Request blocked: potential prompt injection detected",
                        code = "PROMPT_INJECTION_DETECTED",
                        riskScore = safetyResult.RiskScore,
                        patterns = safetyResult.DetectedPatterns,
                        correlationId = context.TraceIdentifier
                    };
                    
                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }

            // Reset stream position and continue
            context.Request.Body.Position = 0;
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in prompt injection analysis");
            
            // Fail open for MVP - if analysis fails, allow request but log the failure
            context.Request.Body.Position = 0;
            await _next(context);
        }
    }

    private static bool IsMcpProxyRequest(HttpContext context)
    {
        // Check if this is a proxied MCP request (not our admin API)
        return context.Request.Path.StartsWithSegments("/mcp") && 
               context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasRequestBody(HttpContext context)
    {
        return context.Request.ContentLength > 0 || 
               context.Request.Headers.ContainsKey("Transfer-Encoding");
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string ExtractTextFromMcpPayload(string jsonBody)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<JsonElement>(jsonBody);
            var textParts = new List<string>();
            
            ExtractTextFromElement(payload, textParts);
            
            return string.Join(" ", textParts);
        }
        catch
        {
            // Fallback to analyzing entire body if JSON parsing fails
            return jsonBody;
        }
    }

    private static void ExtractTextFromElement(JsonElement element, List<string> textParts)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var text = element.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    textParts.Add(text);
                break;
                
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    // Focus on likely user input fields
                    if (IsUserInputField(property.Name))
                    {
                        ExtractTextFromElement(property.Value, textParts);
                    }
                }
                break;
                
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    ExtractTextFromElement(item, textParts);
                }
                break;
        }
    }

    private static bool IsUserInputField(string fieldName)
    {
        var userInputFields = new[] { "prompt", "message", "content", "input", "query", "text", "description", "arguments" };
        return userInputFields.Contains(fieldName.ToLowerInvariant());
    }

    private async Task LogSecurityViolation(HttpContext context, PromptSafetyResult safetyResult)
    {
        var userId = context.User?.Identity?.Name ?? "anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        
        var details = $"Risk: {safetyResult.RiskScore:F2}, Patterns: [{string.Join(", ", safetyResult.DetectedPatterns)}]";
        
        await _userService.LogAuditEventAsync(userId, null, "PROMPT_INJECTION_BLOCKED", details, ipAddress, userAgent);
        
        _logger.LogWarning("Blocked potential prompt injection from user {UserId} at {IpAddress}. Risk: {Risk:F2}, Patterns: {Patterns}", 
            userId, ipAddress, safetyResult.RiskScore, string.Join(", ", safetyResult.DetectedPatterns));
    }
}
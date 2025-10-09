using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

namespace Graph.User.Mcp.Server.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";
        
        if (context.Request.Headers.TryGetValue(correlationIdHeader, out var existingId) && 
            !string.IsNullOrEmpty(existingId.FirstOrDefault()))
        {
            return existingId.First()!;
        }

        var newId = Guid.NewGuid().ToString();
        context.Response.Headers.Append(correlationIdHeader, newId);
        return newId;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}", 
            correlationId, context.Request.Path);

        var (statusCode, errorResponse) = GetErrorResponse(exception, correlationId);
        
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static (HttpStatusCode statusCode, object errorResponse) GetErrorResponse(Exception exception, string correlationId)
    {
        return exception switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                CreateMcpError(-32001, "Unauthorized access", "Authentication or authorization failed", correlationId)
            ),
            
            ArgumentException => (
                HttpStatusCode.BadRequest,
                CreateMcpError(-32602, "Invalid params", exception.Message, correlationId)
            ),
            
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                CreateMcpError(-32003, "Request timeout", "The request took too long to process", correlationId)
            ),
            
            HttpRequestException httpEx => (
                HttpStatusCode.BadGateway,
                CreateMcpError(-32004, "External service error", 
                    $"Microsoft Graph API error: {httpEx.Message}", correlationId)
            ),
            
            TaskCanceledException => (
                HttpStatusCode.RequestTimeout,
                CreateMcpError(-32003, "Request cancelled", "The request was cancelled or timed out", correlationId)
            ),
            
            _ => (
                HttpStatusCode.InternalServerError,
                CreateMcpError(-32603, "Internal error", 
                    "An unexpected error occurred. Please check the correlation ID in logs.", correlationId)
            )
        };
    }

    private static object CreateMcpError(int code, string message, string details, string correlationId)
    {
        return new
        {
            jsonrpc = "2.0",
            error = new
            {
                code = code,
                message = message,
                data = new
                {
                    details = details,
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    service = "Microsoft Graph MCP Server"
                }
            },
            id = (string?)null
        };
    }
}
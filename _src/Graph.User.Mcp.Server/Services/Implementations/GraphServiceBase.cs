using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Graph.User.Mcp.Server.Configuration;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Base class for Graph services with optimized HTTP operations, proper port management, and ValueTask usage
/// </summary>
public abstract class GraphServiceBase : IDisposable
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly McpConfiguration Configuration;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private bool _disposed = false;

    protected GraphServiceBase(HttpClient httpClient, ILogger logger, IOptions<McpConfiguration> configuration)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        
        // Configure HttpClient for Microsoft Graph (only if not already configured)
        if (HttpClient.BaseAddress == null)
        {
            HttpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
        }
        
        // Set timeout and connection management for port exhaustion prevention
        HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.Performance.GraphApiTimeoutSeconds);
        
        // Configure default headers that don't change per request
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Arkana-MCP-Server", "2.0"));
        
        // Initialize retry policy
        _retryPolicy = CreateRetryPolicy();
    }

    /// <summary>
    /// Create optimized retry policy for Graph API calls
    /// </summary>
    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => ShouldRetry(msg))
            .WaitAndRetryAsync(
                retryCount: Configuration.Performance.MaxRetryAttempts,
                sleepDurationProvider: retryAttempt => CalculateDelay(retryAttempt),
                onRetry: OnRetryAttempt);
    }

    /// <summary>
    /// Determine if response should trigger retry
    /// </summary>
    private static bool ShouldRetry(HttpResponseMessage response)
    {
        // Don't retry client errors (4xx) except rate limiting and auth issues
        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
        {
            return response.StatusCode == HttpStatusCode.TooManyRequests ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden;
        }
        
        // Retry server errors (5xx)
        return (int)response.StatusCode >= 500;
    }

    /// <summary>
    /// Calculate exponential backoff with jitter to prevent thundering herd
    /// </summary>
    private TimeSpan CalculateDelay(int retryAttempt)
    {
        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
        return baseDelay.Add(jitter);
    }

    /// <summary>
    /// Handle retry attempt logging
    /// </summary>
    private void OnRetryAttempt(DelegateResult<HttpResponseMessage> outcome, TimeSpan duration, int retryCount, Context context)
    {
        var statusCode = outcome.Result?.StatusCode ?? HttpStatusCode.InternalServerError;
        var correlationId = context.GetValueOrDefault("CorrelationId", "N/A");
        
        Logger.LogWarning(
            "Graph API retry {RetryCount}/{MaxRetries} after {Duration}ms | Status: {StatusCode} | Operation: {Operation} | CorrelationId: {CorrelationId}",
            retryCount, Configuration.Performance.MaxRetryAttempts, duration.TotalMilliseconds,
            statusCode, context.OperationKey, correlationId);
    }

    /// <summary>
    /// Execute a Graph API GET request with optimizations for performance and reliability
    /// </summary>
    protected ValueTask<JsonDocument?> ExecuteGraphGetRequestAsync(string endpoint, string accessToken, string operationName, string? correlationId = null)
    {
        return ExecuteGraphRequestCoreAsync(HttpMethod.Get, endpoint, accessToken, operationName, null, correlationId);
    }

    /// <summary>
    /// Execute a Graph API POST request with JSON body
    /// </summary>
    protected ValueTask<JsonDocument?> ExecuteGraphPostRequestAsync(string endpoint, string accessToken, string operationName, object? requestBody = null, string? correlationId = null)
    {
        return ExecuteGraphRequestCoreAsync(HttpMethod.Post, endpoint, accessToken, operationName, requestBody, correlationId);
    }

    /// <summary>
    /// Execute a Graph API PATCH request with JSON body
    /// </summary>
    protected ValueTask<JsonDocument?> ExecuteGraphPatchRequestAsync(string endpoint, string accessToken, string operationName, object requestBody, string? correlationId = null)
    {
        return ExecuteGraphRequestCoreAsync(HttpMethod.Patch, endpoint, accessToken, operationName, requestBody, correlationId);
    }

    /// <summary>
    /// Execute a Graph API DELETE request
    /// </summary>
    protected ValueTask<JsonDocument?> ExecuteGraphDeleteRequestAsync(string endpoint, string accessToken, string operationName, string? correlationId = null)
    {
        return ExecuteGraphRequestCoreAsync(HttpMethod.Delete, endpoint, accessToken, operationName, null, correlationId);
    }

    /// <summary>
    /// Execute a Graph API request to download file content
    /// </summary>
    protected async ValueTask<Stream?> ExecuteGraphStreamRequestAsync(string endpoint, string accessToken, string operationName, string? correlationId = null)
    {
        using var request = CreateHttpRequest(HttpMethod.Get, endpoint, accessToken, null, correlationId);
        
        try
        {
            Logger.LogDebug("Executing Graph stream request: {Operation} | Endpoint: {Endpoint} | CorrelationId: {CorrelationId}", 
                operationName, endpoint, correlationId ?? "N/A");

            var context = new Context(operationName);
            if (!string.IsNullOrEmpty(correlationId))
            {
                context["CorrelationId"] = correlationId;
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var requestClone = CloneHttpRequest(request);
                return await HttpClient.SendAsync(requestClone);
            });

            if (response.IsSuccessStatusCode)
            {
                Logger.LogDebug("Graph stream request successful: {Operation} | CorrelationId: {CorrelationId}", 
                    operationName, correlationId ?? "N/A");
                return await response.Content.ReadAsStreamAsync();
            }

            await LogAndThrowHttpError(response, operationName, correlationId);
            return null; // Never reached due to exception
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            Logger.LogError(ex, "Error executing Graph stream request: {Operation} | CorrelationId: {CorrelationId}", 
                operationName, correlationId ?? "N/A");
            throw;
        }
    }

    /// <summary>
    /// Core method for executing Graph API requests with proper resource management
    /// </summary>
    private async ValueTask<JsonDocument?> ExecuteGraphRequestCoreAsync(HttpMethod method, string endpoint, string accessToken, string operationName, object? requestBody, string? correlationId)
    {
        using var request = CreateHttpRequest(method, endpoint, accessToken, requestBody, correlationId);
        
        try
        {
            Logger.LogDebug("Executing Graph request: {Method} {Operation} | Endpoint: {Endpoint} | CorrelationId: {CorrelationId}", 
                method, operationName, endpoint, correlationId ?? "N/A");

            var context = new Context(operationName);
            if (!string.IsNullOrEmpty(correlationId))
            {
                context["CorrelationId"] = correlationId;
            }

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var requestClone = CloneHttpRequest(request);
                return await HttpClient.SendAsync(requestClone);
            });

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Handle empty responses (like DELETE operations)
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Logger.LogDebug("Graph request successful with empty response: {Operation} | CorrelationId: {CorrelationId}", 
                            operationName, correlationId ?? "N/A");
                        return null;
                    }

                    Logger.LogDebug("Graph request successful: {Operation} | ResponseLength: {Length} | CorrelationId: {CorrelationId}", 
                        operationName, content.Length, correlationId ?? "N/A");
                    
                    return JsonDocument.Parse(content);
                }

                await LogAndThrowHttpError(response, operationName, correlationId);
                return null; // Never reached due to exception
            }
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            Logger.LogError(ex, "Error executing Graph request: {Method} {Operation} | CorrelationId: {CorrelationId}", 
                method, operationName, correlationId ?? "N/A");
            throw;
        }
    }

    /// <summary>
    /// Create HTTP request with proper headers and content
    /// </summary>
    private HttpRequestMessage CreateHttpRequest(HttpMethod method, string endpoint, string accessToken, object? requestBody, string? correlationId)
    {
        var request = new HttpRequestMessage(method, endpoint);
        
        // Set authorization header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        // Add correlation ID for tracing
        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("X-Correlation-ID", correlationId);
        }

        // Add request body for POST/PATCH operations
        if (requestBody != null && (method == HttpMethod.Post || method == HttpMethod.Patch))
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    /// Clone HTTP request for retry scenarios to avoid request already sent exceptions
    /// </summary>
    private static HttpRequestMessage CloneHttpRequest(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        
        // Copy headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (original.Content != null)
        {
            var content = original.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            clone.Content = new StringContent(content, Encoding.UTF8, "application/json");
            
            // Copy content headers
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    /// <summary>
    /// Log error details and throw appropriate exception
    /// </summary>
    private async Task LogAndThrowHttpError(HttpResponseMessage response, string operationName, string? correlationId)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        var errorMessage = $"Graph API request failed: {response.StatusCode} - {response.ReasonPhrase}";
        
        Logger.LogError(
            "Graph API error: {Operation} | Status: {StatusCode} | Error: {Error} | CorrelationId: {CorrelationId}",
            operationName, response.StatusCode, errorContent, correlationId ?? "N/A");

        throw new HttpRequestException(errorMessage + Environment.NewLine + errorContent, null, response.StatusCode);
    }

    #region Utility Methods

    /// <summary>
    /// Validate GUID format for user/group IDs
    /// </summary>
    protected static void ValidateGuidFormat(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("ID cannot be null or empty", parameterName);
        }

        if (!Guid.TryParse(id, out _))
        {
            throw new ArgumentException($"Invalid GUID format: {id}", parameterName);
        }
    }
    
    protected static void ValidateNonEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty", parameterName);
        }
    }

    /// <summary>
    /// Limit top parameter to prevent excessive results and API throttling
    /// </summary>
    protected static int LimitTopParameter(int top, int maxLimit = 100)
    {
        return Math.Min(Math.Max(top, 1), maxLimit);
    }

    /// <summary>
    /// Sanitize search term for Graph API OData queries
    /// </summary>
    protected static string SanitizeSearchTerm(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
        }

        // Remove potentially dangerous characters and limit length
        var sanitized = searchTerm
            .Replace("'", "''")  // Escape single quotes for OData
            .Replace("\"", "")   // Remove double quotes
            .Replace("\\", "")   // Remove backslashes
            .Replace("\r", "")   // Remove carriage returns
            .Replace("\n", "")   // Remove line feeds
            .Trim();

        // Limit length to prevent issues
        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }

    /// <summary>
    /// Build OData filter string safely
    /// </summary>
    protected static string BuildODataFilter(params string[] conditions)
    {
        if (conditions == null || conditions.Length == 0)
            return string.Empty;

        var validConditions = conditions.Where(c => !string.IsNullOrWhiteSpace(c));
        return string.Join(" and ", validConditions);
    }

    /// <summary>
    /// Build OData select string for optimal data transfer
    /// </summary>
    protected static string BuildODataSelect(params string[] fields)
    {
        if (fields == null || fields.Length == 0)
            return string.Empty;

        var validFields = fields.Where(f => !string.IsNullOrWhiteSpace(f));
        return string.Join(",", validFields);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // HttpClient should NOT be disposed here as it's injected and managed by DI container
            // The DI container will handle HttpClient lifecycle and connection pooling
            _disposed = true;
        }
    }

    #endregion
}
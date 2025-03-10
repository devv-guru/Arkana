using System.Diagnostics;
using Data.Entities.Metrics;
using Domain.Metrics;
using Microsoft.AspNetCore.Http.Extensions;
using Yarp.ReverseProxy.Model;

namespace Gateway.Metrics;

/// <summary>
/// Middleware for collecting request metrics
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(
        RequestDelegate next,
        IMetricsService metricsService,
        ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Continue down the pipeline
            await _next(context);

            stopwatch.Stop();

            // Collect metrics
            await CollectMetricsAsync(context, stopwatch.ElapsedMilliseconds, memoryStream.Length);

            // Copy the response to the original stream
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task CollectMetricsAsync(HttpContext context, long elapsedMilliseconds, long responseSize)
    {
        try
        {
            var requestSize = context.Request.ContentLength ?? 0;
            var routeId = string.Empty;
            var clusterId = string.Empty;
            var destinationId = string.Empty;

            // Try to get YARP routing information
            if (context.GetEndpoint()?.Metadata.GetMetadata<IReverseProxyFeature>() is { } proxyFeature)
            {
                routeId = proxyFeature.Route?.Config.RouteId ?? string.Empty;
                clusterId = proxyFeature.Route?.Config.ClusterId ?? string.Empty;
                
                if (proxyFeature.ProxiedDestination != null)
                {
                    destinationId = proxyFeature.ProxiedDestination.DestinationId ?? string.Empty;
                }
            }

            var metric = new RequestMetric
            {
                RouteId = routeId,
                ClusterId = clusterId,
                DestinationId = destinationId,
                Method = context.Request.Method,
                Path = context.Request.GetDisplayUrl(),
                Host = context.Request.Host.Value,
                StatusCode = context.Response.StatusCode,
                ElapsedMilliseconds = elapsedMilliseconds,
                Timestamp = DateTime.UtcNow,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent,
                RequestSize = requestSize,
                ResponseSize = responseSize
            };

            await _metricsService.RecordRequestMetricAsync(metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting request metrics");
        }
    }
}

/// <summary>
/// Extension methods for the metrics middleware
/// </summary>
public static class MetricsMiddlewareExtensions
{
    /// <summary>
    /// Adds the metrics middleware to the pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MetricsMiddleware>();
    }
}

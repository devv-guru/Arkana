using Data.Contexts.Base;
using Data.Entities.Metrics;
using Domain.Metrics;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Metrics;

/// <summary>
/// Implementation of the metrics service
/// </summary>
public class MetricsService : Domain.Metrics.IMetricsService
{
    private readonly IReadOnlyContext _readContext;
    private readonly MetricsQueue _metricsQueue;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        IReadOnlyContext readContext,
        MetricsQueue metricsQueue,
        ILogger<MetricsService> logger)
    {
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
        _metricsQueue = metricsQueue ?? throw new ArgumentNullException(nameof(metricsQueue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task RecordRequestMetricAsync(RequestMetric metric, CancellationToken ct = default)
    {
        try
        {
            // Enqueue the metric for async processing
            _metricsQueue.EnqueueRequestMetric(metric);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording request metric");
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task RecordSystemMetricAsync(SystemMetric metric, CancellationToken ct = default)
    {
        try
        {
            // Enqueue the metric for async processing
            _metricsQueue.EnqueueSystemMetric(metric);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording system metric");
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SystemMetric>> GetLatestSystemMetricsAsync(int count = 10, CancellationToken ct = default)
    {
        try
        {
            return await _readContext.SystemMetrics
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest system metrics");
            return Enumerable.Empty<SystemMetric>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RequestMetric>> GetLatestRequestMetricsAsync(int count = 10, CancellationToken ct = default)
    {
        try
        {
            return await _readContext.RequestMetrics
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest request metrics");
            return Enumerable.Empty<RequestMetric>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RequestMetric>> GetRequestMetricsForRouteAsync(string routeId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        try
        {
            return await _readContext.RequestMetrics
                .Where(m => m.RouteId == routeId && m.Timestamp >= startTime && m.Timestamp <= endTime)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request metrics for route {RouteId}", routeId);
            return Enumerable.Empty<RequestMetric>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RequestMetric>> GetRequestMetricsForClusterAsync(string clusterId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        try
        {
            return await _readContext.RequestMetrics
                .Where(m => m.ClusterId == clusterId && m.Timestamp >= startTime && m.Timestamp <= endTime)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request metrics for cluster {ClusterId}", clusterId);
            return Enumerable.Empty<RequestMetric>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RequestMetric>> GetRequestMetricsForDestinationAsync(string destinationId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        try
        {
            return await _readContext.RequestMetrics
                .Where(m => m.DestinationId == destinationId && m.Timestamp >= startTime && m.Timestamp <= endTime)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request metrics for destination {DestinationId}", destinationId);
            return Enumerable.Empty<RequestMetric>();
        }
    }
}

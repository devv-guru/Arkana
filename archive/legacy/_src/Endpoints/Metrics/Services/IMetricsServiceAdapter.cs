using Data.Entities.Metrics;
using Domain.Metrics;
using Microsoft.Extensions.Logging;

namespace Endpoints.Metrics.Services;

/// <summary>
/// Interface for metrics service adapter
/// </summary>
public interface IMetricsServiceAdapter
{
    /// <summary>
    /// Gets the latest system metrics
    /// </summary>
    /// <param name="count">Number of metrics to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of system metrics</returns>
    Task<IEnumerable<SystemMetric>> GetLatestSystemMetricsAsync(int count, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the latest request metrics
    /// </summary>
    /// <param name="count">Number of metrics to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of request metrics</returns>
    Task<IEnumerable<RequestMetric>> GetLatestRequestMetricsAsync(int count, CancellationToken ct = default);
    
    /// <summary>
    /// Gets request metrics for a specific route
    /// </summary>
    /// <param name="routeId">Route ID</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of request metrics</returns>
    Task<IEnumerable<RequestMetric>> GetRequestMetricsForRouteAsync(string routeId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    
    /// <summary>
    /// Gets request metrics for a specific cluster
    /// </summary>
    /// <param name="clusterId">Cluster ID</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of request metrics</returns>
    Task<IEnumerable<RequestMetric>> GetRequestMetricsForClusterAsync(string clusterId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    
    /// <summary>
    /// Gets request metrics for a specific destination
    /// </summary>
    /// <param name="destinationId">Destination ID</param>
    /// <param name="startTime">Start time</param>
    /// <param name="endTime">End time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of request metrics</returns>
    Task<IEnumerable<RequestMetric>> GetRequestMetricsForDestinationAsync(string destinationId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
}

/// <summary>
/// Implementation of metrics service adapter
/// </summary>
public class MetricsServiceAdapter : IMetricsServiceAdapter
{
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsServiceAdapter> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="metricsService">Metrics service</param>
    /// <param name="logger">Logger</param>
    public MetricsServiceAdapter(
        IMetricsService metricsService,
        ILogger<MetricsServiceAdapter> logger)
    {
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SystemMetric>> GetLatestSystemMetricsAsync(int count, CancellationToken ct = default)
    {
        try
        {
            return await _metricsService.GetLatestSystemMetricsAsync(count, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest system metrics");
            return Enumerable.Empty<SystemMetric>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RequestMetric>> GetLatestRequestMetricsAsync(int count, CancellationToken ct = default)
    {
        try
        {
            return await _metricsService.GetLatestRequestMetricsAsync(count, ct);
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
            return await _metricsService.GetRequestMetricsForRouteAsync(routeId, startTime, endTime, ct);
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
            return await _metricsService.GetRequestMetricsForClusterAsync(clusterId, startTime, endTime, ct);
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
            return await _metricsService.GetRequestMetricsForDestinationAsync(destinationId, startTime, endTime, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request metrics for destination {DestinationId}", destinationId);
            return Enumerable.Empty<RequestMetric>();
        }
    }
}

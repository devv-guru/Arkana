using Data.Entities.Metrics;

namespace Domain.Metrics;

/// <summary>
/// Interface for metrics service
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Records a request metric
    /// </summary>
    /// <param name="metric">The request metric to record</param>
    /// <param name="ct">Cancellation token</param>
    Task RecordRequestMetricAsync(RequestMetric metric, CancellationToken ct = default);

    /// <summary>
    /// Records a system metric
    /// </summary>
    /// <param name="metric">The system metric to record</param>
    /// <param name="ct">Cancellation token</param>
    Task RecordSystemMetricAsync(SystemMetric metric, CancellationToken ct = default);

    /// <summary>
    /// Gets the latest system metrics
    /// </summary>
    /// <param name="count">The number of metrics to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The latest system metrics</returns>
    Task<IEnumerable<SystemMetric>> GetLatestSystemMetricsAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets the latest request metrics
    /// </summary>
    /// <param name="count">The number of metrics to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The latest request metrics</returns>
    Task<IEnumerable<RequestMetric>> GetLatestRequestMetricsAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets the request metrics for a specific route
    /// </summary>
    /// <param name="routeId">The route ID</param>
    /// <param name="startTime">The start time</param>
    /// <param name="endTime">The end time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The request metrics for the route</returns>
    Task<IEnumerable<RequestMetric>> GetRequestMetricsForRouteAsync(string routeId, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    /// <summary>
    /// Gets the request metrics for a specific cluster
    /// </summary>
    /// <param name="clusterId">The cluster ID</param>
    /// <param name="startTime">The start time</param>
    /// <param name="endTime">The end time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The request metrics for the cluster</returns>
    Task<IEnumerable<RequestMetric>> GetRequestMetricsForClusterAsync(string clusterId, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    /// <summary>
    /// Gets the request metrics for a specific destination
    /// </summary>
    /// <param name="destinationId">The destination ID</param>
    /// <param name="startTime">The start time</param>
    /// <param name="endTime">The end time</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The request metrics for the destination</returns>
    Task<IEnumerable<RequestMetric>> GetRequestMetricsForDestinationAsync(string destinationId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
}

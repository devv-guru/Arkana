using Data.Contexts.Base;
using Data.Entities.Metrics;
using Domain.Metrics;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Services;

/// <summary>
/// Implementation of metrics service
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly IWriteOnlyContext _writeContext;
    private readonly IReadOnlyContext _readContext;

    public MetricsService(IWriteOnlyContext writeContext, IReadOnlyContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task RecordRequestMetricAsync(RequestMetric metric, CancellationToken ct = default)
    {
        _writeContext.RequestMetrics.Add(metric);
        await _writeContext.SaveChangesAsync(ct);
    }

    public async Task RecordSystemMetricAsync(SystemMetric metric, CancellationToken ct = default)
    {
        _writeContext.SystemMetrics.Add(metric);
        await _writeContext.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<SystemMetric>> GetLatestSystemMetricsAsync(int count = 10, CancellationToken ct = default)
    {
        return await _readContext.SystemMetrics
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RequestMetric>> GetLatestRequestMetricsAsync(int count = 10, CancellationToken ct = default)
    {
        return await _readContext.RequestMetrics
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RequestMetric>> GetRequestMetricsForRouteAsync(string routeId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        return await _readContext.RequestMetrics
            .Where(m => m.RouteId == routeId && m.CreatedAt >= startTime && m.CreatedAt <= endTime)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RequestMetric>> GetRequestMetricsForClusterAsync(string clusterId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        return await _readContext.RequestMetrics
            .Where(m => m.ClusterId == clusterId && m.CreatedAt >= startTime && m.CreatedAt <= endTime)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RequestMetric>> GetRequestMetricsForDestinationAsync(string destinationId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        return await _readContext.RequestMetrics
            .Where(m => m.DestinationId == destinationId && m.CreatedAt >= startTime && m.CreatedAt <= endTime)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }
}
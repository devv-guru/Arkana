using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;

internal sealed class HealthCheckMetricQueue : QueueBase<HealthCheckMetric>
{
    public HealthCheckMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<HealthCheckMetricQueue> logger)
        : base(metricsContext, logger)
    { }

    public override int BulkInsert(IEnumerable<HealthCheckMetric> metrics)
    {
        return _metricsContext.HealthCheckMetrics.InsertBulk(metrics);
    }
}
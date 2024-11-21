using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;

internal sealed class LatencyMetricQueue : QueueBase<LatencyMetric>
{
    public LatencyMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<LatencyMetricQueue> logger)
            : base(metricsContext, logger)
    { }

    public override int BulkInsert(IEnumerable<LatencyMetric> metrics)
    {
        return _metricsContext.LatencyMetrics.InsertBulk(metrics);
    }
}
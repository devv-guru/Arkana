using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;
internal sealed class TrafficMetricQueue : QueueBase<TrafficMetric>
{
    public TrafficMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<TrafficMetricQueue> logger)
        : base(metricsContext, logger)
    { }
    public override int BulkInsert(IEnumerable<TrafficMetric> metrics)
    {
        return _metricsContext.TrafficMetrics.InsertBulk(metrics);
    }
}

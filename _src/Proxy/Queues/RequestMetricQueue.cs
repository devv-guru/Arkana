using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;

internal sealed class RequestMetricQueue : QueueBase<RequestMetric>
{
    public RequestMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<RequestMetricQueue> logger)
        : base(metricsContext, logger)
    { }
    public override int BulkInsert(IEnumerable<RequestMetric> metrics)
    {
        return _metricsContext.RequestMetrics.InsertBulk(metrics);
    }
}
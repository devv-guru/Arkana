using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;

internal sealed class RateLimitMetricQueue : QueueBase<RateLimitMetric>
{
    public RateLimitMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<RateLimitMetricQueue> logger)
        : base(metricsContext, logger)
    { }

    public override int BulkInsert(IEnumerable<RateLimitMetric> metrics)
    {
        return _metricsContext.RateLimitMetrics.InsertBulk(metrics);
    }
}
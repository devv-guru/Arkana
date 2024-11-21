using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;
internal sealed class AuthMetricQueue : QueueBase<AuthMetric>
{
    public AuthMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<AuthMetricQueue> logger)
        : base(metricsContext, logger)
    {
    }

    public override int BulkInsert(IEnumerable<AuthMetric> metrics)
    {
        return _metricsContext.AuthMetrics.InsertBulk(metrics);
    }
}
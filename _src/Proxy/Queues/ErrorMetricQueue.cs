using Data.Contexts.Metrics;
using Data.Entities.Metric;
using Microsoft.Extensions.Logging;

namespace Proxy.Queues;

internal sealed class ErrorMetricQueue : QueueBase<ErrorMetric>
{
    public ErrorMetricQueue(IWriteOnlyMetricsContext metricsContext, ILogger<ErrorMetricQueue> logger)
        : base(metricsContext, logger)
    {
    }

    public override int BulkInsert(IEnumerable<ErrorMetric> metrics)
    {
        return _metricsContext.ErrorMetrics.InsertBulk(metrics);
    }
}
using Data.Common;
using Data.Entities.Metric;
using LiteDB;

namespace Data.Contexts.Metrics
{
    public sealed class ReadOnlyMetricsContext : MetricsContext, IReadOnlyMetricsContext
    {
        public ReadOnlyMetricsContext(DataContextOptions options)
           : base(options)
        {

        }

        public ILiteQueryable<RequestMetric> RequestMetricQuery => RequestMetrics.Query();
        public ILiteQueryable<ErrorMetric> ErrorMetricQuery => ErrorMetrics.Query();
        public ILiteQueryable<TrafficMetric> TrafficMetricQuery => TrafficMetrics.Query();
        public ILiteQueryable<RateLimitMetric> RateLimitMetricQuery => RateLimitMetrics.Query();
        public ILiteQueryable<HealthCheckMetric> HealthCheckMetricQuery => HealthCheckMetrics.Query();
        public ILiteQueryable<LatencyMetric> LatencyMetricQuery => LatencyMetrics.Query();
        public ILiteQueryable<AuthMetric> AuthMetricQuery => AuthMetrics.Query();
    }
}

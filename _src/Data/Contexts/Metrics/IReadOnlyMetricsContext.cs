using Data.Entities.Metric;
using LiteDB;

namespace Data.Contexts.Metrics
{
    public interface IReadOnlyMetricsContext
    {
        public ILiteQueryable<RequestMetric> RequestMetricQuery { get; }
        public ILiteQueryable<ErrorMetric> ErrorMetricQuery { get; }
        public ILiteQueryable<RateLimitMetric> RateLimitMetricQuery { get; }
        public ILiteQueryable<TrafficMetric> TrafficMetricQuery { get; }
        public ILiteQueryable<HealthCheckMetric> HealthCheckMetricQuery { get; }
        public ILiteQueryable<LatencyMetric> LatencyMetricQuery { get; }
        public ILiteQueryable<AuthMetric> AuthMetricQuery { get; }
    }
}

using Data.Entities.Metric;
using LiteDB;

namespace Data.Contexts.Metrics
{
    public interface IWriteOnlyMetricsContext
    {
        public ILiteCollection<RequestMetric> RequestMetrics { get; }
        public ILiteCollection<ErrorMetric> ErrorMetrics { get; }
        public ILiteCollection<TrafficMetric> TrafficMetrics { get; }
        public ILiteCollection<RateLimitMetric> RateLimitMetrics { get; }
        public ILiteCollection<HealthCheckMetric> HealthCheckMetrics { get; }
        public ILiteCollection<LatencyMetric> LatencyMetrics { get; }
        public ILiteCollection<AuthMetric> AuthMetrics { get; }
    }
}

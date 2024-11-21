using Data.Common;
using Data.Entities.Metric;
using Domain.Options;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Data.Contexts.Metrics
{
    public sealed class ReadOnlyMetricsContext : MetricsContext, IReadOnlyMetricsContext
    {
        public ReadOnlyMetricsContext(IOptions<EnvironmentOptions> environmentOptions, IOptions<DataContextOptions> contextOptions)
            : base(environmentOptions.Value, contextOptions.Value)
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

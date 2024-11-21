using Data.Contexts.Metrics;
using Proxy.Queues;

namespace Proxy.Ingestion
{
    internal class MetricIngestionService
    {
        private readonly AuthMetricQueue _authMetricQueue;
        private readonly HealthCheckMetricQueue _healthCheckMetricQueue;
        private readonly TrafficMetricQueue _trafficMetricQueue;
        private readonly RateLimitMetricQueue _rateLimitMetricQueue;
        private readonly LatencyMetricQueue _latencyMetricQueue;
        private readonly ErrorMetricQueue _errorMetricQueue;
        private readonly RequestMetricQueue _requestMetricQueue;

        public MetricIngestionService(IWriteOnlyMetricsContext metricsContext, HealthCheckMetricQueue healthCheckMetricQueue, TrafficMetricQueue trafficMetricQueue,
            RateLimitMetricQueue rateLimitMetricQueue, LatencyMetricQueue latencyMetricQueue, ErrorMetricQueue errorMetricQueue,
            RequestMetricQueue requestMetricQueue)
        {
            _healthCheckMetricQueue = healthCheckMetricQueue;
            _trafficMetricQueue = trafficMetricQueue;
            _rateLimitMetricQueue = rateLimitMetricQueue;
            _latencyMetricQueue = latencyMetricQueue;
            _errorMetricQueue = errorMetricQueue;
            _requestMetricQueue = requestMetricQueue;
        }

        public async Task Start(CancellationToken ct)
        {
            await Task.WhenAll(
                IngestMetrics(_authMetricQueue.ProcessBatch, ct),
                IngestMetrics(_errorMetricQueue.ProcessBatch, ct),
                IngestMetrics(_healthCheckMetricQueue.ProcessBatch, ct),
                IngestMetrics(_trafficMetricQueue.ProcessBatch, ct),
                IngestMetrics(_rateLimitMetricQueue.ProcessBatch, ct),
                IngestMetrics(_latencyMetricQueue.ProcessBatch, ct),
                IngestMetrics(_requestMetricQueue.ProcessBatch, ct)
            );
        }

        private static async Task IngestMetrics(Action<CancellationToken> ingestMethod, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                ingestMethod(ct);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }
}

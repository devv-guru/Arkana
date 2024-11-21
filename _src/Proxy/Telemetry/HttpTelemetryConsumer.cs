using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Proxy.Queues;
using System.Collections.Concurrent;
using Yarp.Telemetry.Consumption;

namespace Proxy.Telemetry
{
    internal sealed class HttpTelemetryConsumer : IHttpTelemetryConsumer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HttpTelemetryConsumer> _logger;
        private readonly ConcurrentDictionary<string, HttpTelemetryMetric> _metrics;

        private readonly RequestMetricQueue _requestMetricQueue;
        private readonly ErrorMetricQueue _errorMetricQueue;

        public HttpTelemetryConsumer(IHttpContextAccessor httpContextAccessor, ILogger<HttpTelemetryConsumer> logger,
            RequestMetricQueue requestMetricQueue, ErrorMetricQueue errorMetricQueue)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestMetricQueue = requestMetricQueue;
            _errorMetricQueue = errorMetricQueue;
            _metrics = new ConcurrentDictionary<string, HttpTelemetryMetric>();
        }

        private string GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.TraceIdentifier ?? Ulid.NewUlid().ToString();
        }

        public void OnRequestStart(DateTime timestamp, string scheme, string host, int port, string pathAndQuery, int versionMajor, int versionMinor, HttpVersionPolicy versionPolicy)
        {
            var correlationId = GetCorrelationId();
            var metric = new HttpTelemetryMetric
            {
                CorrelationId = correlationId,
                RequestStartTime = timestamp,
                Scheme = scheme,
                Host = host,
                Port = port,
                PathAndQuery = pathAndQuery
            };

            _metrics[correlationId] = metric;
        }

        public void OnRequestStop(DateTime timestamp, int statusCode)
        {
            var correlationId = GetCorrelationId();

            if (_metrics.TryGetValue(correlationId, out var metric))
            {
                metric.RequestStopTime = timestamp;
                metric.StatusCode = statusCode;

                // Remove from dictionary and push to queue
                if (metric.IsComplete)
                {
                    _metrics.TryRemove(correlationId, out _);
                    //_queue.Enqueue(metric);
                }
            }
            else
            {
                _logger.LogWarning("RequestStop received without matching RequestStart [CorrelationId: {CorrelationId}]", correlationId);
            }
        }

        public void OnRequestFailed(DateTime timestamp, string exceptionMessage)
        {
            var correlationId = GetCorrelationId();

            if (_metrics.TryGetValue(correlationId, out var metric))
            {
                metric.RequestStopTime = timestamp;
                metric.ErrorMessage = exceptionMessage;

                // Remove from dictionary and push to queue
                if (metric.IsComplete)
                {
                    _metrics.TryRemove(correlationId, out _);
                    //_queue.Enqueue(metric);
                }
            }
            else
            {
                _logger.LogWarning("RequestFailed received without matching RequestStart [CorrelationId: {CorrelationId}]", correlationId);
            }
        }
    }
}

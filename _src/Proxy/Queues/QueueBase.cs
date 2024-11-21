using Data.Contexts.Metrics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Proxy.Queues
{
    internal abstract class QueueBase<T>
    {
        protected readonly IWriteOnlyMetricsContext _metricsContext;
        private readonly ConcurrentQueue<T> _queue;
        private readonly ILogger<QueueBase<T>> _logger;

        private const int BatchSize = 1000;

        public QueueBase(IWriteOnlyMetricsContext metricsContext, ILogger<QueueBase<T>> logger)
        {
            _metricsContext = metricsContext ?? throw new ArgumentNullException(nameof(metricsContext));
            _queue = new ConcurrentQueue<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Enqueue(T metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException(nameof(metric), "Cannot enqueue a null metric.");
            }
            _queue.Enqueue(metric);
        }

        public bool TryDequeue(out T metric)
        {
            return _queue.TryDequeue(out metric);
        }

        public void ProcessBatch(CancellationToken ct)
        {
            var metrics = new List<T>(BatchSize);
            int processedCount = 0;

            try
            {
                while (_queue.TryDequeue(out var metric)
                    && processedCount < BatchSize
                    && !ct.IsCancellationRequested)
                {
                    metrics.Add(metric);
                    processedCount++;
                }

                if (metrics.Count > 0)
                {
                    _logger.LogInformation("Processing batch of {Count} metrics.", metrics.Count);
                    BulkInsert(metrics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing metrics batch.");
            }
            finally
            {
                metrics.Clear();
            }
        }

        public abstract int BulkInsert(IEnumerable<T> metrics);
    }
}

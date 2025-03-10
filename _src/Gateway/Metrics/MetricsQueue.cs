using System.Collections.Concurrent;
using System.Threading.Channels;
using Data.Contexts.Base;
using Data.Entities.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gateway.Metrics;

/// <summary>
/// Provides a queue for metrics to be processed asynchronously.
/// This helps avoid performance impact on the main request processing pipeline.
/// </summary>
public class MetricsQueue : BackgroundService
{
    private readonly Channel<object> _metricsChannel;
    private readonly IWriteOnlyContext _writeContext;
    private readonly ILogger<MetricsQueue> _logger;

    public MetricsQueue(
        IWriteOnlyContext writeContext,
        ILogger<MetricsQueue> logger)
    {
        _writeContext = writeContext ?? throw new ArgumentNullException(nameof(writeContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Create an unbounded channel for metrics
        _metricsChannel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    /// <summary>
    /// Enqueues a request metric to be processed asynchronously.
    /// </summary>
    /// <param name="metric">The request metric to enqueue.</param>
    public void EnqueueRequestMetric(RequestMetric metric)
    {
        if (!_metricsChannel.Writer.TryWrite(metric))
        {
            _logger.LogWarning("Failed to enqueue request metric");
        }
    }

    /// <summary>
    /// Enqueues a system metric to be processed asynchronously.
    /// </summary>
    /// <param name="metric">The system metric to enqueue.</param>
    public void EnqueueSystemMetric(SystemMetric metric)
    {
        if (!_metricsChannel.Writer.TryWrite(metric))
        {
            _logger.LogWarning("Failed to enqueue system metric");
        }
    }

    /// <summary>
    /// Processes metrics from the queue in the background.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics queue background service started");

        // Process metrics in batches for better performance
        var batch = new List<object>(100);
        var requestMetrics = new List<RequestMetric>();
        var systemMetrics = new List<SystemMetric>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Clear the batch
                batch.Clear();
                requestMetrics.Clear();
                systemMetrics.Clear();

                // Try to read up to 100 metrics with a timeout
                var readCount = 0;
                var readTask = Task.Run(async () =>
                {
                    while (readCount < 100 && await _metricsChannel.Reader.WaitToReadAsync(stoppingToken))
                    {
                        if (_metricsChannel.Reader.TryRead(out var metric))
                        {
                            batch.Add(metric);
                            readCount++;
                        }
                    }
                }, stoppingToken);

                // Wait for the read task to complete or timeout after 5 seconds
                await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(5), stoppingToken));

                // If we have metrics to process
                if (batch.Count > 0)
                {
                    // Separate metrics by type
                    foreach (var metric in batch)
                    {
                        if (metric is RequestMetric requestMetric)
                        {
                            requestMetrics.Add(requestMetric);
                        }
                        else if (metric is SystemMetric systemMetric)
                        {
                            systemMetrics.Add(systemMetric);
                        }
                    }

                    // Save metrics to the database
                    if (requestMetrics.Count > 0)
                    {
                        foreach (var metric in requestMetrics)
                        {
                            _writeContext.RequestMetrics.Add(metric);
                        }
                    }

                    if (systemMetrics.Count > 0)
                    {
                        foreach (var metric in systemMetrics)
                        {
                            _writeContext.SystemMetrics.Add(metric);
                        }
                    }

                    await _writeContext.SaveChangesAsync(stoppingToken);
                    
                    _logger.LogDebug("Processed {RequestCount} request metrics and {SystemCount} system metrics", 
                        requestMetrics.Count, systemMetrics.Count);
                }
                else
                {
                    // If no metrics were processed, wait a bit to avoid spinning
                    await Task.Delay(100, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing metrics batch");
                
                // Wait a bit before retrying to avoid spinning on errors
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Metrics queue background service stopped");
    }
}

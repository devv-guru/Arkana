using System.Diagnostics;
using Data.Entities.Metrics;
using Domain.Metrics;

namespace Gateway.Metrics;

/// <summary>
/// Background service for collecting system metrics
/// </summary>
public class SystemMetricsCollector : BackgroundService
{
    private readonly IMetricsService _metricsService;
    private readonly ILogger<SystemMetricsCollector> _logger;
    private readonly TimeSpan _interval;
    private readonly Process _currentProcess;
    private readonly string _hostName;
    
    // Performance counters
    private long _lastTotalProcessorTime;
    private DateTime _lastCpuCheckTime;
    private long _lastNetworkInBytes;
    private long _lastNetworkOutBytes;
    private long _lastDiskReadBytes;
    private long _lastDiskWriteBytes;
    private DateTime _lastIoCheckTime;

    public SystemMetricsCollector(
        IMetricsService metricsService,
        ILogger<SystemMetricsCollector> logger,
        IConfiguration configuration)
    {
        _metricsService = metricsService;
        _logger = logger;
        
        // Get the interval from configuration or use default
        var intervalSeconds = configuration.GetValue<int>("Metrics:SystemMetricsInterval", 60);
        _interval = TimeSpan.FromSeconds(intervalSeconds);
        
        _currentProcess = Process.GetCurrentProcess();
        _hostName = Environment.MachineName;
        
        // Initialize performance counters
        _lastTotalProcessorTime = _currentProcess.TotalProcessorTime.Ticks;
        _lastCpuCheckTime = DateTime.UtcNow;
        _lastNetworkInBytes = 0;
        _lastNetworkOutBytes = 0;
        _lastDiskReadBytes = _currentProcess.PagedMemorySize64;
        _lastDiskWriteBytes = _currentProcess.PagedSystemMemorySize64;
        _lastIoCheckTime = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("System metrics collector started with interval {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var metric = CollectSystemMetrics();
                await _metricsService.RecordSystemMetricAsync(metric, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting system metrics");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private SystemMetric CollectSystemMetrics()
    {
        // Refresh process info
        _currentProcess.Refresh();
        
        // Calculate CPU usage
        var currentTotalProcessorTime = _currentProcess.TotalProcessorTime.Ticks;
        var currentCpuCheckTime = DateTime.UtcNow;
        var cpuUsedMs = (currentTotalProcessorTime - _lastTotalProcessorTime) / TimeSpan.TicksPerMillisecond;
        var totalElapsedMs = (currentCpuCheckTime - _lastCpuCheckTime).TotalMilliseconds;
        var cpuUsagePercent = (cpuUsedMs / (Environment.ProcessorCount * totalElapsedMs)) * 100;
        
        // Update CPU counters
        _lastTotalProcessorTime = currentTotalProcessorTime;
        _lastCpuCheckTime = currentCpuCheckTime;
        
        // Calculate memory usage
        var memoryUsageMB = _currentProcess.WorkingSet64 / (1024 * 1024);
        var totalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
        
        // Calculate IO rates
        var currentNetworkInBytes = 0L; // Would need a network counter
        var currentNetworkOutBytes = 0L; // Would need a network counter
        var currentDiskReadBytes = _currentProcess.PagedMemorySize64;
        var currentDiskWriteBytes = _currentProcess.PagedSystemMemorySize64;
        var currentIoCheckTime = DateTime.UtcNow;
        
        var elapsedSeconds = (currentIoCheckTime - _lastIoCheckTime).TotalSeconds;
        var networkInKbps = (currentNetworkInBytes - _lastNetworkInBytes) / (1024 * elapsedSeconds);
        var networkOutKbps = (currentNetworkOutBytes - _lastNetworkOutBytes) / (1024 * elapsedSeconds);
        var diskReadKbps = (currentDiskReadBytes - _lastDiskReadBytes) / (1024 * elapsedSeconds);
        var diskWriteKbps = (currentDiskWriteBytes - _lastDiskWriteBytes) / (1024 * elapsedSeconds);
        
        // Update IO counters
        _lastNetworkInBytes = currentNetworkInBytes;
        _lastNetworkOutBytes = currentNetworkOutBytes;
        _lastDiskReadBytes = currentDiskReadBytes;
        _lastDiskWriteBytes = currentDiskWriteBytes;
        _lastIoCheckTime = currentIoCheckTime;
        
        return new SystemMetric
        {
            CpuUsagePercent = cpuUsagePercent,
            MemoryUsageMB = memoryUsageMB,
            TotalMemoryMB = totalMemoryMB,
            ThreadCount = _currentProcess.Threads.Count,
            HandleCount = _currentProcess.HandleCount,
            Timestamp = DateTime.UtcNow,
            NetworkInboundKbps = networkInKbps,
            NetworkOutboundKbps = networkOutKbps,
            DiskReadKbps = diskReadKbps,
            DiskWriteKbps = diskWriteKbps,
            ActiveConnections = 0, // Would need a connection counter
            HostName = _hostName
        };
    }
}

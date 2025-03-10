using System;

namespace Shared.Models.Metrics;

/// <summary>
/// Data transfer object for system metrics
/// </summary>
public class SystemMetricDto
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the CPU usage as a percentage (0-1)
    /// </summary>
    public double CpuUsage { get; set; }
    
    /// <summary>
    /// Gets or sets the memory usage in MB
    /// </summary>
    public double MemoryUsage { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of requests processed
    /// </summary>
    public long TotalRequests { get; set; }
    
    /// <summary>
    /// Gets or sets the number of active connections
    /// </summary>
    public int ActiveConnections { get; set; }
    
    /// <summary>
    /// Gets or sets the requests per second
    /// </summary>
    public double RequestsPerSecond { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the metric was recorded
    /// </summary>
    public DateTime Timestamp { get; set; }
}

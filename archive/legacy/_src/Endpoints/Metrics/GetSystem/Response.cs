using Data.Entities.Metrics;

namespace Endpoints.Metrics.GetSystem;

public class Response
{
    public List<SystemMetricDto> Metrics { get; set; } = new();
}

public class SystemMetricDto
{
    public string Id { get; set; } = string.Empty;
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public double TotalMemoryMB { get; set; }
    public double MemoryUsagePercent { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public DateTime Timestamp { get; set; }
    public double NetworkInboundKbps { get; set; }
    public double NetworkOutboundKbps { get; set; }
    public double DiskReadKbps { get; set; }
    public double DiskWriteKbps { get; set; }
    public int ActiveConnections { get; set; }
    public string HostName { get; set; } = string.Empty;
}

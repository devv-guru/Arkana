using Data.Entities.Metrics;

namespace Endpoints.Metrics.GetSystem;

public static class Mapper
{
    public static SystemMetricDto ToDto(this SystemMetric metric)
    {
        return new SystemMetricDto
        {
            Id = metric.Id.ToString(),
            CpuUsagePercent = metric.CpuUsagePercent,
            MemoryUsageMB = metric.MemoryUsageMB,
            TotalMemoryMB = metric.TotalMemoryMB,
            MemoryUsagePercent = metric.MemoryUsagePercent,
            ThreadCount = metric.ThreadCount,
            HandleCount = metric.HandleCount,
            Timestamp = metric.Timestamp,
            NetworkInboundKbps = metric.NetworkInboundKbps,
            NetworkOutboundKbps = metric.NetworkOutboundKbps,
            DiskReadKbps = metric.DiskReadKbps,
            DiskWriteKbps = metric.DiskWriteKbps,
            ActiveConnections = metric.ActiveConnections,
            HostName = metric.HostName ?? string.Empty
        };
    }
}

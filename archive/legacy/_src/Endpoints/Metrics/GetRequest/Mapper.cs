using Data.Entities.Metrics;

namespace Endpoints.Metrics.GetRequest;

public static class Mapper
{
    public static RequestMetricDto ToDto(this RequestMetric metric)
    {
        return new RequestMetricDto
        {
            Id = metric.Id.ToString(),
            RouteId = metric.RouteId,
            ClusterId = metric.ClusterId,
            DestinationId = metric.DestinationId,
            Method = metric.Method,
            Path = metric.Path,
            Host = metric.Host,
            StatusCode = metric.StatusCode,
            ElapsedMilliseconds = metric.ElapsedMilliseconds,
            Timestamp = metric.Timestamp,
            ClientIp = metric.ClientIp,
            UserAgent = metric.UserAgent,
            RequestSize = metric.RequestSize,
            ResponseSize = metric.ResponseSize,
            IsSuccess = metric.IsSuccess,
            IsError = metric.IsError,
            IsServerError = metric.IsServerError
        };
    }
}

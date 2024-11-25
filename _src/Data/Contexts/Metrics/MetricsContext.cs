using Data.Common;
using Data.Entities.Metric;
using LiteDB;

namespace Data.Contexts.Metrics;

public class MetricsContext : IDisposable
{
    private readonly LiteDatabase _database;

    public MetricsContext(DataContextOptions options, bool readOnly = true)
    {
        var connection = new ConnectionString
        {
            Connection = ConnectionType.Shared,
            Filename = options.ConfigurationConnectionString,
            Password = options.DatabasePassword,
            ReadOnly = readOnly,
        };

        _database = new LiteDatabase(connection);
    }

    public ILiteCollection<T> GetCollection<T>(string collectionName) => _database.GetCollection<T>(collectionName);

    public ILiteCollection<RequestMetric> RequestMetrics => GetCollection<RequestMetric>(nameof(RequestMetric).ToLower());
    public ILiteCollection<ErrorMetric> ErrorMetrics => GetCollection<ErrorMetric>(nameof(ErrorMetric).ToLower());
    public ILiteCollection<TrafficMetric> TrafficMetrics => GetCollection<TrafficMetric>(nameof(TrafficMetric).ToLower());
    public ILiteCollection<RateLimitMetric> RateLimitMetrics => GetCollection<RateLimitMetric>(nameof(RateLimitMetric).ToLower());
    public ILiteCollection<HealthCheckMetric> HealthCheckMetrics => GetCollection<HealthCheckMetric>(nameof(HealthCheckMetric).ToLower());
    public ILiteCollection<LatencyMetric> LatencyMetrics => GetCollection<LatencyMetric>(nameof(LatencyMetric).ToLower());
    public ILiteCollection<AuthMetric> AuthMetrics => GetCollection<AuthMetric>(nameof(AuthMetric).ToLower());

    public void Dispose()
    {
        _database?.Dispose();
    }
}

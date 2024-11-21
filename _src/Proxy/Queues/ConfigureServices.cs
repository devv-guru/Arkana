using Microsoft.Extensions.DependencyInjection;

namespace Proxy.Queues
{
    internal static class ConfigureServices
    {
        internal static IServiceCollection AddQueues(this IServiceCollection services)
        {
            services.AddSingleton<HealthCheckMetricQueue>();
            services.AddSingleton<AuthMetricQueue>();
            services.AddSingleton<TrafficMetricQueue>();
            services.AddSingleton<RateLimitMetricQueue>();
            services.AddSingleton<LatencyMetricQueue>();
            services.AddSingleton<ErrorMetricQueue>();
            services.AddSingleton<RequestMetricQueue>();

            return services;
        }
    }
}

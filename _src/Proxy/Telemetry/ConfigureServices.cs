using Microsoft.Extensions.DependencyInjection;

namespace Proxy.Telemetry
{
    internal static class ConfigureServices
    {
        internal static IServiceCollection AddTelemetryConsumers(this IServiceCollection services)
        {
            services.AddTelemetryConsumer<HttpTelemetryConsumer>();

            return services;
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Proxy.Ingestion;
using Proxy.Queues;
using Yarp.ReverseProxy.Configuration;

namespace Proxy
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddProxy(this IServiceCollection services)
        {
            services.AddSingleton(new InMemoryConfigProvider(GetDefaultRoute(), GetDefaultCluster()));
            services.AddSingleton<IProxyConfigProvider>(s => s.GetRequiredService<InMemoryConfigProvider>());
            services.AddReverseProxy();
            services.AddQueues();
            services.AddSingleton<MetricIngestionService>();
            services.AddHostedService<MetricIngestionWorker>();

            return services;
        }

        public static IApplicationBuilder UseProxy(this WebApplication app)
        {
            app.MapReverseProxy(pipeline =>
            {
                pipeline.UseSessionAffinity();
                pipeline.UseLoadBalancing();
            });

            return app;
        }

        private static RouteConfig[] GetDefaultRoute()
        {
            return new[]
            {
            new RouteConfig()
            {
                RouteId = "defaultRoute",
                ClusterId = "defaultCluster",
                Match = new RouteMatch
                {
                    Path = "api/{**catch-all}",
                    Methods = new[] { "POST", "GET", "PUT", "DELETE" }
                }
            }
        };
        }

        private static ClusterConfig[] GetDefaultCluster()
        {
            return new[]
            {
            new ClusterConfig()
            {
                ClusterId = "defaultCluster",
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    { "default", new DestinationConfig() { Address = "https://localhost" } }
                }
            }
            };
        }
    }
}

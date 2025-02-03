using Data.Entities;
using FastEndpoints;
using Gateway.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace Gateway.Proxy;

public static class ConfigureServices
{
    public static IServiceCollection AddProxy(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationOptions = configuration.GetSection(ConfigurationOptions.SectionName).Get<ConfigurationOptions>();

        services.AddReverseProxy().LoadFromMemory(BuildRoutes(configurationOptions), BuildClusters(configurationOptions));
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

    private static RouteConfig[] BuildRoutes(ConfigurationOptions configurationOptions)
    {
        var routes = new List<RouteConfig>();

        if (configurationOptions?.Services != null)
        {
            foreach (var service in configurationOptions.Services)
            {
                // Build one route per service
                // You can customize the route matching logic (Hosts, Path, etc.) as you see fit
                routes.Add(
                    new RouteConfig
                    {
                        RouteId = service.Name,                 // or "Route_" + service.Name
                        ClusterId = $"{service.Name}Cluster",   // tie route to the same cluster
                        Match = new RouteMatch
                        {
                            // If you want to match the service's target host, include it here
                            // If TargetHost is null or empty, default to "localhost" or skip it
                            Hosts = new[] { service.TargetHost ?? "localhost" },

                            // Example path match - forward everything:
                            Path = "/{**catchAll}",

                            // Example methods - add or remove as needed
                            Methods = new[] { "GET", "POST", "PUT", "DELETE" }
                        },
                        Transforms = service.Transforms
                    }
                );
            }
        }

        return routes.ToArray();
    }

    private static ClusterConfig[] BuildClusters(ConfigurationOptions configurationOptions)
    {
        var clusters = new List<ClusterConfig>();

        if (configurationOptions?.Services != null)
        {
            foreach (var service in configurationOptions.Services)
            {
                // Create one cluster per service
                var cluster = new ClusterConfig
                {
                    ClusterId = $"{service.Name}Cluster",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                };

                Dictionary<string, DestinationConfig> destinations = new(StringComparer.OrdinalIgnoreCase);
                // Populate the destinations
                if (service.Destinations != null)
                {


                    foreach (var destination in service.Destinations)
                    {
                        // If 'Name' is missing, use a default name
                        var destinationKey = string.IsNullOrEmpty(destination.Name)
                            ? "default"
                            : destination.Name;

                        if (!string.IsNullOrEmpty(destination.Address))
                        {
                            destinations[destinationKey] = new DestinationConfig
                            {
                                Address = destination.Address
                            };
                        }
                    }

                    cluster = new ClusterConfig
                    {
                        ClusterId = $"{service.Name}Cluster",
                        Destinations = destinations
                    };
                }

                HealthCheckConfig? healthCheckConfig = null;
                // Map health check settings
                if (service.HealthCheck != null)
                {
                    bool enabled = bool.TryParse(service.HealthCheck.Enabled, out var isEnabled) && isEnabled;

                    var config = new HealthCheckConfig
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = enabled,
                            Interval = TimeSpan.Parse(service.HealthCheck.Interval),
                            Timeout = TimeSpan.Parse(service.HealthCheck.Timeout),
                            Path = service.HealthCheck.Path,
                            Query = service.HealthCheck.Query,
                            Policy = "ConsecutiveFailuresHealthPolicy",
                        }
                    };
                }

                ForwarderRequestConfig? requestConfig = null;

                if (service.HttpRequest != null)
                {

                    var versionPolicy = Enum.TryParse<HttpVersionPolicy>(service.HttpRequest.VersionPolicy, true, out var policy)
                        ? policy
                        : HttpVersionPolicy.RequestVersionOrLower;

                    requestConfig = new ForwarderRequestConfig
                    {
                        Version = new Version(service.HttpRequest.Version),
                        VersionPolicy = versionPolicy,
                        AllowResponseBuffering = service.HttpRequest.AllowResponseBuffering,
                        ActivityTimeout = TimeSpan.Parse(service.HttpRequest.ActivityTimeout)
                    };
                }

                cluster = new ClusterConfig
                {
                    ClusterId = $"{service.Name}Cluster",
                    Destinations = cluster.Destinations,
                    HealthCheck = healthCheckConfig,
                    HttpRequest = requestConfig,
                    Metadata = new Dictionary<string, string>
                    {
                        { "TransportFailureRateHealthPolicy.RateLimit", service.HealthCheck.Threshold ?? "5" }
                    }
                };

                clusters.Add(cluster);
            }
        }

        return clusters.ToArray();
    }
}
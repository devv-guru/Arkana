using Gateway.Models;
using Data.Entities;
using Data.Enums;
using Shared.Services;

namespace Gateway.Services;

public class ConfigurationTranslationService : IConfigurationTranslationService
{
    private readonly ILogger<ConfigurationTranslationService> _logger;
    private readonly IGuidProvider _guidProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConfigurationTranslationService(
        ILogger<ConfigurationTranslationService> logger,
        IGuidProvider guidProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _guidProvider = guidProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TranslationResult> TranslateConfigurationAsync(GatewayConfigurationModel configuration)
    {
        try
        {
            _logger.LogInformation("Translating configuration with {HostCount} hosts and {ProxyRuleCount} proxy rules",
                configuration.Hosts.Count, configuration.ProxyRules.Count);

            var result = new TranslationResult();

            // Step 1: Translate hosts
            var webHosts = new List<WebHost>();
            var hostLookup = new Dictionary<string, WebHost>(StringComparer.OrdinalIgnoreCase);

            foreach (var hostConfig in configuration.Hosts)
            {
                var webHost = TranslateHost(hostConfig);
                webHosts.Add(webHost);

                // Build lookup for proxy rule translation
                foreach (var hostname in hostConfig.HostNames)
                {
                    if (!hostLookup.ContainsKey(hostname))
                    {
                        hostLookup[hostname] = webHost;
                    }
                    else
                    {
                        result.AddWarning($"Duplicate hostname '{hostname}' found, using first occurrence");
                    }
                }
            }

            // Step 2: Translate proxy rules to routes and clusters
            var routes = new List<Route>();
            var clusters = new List<Cluster>();

            foreach (var proxyRule in configuration.ProxyRules)
            {
                var (route, cluster) = TranslateProxyRule(proxyRule, hostLookup);
                routes.Add(route);
                clusters.Add(cluster);
            }

            result.WebHosts = webHosts;
            result.Routes = routes;
            result.Clusters = clusters;
            result.IsSuccess = true;

            _logger.LogInformation("Translation completed successfully: {WebHostCount} hosts, {RouteCount} routes, {ClusterCount} clusters",
                webHosts.Count, routes.Count, clusters.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration translation");
            return TranslationResult.Failure($"Translation failed: {ex.Message}");
        }
    }

    public (Route route, Cluster cluster) TranslateProxyRule(ProxyRuleConfigurationModel proxyRule, Dictionary<string, WebHost> hostLookup)
    {
        // Create cluster first
        var clusterId = _guidProvider.NewGuid();
        var cluster = new Cluster
        {
            Id = clusterId,
            Name = proxyRule.Cluster.Name,
            LoadBalancingPolicy = proxyRule.Cluster.LoadBalancingPolicy,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        // Set cluster host (use first matching host or create default)
        var firstHost = proxyRule.Hosts.FirstOrDefault();
        if (firstHost != null && hostLookup.TryGetValue(firstHost, out var webHost))
        {
            cluster.HostId = webHost.Id;
            cluster.Host = webHost;
        }
        else
        {
            // Create a default host if none exists
            var defaultHost = new WebHost
            {
                Id = _guidProvider.NewGuid(),
                Name = $"default-{proxyRule.Name}",
                HostName = firstHost ?? "localhost",
                IsDefault = true,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
                IsDeleted = false
            };
            cluster.HostId = defaultHost.Id;
            cluster.Host = defaultHost;
        }

        // Add destinations
        cluster.Destinations = proxyRule.Cluster.Destinations.Select(dest => new Destination
        {
            Id = _guidProvider.NewGuid(),
            Name = dest.Name,
            Address = dest.Address,
            Health = dest.Health,
            ClusterConfigId = clusterId,
            ClusterConfig = cluster,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        }).ToList();

        // Add health check if specified
        if (proxyRule.Cluster.HealthCheck != null)
        {
            cluster.HealthCheck = TranslateHealthCheck(proxyRule.Cluster.HealthCheck, clusterId);
        }

        // Add HTTP client settings if specified
        if (proxyRule.Cluster.HttpRequest != null)
        {
            cluster.HttpRequest = TranslateHttpRequestSettings(proxyRule.Cluster.HttpRequest, clusterId);
        }

        // Add metadata
        if (proxyRule.Cluster.Destinations.Any(d => d.Metadata?.Any() == true))
        {
            var metadata = new Dictionary<string, string>();
            foreach (var dest in proxyRule.Cluster.Destinations.Where(d => d.Metadata?.Any() == true))
            {
                foreach (var kvp in dest.Metadata!)
                {
                    metadata[$"destination.{dest.Name}.{kvp.Key}"] = kvp.Value;
                }
            }

            cluster.Metadata = new Metadata
            {
                Id = _guidProvider.NewGuid(),
                Data = metadata,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
                IsDeleted = false
            };
        }

        // Create route
        var routeId = _guidProvider.NewGuid();
        var route = new Route
        {
            Id = routeId,
            ClusterId = clusterId,
            // Note: Route.Cluster navigation property doesn't exist in entity
            Order = 0, // Could be calculated based on path specificity
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        // Set route host (use first matching host)
        if (firstHost != null && hostLookup.TryGetValue(firstHost, out var routeWebHost))
        {
            route.HostId = routeWebHost.Id;
            route.Host = routeWebHost;
        }

        // Create route match
        route.Match = new Match
        {
            Id = _guidProvider.NewGuid(),
            Path = proxyRule.PathPrefix,
            Hosts = proxyRule.Hosts,
            Methods = proxyRule.Methods.Any() ? proxyRule.Methods : null,
            RouteConfigId = routeId,
            Route = route,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        // Add transforms
        if (proxyRule.Cluster.Transforms.Any())
        {
            route.Transforms = proxyRule.Cluster.Transforms.Select(transform => new Transform
            {
                Id = _guidProvider.NewGuid(),
                RequestHeader = transform.RequestHeader,
                Set = transform.Set,
                RouteConfigId = routeId,
                RouteConfig = route,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
                IsDeleted = false
            }).ToList();
        }

        // Handle path stripping transform
        if (proxyRule.StripPrefix && !string.IsNullOrEmpty(proxyRule.PathPrefix))
        {
            var stripTransform = new Transform
            {
                Id = _guidProvider.NewGuid(),
                RequestHeader = "PathSet",
                Set = proxyRule.PathPrefix,
                RouteConfigId = routeId,
                RouteConfig = route,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
                IsDeleted = false
            };

            route.Transforms ??= new List<Transform>();
            route.Transforms.Add(stripTransform);
        }

        return (route, cluster);
    }

    public WebHost TranslateHost(HostConfigurationModel hostConfig)
    {
        var webHost = new WebHost
        {
            Id = _guidProvider.NewGuid(),
            Name = hostConfig.Name,
            HostName = hostConfig.HostNames.FirstOrDefault() ?? hostConfig.Name,
            IsDefault = hostConfig.HostNames.Contains("*") || hostConfig.HostNames.Contains("localhost"),
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        // Certificate handling removed - Azure handles SSL termination
        // if (hostConfig.Certificate != null) - No longer needed

        return webHost;
    }

    // TranslateCertificate method removed - Azure handles SSL termination

    private HealthCheck TranslateHealthCheck(HealthCheckConfigurationModel healthConfig, Guid clusterId)
    {
        var healthCheck = new HealthCheck
        {
            Id = _guidProvider.NewGuid(),
            ClusterConfigId = clusterId,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        // Create active health check
        healthCheck.Active = new ActiveHealthCheck
        {
            Id = _guidProvider.NewGuid(),
            Enabled = healthConfig.Enabled,
            Interval = healthConfig.Interval,
            Timeout = healthConfig.Timeout,
            Policy = "ConsecutiveFailures", // Default policy
            Path = healthConfig.Path,
            HealthCheckConfigId = healthCheck.Id,
            HealthCheckConfig = healthCheck,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        // Create passive health check
        healthCheck.Passive = new PassiveHealthCheck
        {
            Id = _guidProvider.NewGuid(),
            Enabled = true, // Enable by default
            Policy = "TransportFailureRate",
            ReactivationPeriod = TimeSpan.FromMinutes(1),
            HealthCheckConfigId = healthCheck.Id,
            HealthCheckConfig = healthCheck,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };

        return healthCheck;
    }

    private HttpRequestSettings TranslateHttpRequestSettings(HttpRequestConfigurationModel httpConfig, Guid clusterId)
    {
        return new HttpRequestSettings
        {
            Id = _guidProvider.NewGuid(),
            Version = httpConfig.Version,
            VersionPolicy = httpConfig.VersionPolicy,
            AllowResponseBuffering = httpConfig.AllowResponseBuffering,
            ActivityTimeout = httpConfig.ActivityTimeout,
            ClusterConfigId = clusterId,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow,
            IsDeleted = false
        };
    }
}
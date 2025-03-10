using Gateway.Configuration;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using YarpRouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;
using YarpRouteMatch = Yarp.ReverseProxy.Configuration.RouteMatch;
using YarpClusterConfig = Yarp.ReverseProxy.Configuration.ClusterConfig;
using YarpDestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;
using YarpHealthCheckConfig = Yarp.ReverseProxy.Configuration.HealthCheckConfig;
using YarpActiveHealthCheckConfig = Yarp.ReverseProxy.Configuration.ActiveHealthCheckConfig;

namespace Gateway.Proxy;

/// <summary>
/// Provides YARP proxy configuration based on the gateway configuration.
/// </summary>
public class YarpConfigurationService : IProxyConfigProvider, IDisposable
{
    private readonly ILogger<YarpConfigurationService> _logger;
    private readonly GatewayConfigurationService _gatewayConfigurationService;
    private InMemoryConfig? _config;
    private CancellationTokenSource _changeToken = new CancellationTokenSource();
    private bool _disposed;

    public YarpConfigurationService(
        ILogger<YarpConfigurationService> logger,
        GatewayConfigurationService gatewayConfigurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gatewayConfigurationService = gatewayConfigurationService ?? throw new ArgumentNullException(nameof(gatewayConfigurationService));
        
        // Subscribe to configuration changes
        _gatewayConfigurationService.ConfigurationChanged += OnGatewayConfigurationChanged;
    }

    private void OnGatewayConfigurationChanged(object sender, GatewayConfigurationOptions configuration)
    {
        _logger.LogInformation("Gateway configuration changed, updating YARP configuration");
        UpdateConfig();
    }

    /// <summary>
    /// Gets the current proxy configuration.
    /// </summary>
    /// <returns>The current proxy configuration or an empty configuration if none is available.</returns>
    public IProxyConfig GetConfig() => _config ?? new InMemoryConfig(
        Array.Empty<RouteConfig>(),
        Array.Empty<Yarp.ReverseProxy.Configuration.ClusterConfig>(),
        new CancellationChangeToken(_changeToken.Token));

    /// <summary>
    /// Updates the proxy configuration based on the current gateway configuration.
    /// </summary>
    public void UpdateConfig()
    {
        var gatewayConfig = _gatewayConfigurationService.GetConfiguration();
        if (gatewayConfig == null || gatewayConfig.ProxyRules.Count == 0)
        {
            _logger.LogWarning("No proxy rules configured, skipping YARP configuration update");
            return;
        }

        try
        {
            _logger.LogInformation("Updating YARP configuration with {RuleCount} proxy rules", gatewayConfig.ProxyRules.Count);
            
            var routes = BuildRoutes(gatewayConfig);
            var clusters = BuildClusters(gatewayConfig);
            
            // Signal change with the old token
            var oldToken = _changeToken;
            
            // Create new change token for the next change
            _changeToken = new CancellationTokenSource();
            
            // Update the configuration
            _config = new InMemoryConfig(routes, clusters, new CancellationChangeToken(_changeToken.Token));
            
            // Trigger the old token to signal change
            oldToken.Cancel();
            
            _logger.LogInformation("YARP configuration updated successfully with {RouteCount} routes and {ClusterCount} clusters", 
                routes.Length, clusters.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating YARP configuration");
        }
    }

    /// <summary>
    /// Builds route configurations from the gateway configuration.
    /// </summary>
    /// <param name="gatewayConfig">The gateway configuration.</param>
    /// <returns>An array of route configurations.</returns>
    private RouteConfig[] BuildRoutes(GatewayConfigurationOptions gatewayConfig)
    {
        var routes = new List<RouteConfig>();

        foreach (var rule in gatewayConfig.ProxyRules)
        {
            var routeId = rule.Name;
            var clusterId = rule.Cluster.Name;
            
            // Prepare transforms
            var transforms = PrepareTransforms(rule);
            
            // Create a route for this rule
            var routeConfig = new RouteConfig
            {
                RouteId = routeId,
                ClusterId = clusterId,
                Match = new RouteMatch
                {
                    // Match the hosts for this rule
                    Hosts = rule.Hosts.ToArray(),
                    
                    // Match the path for this rule
                    Path = rule.PathPrefix != null 
                        ? $"{rule.PathPrefix}/{{**catchAll}}" 
                        : "/{**catchAll}",
                    
                    // Match the methods for this rule
                    Methods = rule.Methods.Count > 0 ? rule.Methods.ToArray() : null
                },
                Transforms = transforms
            };
            
            routes.Add(routeConfig);
        }

        return routes.ToArray();
    }

    /// <summary>
    /// Prepares transforms for a proxy rule.
    /// </summary>
    /// <param name="rule">The proxy rule.</param>
    /// <returns>A list of transforms or null if none are needed.</returns>
    private List<Dictionary<string, string>>? PrepareTransforms(ProxyRuleConfig rule)
    {
        List<Dictionary<string, string>>? transforms = null;
        
        if (rule.StripPrefix && !string.IsNullOrEmpty(rule.PathPrefix))
        {
            transforms = new List<Dictionary<string, string>>();
            
            // Add the PathRemovePrefix transform
            transforms.Add(new Dictionary<string, string>
            {
                { "PathRemovePrefix", rule.PathPrefix }
            });
            
            // Add any other transforms from the cluster
            if (rule.Cluster.Transforms != null && rule.Cluster.Transforms.Count > 0)
            {
                transforms.AddRange(rule.Cluster.Transforms);
            }
        }
        else if (rule.Cluster.Transforms != null && rule.Cluster.Transforms.Count > 0)
        {
            transforms = rule.Cluster.Transforms;
        }

        return transforms;
    }

    /// <summary>
    /// Builds cluster configurations from the gateway configuration.
    /// </summary>
    /// <param name="gatewayConfig">The gateway configuration.</param>
    /// <returns>An array of cluster configurations.</returns>
    private Yarp.ReverseProxy.Configuration.ClusterConfig[] BuildClusters(GatewayConfigurationOptions gatewayConfig)
    {
        var clusters = new List<Yarp.ReverseProxy.Configuration.ClusterConfig>();

        foreach (var rule in gatewayConfig.ProxyRules)
        {
            var clusterId = rule.Cluster.Name;
            
            // Build destinations
            var destinations = BuildDestinations(rule.Cluster.Destinations);
            
            // Prepare health check config and metadata
            var (healthCheckConfig, metadata) = PrepareHealthCheck(rule.Cluster.HealthCheck);
            
            // Prepare HTTP request config
            var requestConfig = PrepareHttpRequestConfig(rule.Cluster.HttpRequest);
            
            // Create a cluster for this rule
            var clusterConfig = new Yarp.ReverseProxy.Configuration.ClusterConfig
            {
                ClusterId = clusterId,
                LoadBalancingPolicy = rule.Cluster.LoadBalancingPolicy,
                Destinations = destinations,
                HealthCheck = healthCheckConfig,
                Metadata = metadata,
                HttpRequest = requestConfig
            };
            
            clusters.Add(clusterConfig);
        }

        return clusters.ToArray();
    }

    /// <summary>
    /// Builds destination configurations from the destination configs.
    /// </summary>
    /// <param name="destinations">The destination configs.</param>
    /// <returns>A dictionary of destination configurations.</returns>
    private Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig> BuildDestinations(List<Configuration.DestinationConfig> destinations)
    {
        var result = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var destination in destinations)
        {
            var destinationKey = string.IsNullOrEmpty(destination.Name) ? "default" : destination.Name;
            
            if (!string.IsNullOrEmpty(destination.Address))
            {
                result[destinationKey] = new Yarp.ReverseProxy.Configuration.DestinationConfig
                {
                    Address = destination.Address,                    
                    Metadata = (IReadOnlyDictionary<string, string>)destination.Metadata
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Prepares health check configuration and metadata.
    /// </summary>
    /// <param name="healthCheck">The health check configuration.</param>
    /// <returns>A tuple containing the health check configuration and metadata.</returns>
    private (Yarp.ReverseProxy.Configuration.HealthCheckConfig? HealthCheck, Dictionary<string, string>? Metadata) PrepareHealthCheck(Configuration.HealthCheckConfig? healthCheck)
    {
        if (healthCheck == null || !healthCheck.Enabled)
        {
            return (null, null);
        }

        var healthCheckConfig = new Yarp.ReverseProxy.Configuration.HealthCheckConfig
        {
            Active = new ActiveHealthCheckConfig
            {
                Enabled = true,
                Interval = TimeSpan.Parse(healthCheck.Interval),
                Timeout = TimeSpan.Parse(healthCheck.Timeout),
                Path = healthCheck.Path,
                Policy = "ConsecutiveFailuresHealthPolicy"
            }
        };
        
        // Add metadata for health check threshold
        var metadata = new Dictionary<string, string>
        {
            { "TransportFailureRateHealthPolicy.RateLimit", healthCheck.Threshold.ToString() }
        };

        return (healthCheckConfig, metadata);
    }

    /// <summary>
    /// Prepares HTTP request configuration.
    /// </summary>
    /// <param name="httpRequest">The HTTP request settings.</param>
    /// <returns>A forwarder request configuration or null if none is needed.</returns>
    private ForwarderRequestConfig? PrepareHttpRequestConfig(HttpRequestSettingsConfig? httpRequest)
    {
        if (httpRequest == null)
        {
            return null;
        }

        var versionPolicy = Enum.TryParse<HttpVersionPolicy>(httpRequest.VersionPolicy, true, out var policy)
            ? policy
            : HttpVersionPolicy.RequestVersionOrLower;
        
        return new ForwarderRequestConfig
        {
            Version = Version.TryParse(httpRequest.Version, out var version) ? version : new Version(1, 1),
            VersionPolicy = versionPolicy,
            AllowResponseBuffering = httpRequest.AllowResponseBuffering,
            ActivityTimeout = !string.IsNullOrEmpty(httpRequest.ActivityTimeout) 
                ? TimeSpan.Parse(httpRequest.ActivityTimeout) 
                : TimeSpan.FromSeconds(100)
        };
    }

    /// <summary>
    /// Disposes the service.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // Unsubscribe from configuration changes
            _gatewayConfigurationService.ConfigurationChanged -= OnGatewayConfigurationChanged;
            
            _changeToken.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// In-memory implementation of IProxyConfig.
    /// </summary>
    private class InMemoryConfig : IProxyConfig
    {
        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<Yarp.ReverseProxy.Configuration.ClusterConfig> clusters, IChangeToken changeToken)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = changeToken;
        }

        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<Yarp.ReverseProxy.Configuration.ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }
    }
}

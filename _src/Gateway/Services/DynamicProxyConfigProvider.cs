using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Data.Contexts.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Data.Entities;

namespace Gateway.Services;

/// <summary>
/// Provides YARP proxy configuration from database with runtime update support
/// </summary>
public class DynamicProxyConfigProvider : IProxyConfigProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DynamicProxyConfigProvider> _logger;
    private CancellationTokenSource _changeTokenSource = new();
    private readonly ConcurrentDictionary<string, RouteConfig> _routes = new();
    private readonly ConcurrentDictionary<string, ClusterConfig> _clusters = new();
    private volatile ProxyConfig? _config;

    public DynamicProxyConfigProvider(
        IServiceScopeFactory scopeFactory,
        ILogger<DynamicProxyConfigProvider> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public IProxyConfig GetConfig()
    {
        if (_config == null)
        {
            _logger.LogInformation("Loading initial proxy configuration from database");
            LoadConfigurationAsync().GetAwaiter().GetResult();
        }
        
        return _config ?? new ProxyConfig([], []);
    }

    /// <summary>
    /// Triggers configuration reload from database
    /// </summary>
    public async Task ReloadConfigurationAsync()
    {
        _logger.LogInformation("Reloading proxy configuration from database");
        await LoadConfigurationAsync();
        
        // Signal that configuration has changed
        var oldTokenSource = _changeTokenSource;
        var newTokenSource = new CancellationTokenSource();
        
        Interlocked.Exchange(ref _changeTokenSource, newTokenSource);
        oldTokenSource.Cancel();
        oldTokenSource.Dispose();
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IWriteOnlyContext>();

            // Load routes with related entities using query splitting to avoid Cartesian product
            var routes = await context.Routes
                .AsSplitQuery()
                .Include(r => r.Host)
                .Include(r => r.Match)
                    .ThenInclude(m => m.Headers)
                .Include(r => r.Match)
                    .ThenInclude(m => m.QueryParameters)
                .Include(r => r.Transforms)
                .Include(r => r.Metadata)
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            // Load clusters with related entities using query splitting
            var clusters = await context.Clusters
                .AsSplitQuery()
                .Include(c => c.Host)
                .Include(c => c.Destinations)
                .Include(c => c.HealthCheck)
                    .ThenInclude(h => h.Active)
                .Include(c => c.HealthCheck)
                    .ThenInclude(h => h.Passive)
                .Include(c => c.SessionAffinity)
                .Include(c => c.HttpClient)
                .Include(c => c.HttpRequest)
                .Include(c => c.Metadata)
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            // Convert to YARP configuration
            var routeConfigs = routes.Select(ConvertToRouteConfig).ToList();
            var clusterConfigs = clusters.Select(ConvertToClusterConfig).ToList();

            _config = new ProxyConfig(routeConfigs, clusterConfigs);
            
            _logger.LogInformation("Loaded {RouteCount} routes and {ClusterCount} clusters from database", 
                routeConfigs.Count, clusterConfigs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load proxy configuration from database");
            
            // Fallback to empty configuration to prevent startup failures
            _config = new ProxyConfig([], []);
        }
    }

    private static RouteConfig ConvertToRouteConfig(Route route)
    {
        var transforms = new List<Dictionary<string, string>>();
        
        if (route.Transforms != null)
        {
            foreach (var transform in route.Transforms)
            {
                var transformDict = new Dictionary<string, string>();
                
                // Convert to YARP transform format
                if (!string.IsNullOrEmpty(transform.RequestHeader) && !string.IsNullOrEmpty(transform.Set))
                {
                    // Handle different transform types
                    if (transform.RequestHeader == "PathRemovePrefix")
                    {
                        transformDict["PathRemovePrefix"] = transform.Set;
                    }
                    else if (transform.RequestHeader == "PathSet")
                    {
                        transformDict["PathSet"] = transform.Set;
                    }
                    else if (transform.RequestHeader == "PathPattern")
                    {
                        transformDict["PathPattern"] = transform.Set;
                    }
                    else
                    {
                        // Default behavior for request headers
                        transformDict["RequestHeader"] = transform.RequestHeader;
                        transformDict["Set"] = transform.Set;
                    }
                }
                
                if (transformDict.Count > 0)
                {
                    transforms.Add(transformDict);
                }
            }
        }

        var routeMatch = new RouteMatch();
        
        if (route.Match != null)
        {
            if (!string.IsNullOrEmpty(route.Match.Path))
                routeMatch = routeMatch with { Path = route.Match.Path };
                
            if (route.Match.Hosts?.Any() == true)
                routeMatch = routeMatch with { Hosts = route.Match.Hosts };
                
            if (route.Match.Methods?.Any() == true)
                routeMatch = routeMatch with { Methods = route.Match.Methods };
                
            if (route.Match.Headers?.Any() == true)
            {
                var headers = route.Match.Headers.Select(h => new RouteHeader
                {
                    Name = h.Name,
                    Values = h.Values,
                    Mode = Enum.Parse<HeaderMatchMode>(h.Mode, true),
                    IsCaseSensitive = h.IsCaseSensitive
                }).ToList();
                routeMatch = routeMatch with { Headers = headers };
            }
            
            if (route.Match.QueryParameters?.Any() == true)
            {
                var queryParams = route.Match.QueryParameters.Select(qp => new RouteQueryParameter
                {
                    Name = qp.Name,
                    Values = qp.Values,
                    Mode = Enum.Parse<QueryParameterMatchMode>(qp.Mode, true),
                    IsCaseSensitive = qp.IsCaseSensitive
                }).ToList();
                routeMatch = routeMatch with { QueryParameters = queryParams };
            }
        }

        return new RouteConfig
        {
            RouteId = route.Id.ToString(),
            ClusterId = route.ClusterId.ToString(),
            Match = routeMatch,
            Order = route.Order,
            MaxRequestBodySize = route.MaxRequestBodySize,
            AuthorizationPolicy = route.AuthorizationPolicy,
            CorsPolicy = route.CorsPolicy,
            Transforms = transforms,
            Metadata = route.Metadata?.Data
        };
    }

    private static ClusterConfig ConvertToClusterConfig(Cluster cluster)
    {
        var destinations = new Dictionary<string, DestinationConfig>();
        
        if (cluster.Destinations != null)
        {
            foreach (var dest in cluster.Destinations.Where(d => !d.IsDeleted))
            {
                destinations[dest.Id.ToString()] = new DestinationConfig
                {
                    Address = dest.Address,
                    Health = dest.Health
                };
            }
        }

        var healthCheckConfig = cluster.HealthCheck != null ? new HealthCheckConfig
        {
            Active = cluster.HealthCheck.Active != null ? new ActiveHealthCheckConfig
            {
                Enabled = cluster.HealthCheck.Active.Enabled,
                Interval = cluster.HealthCheck.Active.Interval,
                Timeout = cluster.HealthCheck.Active.Timeout,
                Policy = cluster.HealthCheck.Active.Policy,
                Path = cluster.HealthCheck.Active.Path
            } : null,
            
            Passive = cluster.HealthCheck.Passive != null ? new PassiveHealthCheckConfig
            {
                Enabled = cluster.HealthCheck.Passive.Enabled,
                Policy = cluster.HealthCheck.Passive.Policy,
                ReactivationPeriod = cluster.HealthCheck.Passive.ReactivationPeriod
            } : null
        } : null;

        var sessionAffinityConfig = cluster.SessionAffinity != null ? new SessionAffinityConfig
        {
            Enabled = cluster.SessionAffinity.Enabled,
            Policy = cluster.SessionAffinity.Policy,
            FailurePolicy = cluster.SessionAffinity.FailurePolicy
        } : null;

        var httpClientConfig = cluster.HttpClient != null ? new HttpClientConfig
        {
            SslProtocols = !string.IsNullOrEmpty(cluster.HttpClient.SslProtocols) ? 
                Enum.Parse<System.Security.Authentication.SslProtocols>(cluster.HttpClient.SslProtocols) : null,
            DangerousAcceptAnyServerCertificate = cluster.HttpClient.DangerousAcceptAnyServerCertificate,
            MaxConnectionsPerServer = cluster.HttpClient.MaxConnectionsPerServer,
            EnableMultipleHttp2Connections = cluster.HttpClient.EnableMultipleHttp2Connections,
            RequestHeaderEncoding = cluster.HttpClient.RequestHeaderEncoding
        } : null;

        // ForwarderRequestConfig is not available in current YARP version
        // TODO: Update when YARP version supports this configuration
        var forwarderRequestConfig = (object?)null;

        return new ClusterConfig
        {
            ClusterId = cluster.Id.ToString(),
            LoadBalancingPolicy = cluster.LoadBalancingPolicy,
            Destinations = destinations,
            HealthCheck = healthCheckConfig,
            SessionAffinity = sessionAffinityConfig,
            HttpClient = httpClientConfig,
            HttpRequest = null, // TODO: Implement when YARP supports ForwarderRequestConfig
            Metadata = cluster.Metadata?.Data
        };
    }
}

/// <summary>
/// Simple proxy configuration implementation
/// </summary>
internal class ProxyConfig : IProxyConfig
{
    private readonly CancellationTokenSource _cts = new();

    public ProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = new CancellationChangeToken(_cts.Token);
    }

    public IReadOnlyList<RouteConfig> Routes { get; }
    public IReadOnlyList<ClusterConfig> Clusters { get; }
    public IChangeToken ChangeToken { get; }
}
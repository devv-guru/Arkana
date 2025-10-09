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

            // Load routes with all related entities
            var routes = await context.Routes
                .Include(r => r.Host)
                .Include(r => r.Match)
                    .ThenInclude(m => m.Headers)
                .Include(r => r.Match)
                    .ThenInclude(m => m.QueryParameters)
                .Include(r => r.Transforms)
                .Include(r => r.Metadata)
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            // Load clusters with all related entities
            var clusters = await context.Clusters
                .Include(c => c.Host)
                .Include(c => c.Destinations)
                .Include(c => c.HealthCheck)
                    .ThenInclude(h => h.ActiveHealthCheck)
                .Include(c => c.HealthCheck)
                    .ThenInclude(h => h.PassiveHealthCheck)
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
                
                // Parse transform data (stored as JSON or key-value pairs)
                if (!string.IsNullOrEmpty(transform.RequestHeader))
                {
                    transformDict["RequestHeader"] = transform.RequestHeader;
                }
                if (!string.IsNullOrEmpty(transform.Set))
                {
                    transformDict["Set"] = transform.Set;
                }
                if (!string.IsNullOrEmpty(transform.PathSet))
                {
                    transformDict["PathSet"] = transform.PathSet;
                }
                if (!string.IsNullOrEmpty(transform.PathPattern))
                {
                    transformDict["PathPattern"] = transform.PathPattern;
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
                    Health = dest.Health,
                    Metadata = dest.Metadata?.Data
                };
            }
        }

        var healthCheckConfig = cluster.HealthCheck != null ? new HealthCheckConfig
        {
            Enabled = cluster.HealthCheck.Enabled,
            Interval = cluster.HealthCheck.Interval,
            Timeout = cluster.HealthCheck.Timeout,
            Path = cluster.HealthCheck.Path,
            Query = cluster.HealthCheck.Query,
            
            Active = cluster.HealthCheck.ActiveHealthCheck != null ? new ActiveHealthCheckConfig
            {
                Enabled = cluster.HealthCheck.ActiveHealthCheck.Enabled,
                Interval = cluster.HealthCheck.ActiveHealthCheck.Interval,
                Timeout = cluster.HealthCheck.ActiveHealthCheck.Timeout,
                Policy = cluster.HealthCheck.ActiveHealthCheck.Policy,
                Path = cluster.HealthCheck.ActiveHealthCheck.Path
            } : null,
            
            Passive = cluster.HealthCheck.PassiveHealthCheck != null ? new PassiveHealthCheckConfig
            {
                Enabled = cluster.HealthCheck.PassiveHealthCheck.Enabled,
                Policy = cluster.HealthCheck.PassiveHealthCheck.Policy,
                ReactivationPeriod = cluster.HealthCheck.PassiveHealthCheck.ReactivationPeriod
            } : null
        } : null;

        var sessionAffinityConfig = cluster.SessionAffinity != null ? new SessionAffinityConfig
        {
            Enabled = cluster.SessionAffinity.Enabled,
            Policy = cluster.SessionAffinity.Policy,
            FailurePolicy = cluster.SessionAffinity.FailurePolicy,
            AffinityKeyName = cluster.SessionAffinity.AffinityKeyName,
            Cookie = cluster.SessionAffinity.Cookie != null ? new SessionAffinityCookieConfig
            {
                Domain = cluster.SessionAffinity.Cookie.Domain,
                Expiration = cluster.SessionAffinity.Cookie.Expiration,
                HttpOnly = cluster.SessionAffinity.Cookie.HttpOnly,
                IsEssential = cluster.SessionAffinity.Cookie.IsEssential,
                MaxAge = cluster.SessionAffinity.Cookie.MaxAge,
                Path = cluster.SessionAffinity.Cookie.Path,
                SameSite = cluster.SessionAffinity.Cookie.SameSite,
                SecurePolicy = cluster.SessionAffinity.Cookie.SecurePolicy
            } : null,
            Settings = cluster.SessionAffinity.Settings
        } : null;

        var httpClientConfig = cluster.HttpClient != null ? new HttpClientConfig
        {
            SslProtocols = cluster.HttpClient.SslProtocols,
            DangerousAcceptAnyServerCertificate = cluster.HttpClient.DangerousAcceptAnyServerCertificate,
            MaxConnectionsPerServer = cluster.HttpClient.MaxConnectionsPerServer,
            EnableMultipleHttp2Connections = cluster.HttpClient.EnableMultipleHttp2Connections,
            RequestHeaderEncoding = cluster.HttpClient.RequestHeaderEncoding
        } : null;

        var forwarderRequestConfig = cluster.HttpRequest != null ? new ForwarderRequestConfig
        {
            ActivityTimeout = cluster.HttpRequest.ActivityTimeout,
            Version = cluster.HttpRequest.Version != null ? Version.Parse(cluster.HttpRequest.Version) : null,
            VersionPolicy = cluster.HttpRequest.VersionPolicy != null ? 
                Enum.Parse<HttpVersionPolicy>(cluster.HttpRequest.VersionPolicy) : null,
            AllowResponseBuffering = cluster.HttpRequest.AllowResponseBuffering
        } : null;

        return new ClusterConfig
        {
            ClusterId = cluster.Id.ToString(),
            LoadBalancingPolicy = cluster.LoadBalancingPolicy,
            Destinations = destinations,
            HealthCheck = healthCheckConfig,
            SessionAffinity = sessionAffinityConfig,
            HttpClient = httpClientConfig,
            HttpRequest = forwarderRequestConfig,
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
using Devv.Gateway.Data.Entities;

namespace Devv.Gateway.Data.Contexts.Base;

public interface IReadonlyContext
{
    IQueryable<RouteConfig> Routes { get; }
    IQueryable<ClusterConfig> Clusters { get; }
    IQueryable<MatchConfig> Matches { get; }
    IQueryable<CertificateConfig> Certificates { get; }
    IQueryable<SessionAffinityConfig> SessionAffinities { get; }
    IQueryable<DestinationConfig> Destinations { get; }
    IQueryable<MetadataConfig> Metadata { get; }
    IQueryable<HttpClientConfig> HttpClients { get; }
    IQueryable<HttpRequestConfig> HttpRequests { get; }
    IQueryable<HealthCheckConfig> HealthChecks { get; }
    IQueryable<ActiveHealthCheckConfig> ActiveHealthChecks { get; }
    IQueryable<PassiveHealthCheckConfig> PassiveHealthChecks { get; }
    IQueryable<TransformConfig> Transforms { get; }
    IQueryable<HeaderMatchConfig> HeaderMatches { get; }
    IQueryable<QueryParameterMatchConfig> QueryParameterMatches { get; }
}
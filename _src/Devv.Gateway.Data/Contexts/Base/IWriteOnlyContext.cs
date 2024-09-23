using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Devv.Gateway.Data.Contexts.Base;

public interface IWriteOnlyContext
{
    DbSet<RouteConfig> Routes { get; }
    DbSet<ClusterConfig> Clusters { get; }
    DbSet<MatchConfig> Matches { get; }
    DbSet<CertificateConfig> Certificates { get; }
    DbSet<SessionAffinityConfig> SessionAffinities { get; }
    DbSet<DestinationConfig> Destinations { get; }
    DbSet<MetadataConfig> Metadata { get; }
    DbSet<HttpClientConfig> HttpClients { get; }
    DbSet<HttpRequestConfig> HttpRequests { get; }
    DbSet<HealthCheckConfig> HealthChecks { get; }
    DbSet<ActiveHealthCheckConfig> ActiveHealthChecks { get; }
    DbSet<PassiveHealthCheckConfig> PassiveHealthChecks { get; }
    DbSet<TransformConfig> Transforms { get; }
    DbSet<HeaderMatchConfig> HeaderMatches { get; }
    DbSet<QueryParameterMatchConfig> QueryParameterMatches { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    
    public DatabaseFacade Database { get; }
}
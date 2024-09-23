using System.Reflection;
using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.Base;

public class ReadonlyDataContext : DbContext, IReadonlyContext
{
    public ReadonlyDataContext(DbContextOptions options)
        : base(options)
    {
    }

    public IQueryable<RouteConfig> Routes => Set<RouteConfig>().AsNoTracking();
    public IQueryable<ClusterConfig> Clusters => Set<ClusterConfig>().AsNoTracking();
    public IQueryable<MatchConfig> Matches => Set<MatchConfig>().AsNoTracking();
    public IQueryable<CertificateConfig> Certificates => Set<CertificateConfig>().AsNoTracking();
    public IQueryable<SessionAffinityConfig> SessionAffinities => Set<SessionAffinityConfig>().AsNoTracking();
    public IQueryable<DestinationConfig> Destinations => Set<DestinationConfig>().AsNoTracking();
    public IQueryable<MetadataConfig> Metadata => Set<MetadataConfig>().AsNoTracking();
    public IQueryable<HttpClientConfig> HttpClients => Set<HttpClientConfig>().AsNoTracking();
    public IQueryable<HttpRequestConfig> HttpRequests => Set<HttpRequestConfig>().AsNoTracking();
    public IQueryable<HealthCheckConfig> HealthChecks => Set<HealthCheckConfig>().AsNoTracking();
    public IQueryable<ActiveHealthCheckConfig> ActiveHealthChecks => Set<ActiveHealthCheckConfig>().AsNoTracking();
    public IQueryable<PassiveHealthCheckConfig> PassiveHealthChecks => Set<PassiveHealthCheckConfig>().AsNoTracking();
    public IQueryable<TransformConfig> Transforms => Set<TransformConfig>().AsNoTracking();
    public IQueryable<HeaderMatchConfig> HeaderMatches => Set<HeaderMatchConfig>().AsNoTracking();
    public IQueryable<QueryParameterMatchConfig> QueryParameterMatches => Set<QueryParameterMatchConfig>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

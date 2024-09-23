using System.Reflection;
using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Devv.Gateway.Data.Contexts.Base;

public class WriteOnlyDataContext : DbContext, IWriteOnlyContext
{
    public WriteOnlyDataContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<RouteConfig> Routes { get; set; }
    public DbSet<ClusterConfig> Clusters { get; set; }
    public DbSet<MatchConfig> Matches { get; set; }
    public DbSet<CertificateConfig> Certificates { get; set; }
    public DbSet<SessionAffinityConfig> SessionAffinities { get; set; }
    public DbSet<DestinationConfig> Destinations { get; set; }
    public DbSet<MetadataConfig> Metadata { get; set; }
    public DbSet<HttpClientConfig> HttpClients { get; set; }
    public DbSet<HttpRequestConfig> HttpRequests { get; set; }
    public DbSet<HealthCheckConfig> HealthChecks { get; set; }
    public DbSet<ActiveHealthCheckConfig> ActiveHealthChecks { get; set; }
    public DbSet<PassiveHealthCheckConfig> PassiveHealthChecks { get; set; }
    public DbSet<TransformConfig> Transforms { get; set; }
    public DbSet<HeaderMatchConfig> HeaderMatches { get; set; }
    public DbSet<QueryParameterMatchConfig> QueryParameterMatches { get; set; }

    public int SaveChanges()
    {
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

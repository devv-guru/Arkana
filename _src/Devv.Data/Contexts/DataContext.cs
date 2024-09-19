using System.Reflection;
using Devv.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Devv.Data.Contexts;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options)
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Applies configs from separate classes which implemented IEntityTypeConfiguration<T>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
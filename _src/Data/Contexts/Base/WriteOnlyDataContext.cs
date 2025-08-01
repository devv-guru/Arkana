using System.Reflection;
using Data.Entities;
using Data.Entities.Metrics;
using Data.Entities.Mcp;
using Data.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts.Base;

public class WriteOnlyDataContext : DbContext, IWriteOnlyContext
{
    public WriteOnlyDataContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<WebHost> WebHosts { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Cluster> Clusters { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<SessionAffinity> SessionAffinities { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<Metadata> Metadata { get; set; }
    public DbSet<HttpClientSettings> HttpClients { get; set; }
    public DbSet<HttpRequestSettings> HttpRequests { get; set; }
    public DbSet<HealthCheck> HealthChecks { get; set; }
    public DbSet<ActiveHealthCheck> ActiveHealthChecks { get; set; }
    public DbSet<PassiveHealthCheck> PassiveHealthChecks { get; set; }
    public DbSet<Transform> Transforms { get; set; }
    public DbSet<HeaderMatch> HeaderMatches { get; set; }
    public DbSet<QueryParameterMatch> QueryParameterMatches { get; set; }
    
    // Metrics
    public DbSet<RequestMetric> RequestMetrics { get; set; }
    public DbSet<SystemMetric> SystemMetrics { get; set; }
    
    // MCP
    public DbSet<McpServer> McpServers { get; set; }
    public DbSet<McpUserAssignment> McpUserAssignments { get; set; }
    public DbSet<McpRoleAssignment> McpRoleAssignments { get; set; }
    public DbSet<McpBackendAuth> McpBackendAuths { get; set; }
    public DbSet<McpUserApiKey> McpUserApiKeys { get; set; }
    public DbSet<McpAuditLog> McpAuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new BaseEntityInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

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

    public IQueryable<WebHost> WebHosts => Set<WebHost>().AsNoTracking();
    public IQueryable<Route> Routes => Set<Route>().AsNoTracking();
    public IQueryable<Cluster> Clusters => Set<Cluster>().AsNoTracking();
    public IQueryable<Match> Matches => Set<Match>().AsNoTracking();
    public IQueryable<Certificate> Certificates => Set<Certificate>().AsNoTracking();
    public IQueryable<SessionAffinity> SessionAffinities => Set<SessionAffinity>().AsNoTracking();
    public IQueryable<Destination> Destinations => Set<Destination>().AsNoTracking();
    public IQueryable<Metadata> Metadata => Set<Metadata>().AsNoTracking();
    public IQueryable<HttpClientSettings> HttpClients => Set<HttpClientSettings>().AsNoTracking();
    public IQueryable<HttpRequestSettings> HttpRequests => Set<HttpRequestSettings>().AsNoTracking();
    public IQueryable<HealthCheck> HealthChecks => Set<HealthCheck>().AsNoTracking();
    public IQueryable<ActiveHealthCheck> ActiveHealthChecks => Set<ActiveHealthCheck>().AsNoTracking();
    public IQueryable<PassiveHealthCheck> PassiveHealthChecks => Set<PassiveHealthCheck>().AsNoTracking();
    public IQueryable<Transform> Transforms => Set<Transform>().AsNoTracking();
    public IQueryable<HeaderMatch> HeaderMatches => Set<HeaderMatch>().AsNoTracking();

    public IQueryable<QueryParameterMatch> QueryParameterMatches =>
        Set<QueryParameterMatch>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
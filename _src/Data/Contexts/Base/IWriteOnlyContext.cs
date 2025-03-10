﻿using Data.Entities;
using Data.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Data.Contexts.Base;

public interface IWriteOnlyContext
{
    DbSet<WebHost> WebHosts { get; }
    DbSet<Route> Routes { get; }
    DbSet<Cluster> Clusters { get; }
    DbSet<Match> Matches { get; }
    DbSet<Certificate> Certificates { get; }
    DbSet<SessionAffinity> SessionAffinities { get; }
    DbSet<Destination> Destinations { get; }
    DbSet<Metadata> Metadata { get; }
    DbSet<HttpClientSettings> HttpClients { get; }
    DbSet<HttpRequestSettings> HttpRequests { get; }
    DbSet<HealthCheck> HealthChecks { get; }
    DbSet<ActiveHealthCheck> ActiveHealthChecks { get; }
    DbSet<PassiveHealthCheck> PassiveHealthChecks { get; }
    DbSet<Transform> Transforms { get; }
    DbSet<HeaderMatch> HeaderMatches { get; }
    DbSet<QueryParameterMatch> QueryParameterMatches { get; }
    
    // Metrics
    DbSet<RequestMetric> RequestMetrics { get; }
    DbSet<SystemMetric> SystemMetrics { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    public DatabaseFacade Database { get; }
}

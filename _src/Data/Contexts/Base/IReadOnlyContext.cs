﻿using Data.Entities;
using Data.Entities.Metrics;
using Data.Entities.Mcp;

namespace Data.Contexts.Base;

public interface IReadOnlyContext
{
    IQueryable<WebHost> WebHosts { get; }
    IQueryable<Route> Routes { get; }
    IQueryable<Cluster> Clusters { get; }
    IQueryable<Match> Matches { get; }
    IQueryable<Certificate> Certificates { get; }
    IQueryable<SessionAffinity> SessionAffinities { get; }
    IQueryable<Destination> Destinations { get; }
    IQueryable<Metadata> Metadata { get; }
    IQueryable<HttpClientSettings> HttpClients { get; }
    IQueryable<HttpRequestSettings> HttpRequests { get; }
    IQueryable<HealthCheck> HealthChecks { get; }
    IQueryable<ActiveHealthCheck> ActiveHealthChecks { get; }
    IQueryable<PassiveHealthCheck> PassiveHealthChecks { get; }
    IQueryable<Transform> Transforms { get; }
    IQueryable<HeaderMatch> HeaderMatches { get; }
    IQueryable<QueryParameterMatch> QueryParameterMatches { get; }
    
    // Metrics
    IQueryable<RequestMetric> RequestMetrics { get; }
    IQueryable<SystemMetric> SystemMetrics { get; }
    
    // MCP
    IQueryable<McpServer> McpServers { get; }
    IQueryable<McpUserAssignment> McpUserAssignments { get; }
    IQueryable<McpRoleAssignment> McpRoleAssignments { get; }
    IQueryable<McpBackendAuth> McpBackendAuths { get; }
    IQueryable<McpUserApiKey> McpUserApiKeys { get; }
    IQueryable<McpAuditLog> McpAuditLogs { get; }
}

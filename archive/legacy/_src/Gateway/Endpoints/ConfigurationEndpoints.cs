using Gateway.Services;
using Gateway.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Gateway.Endpoints;

public static class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/configuration")
            .WithTags("Configuration Management")
            .RequireAuthorization();

        // Configuration Status
        group.MapGet("/status", GetConfigurationStatus)
            .WithName("GetConfigurationStatus")
            .WithSummary("Get overall configuration status and health");

        // Routes
        group.MapGet("/routes", GetRoutes)
            .WithName("GetRoutes")
            .WithSummary("Get all active routes");

        group.MapGet("/routes/{id:guid}", GetRoute)
            .WithName("GetRoute")
            .WithSummary("Get a specific route by ID");

        group.MapPost("/routes", CreateRoute)
            .WithName("CreateRoute")
            .WithSummary("Create a new route");

        group.MapPut("/routes/{id:guid}", UpdateRoute)
            .WithName("UpdateRoute")
            .WithSummary("Update an existing route");

        group.MapDelete("/routes/{id:guid}", DeleteRoute)
            .WithName("DeleteRoute")
            .WithSummary("Delete a route");

        // Clusters
        group.MapGet("/clusters", GetClusters)
            .WithName("GetClusters")
            .WithSummary("Get all active clusters");

        group.MapGet("/clusters/{id:guid}", GetCluster)
            .WithName("GetCluster")
            .WithSummary("Get a specific cluster by ID");

        group.MapPost("/clusters", CreateCluster)
            .WithName("CreateCluster")
            .WithSummary("Create a new cluster");

        group.MapPut("/clusters/{id:guid}", UpdateCluster)
            .WithName("UpdateCluster")
            .WithSummary("Update an existing cluster");

        group.MapDelete("/clusters/{id:guid}", DeleteCluster)
            .WithName("DeleteCluster")
            .WithSummary("Delete a cluster");

        // Destinations
        group.MapGet("/clusters/{clusterId:guid}/destinations", GetDestinations)
            .WithName("GetDestinations")
            .WithSummary("Get all destinations for a cluster");

        group.MapGet("/destinations/{id:guid}", GetDestination)
            .WithName("GetDestination")
            .WithSummary("Get a specific destination by ID");

        group.MapPost("/destinations", CreateDestination)
            .WithName("CreateDestination")
            .WithSummary("Create a new destination");

        group.MapPut("/destinations/{id:guid}", UpdateDestination)
            .WithName("UpdateDestination")
            .WithSummary("Update an existing destination");

        group.MapDelete("/destinations/{id:guid}", DeleteDestination)
            .WithName("DeleteDestination")
            .WithSummary("Delete a destination");

        // WebHosts
        group.MapGet("/webhosts", GetWebHosts)
            .WithName("GetWebHosts")
            .WithSummary("Get all active web hosts");

        group.MapGet("/webhosts/{id:guid}", GetWebHost)
            .WithName("GetWebHost")
            .WithSummary("Get a specific web host by ID");

        group.MapPost("/webhosts", CreateWebHost)
            .WithName("CreateWebHost")
            .WithSummary("Create a new web host");

        group.MapPut("/webhosts/{id:guid}", UpdateWebHost)
            .WithName("UpdateWebHost")
            .WithSummary("Update an existing web host");

        group.MapDelete("/webhosts/{id:guid}", DeleteWebHost)
            .WithName("DeleteWebHost")
            .WithSummary("Delete a web host");

        // Configuration Operations
        group.MapPost("/reload", ReloadConfiguration)
            .WithName("ReloadConfiguration")
            .WithSummary("Force reload of proxy configuration from database");

        group.MapPost("/validate", ValidateConfiguration)
            .WithName("ValidateConfiguration")
            .WithSummary("Validate current configuration");

        group.MapPost("/import", ImportConfiguration)
            .WithName("ImportConfiguration")
            .WithSummary("Import configuration from JSON");

        group.MapGet("/export", ExportConfiguration)
            .WithName("ExportConfiguration")
            .WithSummary("Export current configuration as JSON");
    }

    #region Status Endpoints

    private static async Task<IResult> GetConfigurationStatus(IConfigurationService configService)
    {
        try
        {
            var status = await configService.GetConfigurationStatusAsync();
            return Results.Ok(status);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get configuration status: {ex.Message}");
        }
    }

    #endregion

    #region Route Endpoints

    private static async Task<IResult> GetRoutes(IConfigurationService configService)
    {
        try
        {
            var routes = await configService.GetRoutesAsync();
            return Results.Ok(routes);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get routes: {ex.Message}");
        }
    }

    private static async Task<IResult> GetRoute(Guid id, IConfigurationService configService)
    {
        try
        {
            var route = await configService.GetRouteAsync(id);
            return route != null ? Results.Ok(route) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get route: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateRoute(Route route, IConfigurationService configService)
    {
        try
        {
            var createdRoute = await configService.CreateRouteAsync(route);
            return Results.Created($"/api/configuration/routes/{createdRoute.Id}", createdRoute);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to create route: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateRoute(Guid id, Route route, IConfigurationService configService)
    {
        try
        {
            route.Id = id;
            var updatedRoute = await configService.UpdateRouteAsync(route);
            return Results.Ok(updatedRoute);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update route: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteRoute(Guid id, IConfigurationService configService)
    {
        try
        {
            await configService.DeleteRouteAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to delete route: {ex.Message}");
        }
    }

    #endregion

    #region Cluster Endpoints

    private static async Task<IResult> GetClusters(IConfigurationService configService)
    {
        try
        {
            var clusters = await configService.GetClustersAsync();
            return Results.Ok(clusters);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get clusters: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCluster(Guid id, IConfigurationService configService)
    {
        try
        {
            var cluster = await configService.GetClusterAsync(id);
            return cluster != null ? Results.Ok(cluster) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get cluster: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateCluster(Cluster cluster, IConfigurationService configService)
    {
        try
        {
            var createdCluster = await configService.CreateClusterAsync(cluster);
            return Results.Created($"/api/configuration/clusters/{createdCluster.Id}", createdCluster);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to create cluster: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateCluster(Guid id, Cluster cluster, IConfigurationService configService)
    {
        try
        {
            cluster.Id = id;
            var updatedCluster = await configService.UpdateClusterAsync(cluster);
            return Results.Ok(updatedCluster);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update cluster: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteCluster(Guid id, IConfigurationService configService)
    {
        try
        {
            await configService.DeleteClusterAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to delete cluster: {ex.Message}");
        }
    }

    #endregion

    #region Destination Endpoints

    private static async Task<IResult> GetDestinations(Guid clusterId, IConfigurationService configService)
    {
        try
        {
            var destinations = await configService.GetDestinationsAsync(clusterId);
            return Results.Ok(destinations);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get destinations: {ex.Message}");
        }
    }

    private static async Task<IResult> GetDestination(Guid id, IConfigurationService configService)
    {
        try
        {
            var destination = await configService.GetDestinationAsync(id);
            return destination != null ? Results.Ok(destination) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get destination: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateDestination(Destination destination, IConfigurationService configService)
    {
        try
        {
            var createdDestination = await configService.CreateDestinationAsync(destination);
            return Results.Created($"/api/configuration/destinations/{createdDestination.Id}", createdDestination);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to create destination: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateDestination(Guid id, Destination destination, IConfigurationService configService)
    {
        try
        {
            destination.Id = id;
            var updatedDestination = await configService.UpdateDestinationAsync(destination);
            return Results.Ok(updatedDestination);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update destination: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteDestination(Guid id, IConfigurationService configService)
    {
        try
        {
            await configService.DeleteDestinationAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to delete destination: {ex.Message}");
        }
    }

    #endregion

    #region WebHost Endpoints

    private static async Task<IResult> GetWebHosts(IConfigurationService configService)
    {
        try
        {
            var webHosts = await configService.GetWebHostsAsync();
            return Results.Ok(webHosts);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get web hosts: {ex.Message}");
        }
    }

    private static async Task<IResult> GetWebHost(Guid id, IConfigurationService configService)
    {
        try
        {
            var webHost = await configService.GetWebHostAsync(id);
            return webHost != null ? Results.Ok(webHost) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to get web host: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateWebHost(WebHost webHost, IConfigurationService configService)
    {
        try
        {
            var createdWebHost = await configService.CreateWebHostAsync(webHost);
            return Results.Created($"/api/configuration/webhosts/{createdWebHost.Id}", createdWebHost);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to create web host: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateWebHost(Guid id, WebHost webHost, IConfigurationService configService)
    {
        try
        {
            webHost.Id = id;
            var updatedWebHost = await configService.UpdateWebHostAsync(webHost);
            return Results.Ok(updatedWebHost);
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to update web host: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteWebHost(Guid id, IConfigurationService configService)
    {
        try
        {
            await configService.DeleteWebHostAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to delete web host: {ex.Message}");
        }
    }

    #endregion

    #region Configuration Operations

    private static async Task<IResult> ReloadConfiguration(IConfigurationService configService)
    {
        try
        {
            await configService.ReloadProxyConfigurationAsync();
            return Results.Ok(new { Message = "Configuration reloaded successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to reload configuration: {ex.Message}");
        }
    }

    private static async Task<IResult> ValidateConfiguration(IConfigurationService configService)
    {
        try
        {
            var isValid = await configService.ValidateConfigurationAsync();
            var status = await configService.GetConfigurationStatusAsync();
            
            return Results.Ok(new 
            { 
                IsValid = isValid,
                Status = status
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to validate configuration: {ex.Message}");
        }
    }

    private static async Task<IResult> ImportConfiguration(GatewayConfigurationModel configuration, IConfigurationService configService)
    {
        try
        {
            await configService.ImportConfigurationAsync(configuration);
            return Results.Ok(new { Message = "Configuration imported successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to import configuration: {ex.Message}");
        }
    }

    private static async Task<IResult> ExportConfiguration(IConfigurationService configService)
    {
        try
        {
            var configuration = await configService.ExportConfigurationAsync();
            return Results.Ok(configuration);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to export configuration: {ex.Message}");
        }
    }

    #endregion
}
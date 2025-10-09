using Gateway.Models;
using Gateway.Services;
using Gateway.Data;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Endpoints;

public static class McpServerEndpoints
{
    public static void MapMcpServerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/servers")
            .WithTags("MCP Servers")
            .RequireAuthorization();

        group.MapGet("/", GetAllServers)
            .WithName("GetAllServers")
            .WithSummary("Get all MCP servers")
            .Produces<IEnumerable<McpServer>>();

        group.MapGet("/{id:int}", GetServerById)
            .WithName("GetServerById")
            .WithSummary("Get MCP server by ID")
            .Produces<McpServer>()
            .Produces(404);

        group.MapGet("/name/{name}", GetServerByName)
            .WithName("GetServerByName")
            .WithSummary("Get MCP server by name")
            .Produces<McpServer>()
            .Produces(404);

        group.MapPost("/", CreateServer)
            .WithName("CreateServer")
            .WithSummary("Create new MCP server")
            .Produces<McpServer>(201)
            .Produces<ValidationProblemDetails>(400);

        group.MapPut("/{id:int}", UpdateServer)
            .WithName("UpdateServer")
            .WithSummary("Update MCP server")
            .Produces<McpServer>()
            .Produces(404)
            .Produces<ValidationProblemDetails>(400);

        group.MapDelete("/{id:int}", DeleteServer)
            .WithName("DeleteServer")
            .WithSummary("Delete MCP server")
            .Produces(204)
            .Produces(404);
    }

    private static async Task<IResult> GetAllServers(IMcpServerService serverService)
    {
        var servers = await serverService.GetAllServersAsync();
        return Results.Ok(servers);
    }

    private static async Task<IResult> GetServerById(int id, IMcpServerService serverService)
    {
        var server = await serverService.GetServerByIdAsync(id);
        return server == null ? Results.NotFound() : Results.Ok(server);
    }

    private static async Task<IResult> GetServerByName(string name, IMcpServerService serverService)
    {
        var server = await serverService.GetServerByNameAsync(name);
        return server == null ? Results.NotFound() : Results.Ok(server);
    }

    private static async Task<IResult> CreateServer(
        CreateMcpServerRequest request, 
        IMcpServerService serverService,
        IValidator<CreateMcpServerRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var server = new McpServer
        {
            Name = request.Name,
            Description = request.Description,
            Endpoint = request.Endpoint,
            Protocol = request.Protocol,
            AuthType = request.AuthType,
            AuthSettings = request.AuthSettings,
            IsEnabled = request.IsEnabled
        };

        var created = await serverService.CreateServerAsync(server);
        return Results.Created($"/api/servers/{created.Id}", created);
    }

    private static async Task<IResult> UpdateServer(
        int id, 
        UpdateMcpServerRequest request, 
        IMcpServerService serverService,
        IValidator<UpdateMcpServerRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var server = new McpServer
        {
            Name = request.Name,
            Description = request.Description,
            Endpoint = request.Endpoint,
            Protocol = request.Protocol,
            AuthType = request.AuthType,
            AuthSettings = request.AuthSettings,
            IsEnabled = request.IsEnabled
        };

        var updated = await serverService.UpdateServerAsync(id, server);
        return updated == null ? Results.NotFound() : Results.Ok(updated);
    }

    private static async Task<IResult> DeleteServer(int id, IMcpServerService serverService)
    {
        var deleted = await serverService.DeleteServerAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}

public record CreateMcpServerRequest(
    string Name,
    string? Description,
    string Endpoint,
    McpProtocolType Protocol = McpProtocolType.Http,
    McpAuthType AuthType = McpAuthType.None,
    string? AuthSettings = null,
    bool IsEnabled = true
);

public record UpdateMcpServerRequest(
    string Name,
    string? Description,
    string Endpoint,
    McpProtocolType Protocol,
    McpAuthType AuthType,
    string? AuthSettings,
    bool IsEnabled
);
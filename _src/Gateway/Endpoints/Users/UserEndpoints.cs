using Data.Entities.Mcp;
using Data.Contexts.Base;
using Gateway.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Endpoints.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("User Management");

        group.MapGet("/{userId}/servers", GetUserServers)
            .WithName("GetUserServers")
            .WithSummary("Get servers accessible by user")
            .Produces<IEnumerable<McpServer>>();

        group.MapPost("/{userId}/access", GrantUserAccess)
            .WithName("GrantUserAccess")
            .WithSummary("Grant user access to MCP server")
            .Produces(200)
            .Produces<ValidationProblemDetails>(400);

        group.MapDelete("/{userId}/access/{serverId:guid}", RevokeUserAccess)
            .WithName("RevokeUserAccess")
            .WithSummary("Revoke user access to MCP server")
            .Produces(200)
            .Produces(404);

        group.MapGet("/{userId}/audit", GetUserAuditLog)
            .WithName("GetUserAuditLog")
            .WithSummary("Get user audit log")
            .Produces<IEnumerable<McpAuditLog>>();

        group.MapPost("/{userId}/audit", LogUserEvent)
            .WithName("LogUserEvent")
            .WithSummary("Log user event")
            .Produces(201);
    }

    private static async Task<IResult> GetUserServers(string userId, IUserService userService)
    {
        var servers = await userService.GetUserServersAsync(userId);
        return Results.Ok(servers);
    }

    private static async Task<IResult> GrantUserAccess(
        string userId, 
        GrantAccessRequest request, 
        IUserService userService,
        IValidator<GrantAccessRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var success = await userService.GrantAccessAsync(
            userId, 
            request.ServerId, 
            request.UserEmail);

        return success ? Results.Ok() : Results.BadRequest("Failed to grant access");
    }

    private static async Task<IResult> RevokeUserAccess(
        string userId, 
        Guid serverId, 
        IUserService userService)
    {
        var success = await userService.RevokeAccessAsync(userId, serverId);
        return success ? Results.Ok() : Results.NotFound("Access record not found");
    }

    private static async Task<IResult> GetUserAuditLog(
        string userId,
        [FromServices] IWriteOnlyContext context,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var logs = await context.McpAuditLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .Skip(skip)
            .Take(Math.Min(take, 1000))
            .ToListAsync();

        return Results.Ok(logs);
    }

    private static async Task<IResult> LogUserEvent(
        string userId,
        LogEventRequest request,
        IUserService userService,
        IValidator<LogEventRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        await userService.LogAuditEventAsync(
            userId,
            request.ServerId,
            request.Action,
            request.Details,
            request.IpAddress,
            request.UserAgent);

        return Results.Created($"/api/users/{userId}/audit", null);
    }
}

public record GrantAccessRequest(
    Guid ServerId,
    string? UserEmail = null
);

public record LogEventRequest(
    Guid? ServerId,
    string Action,
    string? Details = null,
    string? IpAddress = null,
    string? UserAgent = null
);
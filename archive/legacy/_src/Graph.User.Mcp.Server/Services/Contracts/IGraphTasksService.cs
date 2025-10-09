using System.Text.Json;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Interface for Microsoft Graph tasks and planning operations
/// Based on Tasks.Read, Tasks.ReadWrite, Group.Read.All (for Planner) permissions
/// </summary>
public interface IGraphTasksService
{
    // Microsoft To Do
    ValueTask<JsonDocument?> GetTodoListsAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetTodoListAsync(string accessToken, string listId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateTodoListAsync(string accessToken, object listData, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateTodoListAsync(string accessToken, string listId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteTodoListAsync(string accessToken, string listId, string? correlationId = null);
    
    // To Do Tasks
    ValueTask<JsonDocument?> GetTodoTasksAsync(string accessToken, string listId, string? correlationId = null);
    ValueTask<JsonDocument?> GetTodoTaskAsync(string accessToken, string listId, string taskId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateTodoTaskAsync(string accessToken, string listId, object taskData, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateTodoTaskAsync(string accessToken, string listId, string taskId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteTodoTaskAsync(string accessToken, string listId, string taskId, string? correlationId = null);
    ValueTask<JsonDocument?> CompleteTodoTaskAsync(string accessToken, string listId, string taskId, string? correlationId = null);
    
    // Task Attachments
    ValueTask<JsonDocument?> GetTodoTaskAttachmentsAsync(string accessToken, string listId, string taskId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateTodoTaskAttachmentAsync(string accessToken, string listId, string taskId, object attachment, string? correlationId = null);
    
    // Planner Plans
    ValueTask<JsonDocument?> GetMyPlannerTasksAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetPlannerPlanAsync(string accessToken, string planId, string? correlationId = null);
    ValueTask<JsonDocument?> GetPlannerPlanTasksAsync(string accessToken, string planId, string? correlationId = null);
    ValueTask<JsonDocument?> GetPlannerPlanBucketsAsync(string accessToken, string planId, string? correlationId = null);
    
    // Planner Tasks
    ValueTask<JsonDocument?> GetPlannerTaskAsync(string accessToken, string taskId, string? correlationId = null);
    ValueTask<JsonDocument?> CreatePlannerTaskAsync(string accessToken, object taskData, string? correlationId = null);
    ValueTask<JsonDocument?> UpdatePlannerTaskAsync(string accessToken, string taskId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeletePlannerTaskAsync(string accessToken, string taskId, string? correlationId = null);
    
    // Planner Task Details
    ValueTask<JsonDocument?> GetPlannerTaskDetailsAsync(string accessToken, string taskId, string? correlationId = null);
    ValueTask<JsonDocument?> UpdatePlannerTaskDetailsAsync(string accessToken, string taskId, object details, string? correlationId = null);
}
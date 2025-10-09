using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Graph.User.Mcp.Server.Services.Models;
using Graph.User.Mcp.Server.Services.Utilities;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Microsoft Graph service for tasks and planning operations
/// </summary>
public class GraphTasksService : GraphServiceBase, IGraphTasksService
{
    public GraphTasksService(HttpClient httpClient, ILogger<GraphTasksService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Microsoft To Do

    public async ValueTask<JsonDocument?> GetTodoListsAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/todo/lists";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTodoLists", correlationId);
    }

    public async ValueTask<JsonDocument?> GetTodoListAsync(string accessToken, string listId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        var endpoint = $"me/todo/lists/{listId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTodoList", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateTodoListAsync(string accessToken, object listData, string? correlationId = null)
    {
        const string endpoint = "me/todo/lists";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateTodoList", listData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateTodoListAsync(string accessToken, string listId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        var endpoint = $"me/todo/lists/{listId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateTodoList", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteTodoListAsync(string accessToken, string listId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        var endpoint = $"me/todo/lists/{listId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteTodoList", correlationId);
    }

    #endregion

    #region To Do Tasks

    public async ValueTask<JsonDocument?> GetTodoTasksAsync(string accessToken, string listId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        var endpoint = $"me/todo/lists/{listId}/tasks";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTodoTasks", correlationId);
    }

    public async ValueTask<JsonDocument?> GetTodoTaskAsync(string accessToken, string listId, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"me/todo/lists/{listId}/tasks/{taskId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTodoTask", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateTodoTaskAsync(string accessToken, string listId, object taskData, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        var endpoint = $"me/todo/lists/{listId}/tasks";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateTodoTask", taskData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateTodoTaskAsync(string accessToken, string listId, string taskId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"me/todo/lists/{listId}/tasks/{taskId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateTodoTask", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteTodoTaskAsync(string accessToken, string listId, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"me/todo/lists/{listId}/tasks/{taskId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteTodoTask", correlationId);
    }

    public async ValueTask<JsonDocument?> CompleteTodoTaskAsync(string accessToken, string listId, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"me/todo/lists/{listId}/tasks/{taskId}";
        var update = new { status = "completed" };
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "CompleteTodoTask", update, correlationId);
    }

    #endregion

    #region Task Attachments

    public async ValueTask<JsonDocument?> GetTodoTaskAttachmentsAsync(string accessToken, string listId, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"me/todo/lists/{listId}/tasks/{taskId}/attachments";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTodoTaskAttachments", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateTodoTaskAttachmentAsync(string accessToken, string listId, string taskId, object attachment, string? correlationId = null)
    {
        ValidateGuidFormat(listId, nameof(listId));
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"me/todo/lists/{listId}/tasks/{taskId}/attachments";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateTodoTaskAttachment", attachment, correlationId);
    }

    #endregion

    #region Planner Plans

    public async ValueTask<JsonDocument?> GetMyPlannerTasksAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/planner/tasks";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMyPlannerTasks", correlationId);
    }

    public async ValueTask<JsonDocument?> GetPlannerPlanAsync(string accessToken, string planId, string? correlationId = null)
    {
        ValidateGuidFormat(planId, nameof(planId));
        var endpoint = $"planner/plans/{planId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPlannerPlan", correlationId);
    }

    public async ValueTask<JsonDocument?> GetPlannerPlanTasksAsync(string accessToken, string planId, string? correlationId = null)
    {
        ValidateGuidFormat(planId, nameof(planId));
        var endpoint = $"planner/plans/{planId}/tasks";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPlannerPlanTasks", correlationId);
    }

    public async ValueTask<JsonDocument?> GetPlannerPlanBucketsAsync(string accessToken, string planId, string? correlationId = null)
    {
        ValidateGuidFormat(planId, nameof(planId));
        var endpoint = $"planner/plans/{planId}/buckets";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPlannerPlanBuckets", correlationId);
    }

    #endregion

    #region Planner Tasks

    public async ValueTask<JsonDocument?> GetPlannerTaskAsync(string accessToken, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"planner/tasks/{taskId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPlannerTask", correlationId);
    }

    public async ValueTask<JsonDocument?> CreatePlannerTaskAsync(string accessToken, object taskData, string? correlationId = null)
    {
        const string endpoint = "planner/tasks";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreatePlannerTask", taskData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdatePlannerTaskAsync(string accessToken, string taskId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"planner/tasks/{taskId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdatePlannerTask", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeletePlannerTaskAsync(string accessToken, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"planner/tasks/{taskId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeletePlannerTask", correlationId);
    }

    #endregion

    #region Planner Task Details

    public async ValueTask<JsonDocument?> GetPlannerTaskDetailsAsync(string accessToken, string taskId, string? correlationId = null)
    {
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"planner/tasks/{taskId}/details";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPlannerTaskDetails", correlationId);
    }

    public async ValueTask<JsonDocument?> UpdatePlannerTaskDetailsAsync(string accessToken, string taskId, object details, string? correlationId = null)
    {
        ValidateGuidFormat(taskId, nameof(taskId));
        var endpoint = $"planner/tasks/{taskId}/details";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdatePlannerTaskDetails", details, correlationId);
    }

    #endregion
}
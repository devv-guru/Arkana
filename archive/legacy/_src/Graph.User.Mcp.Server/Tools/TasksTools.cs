using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph tasks and planning-related MCP tools
/// </summary>
[McpServerToolType]
public static class TasksTools
{
    [McpServerTool, Description("Get To Do task lists")]
    public static async Task<string> GetTodoLists(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var listsResponse = await tasksService.GetTodoListsAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Lists = listsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get tasks from a specific To Do list")]
    public static async Task<string> GetTodoTasks(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The To Do list ID")] string listId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var tasksResponse = await tasksService.GetTodoTasksAsync(accessToken, listId, correlationId);
            
            var result = new
            {
                Success = true,
                ListId = listId,
                Tasks = tasksResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new To Do task")]
    public static async Task<string> CreateTodoTask(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The To Do list ID")] string listId,
        [Description("Task title")] string title,
        [Description("Task body/notes (optional)")] string? body = null,
        [Description("Due date (YYYY-MM-DD format, optional)")] string? dueDate = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var taskData = new
            {
                title = title,
                body = string.IsNullOrEmpty(body) ? null : new
                {
                    content = body,
                    contentType = "text"
                },
                dueDateTime = string.IsNullOrEmpty(dueDate) ? null : new
                {
                    dateTime = DateTime.Parse(dueDate).ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                    timeZone = "UTC"
                }
            };

            var response = await tasksService.CreateTodoTaskAsync(accessToken, listId, taskData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Task created successfully",
                Task = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Complete a To Do task")]
    public static async Task<string> CompleteTodoTask(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The To Do list ID")] string listId,
        [Description("The task ID")] string taskId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var response = await tasksService.CompleteTodoTaskAsync(accessToken, listId, taskId, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Task marked as completed",
                Task = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get my Planner tasks")]
    public static async Task<string> GetMyPlannerTasks(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var tasksResponse = await tasksService.GetMyPlannerTasksAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Tasks = tasksResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get Planner plan details")]
    public static async Task<string> GetPlannerPlan(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The Planner plan ID")] string planId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var planResponse = await tasksService.GetPlannerPlanAsync(accessToken, planId, correlationId);
            
            var result = new
            {
                Success = true,
                Plan = planResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get tasks from a Planner plan")]
    public static async Task<string> GetPlannerPlanTasks(
        IGraphTasksService tasksService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The Planner plan ID")] string planId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var tasksResponse = await tasksService.GetPlannerPlanTasksAsync(accessToken, planId, correlationId);
            
            var result = new
            {
                Success = true,
                PlanId = planId,
                Tasks = tasksResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
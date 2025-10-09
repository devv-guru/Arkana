using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph user-related MCP tools
/// </summary>
[McpServerToolType]
public static class UserTools
{
    [McpServerTool, Description("Get current user profile information")]
    public static async Task<string> GetMyProfile(
        IGraphUserService userService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var userResponse = await userService.GetCurrentUserProfileAsync(accessToken, null, correlationId);
            
            var result = new
            {
                Success = true,
                User = userResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("List users in the organization")]
    public static async Task<string> ListUsers(
        IGraphUserService userService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of users to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var options = new GraphUserQueryOptions { Top = top };
            var usersResponse = await userService.GetUsersAsync(accessToken, options, correlationId);
            
            var result = new
            {
                Success = true,
                Data = usersResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search for users by display name or email")]
    public static async Task<string> SearchUsers(
        IGraphUserService userService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for user display names or emails")] string searchTerm,
        [Description("Number of results to return (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var options = new GraphUserQueryOptions { Top = top };
            var usersResponse = await userService.SearchUsersAsync(accessToken, searchTerm, options, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Data = usersResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get user by ID")]
    public static async Task<string> GetUserById(
        IGraphUserService userService,
        IHttpContextAccessor httpContextAccessor,
        [Description("User ID (GUID)")] string userId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var userResponse = await userService.GetUserByIdAsync(accessToken, userId, null, correlationId);
            
            var result = new
            {
                Success = true,
                User = userResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get user's manager")]
    public static async Task<string> GetMyManager(
        IGraphUserService userService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var managerResponse = await userService.GetCurrentUserManagerAsync(accessToken, null, correlationId);
            
            var result = new
            {
                Success = true,
                Manager = managerResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get user's direct reports")]
    public static async Task<string> GetMyDirectReports(
        IGraphUserService userService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var reportsResponse = await userService.GetCurrentUserDirectReportsAsync(accessToken, null, correlationId);
            
            var result = new
            {
                Success = true,
                DirectReports = reportsResponse?.RootElement
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
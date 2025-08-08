using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph group-related MCP tools
/// </summary>
[McpServerToolType]
public static class GroupTools
{
    [McpServerTool, Description("List groups in the organization")]
    public static async Task<string> ListGroups(
        IGraphGroupService groupService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of groups to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var groupsResponse = await groupService.GetGroupsAsync(accessToken, top, correlationId);
            
            var result = new
            {
                Success = true,
                Data = groupsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get group by ID")]
    public static async Task<string> GetGroupById(
        IGraphGroupService groupService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Group ID (GUID)")] string groupId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var groupResponse = await groupService.GetGroupByIdAsync(accessToken, groupId, correlationId);
            
            var result = new
            {
                Success = true,
                Group = groupResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get members of a specific group")]
    public static async Task<string> GetGroupMembers(
        IGraphGroupService groupService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Group ID to get members for")] string groupId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var membersResponse = await groupService.GetGroupMembersAsync(accessToken, groupId, correlationId);
            
            var result = new
            {
                Success = true,
                GroupId = groupId,
                Members = membersResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get owners of a specific group")]
    public static async Task<string> GetGroupOwners(
        IGraphGroupService groupService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Group ID to get owners for")] string groupId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var ownersResponse = await groupService.GetGroupOwnersAsync(accessToken, groupId, correlationId);
            
            var result = new
            {
                Success = true,
                GroupId = groupId,
                Owners = ownersResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search for groups by name or description")]
    public static async Task<string> SearchGroups(
        IGraphGroupService groupService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for group names or descriptions")] string searchTerm,
        [Description("Number of results to return (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var groupsResponse = await groupService.SearchGroupsAsync(accessToken, searchTerm, top, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Data = groupsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get current user's group memberships")]
    public static async Task<string> GetMyGroupMemberships(
        IGraphGroupService groupService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var membershipsResponse = await groupService.GetUserGroupMembershipsAsync(accessToken, null, correlationId);
            
            var result = new
            {
                Success = true,
                GroupMemberships = membershipsResponse?.RootElement
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
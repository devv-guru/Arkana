using System.Text.Json;
using Graph.User.Mcp.Server.Services.Models;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Enhanced interface for Microsoft Graph group and team operations
/// Based on Group.Read.All, Group.ReadWrite.All, Team.ReadBasic.All permissions
/// </summary>
public interface IGraphGroupService
{
    // Groups
    ValueTask<JsonDocument?> GetGroupsAsync(string accessToken, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetGroupByIdAsync(string accessToken, string groupId, string? correlationId = null);
    ValueTask<JsonDocument?> SearchGroupsAsync(string accessToken, string searchTerm, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetUserGroupMembershipsAsync(string accessToken, string? userId = null, string? correlationId = null);
    
    // Enhanced Group Operations
    ValueTask<JsonDocument?> GetGroupsWithOptionsAsync(string accessToken, GraphGroupQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetGroupsByTypeAsync(string accessToken, GroupType[] groupTypes, GraphGroupQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetGroupsByVisibilityAsync(string accessToken, GroupVisibility visibility, GraphGroupQueryOptions? options = null, string? correlationId = null);
    
    // Group Membership
    ValueTask<JsonDocument?> GetGroupMembersAsync(string accessToken, string groupId, string? correlationId = null);
    ValueTask<JsonDocument?> GetGroupOwnersAsync(string accessToken, string groupId, string? correlationId = null);
    ValueTask<JsonDocument?> CheckGroupMembershipAsync(string accessToken, string groupId, string userId, string? correlationId = null);
    
    // Group Operations
    ValueTask<JsonDocument?> CreateGroupAsync(string accessToken, object groupData, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateGroupAsync(string accessToken, string groupId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteGroupAsync(string accessToken, string groupId, string? correlationId = null);
    
    // Group Conversations
    ValueTask<JsonDocument?> GetGroupConversationsAsync(string accessToken, string groupId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateGroupConversationAsync(string accessToken, string groupId, object conversation, string? correlationId = null);
    ValueTask<JsonDocument?> GetGroupConversationThreadsAsync(string accessToken, string groupId, string conversationId, string? correlationId = null);
    
    // Teams (if group is team-enabled)
    ValueTask<JsonDocument?> GetTeamAsync(string accessToken, string teamId, string? correlationId = null);
    ValueTask<JsonDocument?> GetTeamChannelsAsync(string accessToken, string teamId, string? correlationId = null);
    ValueTask<JsonDocument?> GetTeamChannelAsync(string accessToken, string teamId, string channelId, string? correlationId = null);
    ValueTask<JsonDocument?> GetChannelMessagesAsync(string accessToken, string teamId, string channelId, int top = 10, string? correlationId = null);
    
    // Group Files & Sites
    ValueTask<JsonDocument?> GetGroupDriveAsync(string accessToken, string groupId, string? correlationId = null);
    ValueTask<JsonDocument?> GetGroupSiteAsync(string accessToken, string groupId, string? correlationId = null);
    
    // Pagination Support
    ValueTask<JsonDocument?> GetNextPageAsync(string accessToken, string nextLink, string? correlationId = null);
    ValueTask<GraphPagedResponse<JsonElement>?> GetGroupsPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphGroupQueryOptions? queryOptions = null, string? correlationId = null);
    
    // Delta Queries
    ValueTask<GraphDeltaResponse<JsonElement>?> GetGroupsDeltaAsync(string accessToken, string? deltaLink = null, GraphGroupQueryOptions? options = null, string? correlationId = null);
    
    // Batch Operations
    ValueTask<BatchResponse[]> BatchGetGroupsAsync(string accessToken, BatchRequest[] requests, string? correlationId = null);
}
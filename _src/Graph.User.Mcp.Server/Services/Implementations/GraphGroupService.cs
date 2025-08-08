using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Graph.User.Mcp.Server.Services.Utilities;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Enhanced Microsoft Graph service for group and team operations
/// </summary>
public class GraphGroupService : GraphServiceBase, IGraphGroupService
{
    public GraphGroupService(HttpClient httpClient, ILogger<GraphGroupService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Groups

    public async ValueTask<JsonDocument?> GetGroupsAsync(string accessToken, int top = 10, string? correlationId = null)
    {
        var options = new GraphGroupQueryOptions
        {
            Top = LimitTopParameter(top)
        };

        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"groups{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroups", correlationId);
    }

    public async ValueTask<JsonDocument?> GetGroupByIdAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupById", correlationId);
    }

    public async ValueTask<JsonDocument?> SearchGroupsAsync(string accessToken, string searchTerm, int top = 10, string? correlationId = null)
    {
        ValidateSearchTerm(searchTerm, nameof(searchTerm));
        
        var options = new GraphGroupQueryOptions
        {
            Top = LimitTopParameter(top),
            Search = SanitizeSearchTerm(searchTerm)
        };

        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"groups{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchGroups", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUserGroupMembershipsAsync(string accessToken, string? userId = null, string? correlationId = null)
    {
        string endpoint;
        if (string.IsNullOrEmpty(userId))
        {
            endpoint = "me/memberOf";
        }
        else
        {
            ValidateGuidFormat(userId, nameof(userId));
            endpoint = $"users/{userId}/memberOf";
        }
        
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUserGroupMemberships", correlationId);
    }

    #endregion

    #region Enhanced Group Operations

    public async ValueTask<JsonDocument?> GetGroupsWithOptionsAsync(string accessToken, GraphGroupQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"groups{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupsWithOptions", correlationId);
    }

    public async ValueTask<JsonDocument?> GetGroupsByTypeAsync(string accessToken, GroupType[] groupTypes, GraphGroupQueryOptions? options = null, string? correlationId = null)
    {
        if (groupTypes == null || groupTypes.Length == 0)
            throw new ArgumentException("Group types cannot be null or empty", nameof(groupTypes));

        var typeOptions = options ?? new GraphGroupQueryOptions();
        typeOptions.GroupTypes = groupTypes;

        var queryString = ODataQueryBuilder.BuildQuery(typeOptions);
        var endpoint = $"groups{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupsByType", correlationId);
    }

    public async ValueTask<JsonDocument?> GetGroupsByVisibilityAsync(string accessToken, GroupVisibility visibility, GraphGroupQueryOptions? options = null, string? correlationId = null)
    {
        var visibilityOptions = options ?? new GraphGroupQueryOptions();
        visibilityOptions.Visibility = visibility;

        var queryString = ODataQueryBuilder.BuildQuery(visibilityOptions);
        var endpoint = $"groups{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupsByVisibility", correlationId);
    }

    #endregion

    #region Group Membership

    public async ValueTask<JsonDocument?> GetGroupMembersAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}/members";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupMembers", correlationId);
    }

    public async ValueTask<JsonDocument?> GetGroupOwnersAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}/owners";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupOwners", correlationId);
    }

    public async ValueTask<JsonDocument?> CheckGroupMembershipAsync(string accessToken, string groupId, string userId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        ValidateGuidFormat(userId, nameof(userId));
        var endpoint = $"groups/{groupId}/members/{userId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "CheckGroupMembership", correlationId);
    }

    #endregion

    #region Group Operations

    public async ValueTask<JsonDocument?> CreateGroupAsync(string accessToken, object groupData, string? correlationId = null)
    {
        const string endpoint = "groups";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateGroup", groupData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateGroupAsync(string accessToken, string groupId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateGroup", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteGroupAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteGroup", correlationId);
    }

    #endregion

    #region Group Conversations

    public async ValueTask<JsonDocument?> GetGroupConversationsAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}/conversations";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupConversations", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateGroupConversationAsync(string accessToken, string groupId, object conversation, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}/conversations";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateGroupConversation", conversation, correlationId);
    }

    public async ValueTask<JsonDocument?> GetGroupConversationThreadsAsync(string accessToken, string groupId, string conversationId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        ValidateGuidFormat(conversationId, nameof(conversationId));
        var endpoint = $"groups/{groupId}/conversations/{conversationId}/threads";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupConversationThreads", correlationId);
    }

    #endregion

    #region Teams

    public async ValueTask<JsonDocument?> GetTeamAsync(string accessToken, string teamId, string? correlationId = null)
    {
        ValidateGuidFormat(teamId, nameof(teamId));
        var endpoint = $"teams/{teamId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTeam", correlationId);
    }

    public async ValueTask<JsonDocument?> GetTeamChannelsAsync(string accessToken, string teamId, string? correlationId = null)
    {
        ValidateGuidFormat(teamId, nameof(teamId));
        var endpoint = $"teams/{teamId}/channels";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTeamChannels", correlationId);
    }

    public async ValueTask<JsonDocument?> GetTeamChannelAsync(string accessToken, string teamId, string channelId, string? correlationId = null)
    {
        ValidateGuidFormat(teamId, nameof(teamId));
        ValidateGuidFormat(channelId, nameof(channelId));
        var endpoint = $"teams/{teamId}/channels/{channelId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetTeamChannel", correlationId);
    }

    public async ValueTask<JsonDocument?> GetChannelMessagesAsync(string accessToken, string teamId, string channelId, int top = 10, string? correlationId = null)
    {
        ValidateGuidFormat(teamId, nameof(teamId));
        ValidateGuidFormat(channelId, nameof(channelId));
        var topParameter = LimitTopParameter(top);
        var endpoint = $"teams/{teamId}/channels/{channelId}/messages?$top={topParameter}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetChannelMessages", correlationId);
    }

    #endregion

    #region Group Files & Sites

    public async ValueTask<JsonDocument?> GetGroupDriveAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}/drive";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupDrive", correlationId);
    }

    public async ValueTask<JsonDocument?> GetGroupSiteAsync(string accessToken, string groupId, string? correlationId = null)
    {
        ValidateGuidFormat(groupId, nameof(groupId));
        var endpoint = $"groups/{groupId}/sites/root";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupSite", correlationId);
    }

    #endregion

    #region Pagination Support

    public async ValueTask<JsonDocument?> GetNextPageAsync(string accessToken, string nextLink, string? correlationId = null)
    {
        if (string.IsNullOrEmpty(nextLink))
            throw new ArgumentException("NextLink cannot be null or empty", nameof(nextLink));

        var queryString = ODataQueryBuilder.ExtractQueryFromNextLink(nextLink);
        var endpoint = ExtractEndpointFromNextLink(nextLink);
        return await ExecuteGraphGetRequestAsync($"{endpoint}{queryString}", accessToken, "GetNextPage", correlationId);
    }

    public async ValueTask<GraphPagedResponse<JsonElement>?> GetGroupsPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphGroupQueryOptions? queryOptions = null, string? correlationId = null)
    {
        // Handle NextLink pagination
        if (!string.IsNullOrEmpty(pagination.NextLink))
        {
            var nextLinkQuery = ODataQueryBuilder.ExtractQueryFromNextLink(pagination.NextLink);
            var endpoint = ExtractEndpointFromNextLink(pagination.NextLink);
            var result = await ExecuteGraphGetRequestAsync($"{endpoint}{nextLinkQuery}", accessToken, "GetGroupsNextPage", correlationId);
            return ParsePagedResponse<JsonElement>(result);
        }

        // Build query with pagination options
        var combinedOptions = CombineGroupQueryWithPagination(queryOptions, pagination);
        var queryString = ODataQueryBuilder.BuildQuery(combinedOptions);
        var groupsEndpoint = $"groups{queryString}";
        
        var response = await ExecuteGraphGetRequestAsync(groupsEndpoint, accessToken, "GetGroupsPaged", correlationId);
        return ParsePagedResponse<JsonElement>(response);
    }

    #endregion

    #region Delta Queries

    public async ValueTask<GraphDeltaResponse<JsonElement>?> GetGroupsDeltaAsync(string accessToken, string? deltaLink = null, GraphGroupQueryOptions? options = null, string? correlationId = null)
    {
        string endpoint;
        
        if (!string.IsNullOrEmpty(deltaLink))
        {
            // Use existing delta link
            var queryString = ODataQueryBuilder.ExtractQueryFromNextLink(deltaLink);
            endpoint = $"groups/delta{queryString}";
        }
        else
        {
            // Start new delta query
            var queryString = ODataQueryBuilder.BuildQuery(options);
            endpoint = $"groups/delta{queryString}";
        }

        var response = await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetGroupsDelta", correlationId);
        return ParseDeltaResponse<JsonElement>(response);
    }

    #endregion

    #region Batch Operations

    public async ValueTask<BatchResponse[]> BatchGetGroupsAsync(string accessToken, BatchRequest[] requests, string? correlationId = null)
    {
        if (requests == null || requests.Length == 0)
            throw new ArgumentException("Batch requests cannot be null or empty", nameof(requests));

        var batchPayload = new
        {
            requests = requests
        };

        const string endpoint = "$batch";
        var response = await ExecuteGraphPostRequestAsync(endpoint, accessToken, "BatchGetGroups", batchPayload, correlationId);
        
        return ParseBatchResponse(response);
    }

    #endregion

    #region Private Helper Methods

    private static GraphGroupQueryOptions CombineGroupQueryWithPagination(GraphGroupQueryOptions? queryOptions, GraphPaginationOptions pagination)
    {
        var combined = queryOptions ?? new GraphGroupQueryOptions();
        
        if (pagination.Top.HasValue)
            combined.Top = pagination.Top.Value;
        
        if (pagination.Skip.HasValue)
            combined.Skip = pagination.Skip.Value;
        
        if (pagination.IncludeCount)
            combined.Count = true;

        return combined;
    }

    private static void ValidateSearchTerm(string searchTerm, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be null or empty", parameterName);
        
        if (searchTerm.Length > 100)
            throw new ArgumentException("Search term cannot exceed 100 characters", parameterName);
    }

    private static string ExtractEndpointFromNextLink(string nextLink)
    {
        try
        {
            var uri = new Uri(nextLink);
            var path = uri.AbsolutePath;
            
            // Remove /v1.0/ prefix if present
            if (path.StartsWith("/v1.0/"))
                return path[6..]; // Remove "/v1.0/"
            
            return path.TrimStart('/');
        }
        catch
        {
            throw new ArgumentException("Invalid nextLink format", nameof(nextLink));
        }
    }

    private static GraphPagedResponse<T>? ParsePagedResponse<T>(JsonDocument? response)
    {
        if (response == null) return null;

        var root = response.RootElement;
        var pagedResponse = new GraphPagedResponse<T>();

        if (root.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.Array)
        {
            pagedResponse.Value = valueElement.EnumerateArray()
                .Cast<T>()
                .ToArray();
        }

        if (root.TryGetProperty("@odata.nextLink", out var nextLinkElement))
        {
            pagedResponse.NextLink = nextLinkElement.GetString();
        }

        if (root.TryGetProperty("@odata.deltaLink", out var deltaLinkElement))
        {
            pagedResponse.DeltaLink = deltaLinkElement.GetString();
        }

        if (root.TryGetProperty("@odata.count", out var countElement))
        {
            pagedResponse.Count = countElement.GetInt32();
        }

        return pagedResponse;
    }

    private static GraphDeltaResponse<T>? ParseDeltaResponse<T>(JsonDocument? response)
    {
        if (response == null) return null;

        var root = response.RootElement;
        var deltaResponse = new GraphDeltaResponse<T>();

        if (root.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.Array)
        {
            var items = new List<T>();
            var deletedItems = new List<T>();

            foreach (var item in valueElement.EnumerateArray())
            {
                if (item.TryGetProperty("@removed", out _))
                {
                    deletedItems.Add((T)(object)item);
                }
                else
                {
                    items.Add((T)(object)item);
                }
            }

            deltaResponse.Value = items.ToArray();
            deltaResponse.Deleted = deletedItems.ToArray();
        }

        if (root.TryGetProperty("@odata.nextLink", out var nextLinkElement))
        {
            deltaResponse.NextLink = nextLinkElement.GetString();
        }

        if (root.TryGetProperty("@odata.deltaLink", out var deltaLinkElement))
        {
            deltaResponse.DeltaLink = deltaLinkElement.GetString();
        }

        return deltaResponse;
    }

    private static BatchResponse[] ParseBatchResponse(JsonDocument? response)
    {
        if (response == null) return Array.Empty<BatchResponse>();

        var root = response.RootElement;
        if (!root.TryGetProperty("responses", out var responsesElement) || 
            responsesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<BatchResponse>();
        }

        var batchResponses = new List<BatchResponse>();
        foreach (var responseElement in responsesElement.EnumerateArray())
        {
            var batchResponse = new BatchResponse();

            if (responseElement.TryGetProperty("id", out var idElement))
                batchResponse.Id = idElement.GetString() ?? string.Empty;

            if (responseElement.TryGetProperty("status", out var statusElement))
                batchResponse.Status = statusElement.GetInt32();

            if (responseElement.TryGetProperty("headers", out var headersElement))
            {
                batchResponse.Headers = new Dictionary<string, string>();
                foreach (var header in headersElement.EnumerateObject())
                {
                    batchResponse.Headers[header.Name] = header.Value.GetString() ?? string.Empty;
                }
            }

            if (responseElement.TryGetProperty("body", out var bodyElement))
                batchResponse.Body = bodyElement;

            batchResponses.Add(batchResponse);
        }

        return batchResponses.ToArray();
    }

    #endregion
}
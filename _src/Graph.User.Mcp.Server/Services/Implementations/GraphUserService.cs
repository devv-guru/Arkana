using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Graph.User.Mcp.Server.Services.Utilities;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Enhanced Microsoft Graph service for user operations with comprehensive query support
/// </summary>
public class GraphUserService : GraphServiceBase, IGraphUserService
{
    public GraphUserService(HttpClient httpClient, ILogger<GraphUserService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Profile & Identity Operations

    public async ValueTask<JsonDocument?> GetCurrentUserAsync(string accessToken, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildUserQuery(options);
        var endpoint = $"me{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCurrentUser", correlationId);
    }

    public async ValueTask<JsonDocument?> GetCurrentUserProfileAsync(string accessToken, GraphQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"me/profile{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCurrentUserProfile", correlationId);
    }

    public async ValueTask<Stream?> GetCurrentUserPhotoAsync(string accessToken, string size = "48x48", string? correlationId = null)
    {
        ValidatePhotoSize(size);
        var endpoint = $"me/photos/{size}/$value";
        return await ExecuteGraphStreamRequestAsync(endpoint, accessToken, "GetCurrentUserPhoto", correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateCurrentUserAsync(string accessToken, object updates, string? correlationId = null)
    {
        const string endpoint = "me";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateCurrentUser", updates, correlationId);
    }

    #endregion

    #region Organizational Structure

    public async ValueTask<JsonDocument?> GetCurrentUserManagerAsync(string accessToken, GraphQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"me/manager{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCurrentUserManager", correlationId);
    }

    public async ValueTask<JsonDocument?> GetCurrentUserDirectReportsAsync(string accessToken, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildUserQuery(options);
        var endpoint = $"me/directReports{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCurrentUserDirectReports", correlationId);
    }

    #endregion

    #region User Directory Operations

    public async ValueTask<JsonDocument?> GetUsersAsync(string accessToken, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildUserQuery(options);
        var endpoint = $"users{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUsers", correlationId);
    }

    public async ValueTask<GraphPagedResponse<JsonElement>?> GetUsersPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphUserQueryOptions? queryOptions = null, string? correlationId = null)
    {
        // Handle NextLink pagination
        if (!string.IsNullOrEmpty(pagination.NextLink))
        {
            var nextLinkQuery = ODataQueryBuilder.ExtractQueryFromNextLink(pagination.NextLink);
            var endpoint = $"users{nextLinkQuery}";
            var result = await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUsersNextPage", correlationId);
            return ParsePagedResponse<JsonElement>(result);
        }

        // Build query with pagination options
        var combinedOptions = CombineUserQueryWithPagination(queryOptions, pagination);
        var queryString = ODataQueryBuilder.BuildUserQuery(combinedOptions);
        var usersEndpoint = $"users{queryString}";
        
        var response = await ExecuteGraphGetRequestAsync(usersEndpoint, accessToken, "GetUsersPaged", correlationId);
        return ParsePagedResponse<JsonElement>(response);
    }

    public async ValueTask<JsonDocument?> SearchUsersAsync(string accessToken, string searchTerm, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        ValidateSearchTerm(searchTerm, nameof(searchTerm));
        
        // Combine search term with existing options
        var searchOptions = options ?? new GraphUserQueryOptions();
        searchOptions.Search = SanitizeSearchTerm(searchTerm);
        
        var queryString = ODataQueryBuilder.BuildUserQuery(searchOptions);
        var endpoint = $"users{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchUsers", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUserByIdAsync(string accessToken, string userId, GraphQueryOptions? options = null, string? correlationId = null)
    {
        ValidateGuidFormat(userId, nameof(userId));
        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"users/{userId}{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUserById", correlationId);
    }

    public async ValueTask<Stream?> GetUserPhotoAsync(string accessToken, string userId, string size = "48x48", string? correlationId = null)
    {
        ValidateGuidFormat(userId, nameof(userId));
        ValidatePhotoSize(size);
        var endpoint = $"users/{userId}/photos/{size}/$value";
        return await ExecuteGraphStreamRequestAsync(endpoint, accessToken, "GetUserPhoto", correlationId);
    }

    #endregion

    #region People & Insights

    public async ValueTask<JsonDocument?> GetRelevantPeopleAsync(string accessToken, GraphQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"me/people{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetRelevantPeople", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUserInsightsAsync(string accessToken, string insightType, GraphQueryOptions? options = null, string? correlationId = null)
    {
        ValidateInsightType(insightType);
        var queryString = ODataQueryBuilder.BuildQuery(options);
        var endpoint = $"me/insights/{insightType}{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUserInsights", correlationId);
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

    #endregion

    #region Delta Queries

    public async ValueTask<GraphDeltaResponse<JsonElement>?> GetUsersDeltaAsync(string accessToken, string? deltaLink = null, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        string endpoint;
        
        if (!string.IsNullOrEmpty(deltaLink))
        {
            // Use existing delta link
            var queryString = ODataQueryBuilder.ExtractQueryFromNextLink(deltaLink);
            endpoint = $"users/delta{queryString}";
        }
        else
        {
            // Start new delta query
            var queryString = ODataQueryBuilder.BuildUserQuery(options);
            endpoint = $"users/delta{queryString}";
        }

        var response = await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUsersDelta", correlationId);
        return ParseDeltaResponse<JsonElement>(response);
    }

    #endregion

    #region Batch Operations

    public async ValueTask<BatchResponse[]> BatchGetUsersAsync(string accessToken, BatchRequest[] requests, string? correlationId = null)
    {
        if (requests == null || requests.Length == 0)
            throw new ArgumentException("Batch requests cannot be null or empty", nameof(requests));

        var batchPayload = new
        {
            requests = requests
        };

        const string endpoint = "$batch";
        var response = await ExecuteGraphPostRequestAsync(endpoint, accessToken, "BatchGetUsers", batchPayload, correlationId);
        
        return ParseBatchResponse(response);
    }

    #endregion

    #region Advanced Filtering

    public async ValueTask<JsonDocument?> GetUsersByDepartmentAsync(string accessToken, string department, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department cannot be null or empty", nameof(department));

        var departmentOptions = options ?? new GraphUserQueryOptions();
        departmentOptions.Department = department;

        var queryString = ODataQueryBuilder.BuildUserQuery(departmentOptions);
        var endpoint = $"users{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUsersByDepartment", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUsersByJobTitleAsync(string accessToken, string jobTitle, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title cannot be null or empty", nameof(jobTitle));

        var jobTitleOptions = options ?? new GraphUserQueryOptions();
        jobTitleOptions.JobTitle = jobTitle;

        var queryString = ODataQueryBuilder.BuildUserQuery(jobTitleOptions);
        var endpoint = $"users{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUsersByJobTitle", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUsersByLocationAsync(string accessToken, string location, GraphUserQueryOptions? options = null, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be null or empty", nameof(location));

        var locationOptions = options ?? new GraphUserQueryOptions();
        locationOptions.OfficeLocation = location;

        var queryString = ODataQueryBuilder.BuildUserQuery(locationOptions);
        var endpoint = $"users{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUsersByLocation", correlationId);
    }

    #endregion

    #region Private Helper Methods

    private static GraphUserQueryOptions CombineUserQueryWithPagination(GraphUserQueryOptions? queryOptions, GraphPaginationOptions pagination)
    {
        var combined = queryOptions ?? new GraphUserQueryOptions();
        
        if (pagination.Top.HasValue)
            combined.Top = pagination.Top.Value;
        
        if (pagination.Skip.HasValue)
            combined.Skip = pagination.Skip.Value;
        
        if (pagination.IncludeCount)
            combined.Count = true;

        return combined;
    }

    private static void ValidatePhotoSize(string size)
    {
        var validSizes = new[] { "48x48", "64x64", "96x96", "120x120", "240x240", "360x360", "432x432", "504x504", "648x648" };
        if (!validSizes.Contains(size, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid photo size. Valid sizes: {string.Join(", ", validSizes)}", nameof(size));
        }
    }

    private static void ValidateInsightType(string insightType)
    {
        var validTypes = new[] { "trending", "used", "shared" };
        if (!validTypes.Contains(insightType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid insight type. Valid types: {string.Join(", ", validTypes)}", nameof(insightType));
        }
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

        if (root.TryGetProperty("@odata.context", out var contextElement))
        {
            pagedResponse.Context = contextElement.GetString();
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
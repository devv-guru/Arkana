using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Graph.User.Mcp.Server.Services.Utilities;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Enhanced Microsoft Graph service for files and OneDrive operations
/// </summary>
public class GraphFilesService : GraphServiceBase, IGraphFilesService
{
    public GraphFilesService(HttpClient httpClient, ILogger<GraphFilesService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Drive Information

    public async ValueTask<JsonDocument?> GetMyDriveAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/drive";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMyDrive", correlationId);
    }

    public async ValueTask<JsonDocument?> GetDriveAsync(string accessToken, string driveId, string? correlationId = null)
    {
        ValidateGuidFormat(driveId, nameof(driveId));
        var endpoint = $"drives/{driveId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDrive", correlationId);
    }

    public async ValueTask<JsonDocument?> GetDrivesAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/drives";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDrives", correlationId);
    }

    #endregion

    #region SharePoint Sites and Document Libraries

    public async ValueTask<JsonDocument?> GetSitesAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "sites?$select=id,name,displayName,webUrl,description,createdDateTime,lastModifiedDateTime";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSites", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSiteAsync(string accessToken, string siteId, string? correlationId = null)
    {
        ValidateNonEmpty(siteId, nameof(siteId));
        var endpoint = $"sites/{siteId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSite", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSiteDriveAsync(string accessToken, string siteId, string? correlationId = null)
    {
        ValidateNonEmpty(siteId, nameof(siteId));
        var endpoint = $"sites/{siteId}/drive";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSiteDrive", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSiteDrivesAsync(string accessToken, string siteId, string? correlationId = null)
    {
        ValidateNonEmpty(siteId, nameof(siteId));
        var endpoint = $"sites/{siteId}/drives";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSiteDrives", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSiteDocumentLibraryAsync(string accessToken, string siteId, string libraryName, string? correlationId = null)
    {
        ValidateNonEmpty(siteId, nameof(siteId));
        ValidateNonEmpty(libraryName, nameof(libraryName));
        var endpoint = $"sites/{siteId}/lists/{libraryName}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSiteDocumentLibrary", correlationId);
    }

    public async ValueTask<JsonDocument?> SearchSitesAsync(string accessToken, string searchTerm, string? correlationId = null)
    {
        ValidateNonEmpty(searchTerm, nameof(searchTerm));
        var encodedSearchTerm = Uri.EscapeDataString(searchTerm);
        var endpoint = $"sites?$search=\"{encodedSearchTerm}\"";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchSites", correlationId);
    }

    #endregion

    #region Files and Folders

    public async ValueTask<JsonDocument?> GetRootFolderChildrenAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/drive/root/children";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetRootFolderChildren", correlationId);
    }

    public async ValueTask<JsonDocument?> GetDriveItemAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDriveItem", correlationId);
    }

    public async ValueTask<JsonDocument?> GetDriveItemChildrenAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/children";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDriveItemChildren", correlationId);
    }

    public async ValueTask<JsonDocument?> SearchDriveItemsAsync(string accessToken, string searchTerm, string? correlationId = null)
    {
        ValidateSearchTerm(searchTerm, nameof(searchTerm));
        var sanitizedTerm = SanitizeSearchTerm(searchTerm);
        var endpoint = $"me/drive/root/search(q='{sanitizedTerm}')";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchDriveItems", correlationId);
    }

    #endregion

    #region Enhanced File Operations

    public async ValueTask<JsonDocument?> GetDriveItemsWithOptionsAsync(string accessToken, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildFileQuery(options);
        var endpoint = $"me/drive/root/children{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDriveItemsWithOptions", correlationId);
    }

    public async ValueTask<JsonDocument?> GetFilesByTypeAsync(string accessToken, string[] fileTypes, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        if (fileTypes == null || fileTypes.Length == 0)
            throw new ArgumentException("File types cannot be null or empty", nameof(fileTypes));

        var typeOptions = options ?? new GraphFileQueryOptions();
        typeOptions.FileTypes = fileTypes;

        var queryString = ODataQueryBuilder.BuildFileQuery(typeOptions);
        var endpoint = $"me/drive/root/children{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetFilesByType", correlationId);
    }

    public async ValueTask<JsonDocument?> GetFilesByDateRangeAsync(string accessToken, DateTime? modifiedAfter, DateTime? modifiedBefore, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        var dateOptions = options ?? new GraphFileQueryOptions();
        dateOptions.ModifiedAfter = modifiedAfter;
        dateOptions.ModifiedBefore = modifiedBefore;

        var queryString = ODataQueryBuilder.BuildFileQuery(dateOptions);
        var endpoint = $"me/drive/root/children{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetFilesByDateRange", correlationId);
    }

    public async ValueTask<JsonDocument?> GetFilesBySizeRangeAsync(string accessToken, long? minSize, long? maxSize, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        var sizeOptions = options ?? new GraphFileQueryOptions();
        sizeOptions.MinSize = minSize;
        sizeOptions.MaxSize = maxSize;

        var queryString = ODataQueryBuilder.BuildFileQuery(sizeOptions);
        var endpoint = $"me/drive/root/children{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetFilesBySizeRange", correlationId);
    }

    #endregion

    #region File Operations

    public async ValueTask<JsonDocument?> CreateFolderAsync(string accessToken, string parentId, object folderData, string? correlationId = null)
    {
        ValidateGuidFormat(parentId, nameof(parentId));
        var endpoint = $"me/drive/items/{parentId}/children";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateFolder", folderData, correlationId);
    }

    public async ValueTask<JsonDocument?> UploadFileAsync(string accessToken, string parentId, string fileName, Stream fileContent, string? correlationId = null)
    {
        ValidateGuidFormat(parentId, nameof(parentId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

        var endpoint = $"me/drive/items/{parentId}:/{fileName}:/content";
        
        // For small files, use simple upload
        if (fileContent.Length <= 4 * 1024 * 1024) // 4MB
        {
            return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "UploadFile", fileContent, correlationId);
        }
        else
        {
            // For large files, would need resumable upload session - simplified for now
            throw new NotSupportedException("Large file uploads require resumable upload sessions. Use UploadSmallFileAsync for files under 4MB.");
        }
    }

    public async ValueTask<JsonDocument?> UploadSmallFileAsync(string accessToken, string itemPath, Stream fileContent, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(itemPath))
            throw new ArgumentException("Item path cannot be null or empty", nameof(itemPath));

        var endpoint = $"me/drive/root:/{itemPath}:/content";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "UploadSmallFile", fileContent, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateDriveItemAsync(string accessToken, string itemId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateDriveItem", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteDriveItemAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteDriveItem", correlationId);
    }

    #endregion

    #region File Content

    public async ValueTask<Stream?> DownloadFileContentAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/content";
        return await ExecuteGraphStreamRequestAsync(endpoint, accessToken, "DownloadFileContent", correlationId);
    }

    public async ValueTask<JsonDocument?> GetFilePreviewAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/preview";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "GetFilePreview", null, correlationId);
    }

    public async ValueTask<JsonDocument?> GetFileThumbnailAsync(string accessToken, string itemId, string size = "medium", string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        ValidateThumbnailSize(size);
        var endpoint = $"me/drive/items/{itemId}/thumbnails/0/{size}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetFileThumbnail", correlationId);
    }

    #endregion

    #region Special Folders

    public async ValueTask<JsonDocument?> GetRecentFilesAsync(string accessToken, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildFileQuery(options);
        var endpoint = $"me/drive/recent{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetRecentFiles", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSharedWithMeAsync(string accessToken, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildFileQuery(options);
        var endpoint = $"me/drive/sharedWithMe{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSharedWithMe", correlationId);
    }

    public async ValueTask<JsonDocument?> GetDeletedItemsAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/drive/special/recycle";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDeletedItems", correlationId);
    }

    #endregion

    #region Sharing

    public async ValueTask<JsonDocument?> CreateSharingLinkAsync(string accessToken, string itemId, object sharingRequest, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/createLink";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateSharingLink", sharingRequest, correlationId);
    }

    public async ValueTask<JsonDocument?> GetItemPermissionsAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/permissions";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetItemPermissions", correlationId);
    }

    public async ValueTask<JsonDocument?> InviteUsersAsync(string accessToken, string itemId, object inviteRequest, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/invite";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "InviteUsers", inviteRequest, correlationId);
    }

    #endregion

    #region Workbook (Excel files)

    public async ValueTask<JsonDocument?> GetWorkbookAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/workbook";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetWorkbook", correlationId);
    }

    public async ValueTask<JsonDocument?> GetWorkbookWorksheetsAsync(string accessToken, string itemId, string? correlationId = null)
    {
        ValidateGuidFormat(itemId, nameof(itemId));
        var endpoint = $"me/drive/items/{itemId}/workbook/worksheets";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetWorkbookWorksheets", correlationId);
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

    public async ValueTask<GraphPagedResponse<JsonElement>?> GetDriveItemsPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphFileQueryOptions? queryOptions = null, string? correlationId = null)
    {
        // Handle NextLink pagination
        if (!string.IsNullOrEmpty(pagination.NextLink))
        {
            var nextLinkQuery = ODataQueryBuilder.ExtractQueryFromNextLink(pagination.NextLink);
            var endpoint = ExtractEndpointFromNextLink(pagination.NextLink);
            var result = await ExecuteGraphGetRequestAsync($"{endpoint}{nextLinkQuery}", accessToken, "GetDriveItemsNextPage", correlationId);
            return ParsePagedResponse<JsonElement>(result);
        }

        // Build query with pagination options
        var combinedOptions = CombineFileQueryWithPagination(queryOptions, pagination);
        var queryString = ODataQueryBuilder.BuildFileQuery(combinedOptions);
        var itemsEndpoint = $"me/drive/root/children{queryString}";
        
        var response = await ExecuteGraphGetRequestAsync(itemsEndpoint, accessToken, "GetDriveItemsPaged", correlationId);
        return ParsePagedResponse<JsonElement>(response);
    }

    #endregion

    #region Delta Queries

    public async ValueTask<GraphDeltaResponse<JsonElement>?> GetDriveItemsDeltaAsync(string accessToken, string? deltaLink = null, GraphFileQueryOptions? options = null, string? correlationId = null)
    {
        string endpoint;
        
        if (!string.IsNullOrEmpty(deltaLink))
        {
            // Use existing delta link
            var queryString = ODataQueryBuilder.ExtractQueryFromNextLink(deltaLink);
            endpoint = $"me/drive/root/delta{queryString}";
        }
        else
        {
            // Start new delta query
            var queryString = ODataQueryBuilder.BuildFileQuery(options);
            endpoint = $"me/drive/root/delta{queryString}";
        }

        var response = await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDriveItemsDelta", correlationId);
        return ParseDeltaResponse<JsonElement>(response);
    }

    #endregion

    #region Batch Operations

    public async ValueTask<BatchResponse[]> BatchGetDriveItemsAsync(string accessToken, BatchRequest[] requests, string? correlationId = null)
    {
        if (requests == null || requests.Length == 0)
            throw new ArgumentException("Batch requests cannot be null or empty", nameof(requests));

        var batchPayload = new
        {
            requests = requests
        };

        const string endpoint = "$batch";
        var response = await ExecuteGraphPostRequestAsync(endpoint, accessToken, "BatchGetDriveItems", batchPayload, correlationId);
        
        return ParseBatchResponse(response);
    }

    #endregion

    #region Private Helper Methods

    private static GraphFileQueryOptions CombineFileQueryWithPagination(GraphFileQueryOptions? queryOptions, GraphPaginationOptions pagination)
    {
        var combined = queryOptions ?? new GraphFileQueryOptions();
        
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

    private static void ValidateThumbnailSize(string size)
    {
        var validSizes = new[] { "small", "medium", "large" };
        if (!validSizes.Contains(size, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid thumbnail size. Valid sizes: {string.Join(", ", validSizes)}", nameof(size));
        }
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
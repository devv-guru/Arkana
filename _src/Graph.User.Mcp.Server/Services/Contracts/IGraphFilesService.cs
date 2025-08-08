using System.Text.Json;
using Graph.User.Mcp.Server.Services.Models;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Enhanced interface for Microsoft Graph files, OneDrive, and SharePoint operations
/// Based on Files.Read, Files.ReadWrite, Files.Read.All, Files.ReadWrite.All, Sites.Read.All permissions
/// </summary>
public interface IGraphFilesService
{
    // Drive Information
    ValueTask<JsonDocument?> GetMyDriveAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetDriveAsync(string accessToken, string driveId, string? correlationId = null);
    ValueTask<JsonDocument?> GetDrivesAsync(string accessToken, string? correlationId = null);
    
    // SharePoint Sites and Document Libraries
    ValueTask<JsonDocument?> GetSitesAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetSiteAsync(string accessToken, string siteId, string? correlationId = null);
    ValueTask<JsonDocument?> GetSiteDriveAsync(string accessToken, string siteId, string? correlationId = null);
    ValueTask<JsonDocument?> GetSiteDrivesAsync(string accessToken, string siteId, string? correlationId = null);
    ValueTask<JsonDocument?> GetSiteDocumentLibraryAsync(string accessToken, string siteId, string libraryName, string? correlationId = null);
    ValueTask<JsonDocument?> SearchSitesAsync(string accessToken, string searchTerm, string? correlationId = null);
    
    // Files and Folders
    ValueTask<JsonDocument?> GetRootFolderChildrenAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetDriveItemAsync(string accessToken, string itemId, string? correlationId = null);
    ValueTask<JsonDocument?> GetDriveItemChildrenAsync(string accessToken, string itemId, string? correlationId = null);
    ValueTask<JsonDocument?> SearchDriveItemsAsync(string accessToken, string searchTerm, string? correlationId = null);
    
    // Enhanced File Operations with Query Options
    ValueTask<JsonDocument?> GetDriveItemsWithOptionsAsync(string accessToken, GraphFileQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetFilesByTypeAsync(string accessToken, string[] fileTypes, GraphFileQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetFilesByDateRangeAsync(string accessToken, DateTime? modifiedAfter, DateTime? modifiedBefore, GraphFileQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetFilesBySizeRangeAsync(string accessToken, long? minSize, long? maxSize, GraphFileQueryOptions? options = null, string? correlationId = null);
    
    // File Operations
    ValueTask<JsonDocument?> CreateFolderAsync(string accessToken, string parentId, object folderData, string? correlationId = null);
    ValueTask<JsonDocument?> UploadFileAsync(string accessToken, string parentId, string fileName, Stream fileContent, string? correlationId = null);
    ValueTask<JsonDocument?> UploadSmallFileAsync(string accessToken, string itemPath, Stream fileContent, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateDriveItemAsync(string accessToken, string itemId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteDriveItemAsync(string accessToken, string itemId, string? correlationId = null);
    
    // File Content
    ValueTask<Stream?> DownloadFileContentAsync(string accessToken, string itemId, string? correlationId = null);
    ValueTask<JsonDocument?> GetFilePreviewAsync(string accessToken, string itemId, string? correlationId = null);
    ValueTask<JsonDocument?> GetFileThumbnailAsync(string accessToken, string itemId, string size = "medium", string? correlationId = null);
    
    // Special Folders
    ValueTask<JsonDocument?> GetRecentFilesAsync(string accessToken, GraphFileQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetSharedWithMeAsync(string accessToken, GraphFileQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetDeletedItemsAsync(string accessToken, string? correlationId = null);
    
    // Sharing
    ValueTask<JsonDocument?> CreateSharingLinkAsync(string accessToken, string itemId, object sharingRequest, string? correlationId = null);
    ValueTask<JsonDocument?> GetItemPermissionsAsync(string accessToken, string itemId, string? correlationId = null);
    ValueTask<JsonDocument?> InviteUsersAsync(string accessToken, string itemId, object inviteRequest, string? correlationId = null);
    
    // Workbook (Excel files)
    ValueTask<JsonDocument?> GetWorkbookAsync(string accessToken, string itemId, string? correlationId = null);
    ValueTask<JsonDocument?> GetWorkbookWorksheetsAsync(string accessToken, string itemId, string? correlationId = null);
    
    // Pagination Support
    ValueTask<JsonDocument?> GetNextPageAsync(string accessToken, string nextLink, string? correlationId = null);
    ValueTask<GraphPagedResponse<JsonElement>?> GetDriveItemsPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphFileQueryOptions? queryOptions = null, string? correlationId = null);
    
    // Delta Queries
    ValueTask<GraphDeltaResponse<JsonElement>?> GetDriveItemsDeltaAsync(string accessToken, string? deltaLink = null, GraphFileQueryOptions? options = null, string? correlationId = null);
    
    // Batch Operations
    ValueTask<BatchResponse[]> BatchGetDriveItemsAsync(string accessToken, BatchRequest[] requests, string? correlationId = null);
}
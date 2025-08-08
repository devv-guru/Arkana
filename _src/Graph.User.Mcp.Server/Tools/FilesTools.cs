using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph OneDrive files-related MCP tools
/// </summary>
[McpServerToolType]
public static class FilesTools
{
    [McpServerTool, Description("Get files from OneDrive root folder")]
    public static async Task<string> GetFiles(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of files to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var options = new GraphFileQueryOptions { Top = top };
            var filesResponse = await filesService.GetDriveItemsWithOptionsAsync(accessToken, options, correlationId);
            
            var result = new
            {
                Success = true,
                Data = filesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get a specific file or folder by ID")]
    public static async Task<string> GetFile(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The file/folder item ID")] string itemId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var fileResponse = await filesService.GetDriveItemAsync(accessToken, itemId, correlationId);
            
            var result = new
            {
                Success = true,
                Item = fileResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search for files in OneDrive")]
    public static async Task<string> SearchFiles(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for file names or content")] string searchTerm,
        [Description("Number of results to return (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var filesResponse = await filesService.SearchDriveItemsAsync(accessToken, searchTerm, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Data = filesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get recently modified files")]
    public static async Task<string> GetRecentFiles(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of recent files to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var options = new GraphFileQueryOptions { Top = top };
            var filesResponse = await filesService.GetRecentFilesAsync(accessToken, options, correlationId);
            
            var result = new
            {
                Success = true,
                Data = filesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new folder in OneDrive")]
    public static async Task<string> CreateFolder(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Folder name")] string folderName,
        [Description("Parent folder ID (optional, defaults to root)")] string? parentId = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var folderData = new
            {
                name = folderName,
                folder = new { }
            };

            var response = await filesService.CreateFolderAsync(accessToken, parentId ?? "root", folderData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Folder created successfully",
                Folder = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get children of a folder")]
    public static async Task<string> GetFolderChildren(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Folder item ID")] string folderId,
        [Description("Number of items to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var childrenResponse = await filesService.GetDriveItemChildrenAsync(accessToken, folderId, correlationId);
            
            var result = new
            {
                Success = true,
                FolderId = folderId,
                Children = childrenResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get SharePoint sites accessible to the user")]
    public static async Task<string> GetSharePointSites(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var sitesResponse = await filesService.GetSitesAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "SharePoint sites retrieved successfully",
                Sites = sitesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search for SharePoint sites")]
    public static async Task<string> SearchSharePointSites(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for site names")] string searchTerm)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var sitesResponse = await filesService.SearchSitesAsync(accessToken, searchTerm, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Sites = sitesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get SharePoint site document libraries")]
    public static async Task<string> GetSiteDocumentLibraries(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("SharePoint site ID")] string siteId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var drivesResponse = await filesService.GetSiteDrivesAsync(accessToken, siteId, correlationId);
            
            var result = new
            {
                Success = true,
                SiteId = siteId,
                DocumentLibraries = drivesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get files from a SharePoint document library")]
    public static async Task<string> GetSharePointFiles(
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("SharePoint site ID")] string siteId,
        [Description("Document library drive ID (optional, uses default site drive if not provided)")] string? driveId = null,
        [Description("Number of files to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            JsonDocument? filesResponse;
            if (!string.IsNullOrEmpty(driveId))
            {
                // Get files from specific document library drive
                filesResponse = await filesService.GetDriveItemChildrenAsync(accessToken, "root", correlationId);
            }
            else
            {
                // Get files from default site drive
                var siteDriveResponse = await filesService.GetSiteDriveAsync(accessToken, siteId, correlationId);
                if (siteDriveResponse != null)
                {
                    filesResponse = await filesService.GetDriveItemChildrenAsync(accessToken, "root", correlationId);
                }
                else
                {
                    filesResponse = null;
                }
            }
            
            var result = new
            {
                Success = true,
                SiteId = siteId,
                DriveId = driveId ?? "default",
                Files = filesResponse?.RootElement
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
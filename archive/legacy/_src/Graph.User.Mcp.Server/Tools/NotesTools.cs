using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph OneNote-related MCP tools
/// </summary>
[McpServerToolType]
public static class NotesTools
{
    [McpServerTool, Description("Get OneNote notebooks")]
    public static async Task<string> GetNotebooks(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var notebooksResponse = await notesService.GetNotebooksAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Notebooks = notebooksResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get sections from a notebook or all sections")]
    public static async Task<string> GetSections(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Notebook ID (optional, gets all sections if not provided)")] string? notebookId = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var sectionsResponse = await notesService.GetSectionsAsync(accessToken, notebookId, correlationId);
            
            var result = new
            {
                Success = true,
                NotebookId = notebookId,
                Sections = sectionsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get pages from a section or all pages")]
    public static async Task<string> GetPages(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Section ID (optional, gets all pages if not provided)")] string? sectionId = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var pagesResponse = await notesService.GetPagesAsync(accessToken, sectionId, correlationId);
            
            var result = new
            {
                Success = true,
                SectionId = sectionId,
                Pages = pagesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get a specific OneNote page")]
    public static async Task<string> GetPage(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The page ID")] string pageId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var pageResponse = await notesService.GetPageAsync(accessToken, pageId, correlationId);
            
            var result = new
            {
                Success = true,
                Page = pageResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get the content of a OneNote page")]
    public static async Task<string> GetPageContent(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The page ID")] string pageId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var content = await notesService.GetPageContentAsync(accessToken, pageId, correlationId);
            
            var result = new
            {
                Success = true,
                PageId = pageId,
                Content = content
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search OneNote pages")]
    public static async Task<string> SearchPages(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for page content or titles")] string searchTerm)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var pagesResponse = await notesService.SearchPagesAsync(accessToken, searchTerm, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Pages = pagesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new OneNote page")]
    public static async Task<string> CreatePage(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The section ID where the page will be created")] string sectionId,
        [Description("HTML content for the page")] string htmlContent)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var pageResponse = await notesService.CreatePageAsync(accessToken, sectionId, htmlContent, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "OneNote page created successfully",
                Page = pageResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new OneNote notebook")]
    public static async Task<string> CreateNotebook(
        IGraphNotesService notesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Notebook display name")] string displayName)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var notebookData = new
            {
                displayName = displayName
            };

            var response = await notesService.CreateNotebookAsync(accessToken, notebookData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "OneNote notebook created successfully",
                Notebook = response?.RootElement
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
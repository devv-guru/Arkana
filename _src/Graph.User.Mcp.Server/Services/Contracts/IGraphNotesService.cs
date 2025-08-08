using System.Text.Json;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Interface for Microsoft Graph OneNote operations
/// Based on Notes.Read, Notes.ReadWrite, Notes.Create permissions
/// </summary>
public interface IGraphNotesService
{
    // Notebooks
    ValueTask<JsonDocument?> GetNotebooksAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetNotebookAsync(string accessToken, string notebookId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateNotebookAsync(string accessToken, object notebookData, string? correlationId = null);
    
    // Sections
    ValueTask<JsonDocument?> GetSectionsAsync(string accessToken, string? notebookId = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetSectionAsync(string accessToken, string sectionId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateSectionAsync(string accessToken, string notebookId, object sectionData, string? correlationId = null);
    ValueTask<JsonDocument?> GetNotebookSectionsAsync(string accessToken, string notebookId, string? correlationId = null);
    
    // Section Groups
    ValueTask<JsonDocument?> GetSectionGroupsAsync(string accessToken, string? notebookId = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetSectionGroupAsync(string accessToken, string sectionGroupId, string? correlationId = null);
    ValueTask<JsonDocument?> GetSectionGroupSectionsAsync(string accessToken, string sectionGroupId, string? correlationId = null);
    
    // Pages
    ValueTask<JsonDocument?> GetPagesAsync(string accessToken, string? sectionId = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetPageAsync(string accessToken, string pageId, string? correlationId = null);
    ValueTask<JsonDocument?> GetSectionPagesAsync(string accessToken, string sectionId, string? correlationId = null);
    ValueTask<JsonDocument?> CreatePageAsync(string accessToken, string sectionId, string htmlContent, string? correlationId = null);
    ValueTask<JsonDocument?> UpdatePageAsync(string accessToken, string pageId, string htmlContent, string? correlationId = null);
    ValueTask<JsonDocument?> DeletePageAsync(string accessToken, string pageId, string? correlationId = null);
    
    // Page Content
    ValueTask<string?> GetPageContentAsync(string accessToken, string pageId, string? correlationId = null);
    ValueTask<JsonDocument?> GetPagePreviewAsync(string accessToken, string pageId, string? correlationId = null);
    
    // Search
    ValueTask<JsonDocument?> SearchPagesAsync(string accessToken, string searchTerm, string? correlationId = null);
    
    // Resources
    ValueTask<JsonDocument?> GetPageResourcesAsync(string accessToken, string pageId, string? correlationId = null);
    ValueTask<Stream?> GetPageResourceAsync(string accessToken, string pageId, string resourceId, string? correlationId = null);
}
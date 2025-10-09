using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Microsoft Graph service for OneNote operations
/// </summary>
public class GraphNotesService : GraphServiceBase, IGraphNotesService
{
    public GraphNotesService(HttpClient httpClient, ILogger<GraphNotesService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Notebooks

    public async ValueTask<JsonDocument?> GetNotebooksAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/onenote/notebooks";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetNotebooks", correlationId);
    }

    public async ValueTask<JsonDocument?> GetNotebookAsync(string accessToken, string notebookId, string? correlationId = null)
    {
        ValidateGuidFormat(notebookId, nameof(notebookId));
        var endpoint = $"me/onenote/notebooks/{notebookId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetNotebook", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateNotebookAsync(string accessToken, object notebookData, string? correlationId = null)
    {
        const string endpoint = "me/onenote/notebooks";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateNotebook", notebookData, correlationId);
    }

    #endregion

    #region Sections

    public async ValueTask<JsonDocument?> GetSectionsAsync(string accessToken, string? notebookId = null, string? correlationId = null)
    {
        string endpoint;
        if (!string.IsNullOrEmpty(notebookId))
        {
            ValidateGuidFormat(notebookId, nameof(notebookId));
            endpoint = $"me/onenote/notebooks/{notebookId}/sections";
        }
        else
        {
            endpoint = "me/onenote/sections";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSections", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSectionAsync(string accessToken, string sectionId, string? correlationId = null)
    {
        ValidateGuidFormat(sectionId, nameof(sectionId));
        var endpoint = $"me/onenote/sections/{sectionId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSection", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateSectionAsync(string accessToken, string notebookId, object sectionData, string? correlationId = null)
    {
        ValidateGuidFormat(notebookId, nameof(notebookId));
        var endpoint = $"me/onenote/notebooks/{notebookId}/sections";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateSection", sectionData, correlationId);
    }

    public async ValueTask<JsonDocument?> GetNotebookSectionsAsync(string accessToken, string notebookId, string? correlationId = null)
    {
        ValidateGuidFormat(notebookId, nameof(notebookId));
        var endpoint = $"me/onenote/notebooks/{notebookId}/sections";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetNotebookSections", correlationId);
    }

    #endregion

    #region Section Groups

    public async ValueTask<JsonDocument?> GetSectionGroupsAsync(string accessToken, string? notebookId = null, string? correlationId = null)
    {
        string endpoint;
        if (!string.IsNullOrEmpty(notebookId))
        {
            ValidateGuidFormat(notebookId, nameof(notebookId));
            endpoint = $"me/onenote/notebooks/{notebookId}/sectionGroups";
        }
        else
        {
            endpoint = "me/onenote/sectionGroups";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSectionGroups", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSectionGroupAsync(string accessToken, string sectionGroupId, string? correlationId = null)
    {
        ValidateGuidFormat(sectionGroupId, nameof(sectionGroupId));
        var endpoint = $"me/onenote/sectionGroups/{sectionGroupId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSectionGroup", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSectionGroupSectionsAsync(string accessToken, string sectionGroupId, string? correlationId = null)
    {
        ValidateGuidFormat(sectionGroupId, nameof(sectionGroupId));
        var endpoint = $"me/onenote/sectionGroups/{sectionGroupId}/sections";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSectionGroupSections", correlationId);
    }

    #endregion

    #region Pages

    public async ValueTask<JsonDocument?> GetPagesAsync(string accessToken, string? sectionId = null, string? correlationId = null)
    {
        string endpoint;
        if (!string.IsNullOrEmpty(sectionId))
        {
            ValidateGuidFormat(sectionId, nameof(sectionId));
            endpoint = $"me/onenote/sections/{sectionId}/pages";
        }
        else
        {
            endpoint = "me/onenote/pages";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPages", correlationId);
    }

    public async ValueTask<JsonDocument?> GetPageAsync(string accessToken, string pageId, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        var endpoint = $"me/onenote/pages/{pageId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPage", correlationId);
    }

    public async ValueTask<JsonDocument?> GetSectionPagesAsync(string accessToken, string sectionId, string? correlationId = null)
    {
        ValidateGuidFormat(sectionId, nameof(sectionId));
        var endpoint = $"me/onenote/sections/{sectionId}/pages";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetSectionPages", correlationId);
    }

    public async ValueTask<JsonDocument?> CreatePageAsync(string accessToken, string sectionId, string htmlContent, string? correlationId = null)
    {
        ValidateGuidFormat(sectionId, nameof(sectionId));
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("HTML content cannot be null or empty", nameof(htmlContent));

        var endpoint = $"me/onenote/sections/{sectionId}/pages";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreatePage", htmlContent, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdatePageAsync(string accessToken, string pageId, string htmlContent, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("HTML content cannot be null or empty", nameof(htmlContent));

        var endpoint = $"me/onenote/pages/{pageId}/content";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdatePage", htmlContent, correlationId);
    }

    public async ValueTask<JsonDocument?> DeletePageAsync(string accessToken, string pageId, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        var endpoint = $"me/onenote/pages/{pageId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeletePage", correlationId);
    }

    #endregion

    #region Page Content

    public async ValueTask<string?> GetPageContentAsync(string accessToken, string pageId, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        var endpoint = $"me/onenote/pages/{pageId}/content";
        
        using var stream = await ExecuteGraphStreamRequestAsync(endpoint, accessToken, "GetPageContent", correlationId);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public async ValueTask<JsonDocument?> GetPagePreviewAsync(string accessToken, string pageId, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        var endpoint = $"me/onenote/pages/{pageId}/preview";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPagePreview", correlationId);
    }

    #endregion

    #region Search

    public async ValueTask<JsonDocument?> SearchPagesAsync(string accessToken, string searchTerm, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));

        var sanitizedTerm = SanitizeSearchTerm(searchTerm);
        var endpoint = $"me/onenote/pages?$search={sanitizedTerm}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchPages", correlationId);
    }

    #endregion

    #region Resources

    public async ValueTask<JsonDocument?> GetPageResourcesAsync(string accessToken, string pageId, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        var endpoint = $"me/onenote/pages/{pageId}/resources";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetPageResources", correlationId);
    }

    public async ValueTask<Stream?> GetPageResourceAsync(string accessToken, string pageId, string resourceId, string? correlationId = null)
    {
        ValidateGuidFormat(pageId, nameof(pageId));
        ValidateGuidFormat(resourceId, nameof(resourceId));
        var endpoint = $"me/onenote/resources/{resourceId}/content";
        return await ExecuteGraphStreamRequestAsync(endpoint, accessToken, "GetPageResource", correlationId);
    }

    #endregion
}
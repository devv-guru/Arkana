using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Graph.User.Mcp.Server.Services.Models;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Microsoft Graph service for contacts operations
/// </summary>
public class GraphContactsService : GraphServiceBase, IGraphContactsService
{
    public GraphContactsService(HttpClient httpClient, ILogger<GraphContactsService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Contact Folders

    public async ValueTask<JsonDocument?> GetContactFoldersAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/contactFolders";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetContactFolders", correlationId);
    }

    public async ValueTask<JsonDocument?> GetContactFolderAsync(string accessToken, string folderId, string? correlationId = null)
    {
        ValidateGuidFormat(folderId, nameof(folderId));
        var endpoint = $"me/contactFolders/{folderId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetContactFolder", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateContactFolderAsync(string accessToken, object folderData, string? correlationId = null)
    {
        const string endpoint = "me/contactFolders";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateContactFolder", folderData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateContactFolderAsync(string accessToken, string folderId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(folderId, nameof(folderId));
        var endpoint = $"me/contactFolders/{folderId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateContactFolder", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteContactFolderAsync(string accessToken, string folderId, string? correlationId = null)
    {
        ValidateGuidFormat(folderId, nameof(folderId));
        var endpoint = $"me/contactFolders/{folderId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteContactFolder", correlationId);
    }

    #endregion

    #region Contacts

    public async ValueTask<JsonDocument?> GetContactsAsync(string accessToken, string? folderId = null, int top = 10, string? correlationId = null)
    {
        var topParameter = LimitTopParameter(top);
        string endpoint;
        
        if (!string.IsNullOrEmpty(folderId))
        {
            ValidateGuidFormat(folderId, nameof(folderId));
            endpoint = $"me/contactFolders/{folderId}/contacts?$top={topParameter}";
        }
        else
        {
            endpoint = $"me/contacts?$top={topParameter}";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetContacts", correlationId);
    }

    public async ValueTask<JsonDocument?> GetContactAsync(string accessToken, string contactId, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetContact", correlationId);
    }

    public async ValueTask<JsonDocument?> SearchContactsAsync(string accessToken, string searchTerm, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));

        var sanitizedTerm = SanitizeSearchTerm(searchTerm);
        var endpoint = $"me/contacts?$search=\"{sanitizedTerm}\"";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchContacts", correlationId);
    }

    #endregion

    #region Contact Operations

    public async ValueTask<JsonDocument?> CreateContactAsync(string accessToken, object contactData, string? folderId = null, string? correlationId = null)
    {
        string endpoint;
        
        if (!string.IsNullOrEmpty(folderId))
        {
            ValidateGuidFormat(folderId, nameof(folderId));
            endpoint = $"me/contactFolders/{folderId}/contacts";
        }
        else
        {
            endpoint = "me/contacts";
        }

        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateContact", contactData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateContactAsync(string accessToken, string contactId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateContact", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteContactAsync(string accessToken, string contactId, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteContact", correlationId);
    }

    #endregion

    #region Contact Photos

    public async ValueTask<JsonDocument?> GetContactPhotoAsync(string accessToken, string contactId, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}/photo";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetContactPhoto", correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateContactPhotoAsync(string accessToken, string contactId, Stream photo, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}/photo/$value";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "UpdateContactPhoto", photo, correlationId);
    }

    #endregion

    #region Contact Extensions

    public async ValueTask<JsonDocument?> GetContactExtensionsAsync(string accessToken, string contactId, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}/extensions";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetContactExtensions", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateContactExtensionAsync(string accessToken, string contactId, object extension, string? correlationId = null)
    {
        ValidateGuidFormat(contactId, nameof(contactId));
        var endpoint = $"me/contacts/{contactId}/extensions";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateContactExtension", extension, correlationId);
    }

    #endregion
}
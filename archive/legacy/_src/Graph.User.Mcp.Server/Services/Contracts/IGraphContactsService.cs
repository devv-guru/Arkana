using System.Text.Json;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Interface for Microsoft Graph contacts operations
/// Based on Contacts.Read, Contacts.ReadWrite permissions
/// </summary>
public interface IGraphContactsService
{
    // Contact Folders
    ValueTask<JsonDocument?> GetContactFoldersAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetContactFolderAsync(string accessToken, string folderId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateContactFolderAsync(string accessToken, object folderData, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateContactFolderAsync(string accessToken, string folderId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteContactFolderAsync(string accessToken, string folderId, string? correlationId = null);
    
    // Contacts
    ValueTask<JsonDocument?> GetContactsAsync(string accessToken, string? folderId = null, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetContactAsync(string accessToken, string contactId, string? correlationId = null);
    ValueTask<JsonDocument?> SearchContactsAsync(string accessToken, string searchTerm, string? correlationId = null);
    
    // Contact Operations
    ValueTask<JsonDocument?> CreateContactAsync(string accessToken, object contactData, string? folderId = null, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateContactAsync(string accessToken, string contactId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteContactAsync(string accessToken, string contactId, string? correlationId = null);
    
    // Contact Photos
    ValueTask<JsonDocument?> GetContactPhotoAsync(string accessToken, string contactId, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateContactPhotoAsync(string accessToken, string contactId, Stream photo, string? correlationId = null);
    
    // Contact Extensions
    ValueTask<JsonDocument?> GetContactExtensionsAsync(string accessToken, string contactId, string? correlationId = null);
    ValueTask<JsonDocument?> CreateContactExtensionAsync(string accessToken, string contactId, object extension, string? correlationId = null);
}
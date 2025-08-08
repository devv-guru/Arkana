using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph contacts-related MCP tools
/// </summary>
[McpServerToolType]
public static class ContactsTools
{
    [McpServerTool, Description("Get contacts from the user's contact list")]
    public static async Task<string> GetContacts(
        IGraphContactsService contactsService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of contacts to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var contactsResponse = await contactsService.GetContactsAsync(accessToken, null, top, correlationId);
            
            var result = new
            {
                Success = true,
                Data = contactsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get a specific contact by ID")]
    public static async Task<string> GetContact(
        IGraphContactsService contactsService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The contact ID")] string contactId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var contactResponse = await contactsService.GetContactAsync(accessToken, contactId, correlationId);
            
            var result = new
            {
                Success = true,
                Contact = contactResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search for contacts")]
    public static async Task<string> SearchContacts(
        IGraphContactsService contactsService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for contact names or email addresses")] string searchTerm)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var contactsResponse = await contactsService.SearchContactsAsync(accessToken, searchTerm, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Data = contactsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get contact folders")]
    public static async Task<string> GetContactFolders(
        IGraphContactsService contactsService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var foldersResponse = await contactsService.GetContactFoldersAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Folders = foldersResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new contact")]
    public static async Task<string> CreateContact(
        IGraphContactsService contactsService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Contact display name")] string displayName,
        [Description("Contact email address (optional)")] string? email = null,
        [Description("Contact phone number (optional)")] string? phoneNumber = null,
        [Description("Contact company name (optional)")] string? companyName = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var contactData = new
            {
                displayName = displayName,
                emailAddresses = string.IsNullOrEmpty(email) ? null : new[]
                {
                    new
                    {
                        address = email,
                        name = displayName
                    }
                },
                businessPhones = string.IsNullOrEmpty(phoneNumber) ? null : new[] { phoneNumber },
                companyName = companyName
            };

            var response = await contactsService.CreateContactAsync(accessToken, contactData, null, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Contact created successfully",
                Contact = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get contacts from a specific folder")]
    public static async Task<string> GetContactsFromFolder(
        IGraphContactsService contactsService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The contact folder ID")] string folderId,
        [Description("Number of contacts to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var contactsResponse = await contactsService.GetContactsAsync(accessToken, folderId, top, correlationId);
            
            var result = new
            {
                Success = true,
                FolderId = folderId,
                Data = contactsResponse?.RootElement
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
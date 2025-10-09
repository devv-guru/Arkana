using System.Text.Json;
using Graph.User.Mcp.Server.Services.Models;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Enhanced interface for Microsoft Graph mail and messaging operations
/// Based on Mail.Read, Mail.ReadWrite, Mail.Send, MailboxSettings.ReadWrite permissions
/// </summary>
public interface IGraphMailService
{
    // Mail Folders
    ValueTask<JsonDocument?> GetMailFoldersAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetMailFolderAsync(string accessToken, string folderId, string? correlationId = null);
    
    // Messages
    ValueTask<JsonDocument?> GetMessagesAsync(string accessToken, string? folderId = null, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetMessageAsync(string accessToken, string messageId, string? correlationId = null);
    ValueTask<JsonDocument?> GetInboxMessagesAsync(string accessToken, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> SearchMessagesAsync(string accessToken, string searchTerm, int top = 10, string? correlationId = null);
    
    // Enhanced Message Operations with Query Options
    ValueTask<JsonDocument?> GetMessagesWithOptionsAsync(string accessToken, GraphMailQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetMessagesByDateRangeAsync(string accessToken, DateTime startDate, DateTime endDate, GraphMailQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetUnreadMessagesAsync(string accessToken, GraphMailQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetMessagesWithAttachmentsAsync(string accessToken, GraphMailQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetMessagesByImportanceAsync(string accessToken, MailImportance importance, GraphMailQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetMessagesBySenderAsync(string accessToken, string senderEmail, GraphMailQueryOptions? options = null, string? correlationId = null);
    
    // Message Operations
    ValueTask<JsonDocument?> CreateDraftMessageAsync(string accessToken, object message, string? correlationId = null);
    ValueTask<JsonDocument?> SendMailAsync(string accessToken, object message, string? correlationId = null);
    ValueTask<JsonDocument?> ReplyToMessageAsync(string accessToken, string messageId, object replyMessage, string? correlationId = null);
    ValueTask<JsonDocument?> ForwardMessageAsync(string accessToken, string messageId, object forwardMessage, string? correlationId = null);
    ValueTask<JsonDocument?> MarkMessageAsReadAsync(string accessToken, string messageId, bool isRead = true, string? correlationId = null);
    ValueTask<JsonDocument?> MoveMessageAsync(string accessToken, string messageId, string destinationFolderId, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteMessageAsync(string accessToken, string messageId, string? correlationId = null);
    
    // Mailbox Settings
    ValueTask<JsonDocument?> GetMailboxSettingsAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateMailboxSettingsAsync(string accessToken, object settings, string? correlationId = null);
    
    // Message Attachments
    ValueTask<JsonDocument?> GetMessageAttachmentsAsync(string accessToken, string messageId, string? correlationId = null);
    ValueTask<JsonDocument?> GetMessageAttachmentAsync(string accessToken, string messageId, string attachmentId, string? correlationId = null);
    ValueTask<Stream?> DownloadAttachmentAsync(string accessToken, string messageId, string attachmentId, string? correlationId = null);
    ValueTask<JsonDocument?> AddAttachmentAsync(string accessToken, string messageId, object attachment, string? correlationId = null);
    
    // Pagination Support
    ValueTask<JsonDocument?> GetNextPageAsync(string accessToken, string nextLink, string? correlationId = null);
    ValueTask<GraphPagedResponse<JsonElement>?> GetMessagesPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphMailQueryOptions? queryOptions = null, string? correlationId = null);
    
    // Delta Queries
    ValueTask<GraphDeltaResponse<JsonElement>?> GetMessagesDeltaAsync(string accessToken, string? deltaLink = null, GraphMailQueryOptions? options = null, string? correlationId = null);
    
    // Batch Operations
    ValueTask<BatchResponse[]> BatchGetMessagesAsync(string accessToken, BatchRequest[] requests, string? correlationId = null);
}
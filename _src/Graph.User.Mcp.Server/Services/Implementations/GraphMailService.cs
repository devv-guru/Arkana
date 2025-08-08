using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Graph.User.Mcp.Server.Services.Utilities;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Enhanced Microsoft Graph service for mail operations with comprehensive query support
/// </summary>
public class GraphMailService : GraphServiceBase, IGraphMailService
{
    public GraphMailService(HttpClient httpClient, ILogger<GraphMailService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Mail Folders

    public async ValueTask<JsonDocument?> GetMailFoldersAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/mailFolders";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMailFolders", correlationId);
    }

    public async ValueTask<JsonDocument?> GetMailFolderAsync(string accessToken, string folderId, string? correlationId = null)
    {
        ValidateGuidFormat(folderId, nameof(folderId));
        var endpoint = $"me/mailFolders/{folderId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMailFolder", correlationId);
    }

    #endregion

    #region Messages

    public async ValueTask<JsonDocument?> GetMessagesAsync(string accessToken, string? folderId = null, int top = 10, string? correlationId = null)
    {
        var options = new GraphMailQueryOptions
        {
            Top = LimitTopParameter(top),
            FolderId = folderId
        };

        return await GetMessagesWithOptionsAsync(accessToken, options, correlationId);
    }

    public async ValueTask<JsonDocument?> GetMessageAsync(string accessToken, string messageId, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessage", correlationId);
    }

    public async ValueTask<JsonDocument?> GetInboxMessagesAsync(string accessToken, int top = 10, string? correlationId = null)
    {
        var options = new GraphMailQueryOptions
        {
            Top = LimitTopParameter(top),
            FolderId = "inbox"
        };

        var queryString = ODataQueryBuilder.BuildMailQuery(options);
        var endpoint = $"me/mailFolders/inbox/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetInboxMessages", correlationId);
    }

    public async ValueTask<JsonDocument?> SearchMessagesAsync(string accessToken, string searchTerm, int top = 10, string? correlationId = null)
    {
        ValidateSearchTerm(searchTerm, nameof(searchTerm));

        var options = new GraphMailQueryOptions
        {
            Top = LimitTopParameter(top),
            Search = SanitizeSearchTerm(searchTerm)
        };

        var queryString = ODataQueryBuilder.BuildMailQuery(options);
        var endpoint = $"me/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchMessages", correlationId);
    }

    #endregion

    #region Enhanced Message Operations with Query Options

    public async ValueTask<JsonDocument?> GetMessagesWithOptionsAsync(string accessToken, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildMailQuery(options);
        
        string endpoint;
        if (!string.IsNullOrEmpty(options?.FolderId))
        {
            endpoint = $"me/mailFolders/{options.FolderId}/messages{queryString}";
        }
        else
        {
            endpoint = $"me/messages{queryString}";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessagesWithOptions", correlationId);
    }

    public async ValueTask<JsonDocument?> GetMessagesByDateRangeAsync(string accessToken, DateTime startDate, DateTime endDate, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        var dateRangeOptions = options ?? new GraphMailQueryOptions();
        dateRangeOptions.StartDate = startDate;
        dateRangeOptions.EndDate = endDate;

        var queryString = ODataQueryBuilder.BuildMailQuery(dateRangeOptions);
        var endpoint = $"me/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessagesByDateRange", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUnreadMessagesAsync(string accessToken, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        var unreadOptions = options ?? new GraphMailQueryOptions();
        unreadOptions.IsRead = false;

        var queryString = ODataQueryBuilder.BuildMailQuery(unreadOptions);
        var endpoint = $"me/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUnreadMessages", correlationId);
    }

    public async ValueTask<JsonDocument?> GetMessagesWithAttachmentsAsync(string accessToken, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        var attachmentOptions = options ?? new GraphMailQueryOptions();
        attachmentOptions.HasAttachments = true;

        var queryString = ODataQueryBuilder.BuildMailQuery(attachmentOptions);
        var endpoint = $"me/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessagesWithAttachments", correlationId);
    }

    public async ValueTask<JsonDocument?> GetMessagesByImportanceAsync(string accessToken, MailImportance importance, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        var importanceOptions = options ?? new GraphMailQueryOptions();
        importanceOptions.Importance = importance;

        var queryString = ODataQueryBuilder.BuildMailQuery(importanceOptions);
        var endpoint = $"me/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessagesByImportance", correlationId);
    }

    public async ValueTask<JsonDocument?> GetMessagesBySenderAsync(string accessToken, string senderEmail, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        ValidateEmailAddress(senderEmail, nameof(senderEmail));

        var senderOptions = options ?? new GraphMailQueryOptions();
        senderOptions.FromEmail = senderEmail;

        var queryString = ODataQueryBuilder.BuildMailQuery(senderOptions);
        var endpoint = $"me/messages{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessagesBySender", correlationId);
    }

    #endregion

    #region Message Operations

    public async ValueTask<JsonDocument?> CreateDraftMessageAsync(string accessToken, object message, string? correlationId = null)
    {
        const string endpoint = "me/messages";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateDraftMessage", message, correlationId);
    }

    public async ValueTask<JsonDocument?> SendMailAsync(string accessToken, object message, string? correlationId = null)
    {
        const string endpoint = "me/sendMail";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "SendMail", message, correlationId);
    }

    public async ValueTask<JsonDocument?> ReplyToMessageAsync(string accessToken, string messageId, object replyMessage, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}/reply";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "ReplyToMessage", replyMessage, correlationId);
    }

    public async ValueTask<JsonDocument?> ForwardMessageAsync(string accessToken, string messageId, object forwardMessage, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}/forward";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "ForwardMessage", forwardMessage, correlationId);
    }

    public async ValueTask<JsonDocument?> MarkMessageAsReadAsync(string accessToken, string messageId, bool isRead = true, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}";
        var update = new { isRead };
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "MarkMessageAsRead", update, correlationId);
    }

    public async ValueTask<JsonDocument?> MoveMessageAsync(string accessToken, string messageId, string destinationFolderId, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        ValidateGuidFormat(destinationFolderId, nameof(destinationFolderId));
        
        var endpoint = $"me/messages/{messageId}/move";
        var moveRequest = new { destinationId = destinationFolderId };
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "MoveMessage", moveRequest, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteMessageAsync(string accessToken, string messageId, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteMessage", correlationId);
    }

    #endregion

    #region Mailbox Settings

    public async ValueTask<JsonDocument?> GetMailboxSettingsAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/mailboxSettings";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMailboxSettings", correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateMailboxSettingsAsync(string accessToken, object settings, string? correlationId = null)
    {
        const string endpoint = "me/mailboxSettings";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateMailboxSettings", settings, correlationId);
    }

    #endregion

    #region Message Attachments

    public async ValueTask<JsonDocument?> GetMessageAttachmentsAsync(string accessToken, string messageId, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}/attachments";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessageAttachments", correlationId);
    }

    public async ValueTask<JsonDocument?> GetMessageAttachmentAsync(string accessToken, string messageId, string attachmentId, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        ValidateGuidFormat(attachmentId, nameof(attachmentId));
        var endpoint = $"me/messages/{messageId}/attachments/{attachmentId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessageAttachment", correlationId);
    }

    public async ValueTask<Stream?> DownloadAttachmentAsync(string accessToken, string messageId, string attachmentId, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        ValidateGuidFormat(attachmentId, nameof(attachmentId));
        var endpoint = $"me/messages/{messageId}/attachments/{attachmentId}/$value";
        return await ExecuteGraphStreamRequestAsync(endpoint, accessToken, "DownloadAttachment", correlationId);
    }

    public async ValueTask<JsonDocument?> AddAttachmentAsync(string accessToken, string messageId, object attachment, string? correlationId = null)
    {
        ValidateGuidFormat(messageId, nameof(messageId));
        var endpoint = $"me/messages/{messageId}/attachments";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "AddAttachment", attachment, correlationId);
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

    public async ValueTask<GraphPagedResponse<JsonElement>?> GetMessagesPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphMailQueryOptions? queryOptions = null, string? correlationId = null)
    {
        // Handle NextLink pagination
        if (!string.IsNullOrEmpty(pagination.NextLink))
        {
            var nextLinkQuery = ODataQueryBuilder.ExtractQueryFromNextLink(pagination.NextLink);
            var endpoint = ExtractEndpointFromNextLink(pagination.NextLink);
            var result = await ExecuteGraphGetRequestAsync($"{endpoint}{nextLinkQuery}", accessToken, "GetMessagesNextPage", correlationId);
            return ParsePagedResponse<JsonElement>(result);
        }

        // Build query with pagination options
        var combinedOptions = CombineMailQueryWithPagination(queryOptions, pagination);
        var queryString = ODataQueryBuilder.BuildMailQuery(combinedOptions);
        var messagesEndpoint = $"me/messages{queryString}";
        
        var response = await ExecuteGraphGetRequestAsync(messagesEndpoint, accessToken, "GetMessagesPaged", correlationId);
        return ParsePagedResponse<JsonElement>(response);
    }

    #endregion

    #region Delta Queries

    public async ValueTask<GraphDeltaResponse<JsonElement>?> GetMessagesDeltaAsync(string accessToken, string? deltaLink = null, GraphMailQueryOptions? options = null, string? correlationId = null)
    {
        string endpoint;
        
        if (!string.IsNullOrEmpty(deltaLink))
        {
            // Use existing delta link
            var queryString = ODataQueryBuilder.ExtractQueryFromNextLink(deltaLink);
            endpoint = $"me/messages/delta{queryString}";
        }
        else
        {
            // Start new delta query
            var queryString = ODataQueryBuilder.BuildMailQuery(options);
            endpoint = $"me/messages/delta{queryString}";
        }

        var response = await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessagesDelta", correlationId);
        return ParseDeltaResponse<JsonElement>(response);
    }

    #endregion

    #region Batch Operations

    public async ValueTask<BatchResponse[]> BatchGetMessagesAsync(string accessToken, BatchRequest[] requests, string? correlationId = null)
    {
        if (requests == null || requests.Length == 0)
            throw new ArgumentException("Batch requests cannot be null or empty", nameof(requests));

        var batchPayload = new
        {
            requests = requests
        };

        const string endpoint = "$batch";
        var response = await ExecuteGraphPostRequestAsync(endpoint, accessToken, "BatchGetMessages", batchPayload, correlationId);
        
        return ParseBatchResponse(response);
    }

    #endregion

    #region Private Helper Methods

    private static GraphMailQueryOptions CombineMailQueryWithPagination(GraphMailQueryOptions? queryOptions, GraphPaginationOptions pagination)
    {
        var combined = queryOptions ?? new GraphMailQueryOptions();
        
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

    private static void ValidateEmailAddress(string email, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address cannot be null or empty", parameterName);

        if (!email.Contains('@') || email.Length > 254)
            throw new ArgumentException("Invalid email address format", parameterName);
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
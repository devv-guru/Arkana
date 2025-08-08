using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Microsoft Graph service for presence and communication operations
/// </summary>
public class GraphPresenceService : GraphServiceBase, IGraphPresenceService
{
    public GraphPresenceService(HttpClient httpClient, ILogger<GraphPresenceService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Presence

    public async ValueTask<JsonDocument?> GetMyPresenceAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/presence";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMyPresence", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUserPresenceAsync(string accessToken, string userId, string? correlationId = null)
    {
        ValidateGuidFormat(userId, nameof(userId));
        var endpoint = $"users/{userId}/presence";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetUserPresence", correlationId);
    }

    public async ValueTask<JsonDocument?> GetUsersPresenceAsync(string accessToken, string[] userIds, string? correlationId = null)
    {
        if (userIds == null || userIds.Length == 0)
            throw new ArgumentException("User IDs cannot be null or empty", nameof(userIds));

        foreach (var userId in userIds)
        {
            ValidateGuidFormat(userId, nameof(userIds));
        }

        var presenceRequest = new { ids = userIds };
        const string endpoint = "communications/getPresencesByUserId";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "GetUsersPresence", presenceRequest, correlationId);
    }

    public async ValueTask<JsonDocument?> SetPresenceAsync(string accessToken, object presenceData, string? correlationId = null)
    {
        const string endpoint = "me/presence/setPresence";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "SetPresence", presenceData, correlationId);
    }

    public async ValueTask<JsonDocument?> ClearPresenceAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/presence/clearPresence";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "ClearPresence", null, correlationId);
    }

    #endregion

    #region Chats

    public async ValueTask<JsonDocument?> GetChatsAsync(string accessToken, int top = 10, string? correlationId = null)
    {
        var topParameter = LimitTopParameter(top);
        var endpoint = $"me/chats?$top={topParameter}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetChats", correlationId);
    }

    public async ValueTask<JsonDocument?> GetChatAsync(string accessToken, string chatId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));

        var endpoint = $"me/chats/{chatId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetChat", correlationId);
    }

    public async ValueTask<JsonDocument?> GetChatMembersAsync(string accessToken, string chatId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));

        var endpoint = $"me/chats/{chatId}/members";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetChatMembers", correlationId);
    }

    #endregion

    #region Chat Messages

    public async ValueTask<JsonDocument?> GetChatMessagesAsync(string accessToken, string chatId, int top = 10, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));

        var topParameter = LimitTopParameter(top);
        var endpoint = $"me/chats/{chatId}/messages?$top={topParameter}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetChatMessages", correlationId);
    }

    public async ValueTask<JsonDocument?> GetChatMessageAsync(string accessToken, string chatId, string messageId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

        var endpoint = $"me/chats/{chatId}/messages/{messageId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetChatMessage", correlationId);
    }

    public async ValueTask<JsonDocument?> SendChatMessageAsync(string accessToken, string chatId, object message, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));

        var endpoint = $"me/chats/{chatId}/messages";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "SendChatMessage", message, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateChatMessageAsync(string accessToken, string chatId, string messageId, object updates, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

        var endpoint = $"me/chats/{chatId}/messages/{messageId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateChatMessage", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteChatMessageAsync(string accessToken, string chatId, string messageId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

        var endpoint = $"me/chats/{chatId}/messages/{messageId}/softDelete";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "DeleteChatMessage", null, correlationId);
    }

    #endregion

    #region Message Reactions

    public async ValueTask<JsonDocument?> GetMessageReactionsAsync(string accessToken, string chatId, string messageId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

        var endpoint = $"me/chats/{chatId}/messages/{messageId}/reactions";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetMessageReactions", correlationId);
    }

    public async ValueTask<JsonDocument?> AddMessageReactionAsync(string accessToken, string chatId, string messageId, object reaction, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

        var endpoint = $"me/chats/{chatId}/messages/{messageId}/setReaction";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "AddMessageReaction", reaction, correlationId);
    }

    public async ValueTask<JsonDocument?> RemoveMessageReactionAsync(string accessToken, string chatId, string messageId, string reactionId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));
        if (string.IsNullOrWhiteSpace(reactionId))
            throw new ArgumentException("Reaction ID cannot be null or empty", nameof(reactionId));

        var endpoint = $"me/chats/{chatId}/messages/{messageId}/unsetReaction";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "RemoveMessageReaction", new { reactionType = reactionId }, correlationId);
    }

    #endregion

    #region Online Meetings

    public async ValueTask<JsonDocument?> GetOnlineMeetingsAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/onlineMeetings";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetOnlineMeetings", correlationId);
    }

    public async ValueTask<JsonDocument?> CreateOnlineMeetingAsync(string accessToken, object meetingData, string? correlationId = null)
    {
        const string endpoint = "me/onlineMeetings";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateOnlineMeeting", meetingData, correlationId);
    }

    public async ValueTask<JsonDocument?> GetOnlineMeetingAsync(string accessToken, string meetingId, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(meetingId))
            throw new ArgumentException("Meeting ID cannot be null or empty", nameof(meetingId));

        var endpoint = $"me/onlineMeetings/{meetingId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetOnlineMeeting", correlationId);
    }

    #endregion
}
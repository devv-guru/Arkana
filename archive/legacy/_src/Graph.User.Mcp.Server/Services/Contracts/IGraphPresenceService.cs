using System.Text.Json;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Interface for Microsoft Graph presence and communication operations
/// Based on Presence.Read, Presence.ReadWrite, Chat.Read, ChatMessage.Read permissions
/// </summary>
public interface IGraphPresenceService
{
    // Presence
    ValueTask<JsonDocument?> GetMyPresenceAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetUserPresenceAsync(string accessToken, string userId, string? correlationId = null);
    ValueTask<JsonDocument?> GetUsersPresenceAsync(string accessToken, string[] userIds, string? correlationId = null);
    ValueTask<JsonDocument?> SetPresenceAsync(string accessToken, object presenceData, string? correlationId = null);
    ValueTask<JsonDocument?> ClearPresenceAsync(string accessToken, string? correlationId = null);
    
    // Chats
    ValueTask<JsonDocument?> GetChatsAsync(string accessToken, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetChatAsync(string accessToken, string chatId, string? correlationId = null);
    ValueTask<JsonDocument?> GetChatMembersAsync(string accessToken, string chatId, string? correlationId = null);
    
    // Chat Messages
    ValueTask<JsonDocument?> GetChatMessagesAsync(string accessToken, string chatId, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetChatMessageAsync(string accessToken, string chatId, string messageId, string? correlationId = null);
    ValueTask<JsonDocument?> SendChatMessageAsync(string accessToken, string chatId, object message, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateChatMessageAsync(string accessToken, string chatId, string messageId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteChatMessageAsync(string accessToken, string chatId, string messageId, string? correlationId = null);
    
    // Message Reactions
    ValueTask<JsonDocument?> GetMessageReactionsAsync(string accessToken, string chatId, string messageId, string? correlationId = null);
    ValueTask<JsonDocument?> AddMessageReactionAsync(string accessToken, string chatId, string messageId, object reaction, string? correlationId = null);
    ValueTask<JsonDocument?> RemoveMessageReactionAsync(string accessToken, string chatId, string messageId, string reactionId, string? correlationId = null);
    
    // Online Meetings (if accessible)
    ValueTask<JsonDocument?> GetOnlineMeetingsAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> CreateOnlineMeetingAsync(string accessToken, object meetingData, string? correlationId = null);
    ValueTask<JsonDocument?> GetOnlineMeetingAsync(string accessToken, string meetingId, string? correlationId = null);
}
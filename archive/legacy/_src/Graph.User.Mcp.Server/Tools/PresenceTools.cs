using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph Teams presence and communication-related MCP tools
/// </summary>
[McpServerToolType]
public static class PresenceTools
{
    [McpServerTool, Description("Get current user's presence status")]
    public static async Task<string> GetMyPresence(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var presenceResponse = await presenceService.GetMyPresenceAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Presence = presenceResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get presence status of a specific user")]
    public static async Task<string> GetUserPresence(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor,
        [Description("User ID to get presence for")] string userId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var presenceResponse = await presenceService.GetUserPresenceAsync(accessToken, userId, correlationId);
            
            var result = new
            {
                Success = true,
                UserId = userId,
                Presence = presenceResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get Teams chats")]
    public static async Task<string> GetChats(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of chats to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var chatsResponse = await presenceService.GetChatsAsync(accessToken, top, correlationId);
            
            var result = new
            {
                Success = true,
                Data = chatsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get messages from a specific Teams chat")]
    public static async Task<string> GetChatMessages(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The chat ID")] string chatId,
        [Description("Number of messages to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var messagesResponse = await presenceService.GetChatMessagesAsync(accessToken, chatId, top, correlationId);
            
            var result = new
            {
                Success = true,
                ChatId = chatId,
                Messages = messagesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Send a message to a Teams chat")]
    public static async Task<string> SendChatMessage(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The chat ID")] string chatId,
        [Description("Message content")] string message)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var messageData = new
            {
                body = new
                {
                    contentType = "text",
                    content = message
                }
            };

            var response = await presenceService.SendChatMessageAsync(accessToken, chatId, messageData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Chat message sent successfully",
                ChatId = chatId,
                Response = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Set current user's presence status")]
    public static async Task<string> SetPresence(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Availability status (Available, Busy, DoNotDisturb, BeRightBack, Away)")] string availability,
        [Description("Activity status (Available, Away, BeRightBack, Busy, DoNotDisturb, InACall, InAConferenceCall, Inactive, InAMeeting, Offline, OffWork, OutOfOffice, PresenceUnknown, Presenting, UrgentInterruptionsOnly)")] string activity,
        [Description("Custom status message (optional)")] string? statusMessage = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var presenceData = new
            {
                availability = availability,
                activity = activity,
                statusMessage = string.IsNullOrEmpty(statusMessage) ? null : statusMessage
            };

            var response = await presenceService.SetPresenceAsync(accessToken, presenceData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Presence status updated successfully",
                Availability = availability,
                Activity = activity,
                StatusMessage = statusMessage
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get online meetings")]
    public static async Task<string> GetOnlineMeetings(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var meetingsResponse = await presenceService.GetOnlineMeetingsAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Meetings = meetingsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create an online meeting")]
    public static async Task<string> CreateOnlineMeeting(
        IGraphPresenceService presenceService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Meeting subject")] string subject,
        [Description("Meeting start time (ISO 8601 format: YYYY-MM-DDTHH:MM:SS)")] string startTime,
        [Description("Meeting end time (ISO 8601 format: YYYY-MM-DDTHH:MM:SS)")] string endTime)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            var meetingData = new
            {
                subject = subject,
                startDateTime = startTime,
                endDateTime = endTime
            };

            var response = await presenceService.CreateOnlineMeetingAsync(accessToken, meetingData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Online meeting created successfully",
                Meeting = response?.RootElement
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
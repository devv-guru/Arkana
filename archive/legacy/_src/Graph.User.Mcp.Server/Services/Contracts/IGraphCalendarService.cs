using System.Text.Json;
using Graph.User.Mcp.Server.Services.Models;

namespace Graph.User.Mcp.Server.Services.Contracts;

/// <summary>
/// Enhanced interface for Microsoft Graph calendar and events operations
/// Based on Calendars.Read, Calendars.ReadWrite, Calendars.Read.Shared permissions
/// </summary>
public interface IGraphCalendarService
{
    // Calendars
    ValueTask<JsonDocument?> GetDefaultCalendarAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetCalendarsAsync(string accessToken, string? correlationId = null);
    ValueTask<JsonDocument?> GetCalendarAsync(string accessToken, string calendarId, string? correlationId = null);
    ValueTask<JsonDocument?> GetCalendarGroupsAsync(string accessToken, string? correlationId = null);
    
    // Events
    ValueTask<JsonDocument?> GetEventsAsync(string accessToken, string? calendarId = null, int top = 10, string? correlationId = null);
    ValueTask<JsonDocument?> GetEventAsync(string accessToken, string eventId, string? correlationId = null);
    ValueTask<JsonDocument?> GetCalendarViewAsync(string accessToken, DateTime startTime, DateTime endTime, string? calendarId = null, string? correlationId = null);
    ValueTask<JsonDocument?> SearchEventsAsync(string accessToken, string searchTerm, string? correlationId = null);
    
    // Enhanced Event Operations
    ValueTask<JsonDocument?> GetEventsWithOptionsAsync(string accessToken, GraphCalendarQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetEventsByDateRangeAsync(string accessToken, DateTime startTime, DateTime endTime, GraphCalendarQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetTodaysEventsAsync(string accessToken, string? timeZone = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetUpcomingEventsAsync(string accessToken, int daysAhead = 7, GraphCalendarQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetEventsBySensitivityAsync(string accessToken, EventSensitivity sensitivity, GraphCalendarQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetAllDayEventsAsync(string accessToken, DateTime? startDate = null, DateTime? endDate = null, string? correlationId = null);
    
    // Event Operations
    ValueTask<JsonDocument?> CreateEventAsync(string accessToken, object eventData, string? calendarId = null, string? correlationId = null);
    ValueTask<JsonDocument?> UpdateEventAsync(string accessToken, string eventId, object updates, string? correlationId = null);
    ValueTask<JsonDocument?> DeleteEventAsync(string accessToken, string eventId, string? correlationId = null);
    
    // Meeting Operations
    ValueTask<JsonDocument?> FindMeetingTimesAsync(string accessToken, object findMeetingRequest, string? correlationId = null);
    ValueTask<JsonDocument?> AcceptEventAsync(string accessToken, string eventId, object response, string? correlationId = null);
    ValueTask<JsonDocument?> DeclineEventAsync(string accessToken, string eventId, object response, string? correlationId = null);
    ValueTask<JsonDocument?> TentativelyAcceptEventAsync(string accessToken, string eventId, object response, string? correlationId = null);
    ValueTask<JsonDocument?> GetFreeBusyAsync(string accessToken, string[] emails, DateTime startTime, DateTime endTime, string? timeZone = null, string? correlationId = null);
    
    // Recurring Events
    ValueTask<JsonDocument?> GetEventInstancesAsync(string accessToken, string eventId, DateTime startTime, DateTime endTime, string? correlationId = null);
    ValueTask<JsonDocument?> GetRecurringEventsAsync(string accessToken, GraphCalendarQueryOptions? options = null, string? correlationId = null);
    
    // Enhanced Calendar Views
    ValueTask<JsonDocument?> GetCalendarViewWithOptionsAsync(string accessToken, DateTime startTime, DateTime endTime, GraphCalendarQueryOptions? options = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetWeekViewAsync(string accessToken, DateTime weekStartDate, string? timeZone = null, string? correlationId = null);
    ValueTask<JsonDocument?> GetMonthViewAsync(string accessToken, int year, int month, string? timeZone = null, string? correlationId = null);
    
    // Pagination Support
    ValueTask<JsonDocument?> GetNextPageAsync(string accessToken, string nextLink, string? correlationId = null);
    ValueTask<GraphPagedResponse<JsonElement>?> GetEventsPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphCalendarQueryOptions? queryOptions = null, string? correlationId = null);
}
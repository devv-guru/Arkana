using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using Graph.User.Mcp.Server.Configuration;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Graph.User.Mcp.Server.Services.Utilities;

namespace Graph.User.Mcp.Server.Services.Implementations;

/// <summary>
/// Enhanced Microsoft Graph service for calendar operations with comprehensive query support
/// </summary>
public class GraphCalendarService : GraphServiceBase, IGraphCalendarService
{
    public GraphCalendarService(HttpClient httpClient, ILogger<GraphCalendarService> logger, IOptions<McpConfiguration> configuration)
        : base(httpClient, logger, configuration)
    {
    }

    #region Calendars

    public async ValueTask<JsonDocument?> GetDefaultCalendarAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/calendar";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetDefaultCalendar", correlationId);
    }

    public async ValueTask<JsonDocument?> GetCalendarsAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/calendars";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCalendars", correlationId);
    }

    public async ValueTask<JsonDocument?> GetCalendarAsync(string accessToken, string calendarId, string? correlationId = null)
    {
        ValidateGuidFormat(calendarId, nameof(calendarId));
        var endpoint = $"me/calendars/{calendarId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCalendar", correlationId);
    }

    public async ValueTask<JsonDocument?> GetCalendarGroupsAsync(string accessToken, string? correlationId = null)
    {
        const string endpoint = "me/calendarGroups";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCalendarGroups", correlationId);
    }

    #endregion

    #region Events

    public async ValueTask<JsonDocument?> GetEventsAsync(string accessToken, string? calendarId = null, int top = 10, string? correlationId = null)
    {
        var options = new GraphCalendarQueryOptions
        {
            Top = LimitTopParameter(top),
            CalendarId = calendarId
        };

        var queryString = ODataQueryBuilder.BuildCalendarQuery(options);
        
        string endpoint;
        if (!string.IsNullOrEmpty(calendarId))
        {
            endpoint = $"me/calendars/{calendarId}/events{queryString}";
        }
        else
        {
            endpoint = $"me/events{queryString}";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetEvents", correlationId);
    }

    public async ValueTask<JsonDocument?> GetEventAsync(string accessToken, string eventId, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));
        var endpoint = $"me/events/{eventId}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetEvent", correlationId);
    }

    public async ValueTask<JsonDocument?> GetCalendarViewAsync(string accessToken, DateTime startTime, DateTime endTime, string? calendarId = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildCalendarViewQuery(startTime, endTime);
        
        string endpoint;
        if (!string.IsNullOrEmpty(calendarId))
        {
            endpoint = $"me/calendars/{calendarId}/calendarView{queryString}";
        }
        else
        {
            endpoint = $"me/calendarView{queryString}";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCalendarView", correlationId);
    }

    public async ValueTask<JsonDocument?> SearchEventsAsync(string accessToken, string searchTerm, string? correlationId = null)
    {
        ValidateSearchTerm(searchTerm, nameof(searchTerm));

        var options = new GraphCalendarQueryOptions
        {
            Search = SanitizeSearchTerm(searchTerm)
        };

        var queryString = ODataQueryBuilder.BuildCalendarQuery(options);
        var endpoint = $"me/events{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "SearchEvents", correlationId);
    }

    #endregion

    #region Enhanced Event Operations

    public async ValueTask<JsonDocument?> GetEventsWithOptionsAsync(string accessToken, GraphCalendarQueryOptions? options = null, string? correlationId = null)
    {
        var queryString = ODataQueryBuilder.BuildCalendarQuery(options);
        
        string endpoint;
        if (!string.IsNullOrEmpty(options?.CalendarId))
        {
            endpoint = $"me/calendars/{options.CalendarId}/events{queryString}";
        }
        else
        {
            endpoint = $"me/events{queryString}";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetEventsWithOptions", correlationId);
    }

    public async ValueTask<JsonDocument?> GetEventsByDateRangeAsync(string accessToken, DateTime startTime, DateTime endTime, GraphCalendarQueryOptions? options = null, string? correlationId = null)
    {
        var dateRangeOptions = options ?? new GraphCalendarQueryOptions();
        dateRangeOptions.StartTime = startTime;
        dateRangeOptions.EndTime = endTime;

        var queryString = ODataQueryBuilder.BuildCalendarQuery(dateRangeOptions);
        var endpoint = $"me/events{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetEventsByDateRange", correlationId);
    }

    public async ValueTask<JsonDocument?> GetTodaysEventsAsync(string accessToken, string? timeZone = null, string? correlationId = null)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var options = new GraphCalendarQueryOptions
        {
            StartTime = today,
            EndTime = tomorrow,
            TimeZone = timeZone
        };

        return await GetCalendarViewWithOptionsAsync(accessToken, today, tomorrow, options, correlationId);
    }

    public async ValueTask<JsonDocument?> GetUpcomingEventsAsync(string accessToken, int daysAhead = 7, GraphCalendarQueryOptions? options = null, string? correlationId = null)
    {
        var startTime = DateTime.Now;
        var endTime = startTime.AddDays(daysAhead);

        var upcomingOptions = options ?? new GraphCalendarQueryOptions();
        upcomingOptions.StartTime = startTime;
        upcomingOptions.EndTime = endTime;

        return await GetCalendarViewWithOptionsAsync(accessToken, startTime, endTime, upcomingOptions, correlationId);
    }

    public async ValueTask<JsonDocument?> GetEventsBySensitivityAsync(string accessToken, EventSensitivity sensitivity, GraphCalendarQueryOptions? options = null, string? correlationId = null)
    {
        var sensitivityOptions = options ?? new GraphCalendarQueryOptions();
        sensitivityOptions.Sensitivity = sensitivity;

        var queryString = ODataQueryBuilder.BuildCalendarQuery(sensitivityOptions);
        var endpoint = $"me/events{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetEventsBySensitivity", correlationId);
    }

    public async ValueTask<JsonDocument?> GetAllDayEventsAsync(string accessToken, DateTime? startDate = null, DateTime? endDate = null, string? correlationId = null)
    {
        var options = new GraphCalendarQueryOptions
        {
            IsAllDay = true,
            StartTime = startDate,
            EndTime = endDate
        };

        var queryString = ODataQueryBuilder.BuildCalendarQuery(options);
        var endpoint = $"me/events{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetAllDayEvents", correlationId);
    }

    #endregion

    #region Event Operations

    public async ValueTask<JsonDocument?> CreateEventAsync(string accessToken, object eventData, string? calendarId = null, string? correlationId = null)
    {
        string endpoint;
        if (!string.IsNullOrEmpty(calendarId))
        {
            ValidateGuidFormat(calendarId, nameof(calendarId));
            endpoint = $"me/calendars/{calendarId}/events";
        }
        else
        {
            endpoint = "me/events";
        }

        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "CreateEvent", eventData, correlationId);
    }

    public async ValueTask<JsonDocument?> UpdateEventAsync(string accessToken, string eventId, object updates, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));
        var endpoint = $"me/events/{eventId}";
        return await ExecuteGraphPatchRequestAsync(endpoint, accessToken, "UpdateEvent", updates, correlationId);
    }

    public async ValueTask<JsonDocument?> DeleteEventAsync(string accessToken, string eventId, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));
        var endpoint = $"me/events/{eventId}";
        return await ExecuteGraphDeleteRequestAsync(endpoint, accessToken, "DeleteEvent", correlationId);
    }

    #endregion

    #region Meeting Operations

    public async ValueTask<JsonDocument?> FindMeetingTimesAsync(string accessToken, object findMeetingRequest, string? correlationId = null)
    {
        const string endpoint = "me/calendar/getSchedule";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "FindMeetingTimes", findMeetingRequest, correlationId);
    }

    public async ValueTask<JsonDocument?> AcceptEventAsync(string accessToken, string eventId, object response, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));
        var endpoint = $"me/events/{eventId}/accept";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "AcceptEvent", response, correlationId);
    }

    public async ValueTask<JsonDocument?> DeclineEventAsync(string accessToken, string eventId, object response, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));
        var endpoint = $"me/events/{eventId}/decline";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "DeclineEvent", response, correlationId);
    }

    public async ValueTask<JsonDocument?> TentativelyAcceptEventAsync(string accessToken, string eventId, object response, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));
        var endpoint = $"me/events/{eventId}/tentativelyAccept";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "TentativelyAcceptEvent", response, correlationId);
    }

    public async ValueTask<JsonDocument?> GetFreeBusyAsync(string accessToken, string[] emails, DateTime startTime, DateTime endTime, string? timeZone = null, string? correlationId = null)
    {
        ValidateEmailAddresses(emails);

        var freeBusyRequest = new
        {
            schedules = emails,
            startTime = new
            {
                dateTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                timeZone = timeZone ?? "UTC"
            },
            endTime = new
            {
                dateTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                timeZone = timeZone ?? "UTC"
            },
            availabilityViewInterval = 15
        };

        const string endpoint = "me/calendar/getSchedule";
        return await ExecuteGraphPostRequestAsync(endpoint, accessToken, "GetFreeBusy", freeBusyRequest, correlationId);
    }

    #endregion

    #region Recurring Events

    public async ValueTask<JsonDocument?> GetEventInstancesAsync(string accessToken, string eventId, DateTime startTime, DateTime endTime, string? correlationId = null)
    {
        ValidateGuidFormat(eventId, nameof(eventId));

        var queryString = ODataQueryBuilder.BuildCalendarViewQuery(startTime, endTime);
        var endpoint = $"me/events/{eventId}/instances{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetEventInstances", correlationId);
    }

    public async ValueTask<JsonDocument?> GetRecurringEventsAsync(string accessToken, GraphCalendarQueryOptions? options = null, string? correlationId = null)
    {
        var recurringOptions = options ?? new GraphCalendarQueryOptions();
        recurringOptions.IncludeRecurring = true;

        var queryString = ODataQueryBuilder.BuildCalendarQuery(recurringOptions);
        var endpoint = $"me/events{queryString}";
        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetRecurringEvents", correlationId);
    }

    #endregion

    #region Enhanced Calendar Views

    public async ValueTask<JsonDocument?> GetCalendarViewWithOptionsAsync(string accessToken, DateTime startTime, DateTime endTime, GraphCalendarQueryOptions? options = null, string? correlationId = null)
    {
        var calendarOptions = options ?? new GraphCalendarQueryOptions();
        calendarOptions.StartTime = startTime;
        calendarOptions.EndTime = endTime;

        var queryString = ODataQueryBuilder.BuildCalendarViewQuery(startTime, endTime, calendarOptions);
        
        string endpoint;
        if (!string.IsNullOrEmpty(options?.CalendarId))
        {
            endpoint = $"me/calendars/{options.CalendarId}/calendarView{queryString}";
        }
        else
        {
            endpoint = $"me/calendarView{queryString}";
        }

        return await ExecuteGraphGetRequestAsync(endpoint, accessToken, "GetCalendarViewWithOptions", correlationId);
    }

    public async ValueTask<JsonDocument?> GetWeekViewAsync(string accessToken, DateTime weekStartDate, string? timeZone = null, string? correlationId = null)
    {
        var weekStart = weekStartDate.Date;
        var weekEnd = weekStart.AddDays(7);

        var options = new GraphCalendarQueryOptions
        {
            TimeZone = timeZone
        };

        return await GetCalendarViewWithOptionsAsync(accessToken, weekStart, weekEnd, options, correlationId);
    }

    public async ValueTask<JsonDocument?> GetMonthViewAsync(string accessToken, int year, int month, string? timeZone = null, string? correlationId = null)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var options = new GraphCalendarQueryOptions
        {
            TimeZone = timeZone
        };

        return await GetCalendarViewWithOptionsAsync(accessToken, monthStart, monthEnd, options, correlationId);
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

    public async ValueTask<GraphPagedResponse<JsonElement>?> GetEventsPagedAsync(string accessToken, GraphPaginationOptions pagination, GraphCalendarQueryOptions? queryOptions = null, string? correlationId = null)
    {
        // Handle NextLink pagination
        if (!string.IsNullOrEmpty(pagination.NextLink))
        {
            var nextLinkQuery = ODataQueryBuilder.ExtractQueryFromNextLink(pagination.NextLink);
            var endpoint = ExtractEndpointFromNextLink(pagination.NextLink);
            var result = await ExecuteGraphGetRequestAsync($"{endpoint}{nextLinkQuery}", accessToken, "GetEventsNextPage", correlationId);
            return ParsePagedResponse<JsonElement>(result);
        }

        // Build query with pagination options
        var combinedOptions = CombineCalendarQueryWithPagination(queryOptions, pagination);
        var queryString = ODataQueryBuilder.BuildCalendarQuery(combinedOptions);
        var eventsEndpoint = $"me/events{queryString}";
        
        var response = await ExecuteGraphGetRequestAsync(eventsEndpoint, accessToken, "GetEventsPaged", correlationId);
        return ParsePagedResponse<JsonElement>(response);
    }

    #endregion

    #region Private Helper Methods

    private static GraphCalendarQueryOptions CombineCalendarQueryWithPagination(GraphCalendarQueryOptions? queryOptions, GraphPaginationOptions pagination)
    {
        var combined = queryOptions ?? new GraphCalendarQueryOptions();
        
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

    private static void ValidateEmailAddresses(string[] emails)
    {
        if (emails == null || emails.Length == 0)
            throw new ArgumentException("Email addresses cannot be null or empty", nameof(emails));

        foreach (var email in emails)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length > 254)
                throw new ArgumentException($"Invalid email address: {email}", nameof(emails));
        }
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

    #endregion
}
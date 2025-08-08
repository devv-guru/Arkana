using System.Text.Json;

namespace Graph.User.Mcp.Server.Services.Models;

/// <summary>
/// Base response structure for paginated Graph API results
/// </summary>
/// <typeparam name="T">Type of the data items</typeparam>
public class GraphPagedResponse<T>
{
    /// <summary>
    /// Array of result items
    /// </summary>
    public T[] Value { get; set; } = Array.Empty<T>();

    /// <summary>
    /// URL for the next page of results
    /// </summary>
    public string? NextLink { get; set; }

    /// <summary>
    /// URL for delta changes (incremental sync)
    /// </summary>
    public string? DeltaLink { get; set; }

    /// <summary>
    /// Total count of items (when $count=true)
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Additional OData context information
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// Response structure for delta queries with deleted items
/// </summary>
/// <typeparam name="T">Type of the data items</typeparam>
public class GraphDeltaResponse<T> : GraphPagedResponse<T>
{
    /// <summary>
    /// Array of deleted items (tombstones)
    /// </summary>
    public T[] Deleted { get; set; } = Array.Empty<T>();
}

/// <summary>
/// Batch request item for Graph API batch operations
/// </summary>
public class BatchRequest
{
    /// <summary>
    /// Unique identifier for the request within the batch
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// HTTP method (GET, POST, PATCH, DELETE)
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Relative URL for the request (without /v1.0)
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Additional headers for the request
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Request body for POST/PATCH operations
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Array of request IDs this request depends on
    /// </summary>
    public string[]? DependsOn { get; set; }
}

/// <summary>
/// Response item from a batch operation
/// </summary>
public class BatchResponse
{
    /// <summary>
    /// Request ID that corresponds to the batch request
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Response headers
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Response body as JSON element
    /// </summary>
    public JsonElement? Body { get; set; }

    /// <summary>
    /// Error information if the request failed
    /// </summary>
    public GraphError? Error { get; set; }
}

/// <summary>
/// Graph API error information
/// </summary>
public class GraphError
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details
    /// </summary>
    public GraphErrorDetails? Details { get; set; }

    /// <summary>
    /// Inner error information
    /// </summary>
    public GraphInnerError? InnerError { get; set; }
}

/// <summary>
/// Additional error details
/// </summary>
public class GraphErrorDetails
{
    /// <summary>
    /// Target of the error
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Error code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Inner error information for debugging
/// </summary>
public class GraphInnerError
{
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Timestamp of the error
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Additional inner error details
    /// </summary>
    public GraphInnerError? InnerError { get; set; }
}

/// <summary>
/// Response for free/busy calendar queries
/// </summary>
public class FreeBusyResponse
{
    /// <summary>
    /// Email address of the user
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Availability status
    /// </summary>
    public AvailabilityStatus AvailabilityStatus { get; set; }

    /// <summary>
    /// Free/busy time periods
    /// </summary>
    public FreeBusyTime[] FreeBusyViewType { get; set; } = Array.Empty<FreeBusyTime>();

    /// <summary>
    /// Working hours information
    /// </summary>
    public WorkingHours? WorkingHours { get; set; }
}

/// <summary>
/// Availability status for users
/// </summary>
public enum AvailabilityStatus
{
    Free,
    Tentative,
    Busy,
    Oof,
    WorkingElsewhere,
    Unknown
}

/// <summary>
/// Free/busy time period
/// </summary>
public class FreeBusyTime
{
    /// <summary>
    /// Start time of the period
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// End time of the period
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Status during this period
    /// </summary>
    public AvailabilityStatus Status { get; set; }
}

/// <summary>
/// Working hours information
/// </summary>
public class WorkingHours
{
    /// <summary>
    /// Days of the week for working hours
    /// </summary>
    public DayOfWeek[] DaysOfWeek { get; set; } = Array.Empty<DayOfWeek>();

    /// <summary>
    /// Start time of working hours
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of working hours
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Time zone for working hours
    /// </summary>
    public string TimeZone { get; set; } = string.Empty;
}

/// <summary>
/// Response for finding meeting times
/// </summary>
public class MeetingTimeSuggestionsResponse
{
    /// <summary>
    /// Array of meeting time suggestions
    /// </summary>
    public MeetingTimeSuggestion[] MeetingTimeSuggestions { get; set; } = Array.Empty<MeetingTimeSuggestion>();

    /// <summary>
    /// Reason if no suggestions were found
    /// </summary>
    public string? EmptyTimeSuggestionsReason { get; set; }
}

/// <summary>
/// Meeting time suggestion
/// </summary>
public class MeetingTimeSuggestion
{
    /// <summary>
    /// Confidence level of the suggestion
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Organizer availability
    /// </summary>
    public AvailabilityStatus OrganizerAvailability { get; set; }

    /// <summary>
    /// Suggested meeting time
    /// </summary>
    public MeetingTimeSlot? SuggestionReason { get; set; }

    /// <summary>
    /// Meeting time slot
    /// </summary>
    public MeetingTimeSlot? MeetingTimeSlot { get; set; }

    /// <summary>
    /// Attendee availability information
    /// </summary>
    public AttendeeAvailability[] AttendeeAvailability { get; set; } = Array.Empty<AttendeeAvailability>();
}

/// <summary>
/// Meeting time slot information
/// </summary>
public class MeetingTimeSlot
{
    /// <summary>
    /// Start time of the meeting
    /// </summary>
    public DateTimeOffset Start { get; set; }

    /// <summary>
    /// End time of the meeting
    /// </summary>
    public DateTimeOffset End { get; set; }
}

/// <summary>
/// Attendee availability information
/// </summary>
public class AttendeeAvailability
{
    /// <summary>
    /// Attendee information
    /// </summary>
    public Attendee? Attendee { get; set; }

    /// <summary>
    /// Availability status
    /// </summary>
    public AvailabilityStatus Availability { get; set; }
}

/// <summary>
/// Attendee information
/// </summary>
public class Attendee
{
    /// <summary>
    /// Email address
    /// </summary>
    public EmailAddress? EmailAddress { get; set; }
}

/// <summary>
/// Email address information
/// </summary>
public class EmailAddress
{
    /// <summary>
    /// Email address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// File download information
/// </summary>
public class FileDownloadInfo
{
    /// <summary>
    /// Direct download URL
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// File content stream
    /// </summary>
    public Stream? ContentStream { get; set; }

    /// <summary>
    /// Content type/MIME type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// File name
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? Size { get; set; }
}
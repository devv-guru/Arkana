using System.Text.Json;

namespace Graph.User.Mcp.Server.Services.Models;

/// <summary>
/// Base class for Microsoft Graph OData query parameters
/// </summary>
public class GraphQueryOptions
{
    /// <summary>
    /// Select specific properties ($select)
    /// </summary>
    public string[]? Select { get; set; }

    /// <summary>
    /// Filter results ($filter)
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Order by specific properties ($orderby)
    /// </summary>
    public string[]? OrderBy { get; set; }

    /// <summary>
    /// Expand related entities ($expand)
    /// </summary>
    public string[]? Expand { get; set; }

    /// <summary>
    /// Maximum number of results to return ($top)
    /// </summary>
    public int? Top { get; set; }

    /// <summary>
    /// Number of results to skip ($skip)
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Include count of total results ($count)
    /// </summary>
    public bool Count { get; set; }

    /// <summary>
    /// Full-text search query ($search)
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Consistency level for the request
    /// </summary>
    public ConsistencyLevel? ConsistencyLevel { get; set; }
}

/// <summary>
/// Consistency levels for Graph API requests
/// </summary>
public enum ConsistencyLevel
{
    Eventual,
    Session
}

/// <summary>
/// Pagination options for Graph API requests
/// </summary>
public class GraphPaginationOptions
{
    /// <summary>
    /// Maximum number of results per page
    /// </summary>
    public int? Top { get; set; }

    /// <summary>
    /// Number of results to skip (for skip-based pagination)
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Next link URL for continuation (for NextLink pagination)
    /// </summary>
    public string? NextLink { get; set; }

    /// <summary>
    /// Delta link URL for incremental changes
    /// </summary>
    public string? DeltaLink { get; set; }

    /// <summary>
    /// Include total count in response
    /// </summary>
    public bool IncludeCount { get; set; }
}

/// <summary>
/// Mail-specific query options
/// </summary>
public class GraphMailQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Filter messages received after this date
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter messages received before this date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by message importance
    /// </summary>
    public MailImportance? Importance { get; set; }

    /// <summary>
    /// Filter by read status
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Filter messages with attachments
    /// </summary>
    public bool? HasAttachments { get; set; }

    /// <summary>
    /// Specific folder ID to query
    /// </summary>
    public string? FolderId { get; set; }

    /// <summary>
    /// Filter by sender email address
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// Filter by subject contains
    /// </summary>
    public string? SubjectContains { get; set; }

    /// <summary>
    /// Include only messages with specific categories
    /// </summary>
    public string[]? Categories { get; set; }
}

/// <summary>
/// Mail importance levels
/// </summary>
public enum MailImportance
{
    Low,
    Normal,
    High
}

/// <summary>
/// Calendar-specific query options
/// </summary>
public class GraphCalendarQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Start time for event filtering
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// End time for event filtering
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Time zone for the request (e.g., "Pacific Standard Time")
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Filter by event types
    /// </summary>
    public EventType[]? EventTypes { get; set; }

    /// <summary>
    /// Filter by show as status
    /// </summary>
    public EventShowAs[]? ShowAs { get; set; }

    /// <summary>
    /// Filter all-day events
    /// </summary>
    public bool? IsAllDay { get; set; }

    /// <summary>
    /// Include recurring event instances
    /// </summary>
    public bool? IncludeRecurring { get; set; }

    /// <summary>
    /// Filter by sensitivity level
    /// </summary>
    public EventSensitivity? Sensitivity { get; set; }

    /// <summary>
    /// Specific calendar ID to query
    /// </summary>
    public string? CalendarId { get; set; }
}

/// <summary>
/// Event types for calendar filtering
/// </summary>
public enum EventType
{
    SingleInstance,
    Occurrence,
    Exception,
    SeriesMaster
}

/// <summary>
/// Event show as status
/// </summary>
public enum EventShowAs
{
    Free,
    Tentative,
    Busy,
    Oof,
    WorkingElsewhere,
    Unknown
}

/// <summary>
/// Event sensitivity levels
/// </summary>
public enum EventSensitivity
{
    Normal,
    Personal,
    Private,
    Confidential
}

/// <summary>
/// File-specific query options
/// </summary>
public class GraphFileQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Filter by file extensions (e.g., ["pdf", "docx"])
    /// </summary>
    public string[]? FileTypes { get; set; }

    /// <summary>
    /// Filter files modified after this date
    /// </summary>
    public DateTime? ModifiedAfter { get; set; }

    /// <summary>
    /// Filter files modified before this date
    /// </summary>
    public DateTime? ModifiedBefore { get; set; }

    /// <summary>
    /// Minimum file size in bytes
    /// </summary>
    public long? MinSize { get; set; }

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long? MaxSize { get; set; }

    /// <summary>
    /// Include only files shared with the user
    /// </summary>
    public bool? SharedWithMe { get; set; }

    /// <summary>
    /// Include only files owned by the user
    /// </summary>
    public bool? OwnedByMe { get; set; }

    /// <summary>
    /// Specific drive ID to query
    /// </summary>
    public string? DriveId { get; set; }

    /// <summary>
    /// Include deleted items
    /// </summary>
    public bool? IncludeDeleted { get; set; }
}

/// <summary>
/// User-specific query options
/// </summary>
public class GraphUserQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Filter by department
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Filter by job title
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Filter by country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Filter by account enabled status
    /// </summary>
    public bool? AccountEnabled { get; set; }

    /// <summary>
    /// Filter by user type
    /// </summary>
    public UserType? UserType { get; set; }

    /// <summary>
    /// Filter by office location
    /// </summary>
    public string? OfficeLocation { get; set; }

    /// <summary>
    /// Include only users with photos
    /// </summary>
    public bool? HasPhoto { get; set; }
}

/// <summary>
/// User types for filtering
/// </summary>
public enum UserType
{
    Member,
    Guest
}

/// <summary>
/// Group-specific query options
/// </summary>
public class GraphGroupQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Filter by group type
    /// </summary>
    public GroupType[]? GroupTypes { get; set; }

    /// <summary>
    /// Filter by visibility
    /// </summary>
    public GroupVisibility? Visibility { get; set; }

    /// <summary>
    /// Filter by mail enabled status
    /// </summary>
    public bool? MailEnabled { get; set; }

    /// <summary>
    /// Filter by security enabled status
    /// </summary>
    public bool? SecurityEnabled { get; set; }

    /// <summary>
    /// Include only groups the user is a member of
    /// </summary>
    public bool? MembershipOnly { get; set; }

    /// <summary>
    /// Include only groups the user owns
    /// </summary>
    public bool? OwnershipOnly { get; set; }
}

/// <summary>
/// Group types for filtering
/// </summary>
public enum GroupType
{
    Unified,
    Security,
    Distribution,
    DynamicMembership
}

/// <summary>
/// Group visibility levels
/// </summary>
public enum GroupVisibility
{
    Public,
    Private,
    HiddenMembership
}

/// <summary>
/// Contact-specific query options
/// </summary>
public class GraphContactQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Specific contact folder ID
    /// </summary>
    public string? FolderId { get; set; }

    /// <summary>
    /// Filter by company name
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Filter contacts with email addresses
    /// </summary>
    public bool? HasEmailAddress { get; set; }

    /// <summary>
    /// Filter contacts with phone numbers
    /// </summary>
    public bool? HasPhoneNumber { get; set; }
}

/// <summary>
/// Task-specific query options
/// </summary>
public class GraphTaskQueryOptions : GraphQueryOptions
{
    /// <summary>
    /// Filter by task status
    /// </summary>
    public TaskStatus? Status { get; set; }

    /// <summary>
    /// Filter tasks due after this date
    /// </summary>
    public DateTime? DueAfter { get; set; }

    /// <summary>
    /// Filter tasks due before this date
    /// </summary>
    public DateTime? DueBefore { get; set; }

    /// <summary>
    /// Filter by importance
    /// </summary>
    public TaskImportance? Importance { get; set; }

    /// <summary>
    /// Specific list ID for To Do tasks
    /// </summary>
    public string? ListId { get; set; }

    /// <summary>
    /// Include completed tasks
    /// </summary>
    public bool? IncludeCompleted { get; set; }
}

/// <summary>
/// Task status for filtering
/// </summary>
public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    WaitingOnOthers,
    Deferred
}

/// <summary>
/// Task importance levels
/// </summary>
public enum TaskImportance
{
    Low,
    Normal,
    High
}
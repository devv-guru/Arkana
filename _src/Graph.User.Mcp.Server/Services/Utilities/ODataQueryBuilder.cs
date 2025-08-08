using System.Globalization;
using System.Text;
using System.Web;
using Graph.User.Mcp.Server.Services.Models;

namespace Graph.User.Mcp.Server.Services.Utilities;

/// <summary>
/// Utility class for building OData query strings for Microsoft Graph API
/// </summary>
public static class ODataQueryBuilder
{
    /// <summary>
    /// Build OData query string from GraphQueryOptions
    /// </summary>
    /// <param name="options">Query options</param>
    /// <returns>OData query string (including leading ? if parameters exist)</returns>
    public static string BuildQuery(GraphQueryOptions? options)
    {
        if (options == null) return string.Empty;

        var queryParts = new List<string>();

        // $select
        if (options.Select?.Length > 0)
        {
            var selectFields = string.Join(",", options.Select.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrEmpty(selectFields))
                queryParts.Add($"$select={selectFields}");
        }

        // $filter
        if (!string.IsNullOrEmpty(options.Filter))
        {
            queryParts.Add($"$filter={HttpUtility.UrlEncode(options.Filter)}");
        }

        // $orderby
        if (options.OrderBy?.Length > 0)
        {
            var orderByFields = string.Join(",", options.OrderBy.Where(o => !string.IsNullOrWhiteSpace(o)));
            if (!string.IsNullOrEmpty(orderByFields))
                queryParts.Add($"$orderby={orderByFields}");
        }

        // $expand
        if (options.Expand?.Length > 0)
        {
            var expandFields = string.Join(",", options.Expand.Where(e => !string.IsNullOrWhiteSpace(e)));
            if (!string.IsNullOrEmpty(expandFields))
                queryParts.Add($"$expand={expandFields}");
        }

        // $top
        if (options.Top.HasValue && options.Top.Value > 0)
        {
            var topValue = Math.Min(options.Top.Value, 1000); // Graph API limit
            queryParts.Add($"$top={topValue}");
        }

        // $skip
        if (options.Skip.HasValue && options.Skip.Value > 0)
        {
            queryParts.Add($"$skip={options.Skip.Value}");
        }

        // $count
        if (options.Count)
        {
            queryParts.Add("$count=true");
        }

        // $search
        if (!string.IsNullOrEmpty(options.Search))
        {
            var searchQuery = $"\"{HttpUtility.UrlEncode(options.Search.Trim())}\"";
            queryParts.Add($"$search={searchQuery}");
        }

        // Consistency level header (added to query for tracking)
        if (options.ConsistencyLevel.HasValue)
        {
            queryParts.Add($"consistencyLevel={options.ConsistencyLevel.Value.ToString().ToLowerInvariant()}");
        }

        return queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;
    }

    /// <summary>
    /// Build specialized query for mail operations
    /// </summary>
    /// <param name="options">Mail-specific query options</param>
    /// <returns>OData query string</returns>
    public static string BuildMailQuery(GraphMailQueryOptions? options)
    {
        if (options == null) return string.Empty;

        var filters = new List<string>();

        // Add base filter if present
        if (!string.IsNullOrEmpty(options.Filter))
        {
            filters.Add($"({options.Filter})");
        }

        // Date range filters
        if (options.StartDate.HasValue)
        {
            var startDate = options.StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            filters.Add($"receivedDateTime ge {startDate}");
        }

        if (options.EndDate.HasValue)
        {
            var endDate = options.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            filters.Add($"receivedDateTime le {endDate}");
        }

        // Importance filter
        if (options.Importance.HasValue)
        {
            filters.Add($"importance eq '{options.Importance.Value.ToString().ToLowerInvariant()}'");
        }

        // Read status filter
        if (options.IsRead.HasValue)
        {
            filters.Add($"isRead eq {options.IsRead.Value.ToString().ToLowerInvariant()}");
        }

        // Attachments filter
        if (options.HasAttachments.HasValue)
        {
            filters.Add($"hasAttachments eq {options.HasAttachments.Value.ToString().ToLowerInvariant()}");
        }

        // From email filter
        if (!string.IsNullOrEmpty(options.FromEmail))
        {
            filters.Add($"from/emailAddress/address eq '{HttpUtility.UrlEncode(options.FromEmail)}'");
        }

        // Subject contains filter
        if (!string.IsNullOrEmpty(options.SubjectContains))
        {
            filters.Add($"contains(subject, '{HttpUtility.UrlEncode(options.SubjectContains)}')");
        }

        // Categories filter
        if (options.Categories?.Length > 0)
        {
            var categoryFilters = options.Categories
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => $"categories/any(c: c eq '{HttpUtility.UrlEncode(c)}')");
            
            if (categoryFilters.Any())
            {
                filters.Add($"({string.Join(" or ", categoryFilters)})");
            }
        }

        // Combine all filters
        var combinedOptions = new GraphQueryOptions
        {
            Select = options.Select,
            Filter = filters.Count > 0 ? string.Join(" and ", filters) : null,
            OrderBy = options.OrderBy,
            Expand = options.Expand,
            Top = options.Top,
            Skip = options.Skip,
            Count = options.Count,
            Search = options.Search,
            ConsistencyLevel = options.ConsistencyLevel
        };

        return BuildQuery(combinedOptions);
    }

    /// <summary>
    /// Build specialized query for calendar operations
    /// </summary>
    /// <param name="options">Calendar-specific query options</param>
    /// <returns>OData query string</returns>
    public static string BuildCalendarQuery(GraphCalendarQueryOptions? options)
    {
        if (options == null) return string.Empty;

        var filters = new List<string>();

        // Add base filter if present
        if (!string.IsNullOrEmpty(options.Filter))
        {
            filters.Add($"({options.Filter})");
        }

        // Date range filters (for events)
        if (options.StartTime.HasValue)
        {
            var startTime = options.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            filters.Add($"start/dateTime ge '{startTime}'");
        }

        if (options.EndTime.HasValue)
        {
            var endTime = options.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            filters.Add($"end/dateTime le '{endTime}'");
        }

        // All-day events filter
        if (options.IsAllDay.HasValue)
        {
            filters.Add($"isAllDay eq {options.IsAllDay.Value.ToString().ToLowerInvariant()}");
        }

        // Sensitivity filter
        if (options.Sensitivity.HasValue)
        {
            filters.Add($"sensitivity eq '{options.Sensitivity.Value.ToString().ToLowerInvariant()}'");
        }

        // Show as filter
        if (options.ShowAs?.Length > 0)
        {
            var showAsFilters = options.ShowAs
                .Select(s => $"showAs eq '{s.ToString().ToLowerInvariant()}'");
            filters.Add($"({string.Join(" or ", showAsFilters)})");
        }

        // Combine all filters
        var combinedOptions = new GraphQueryOptions
        {
            Select = options.Select,
            Filter = filters.Count > 0 ? string.Join(" and ", filters) : null,
            OrderBy = options.OrderBy,
            Expand = options.Expand,
            Top = options.Top,
            Skip = options.Skip,
            Count = options.Count,
            Search = options.Search,
            ConsistencyLevel = options.ConsistencyLevel
        };

        return BuildQuery(combinedOptions);
    }

    /// <summary>
    /// Build specialized query for file operations
    /// </summary>
    /// <param name="options">File-specific query options</param>
    /// <returns>OData query string</returns>
    public static string BuildFileQuery(GraphFileQueryOptions? options)
    {
        if (options == null) return string.Empty;

        var filters = new List<string>();

        // Add base filter if present
        if (!string.IsNullOrEmpty(options.Filter))
        {
            filters.Add($"({options.Filter})");
        }

        // File type filters
        if (options.FileTypes?.Length > 0)
        {
            var fileTypeFilters = options.FileTypes
                .Where(ft => !string.IsNullOrWhiteSpace(ft))
                .Select(ft => $"endswith(name, '.{ft.Trim().ToLowerInvariant()}')");
            
            if (fileTypeFilters.Any())
            {
                filters.Add($"({string.Join(" or ", fileTypeFilters)})");
            }
        }

        // Modified date filters
        if (options.ModifiedAfter.HasValue)
        {
            var modifiedAfter = options.ModifiedAfter.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            filters.Add($"lastModifiedDateTime ge {modifiedAfter}");
        }

        if (options.ModifiedBefore.HasValue)
        {
            var modifiedBefore = options.ModifiedBefore.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            filters.Add($"lastModifiedDateTime le {modifiedBefore}");
        }

        // File size filters
        if (options.MinSize.HasValue)
        {
            filters.Add($"size ge {options.MinSize.Value}");
        }

        if (options.MaxSize.HasValue)
        {
            filters.Add($"size le {options.MaxSize.Value}");
        }

        // Combine all filters
        var combinedOptions = new GraphQueryOptions
        {
            Select = options.Select,
            Filter = filters.Count > 0 ? string.Join(" and ", filters) : null,
            OrderBy = options.OrderBy,
            Expand = options.Expand,
            Top = options.Top,
            Skip = options.Skip,
            Count = options.Count,
            Search = options.Search,
            ConsistencyLevel = options.ConsistencyLevel
        };

        return BuildQuery(combinedOptions);
    }

    /// <summary>
    /// Build specialized query for user operations
    /// </summary>
    /// <param name="options">User-specific query options</param>
    /// <returns>OData query string</returns>
    public static string BuildUserQuery(GraphUserQueryOptions? options)
    {
        if (options == null) return string.Empty;

        var filters = new List<string>();

        // Add base filter if present
        if (!string.IsNullOrEmpty(options.Filter))
        {
            filters.Add($"({options.Filter})");
        }

        // Department filter
        if (!string.IsNullOrEmpty(options.Department))
        {
            filters.Add($"department eq '{HttpUtility.UrlEncode(options.Department)}'");
        }

        // Job title filter
        if (!string.IsNullOrEmpty(options.JobTitle))
        {
            filters.Add($"jobTitle eq '{HttpUtility.UrlEncode(options.JobTitle)}'");
        }

        // Country filter
        if (!string.IsNullOrEmpty(options.Country))
        {
            filters.Add($"country eq '{HttpUtility.UrlEncode(options.Country)}'");
        }

        // Office location filter
        if (!string.IsNullOrEmpty(options.OfficeLocation))
        {
            filters.Add($"officeLocation eq '{HttpUtility.UrlEncode(options.OfficeLocation)}'");
        }

        // Account enabled filter
        if (options.AccountEnabled.HasValue)
        {
            filters.Add($"accountEnabled eq {options.AccountEnabled.Value.ToString().ToLowerInvariant()}");
        }

        // User type filter
        if (options.UserType.HasValue)
        {
            filters.Add($"userType eq '{options.UserType.Value}'");
        }

        // Combine all filters
        var combinedOptions = new GraphQueryOptions
        {
            Select = options.Select,
            Filter = filters.Count > 0 ? string.Join(" and ", filters) : null,
            OrderBy = options.OrderBy,
            Expand = options.Expand,
            Top = options.Top,
            Skip = options.Skip,
            Count = options.Count,
            Search = options.Search,
            ConsistencyLevel = options.ConsistencyLevel
        };

        return BuildQuery(combinedOptions);
    }

    /// <summary>
    /// Build query for pagination using nextLink
    /// </summary>
    /// <param name="nextLink">Next link URL from previous response</param>
    /// <returns>Query parameters from the next link</returns>
    public static string ExtractQueryFromNextLink(string nextLink)
    {
        if (string.IsNullOrEmpty(nextLink)) return string.Empty;

        try
        {
            var uri = new Uri(nextLink);
            return uri.Query; // Already includes the leading ?
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Build calendar view query with time range
    /// </summary>
    /// <param name="startTime">Start time for the calendar view</param>
    /// <param name="endTime">End time for the calendar view</param>
    /// <param name="options">Additional query options</param>
    /// <returns>OData query string</returns>
    public static string BuildCalendarViewQuery(DateTime startTime, DateTime endTime, GraphCalendarQueryOptions? options = null)
    {
        var startTimeStr = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        var endTimeStr = endTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        
        var queryParts = new List<string>
        {
            $"startDateTime={HttpUtility.UrlEncode(startTimeStr)}",
            $"endDateTime={HttpUtility.UrlEncode(endTimeStr)}"
        };

        // Add additional options
        var additionalQuery = BuildCalendarQuery(options);
        if (!string.IsNullOrEmpty(additionalQuery))
        {
            // Remove leading ? and add to query parts
            queryParts.Add(additionalQuery.TrimStart('?'));
        }

        return "?" + string.Join("&", queryParts);
    }

    /// <summary>
    /// Validate and sanitize OData filter expressions
    /// </summary>
    /// <param name="filter">Raw filter expression</param>
    /// <returns>Sanitized filter expression</returns>
    public static string SanitizeFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return string.Empty;

        // Basic sanitization - remove potentially dangerous characters
        var sanitized = filter
            .Replace("'", "''")     // Escape single quotes
            .Replace("\"", "'")     // Convert double quotes to single quotes
            .Replace(";", "")       // Remove semicolons
            .Replace("--", "")      // Remove SQL comment markers
            .Trim();

        // Limit length to prevent excessively long filters
        return sanitized.Length > 500 ? sanitized[..500] : sanitized;
    }
}
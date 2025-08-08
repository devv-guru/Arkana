using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph calendar-related MCP tools
/// </summary>
[McpServerToolType]
public static class CalendarTools
{
    [McpServerTool, Description("Get calendar events")]
    public static async Task<string> GetEvents(
        IGraphCalendarService calendarService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of events to retrieve (default: 10)")] int top = 10,
        [Description("Start date filter (YYYY-MM-DD format, optional)")] string? startDate = null,
        [Description("End date filter (YYYY-MM-DD format, optional)")] string? endDate = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var options = new GraphCalendarQueryOptions 
            { 
                Top = top,
                StartTime = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate),
                EndTime = string.IsNullOrEmpty(endDate) ? null : DateTime.Parse(endDate)
            };
            var eventsResponse = await calendarService.GetEventsWithOptionsAsync(accessToken, options, correlationId);
            
            var result = new
            {
                Success = true,
                Data = eventsResponse?.RootElement,
                Filters = new { Top = top, StartDate = startDate, EndDate = endDate }
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get a specific calendar event by ID")]
    public static async Task<string> GetEvent(
        IGraphCalendarService calendarService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The event ID")] string eventId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var eventResponse = await calendarService.GetEventAsync(accessToken, eventId, correlationId);
            
            var result = new
            {
                Success = true,
                Event = eventResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get user's calendars")]
    public static async Task<string> GetCalendars(
        IGraphCalendarService calendarService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var calendarsResponse = await calendarService.GetCalendarsAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Calendars = calendarsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a new calendar event")]
    public static async Task<string> CreateEvent(
        IGraphCalendarService calendarService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Event subject/title")] string subject,
        [Description("Event start time (ISO 8601 format: YYYY-MM-DDTHH:MM:SS)")] string startTime,
        [Description("Event end time (ISO 8601 format: YYYY-MM-DDTHH:MM:SS)")] string endTime,
        [Description("Event body/description (optional)")] string? body = null,
        [Description("Event location (optional)")] string? location = null,
        [Description("Attendee email addresses (comma-separated, optional)")] string? attendeeEmails = null,
        [Description("Time zone (optional, defaults to UTC)")] string? timeZone = "UTC")
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Parse attendees
            var attendees = new List<object>();
            if (!string.IsNullOrEmpty(attendeeEmails))
            {
                foreach (var email in attendeeEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    attendees.Add(new
                    {
                        emailAddress = new
                        {
                            address = email.Trim(),
                            name = email.Trim() // Use email as name if no name provided
                        },
                        type = "required"
                    });
                }
            }

            var eventData = new
            {
                subject = subject,
                start = new
                {
                    dateTime = startTime,
                    timeZone = timeZone ?? "UTC"
                },
                end = new
                {
                    dateTime = endTime,
                    timeZone = timeZone ?? "UTC"
                },
                body = string.IsNullOrEmpty(body) ? null : new
                {
                    contentType = "Text",
                    content = body
                },
                location = string.IsNullOrEmpty(location) ? null : new
                {
                    displayName = location
                },
                attendees = attendees.Count > 0 ? attendees.ToArray() : null
            };

            var response = await calendarService.CreateEventAsync(accessToken, eventData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = attendees.Count > 0 ? $"Event created successfully with {attendees.Count} attendees invited" : "Event created successfully",
                Subject = subject,
                StartTime = startTime,
                EndTime = endTime,
                Location = location,
                AttendeeCount = attendees.Count,
                Attendees = attendees.Count > 0 ? attendeeEmails : null,
                Event = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search calendar events")]
    public static async Task<string> SearchEvents(
        IGraphCalendarService calendarService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for event subject or body")] string searchTerm,
        [Description("Number of results to return (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var eventsResponse = await calendarService.SearchEventsAsync(accessToken, searchTerm, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Data = eventsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a meeting invitation with advanced options")]
    public static async Task<string> CreateMeetingInvitation(
        IGraphCalendarService calendarService,
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Meeting subject/title")] string subject,
        [Description("Meeting start time (ISO 8601 format: YYYY-MM-DDTHH:MM:SS)")] string startTime,
        [Description("Meeting end time (ISO 8601 format: YYYY-MM-DDTHH:MM:SS)")] string endTime,
        [Description("Required attendee email addresses (comma-separated)")] string requiredAttendees,
        [Description("Meeting body/description (supports HTML, optional)")] string? body = null,
        [Description("Meeting location (optional)")] string? location = null,
        [Description("Optional attendee email addresses (comma-separated, optional)")] string? optionalAttendees = null,
        [Description("Meeting type: 'teams' for Teams meeting, 'inperson' for physical location (default: inperson)")] string meetingType = "inperson",
        [Description("Time zone (optional, defaults to UTC)")] string? timeZone = "UTC",
        [Description("File references: OneDrive file IDs, SharePoint file paths (drive/site:driveId:itemId), or mixed (comma-separated, optional)")] string? attachmentFileReferences = null,
        [Description("Send meeting invitations (default: true)")] bool sendInvitations = true,
        [Description("Allow response suggestions (default: true)")] bool responseRequested = true)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Parse required attendees
            var attendeesList = new List<object>();
            if (!string.IsNullOrEmpty(requiredAttendees))
            {
                foreach (var email in requiredAttendees.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    attendeesList.Add(new
                    {
                        emailAddress = new
                        {
                            address = email.Trim(),
                            name = email.Trim()
                        },
                        type = "required"
                    });
                }
            }

            // Parse optional attendees
            if (!string.IsNullOrEmpty(optionalAttendees))
            {
                foreach (var email in optionalAttendees.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    attendeesList.Add(new
                    {
                        emailAddress = new
                        {
                            address = email.Trim(),
                            name = email.Trim()
                        },
                        type = "optional"
                    });
                }
            }

            // Process file attachments if provided (supports both OneDrive and SharePoint)
            var attachments = new List<object>();
            if (!string.IsNullOrEmpty(attachmentFileReferences))
            {
                attachments = await ProcessFileAttachments(filesService, accessToken, attachmentFileReferences, correlationId);
            }

            // Determine if this is a Teams meeting
            var isTeamsMeeting = meetingType.Equals("teams", StringComparison.OrdinalIgnoreCase);
            
            // Build event data based on meeting type
            object eventData;
            
            if (isTeamsMeeting)
            {
                // Teams meeting configuration
                eventData = new
                {
                    subject = subject,
                    start = new
                    {
                        dateTime = startTime,
                        timeZone = timeZone ?? "UTC"
                    },
                    end = new
                    {
                        dateTime = endTime,
                        timeZone = timeZone ?? "UTC"
                    },
                    body = string.IsNullOrEmpty(body) ? null : new
                    {
                        contentType = "HTML",
                        content = body
                    },
                    location = string.IsNullOrEmpty(location) ? new
                    {
                        displayName = "Microsoft Teams Meeting"
                    } : new
                    {
                        displayName = $"{location} (Microsoft Teams Meeting)"
                    },
                    attendees = attendeesList.ToArray(),
                    responseRequested = responseRequested,
                    allowNewTimeProposals = true,
                    isOrganizer = true,
                    isOnlineMeeting = true,
                    onlineMeetingProvider = "teamsForBusiness",
                    attachments = attachments.Count > 0 ? attachments.ToArray() : null
                };
            }
            else
            {
                // In-person meeting configuration
                eventData = new
                {
                    subject = subject,
                    start = new
                    {
                        dateTime = startTime,
                        timeZone = timeZone ?? "UTC"
                    },
                    end = new
                    {
                        dateTime = endTime,
                        timeZone = timeZone ?? "UTC"
                    },
                    body = string.IsNullOrEmpty(body) ? null : new
                    {
                        contentType = "HTML",
                        content = body
                    },
                    location = string.IsNullOrEmpty(location) ? null : new
                    {
                        displayName = location
                    },
                    attendees = attendeesList.ToArray(),
                    responseRequested = responseRequested,
                    allowNewTimeProposals = true,
                    isOrganizer = true,
                    isOnlineMeeting = false,
                    attachments = attachments.Count > 0 ? attachments.ToArray() : null
                };
            }

            var response = await calendarService.CreateEventAsync(accessToken, eventData, correlationId);
            
            var requiredCount = requiredAttendees?.Split(',', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            var optionalCount = optionalAttendees?.Split(',', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            var totalAttendees = requiredCount + optionalCount;

            var result = new
            {
                Success = true,
                Message = isTeamsMeeting 
                    ? $"Teams meeting invitation created successfully with {totalAttendees} invitees"
                    : $"In-person meeting invitation created successfully with {totalAttendees} invitees",
                Subject = subject,
                StartTime = startTime,
                EndTime = endTime,
                TimeZone = timeZone,
                Location = location,
                MeetingType = isTeamsMeeting ? "Microsoft Teams Meeting" : "In-Person Meeting",
                IsTeamsMeeting = isTeamsMeeting,
                RequiredAttendees = requiredCount,
                OptionalAttendees = optionalCount,
                TotalInvitees = totalAttendees,
                AttachmentCount = attachments.Count,
                AttachmentNames = attachments.Count > 0 ? attachments.Select(a => ((dynamic)a).name).ToArray() : null,
                AttachmentType = attachments.Count > 0 ? "File Reference (OneDrive/SharePoint)" : null,
                HasBody = !string.IsNullOrEmpty(body),
                BodyFormat = "HTML",
                SendInvitations = sendInvitations,
                ResponseRequested = responseRequested,
                RequiredEmails = requiredAttendees,
                OptionalEmails = optionalAttendees,
                Event = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get event attendees and their response status")]
    public static async Task<string> GetEventAttendees(
        IGraphCalendarService calendarService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The event ID")] string eventId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var eventResponse = await calendarService.GetEventAsync(accessToken, eventId, correlationId);
            
            if (eventResponse == null)
            {
                throw new InvalidOperationException("Event not found");
            }

            // Extract attendee information from the event
            var eventData = eventResponse.RootElement;
            var attendeesData = eventData.TryGetProperty("attendees", out var attendeesElement) 
                ? attendeesElement 
                : new JsonElement();

            var result = new
            {
                Success = true,
                EventId = eventId,
                EventSubject = eventData.TryGetProperty("subject", out var subjectElement) ? subjectElement.GetString() : null,
                Attendees = attendeesData,
                AttendeeCount = attendeesElement.ValueKind == JsonValueKind.Array ? attendeesElement.GetArrayLength() : 0
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Process file attachments from both OneDrive and SharePoint
    /// Supports formats: fileId (OneDrive), drive:driveId:itemId (SharePoint drive), site:siteId:itemId (SharePoint site)
    /// </summary>
    private static async Task<List<object>> ProcessFileAttachments(
        IGraphFilesService filesService,
        string accessToken,
        string attachmentFileReferences,
        string? correlationId = null)
    {
        var attachments = new List<object>();
        var fileReferences = attachmentFileReferences.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var fileRef in fileReferences)
        {
            var trimmedFileRef = fileRef.Trim();
            
            try
            {
                JsonDocument? fileInfo = null;
                string providerType = "oneDrive"; // default
                
                // Check if this is a SharePoint reference (format: drive:driveId:itemId or site:siteId:itemId)
                if (trimmedFileRef.Contains(':'))
                {
                    var parts = trimmedFileRef.Split(':', 3);
                    if (parts.Length == 3)
                    {
                        var refType = parts[0].ToLower();
                        var containerId = parts[1];
                        var itemId = parts[2];
                        
                        if (refType == "drive")
                        {
                            // SharePoint drive reference
                            fileInfo = await filesService.GetDriveItemAsync(accessToken, itemId, correlationId);
                            providerType = "sharePoint";
                        }
                        else if (refType == "site")
                        {
                            // SharePoint site reference - get default drive first
                            var siteDrive = await filesService.GetSiteDriveAsync(accessToken, containerId, correlationId);
                            if (siteDrive != null)
                            {
                                fileInfo = await filesService.GetDriveItemAsync(accessToken, itemId, correlationId);
                                providerType = "sharePoint";
                            }
                        }
                    }
                }
                else
                {
                    // Regular OneDrive file ID
                    fileInfo = await filesService.GetDriveItemAsync(accessToken, trimmedFileRef, correlationId);
                }
                
                if (fileInfo == null)
                {
                    throw new InvalidOperationException($"File with reference '{trimmedFileRef}' not found");
                }

                var fileElement = fileInfo.RootElement;
                var fileName = fileElement.GetProperty("name").GetString();
                var fileSize = fileElement.TryGetProperty("size", out var sizeElement) ? sizeElement.GetInt64() : 0;

                // Check file size (Graph API has limits - typically 25MB for attachments)
                if (fileSize > 25 * 1024 * 1024) // 25MB limit
                {
                    throw new InvalidOperationException($"File '{fileName}' is too large. Maximum size is 25MB.");
                }

                // Use reference attachments for both OneDrive and SharePoint files
                // This is more efficient and maintains file permissions
                attachments.Add(new
                {
                    @type = "#microsoft.graph.referenceAttachment",
                    name = fileName,
                    sourceUrl = fileElement.GetProperty("webUrl").GetString(),
                    providerType = providerType,
                    permission = "view" // Grant view permissions to meeting attendees
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process file reference '{trimmedFileRef}': {ex.Message}");
            }
        }
        
        return attachments;
    }

    /// <summary>
    /// Get MIME content type based on file extension
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".html" => "text/html",
            ".htm" => "text/html",
            ".xml" => "text/xml",
            ".json" => "application/json",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }
}
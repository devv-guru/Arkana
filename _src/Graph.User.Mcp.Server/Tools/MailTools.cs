using System.ComponentModel;
using System.Text.Json;
using Graph.User.Mcp.Server.Services.Contracts;
using Graph.User.Mcp.Server.Services.Models;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace Graph.User.Mcp.Server.Tools;

/// <summary>
/// Microsoft Graph mail-related MCP tools
/// </summary>
[McpServerToolType]
public static class MailTools
{
    [McpServerTool, Description("Get mail messages from the current user's mailbox")]
    public static async Task<string> GetMessages(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of messages to retrieve (default: 10)")] int top = 10,
        [Description("Filter unread messages only")] bool unreadOnly = false)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var options = new GraphMailQueryOptions 
            { 
                Top = top,
                IsRead = unreadOnly ? false : null
            };
            var messagesResponse = await mailService.GetMessagesWithOptionsAsync(accessToken, options, correlationId);
            
            var result = new
            {
                Success = true,
                Data = messagesResponse?.RootElement,
                Filters = new { Top = top, UnreadOnly = unreadOnly }
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get a specific mail message by ID")]
    public static async Task<string> GetMessage(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The message ID")] string messageId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var messageResponse = await mailService.GetMessageAsync(accessToken, messageId, correlationId);
            
            var result = new
            {
                Success = true,
                Message = messageResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Search for mail messages")]
    public static async Task<string> SearchMessages(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Search term for message content, subject, or sender")] string searchTerm,
        [Description("Number of results to return (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var messagesResponse = await mailService.SearchMessagesAsync(accessToken, searchTerm, top, correlationId);
            
            var result = new
            {
                Success = true,
                SearchTerm = searchTerm,
                Data = messagesResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get mail folders")]
    public static async Task<string> GetMailFolders(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var foldersResponse = await mailService.GetMailFoldersAsync(accessToken, correlationId);
            
            var result = new
            {
                Success = true,
                Folders = foldersResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Send an email message")]
    public static async Task<string> SendMessage(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Recipient email address")] string toEmail,
        [Description("Email subject")] string subject,
        [Description("Email body content")] string body,
        [Description("Send as HTML (default: false)")] bool isHtml = false,
        [Description("CC email addresses (comma-separated, optional)")] string? ccEmails = null,
        [Description("BCC email addresses (comma-separated, optional)")] string? bccEmails = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Parse recipients
            var toRecipients = new List<object> 
            {
                new
                {
                    emailAddress = new
                    {
                        address = toEmail
                    }
                }
            };

            var ccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(ccEmails))
            {
                foreach (var cc in ccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    ccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = cc.Trim()
                        }
                    });
                }
            }

            var bccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(bccEmails))
            {
                foreach (var bcc in bccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    bccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = bcc.Trim()
                        }
                    });
                }
            }

            var messageData = new
            {
                message = new
                {
                    subject = subject,
                    body = new
                    {
                        contentType = isHtml ? "HTML" : "Text",
                        content = body
                    },
                    toRecipients = toRecipients.ToArray(),
                    ccRecipients = ccRecipients.Count > 0 ? ccRecipients.ToArray() : null,
                    bccRecipients = bccRecipients.Count > 0 ? bccRecipients.ToArray() : null
                }
            };

            var response = await mailService.SendMailAsync(accessToken, messageData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Email sent successfully",
                To = toEmail,
                Subject = subject
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Send an email message with OneDrive/SharePoint attachments")]
    public static async Task<string> SendMessageWithFileAttachments(
        IGraphMailService mailService,
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Recipient email address")] string toEmail,
        [Description("Email subject")] string subject,
        [Description("Email body content")] string body,
        [Description("File references: OneDrive file IDs, SharePoint file paths (drive/site:driveId:itemId), or mixed (comma-separated)")] string attachmentFileReferences,
        [Description("Send as HTML (default: false)")] bool isHtml = false,
        [Description("CC email addresses (comma-separated, optional)")] string? ccEmails = null,
        [Description("BCC email addresses (comma-separated, optional)")] string? bccEmails = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Parse recipients
            var toRecipients = new List<object> 
            {
                new
                {
                    emailAddress = new
                    {
                        address = toEmail
                    }
                }
            };

            var ccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(ccEmails))
            {
                foreach (var cc in ccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    ccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = cc.Trim()
                        }
                    });
                }
            }

            var bccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(bccEmails))
            {
                foreach (var bcc in bccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    bccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = bcc.Trim()
                        }
                    });
                }
            }

            // Process attachments (supports both OneDrive and SharePoint)
            var attachments = await ProcessFileAttachments(filesService, accessToken, attachmentFileReferences, correlationId);

            var messageData = new
            {
                message = new
                {
                    subject = subject,
                    body = new
                    {
                        contentType = isHtml ? "HTML" : "Text",
                        content = body
                    },
                    toRecipients = toRecipients.ToArray(),
                    ccRecipients = ccRecipients.Count > 0 ? ccRecipients.ToArray() : null,
                    bccRecipients = bccRecipients.Count > 0 ? bccRecipients.ToArray() : null,
                    attachments = attachments.ToArray()
                }
            };

            var response = await mailService.SendMailAsync(accessToken, messageData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Email with OneDrive attachments sent successfully",
                To = toEmail,
                CC = ccEmails,
                BCC = bccEmails,
                Subject = subject,
                AttachmentCount = attachments.Count,
                AttachmentNames = attachments.Select(a => ((dynamic)a).name).ToArray(),
                AttachmentType = "File Reference (OneDrive/SharePoint)"
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get message attachments")]
    public static async Task<string> GetMessageAttachments(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The message ID")] string messageId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var attachmentsResponse = await mailService.GetMessageAttachmentsAsync(accessToken, messageId, correlationId);
            
            var result = new
            {
                Success = true,
                MessageId = messageId,
                Attachments = attachmentsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a draft email message")]
    public static async Task<string> CreateDraftMessage(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Recipient email address")] string toEmail,
        [Description("Email subject")] string subject,
        [Description("Email body content")] string body,
        [Description("Send as HTML (default: false)")] bool isHtml = false,
        [Description("CC email addresses (comma-separated, optional)")] string? ccEmails = null,
        [Description("BCC email addresses (comma-separated, optional)")] string? bccEmails = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Parse recipients
            var toRecipients = new List<object> 
            {
                new
                {
                    emailAddress = new
                    {
                        address = toEmail
                    }
                }
            };

            var ccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(ccEmails))
            {
                foreach (var cc in ccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    ccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = cc.Trim()
                        }
                    });
                }
            }

            var bccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(bccEmails))
            {
                foreach (var bcc in bccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    bccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = bcc.Trim()
                        }
                    });
                }
            }

            var messageData = new
            {
                subject = subject,
                body = new
                {
                    contentType = isHtml ? "HTML" : "Text",
                    content = body
                },
                toRecipients = toRecipients.ToArray(),
                ccRecipients = ccRecipients.Count > 0 ? ccRecipients.ToArray() : null,
                bccRecipients = bccRecipients.Count > 0 ? bccRecipients.ToArray() : null
            };

            var response = await mailService.CreateDraftMessageAsync(accessToken, messageData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Draft email created successfully",
                To = toEmail,
                CC = ccEmails,
                BCC = bccEmails,
                Subject = subject,
                DraftId = response?.RootElement.GetProperty("id").GetString(),
                Draft = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Create a draft email message with OneDrive/SharePoint attachments")]
    public static async Task<string> CreateDraftMessageWithFileAttachments(
        IGraphMailService mailService,
        IGraphFilesService filesService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Recipient email address")] string toEmail,
        [Description("Email subject")] string subject,
        [Description("Email body content")] string body,
        [Description("File references: OneDrive file IDs, SharePoint file paths (drive/site:driveId:itemId), or mixed (comma-separated)")] string attachmentFileReferences,
        [Description("Send as HTML (default: false)")] bool isHtml = false,
        [Description("CC email addresses (comma-separated, optional)")] string? ccEmails = null,
        [Description("BCC email addresses (comma-separated, optional)")] string? bccEmails = null)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Parse recipients
            var toRecipients = new List<object> 
            {
                new
                {
                    emailAddress = new
                    {
                        address = toEmail
                    }
                }
            };

            var ccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(ccEmails))
            {
                foreach (var cc in ccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    ccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = cc.Trim()
                        }
                    });
                }
            }

            var bccRecipients = new List<object>();
            if (!string.IsNullOrEmpty(bccEmails))
            {
                foreach (var bcc in bccEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    bccRecipients.Add(new
                    {
                        emailAddress = new
                        {
                            address = bcc.Trim()
                        }
                    });
                }
            }

            // Process attachments (supports both OneDrive and SharePoint)
            var attachments = await ProcessFileAttachments(filesService, accessToken, attachmentFileReferences, correlationId);

            var messageData = new
            {
                subject = subject,
                body = new
                {
                    contentType = isHtml ? "HTML" : "Text",
                    content = body
                },
                toRecipients = toRecipients.ToArray(),
                ccRecipients = ccRecipients.Count > 0 ? ccRecipients.ToArray() : null,
                bccRecipients = bccRecipients.Count > 0 ? bccRecipients.ToArray() : null,
                attachments = attachments.ToArray()
            };

            var response = await mailService.CreateDraftMessageAsync(accessToken, messageData, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Draft email with OneDrive attachments created successfully",
                To = toEmail,
                CC = ccEmails,
                BCC = bccEmails,
                Subject = subject,
                AttachmentCount = attachments.Count,
                AttachmentNames = attachments.Select(a => ((dynamic)a).name).ToArray(),
                AttachmentType = "File Reference (OneDrive/SharePoint)",
                DraftId = response?.RootElement.GetProperty("id").GetString(),
                Draft = response?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Get draft messages from the Drafts folder")]
    public static async Task<string> GetDraftMessages(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("Number of draft messages to retrieve (default: 10)")] int top = 10)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            
            // Get messages from the Drafts folder (well-known folder name)
            var draftsResponse = await mailService.GetMessagesAsync(accessToken, "Drafts", top, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Draft messages retrieved successfully",
                Count = top,
                Drafts = draftsResponse?.RootElement
            };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var error = new { Success = false, Error = ex.Message, Details = ex.ToString() };
            return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool, Description("Delete a draft message")]
    public static async Task<string> DeleteDraftMessage(
        IGraphMailService mailService,
        IHttpContextAccessor httpContextAccessor,
        [Description("The draft message ID to delete")] string messageId)
    {
        try
        {
            var (accessToken, correlationId) = McpToolsHelper.ExtractTokenAndCorrelationId(httpContextAccessor);
            var response = await mailService.DeleteMessageAsync(accessToken, messageId, correlationId);
            
            var result = new
            {
                Success = true,
                Message = "Draft message deleted successfully",
                MessageId = messageId
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
                    permission = "view"
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
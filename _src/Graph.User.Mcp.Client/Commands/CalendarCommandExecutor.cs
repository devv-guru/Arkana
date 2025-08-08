using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes calendar-related MCP commands
/// </summary>
public static class CalendarCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing calendar command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Calendar command executed without subcommand");
            Console.WriteLine("❌ Calendar command requires subcommand. Try: list, today, create, meeting");
            return;
        }
        
        var subCommand = args[0].ToLower();
        string response;
        
        try
        {
            switch (subCommand)
            {
                case "list":
                    var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 10;
                    logger.LogInformation("Getting {Count} upcoming events", count);
                    Console.WriteLine($"🔄 Getting {count} upcoming events...");
                    response = await mcpClient.CallMcpToolAsync("GetEvents", new { top = count }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} events", count);
                    Console.WriteLine($"✅ Upcoming Events:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "today":
                    var today = DateTime.Today.ToString("yyyy-MM-dd");
                    var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                    logger.LogInformation("Getting today's events for date: {Date}", today);
                    Console.WriteLine("🔄 Getting today's events...");
                    response = await mcpClient.CallMcpToolAsync("GetEvents", new { 
                        top = 20,
                        startDate = today,
                        endDate = tomorrow
                    }, userToken.Token);
                    logger.LogInformation("Successfully retrieved today's events for date: {Date}", today);
                    Console.WriteLine($"✅ Today's Events:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "create":
                    Console.Write("Event title: ");
                    var title = Console.ReadLine() ?? "";
                    Console.Write("Start time (YYYY-MM-DDTHH:MM:SS): ");
                    var startTime = Console.ReadLine() ?? "";
                    Console.Write("End time (YYYY-MM-DDTHH:MM:SS): ");
                    var endTime = Console.ReadLine() ?? "";
                    Console.Write("Location (optional): ");
                    var location = Console.ReadLine();
                    
                    logger.LogInformation("Creating calendar event: {Title} from {StartTime} to {EndTime}", title, startTime, endTime);
                    Console.WriteLine($"🔄 Creating event '{title}'...");
                    response = await mcpClient.CallMcpToolAsync("CreateEvent", new { 
                        subject = title,
                        startTime = startTime,
                        endTime = endTime,
                        location = location ?? "",
                        timeZone = "UTC"
                    }, userToken.Token);
                    logger.LogInformation("Successfully created calendar event: {Title}", title);
                    Console.WriteLine($"✅ Event Created:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "meeting":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Meeting command executed without title");
                        Console.WriteLine("❌ Meeting requires title. Usage: calendar meeting <title>");
                        return;
                    }
                    var meetingTitle = string.Join(" ", args.Skip(1));
                    Console.Write("Start time (YYYY-MM-DDTHH:MM:SS): ");
                    var meetingStart = Console.ReadLine() ?? "";
                    Console.Write("End time (YYYY-MM-DDTHH:MM:SS): ");
                    var meetingEnd = Console.ReadLine() ?? "";
                    Console.Write("Required attendees (comma-separated emails): ");
                    var attendees = Console.ReadLine() ?? "";
                    
                    logger.LogInformation("Creating Teams meeting: {Title} from {StartTime} to {EndTime} with attendees: {Attendees}", 
                        meetingTitle, meetingStart, meetingEnd, attendees);
                    Console.WriteLine($"🔄 Creating Teams meeting '{meetingTitle}'...");
                    response = await mcpClient.CallMcpToolAsync("CreateMeetingInvitation", new { 
                        subject = meetingTitle,
                        startTime = meetingStart,
                        endTime = meetingEnd,
                        requiredAttendees = attendees,
                        meetingType = "teams",
                        timeZone = "UTC"
                    }, userToken.Token);
                    logger.LogInformation("Successfully created Teams meeting: {Title}", meetingTitle);
                    Console.WriteLine($"✅ Teams Meeting Created:");
                    Console.WriteLine($"   {response}");
                    break;
                
                default:
                    logger.LogWarning("Unknown calendar subcommand: {SubCommand}", subCommand);
                    Console.WriteLine($"❌ Unknown calendar command: {subCommand}");
                    Console.WriteLine("Available: list, today, create, meeting");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing calendar command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing calendar command: {ex.Message}");
            throw;
        }
    }
}
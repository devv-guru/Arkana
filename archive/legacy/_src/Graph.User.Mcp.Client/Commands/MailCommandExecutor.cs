using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes mail-related MCP commands
/// </summary>
public static class MailCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing mail command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Mail command executed without subcommand");
            Console.WriteLine("❌ Mail command requires subcommand. Try: list, search, send, drafts, folders");
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
                    logger.LogInformation("Getting {Count} recent emails", count);
                    Console.WriteLine($"🔄 Getting {count} recent emails...");
                    response = await mcpClient.CallMcpToolAsync("GetMessages", new { top = count }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} emails", count);
                    Console.WriteLine($"✅ Recent Emails:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "search":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Search command executed without search term");
                        Console.WriteLine("❌ Search requires a term. Usage: mail search <term>");
                        return;
                    }
                    var searchTerm = string.Join(" ", args.Skip(1));
                    logger.LogInformation("Searching emails for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"🔄 Searching emails for '{searchTerm}'...");
                    response = await mcpClient.CallMcpToolAsync("SearchMessages", new { searchTerm = searchTerm }, userToken.Token);
                    logger.LogInformation("Email search completed for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"✅ Search Results:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "send":
                    if (args.Length < 3)
                    {
                        logger.LogWarning("Send command executed without recipient or subject");
                        Console.WriteLine("❌ Send requires recipient and subject. Usage: mail send <to> <subject>");
                        return;
                    }
                    var toEmail = args[1];
                    var subject = string.Join(" ", args.Skip(2));
                    Console.Write("Enter email body: ");
                    var body = Console.ReadLine() ?? "";
                    
                    logger.LogInformation("Sending email to {ToEmail} with subject: {Subject}", toEmail, subject);
                    Console.WriteLine($"🔄 Sending email to {toEmail}...");
                    response = await mcpClient.CallMcpToolAsync("SendMessage", new { 
                        toEmail = toEmail,
                        subject = subject,
                        body = body,
                        isHtml = false
                    }, userToken.Token);
                    logger.LogInformation("Successfully sent email to {ToEmail}", toEmail);
                    Console.WriteLine($"✅ Email Sent:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "drafts":
                    logger.LogInformation("Getting draft emails");
                    Console.WriteLine("🔄 Getting draft emails...");
                    response = await mcpClient.CallMcpToolAsync("GetDraftMessages", new { top = 10 }, userToken.Token);
                    logger.LogInformation("Successfully retrieved draft emails");
                    Console.WriteLine($"✅ Draft Emails:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                case "folders":
                    logger.LogInformation("Getting mail folders");
                    Console.WriteLine("🔄 Getting mail folders...");
                    response = await mcpClient.CallMcpToolAsync("GetMailFolders", null, userToken.Token);
                    logger.LogInformation("Successfully retrieved mail folders");
                    Console.WriteLine($"✅ Mail Folders:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                default:
                    logger.LogWarning("Unknown mail subcommand: {SubCommand}", subCommand);
                    Console.WriteLine($"❌ Unknown mail command: {subCommand}");
                    Console.WriteLine("Available: list, search, send, drafts, folders");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing mail command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing mail command: {ex.Message}");
            throw;
        }
    }
}
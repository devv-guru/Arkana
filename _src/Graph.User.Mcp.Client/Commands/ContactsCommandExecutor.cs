using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes contacts-related MCP commands
/// </summary>
public static class ContactsCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing contacts command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Contacts command executed without subcommand");
            Console.WriteLine("❌ Contacts command requires subcommand. Try: list, search, folders");
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
                    logger.LogInformation("Getting {Count} contacts", count);
                    Console.WriteLine($"🔄 Getting {count} contacts...");
                    response = await mcpClient.CallMcpToolAsync("GetContacts", new { top = count }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} contacts", count);
                    Console.WriteLine($"✅ Contacts:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                case "search":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Search command executed without search term");
                        Console.WriteLine("❌ Search requires a term. Usage: contacts search <term>");
                        return;
                    }
                    var searchTerm = string.Join(" ", args.Skip(1));
                    logger.LogInformation("Searching contacts for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"🔄 Searching contacts for '{searchTerm}'...");
                    response = await mcpClient.CallMcpToolAsync("SearchContacts", new { searchTerm = searchTerm }, userToken.Token);
                    logger.LogInformation("Contacts search completed for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"✅ Contact Search Results:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                case "folders":
                    logger.LogInformation("Getting contact folders");
                    Console.WriteLine("🔄 Getting contact folders...");
                    response = await mcpClient.CallMcpToolAsync("GetContactFolders", null, userToken.Token);
                    logger.LogInformation("Successfully retrieved contact folders");
                    Console.WriteLine($"✅ Contact Folders:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                default:
                    logger.LogWarning("Unknown contacts subcommand: {SubCommand}", subCommand);
                    Console.WriteLine($"❌ Unknown contacts command: {subCommand}");
                    Console.WriteLine("Available: list, search, folders");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing contacts command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing contacts command: {ex.Message}");
            throw;
        }
    }
}
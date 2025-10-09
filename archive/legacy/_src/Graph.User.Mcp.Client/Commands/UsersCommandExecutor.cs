using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes users-related MCP commands
/// </summary>
public static class UsersCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing users command with args: {Args}", string.Join(" ", args));
        
        var subCommand = args.Length > 0 ? args[0].ToLower() : "list";
        string response;
        
        try
        {
            switch (subCommand)
            {
                case "list":
                    var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 10;
                    logger.LogInformation("Getting {Count} organization users", count);
                    Console.WriteLine($"🔄 Getting {count} organization users...");
                    response = await mcpClient.CallMcpToolAsync("ListUsers", new { top = count }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} organization users", count);
                    Console.WriteLine($"✅ Organization Users:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "profile":
                    logger.LogInformation("Getting user profile");
                    Console.WriteLine("🔄 Getting your profile...");
                    response = await mcpClient.CallMcpToolAsync("GetMyProfile", null, userToken.Token);
                    logger.LogInformation("Successfully retrieved user profile");
                    Console.WriteLine($"✅ Your Profile:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "search":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Search command executed without search term");
                        Console.WriteLine("❌ Search requires a term. Usage: users search <term>");
                        return;
                    }
                    var searchTerm = string.Join(" ", args.Skip(1));
                    logger.LogInformation("Searching users for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"🔄 Searching users for '{searchTerm}'...");
                    response = await mcpClient.CallMcpToolAsync("SearchUsers", new { searchTerm = searchTerm }, userToken.Token);
                    logger.LogInformation("User search completed for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"✅ User Search Results:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "photo":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Photo command executed without email");
                        Console.WriteLine("❌ Photo requires email. Usage: users photo <email>");
                        return;
                    }
                    var email = args[1];
                    logger.LogInformation("Getting photo for user: {Email}", email);
                    Console.WriteLine($"🔄 Getting photo for {email}...");
                    response = await mcpClient.CallMcpToolAsync("GetUserPhoto", new { email = email }, userToken.Token);
                    logger.LogInformation("Successfully retrieved photo for user: {Email}", email);
                    Console.WriteLine($"✅ User Photo:");
                    Console.WriteLine($"   {response}");
                    break;
                
                default:
                    // Backward compatibility - treat first arg as count for listing
                    if (int.TryParse(subCommand, out var backwardCount))
                    {
                        logger.LogInformation("Getting {Count} organization users (backward compatibility)", backwardCount);
                        Console.WriteLine($"🔄 Getting {backwardCount} organization users...");
                        response = await mcpClient.CallMcpToolAsync("ListUsers", new { top = backwardCount }, userToken.Token);
                        logger.LogInformation("Successfully retrieved {Count} organization users (backward compatibility)", backwardCount);
                        Console.WriteLine($"✅ Organization Users:");
                        Console.WriteLine($"   {response}");
                    }
                    else
                    {
                        logger.LogWarning("Unknown users subcommand: {SubCommand}", subCommand);
                        Console.WriteLine($"❌ Unknown users command: {subCommand}");
                        Console.WriteLine("Available: list, profile, search, photo");
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing users command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing users command: {ex.Message}");
            throw;
        }
    }
}
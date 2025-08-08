using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes groups-related MCP commands
/// </summary>
public static class GroupsCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing groups command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Groups command executed without subcommand");
            Console.WriteLine("❌ Groups command requires subcommand. Try: list, my, members, search");
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
                    logger.LogInformation("Getting {Count} groups", count);
                    Console.WriteLine($"🔄 Getting {count} groups...");
                    response = await mcpClient.CallMcpToolAsync("ListGroups", new { top = count }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} groups", count);
                    Console.WriteLine($"✅ Groups:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "my":
                    logger.LogInformation("Getting user's groups");
                    Console.WriteLine("🔄 Getting your groups...");
                    response = await mcpClient.CallMcpToolAsync("GetMyGroups", null, userToken.Token);
                    logger.LogInformation("Successfully retrieved user's groups");
                    Console.WriteLine($"✅ Your Groups:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "members":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Members command executed without group ID");
                        Console.WriteLine("❌ Members requires group ID. Usage: groups members <groupId>");
                        return;
                    }
                    var groupId = args[1];
                    logger.LogInformation("Getting members for group: {GroupId}", groupId);
                    Console.WriteLine($"🔄 Getting members for group {groupId}...");
                    response = await mcpClient.CallMcpToolAsync("GetGroupMembers", new { groupId = groupId }, userToken.Token);
                    logger.LogInformation("Successfully retrieved members for group: {GroupId}", groupId);
                    Console.WriteLine($"✅ Group Members:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "search":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Search command executed without search term");
                        Console.WriteLine("❌ Search requires a term. Usage: groups search <term>");
                        return;
                    }
                    var searchTerm = string.Join(" ", args.Skip(1));
                    logger.LogInformation("Searching groups for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"🔄 Searching groups for '{searchTerm}'...");
                    response = await mcpClient.CallMcpToolAsync("SearchGroups", new { searchTerm = searchTerm }, userToken.Token);
                    logger.LogInformation("Groups search completed for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"✅ Group Search Results:");
                    Console.WriteLine($"   {response}");
                    break;
                
                default:
                    logger.LogWarning("Unknown groups subcommand: {SubCommand}", subCommand);
                    Console.WriteLine($"❌ Unknown groups command: {subCommand}");
                    Console.WriteLine("Available: list, my, members, search");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing groups command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing groups command: {ex.Message}");
            throw;
        }
    }
}
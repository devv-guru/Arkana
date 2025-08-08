using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes files-related MCP commands (OneDrive + SharePoint)
/// </summary>
public static class FilesCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing files command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Files command executed without subcommand");
            Console.WriteLine("❌ Files command requires subcommand. Try: list, recent, search, sharepoint, sp-search");
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
                    logger.LogInformation("Getting {Count} OneDrive files", count);
                    Console.WriteLine($"🔄 Getting {count} OneDrive files...");
                    response = await mcpClient.CallMcpToolAsync("GetFiles", new { top = count }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} OneDrive files", count);
                    Console.WriteLine($"✅ OneDrive Files:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "recent":
                    var recentCount = args.Length > 1 && int.TryParse(args[1], out var rc) ? rc : 10;
                    logger.LogInformation("Getting {Count} recent files", recentCount);
                    Console.WriteLine($"🔄 Getting {recentCount} recent files...");
                    response = await mcpClient.CallMcpToolAsync("GetRecentFiles", new { top = recentCount }, userToken.Token);
                    logger.LogInformation("Successfully retrieved {Count} recent files", recentCount);
                    Console.WriteLine($"✅ Recent Files:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "search":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Search command executed without search term");
                        Console.WriteLine("❌ Search requires a term. Usage: files search <term>");
                        return;
                    }
                    var searchTerm = string.Join(" ", args.Skip(1));
                    logger.LogInformation("Searching files for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"🔄 Searching files for '{searchTerm}'...");
                    response = await mcpClient.CallMcpToolAsync("SearchFiles", new { searchTerm = searchTerm }, userToken.Token);
                    logger.LogInformation("File search completed for term: {SearchTerm}", searchTerm);
                    Console.WriteLine($"✅ File Search Results:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "sharepoint":
                    logger.LogInformation("Getting SharePoint sites");
                    Console.WriteLine("🔄 Getting SharePoint sites...");
                    response = await mcpClient.CallMcpToolAsync("GetSharePointSites", null, userToken.Token);
                    logger.LogInformation("Successfully retrieved SharePoint sites");
                    Console.WriteLine($"✅ SharePoint Sites:");
                    Console.WriteLine($"   {response}");
                    break;
                
                case "sp-search":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("SharePoint search command executed without search term");
                        Console.WriteLine("❌ SharePoint search requires a term. Usage: files sp-search <term>");
                        return;
                    }
                    var spSearchTerm = string.Join(" ", args.Skip(1));
                    logger.LogInformation("Searching SharePoint sites for term: {SearchTerm}", spSearchTerm);
                    Console.WriteLine($"🔄 Searching SharePoint sites for '{spSearchTerm}'...");
                    response = await mcpClient.CallMcpToolAsync("SearchSharePointSites", new { searchTerm = spSearchTerm }, userToken.Token);
                    logger.LogInformation("SharePoint search completed for term: {SearchTerm}", spSearchTerm);
                    Console.WriteLine($"✅ SharePoint Search Results:");
                    Console.WriteLine($"   {response}");
                    break;
                
                default:
                    logger.LogWarning("Unknown files subcommand: {SubCommand}", subCommand);
                    Console.WriteLine($"❌ Unknown files command: {subCommand}");
                    Console.WriteLine("Available: list, recent, search, sharepoint, sp-search");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing files command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing files command: {ex.Message}");
            throw;
        }
    }
}
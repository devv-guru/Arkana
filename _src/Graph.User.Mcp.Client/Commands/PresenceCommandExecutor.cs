using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes presence-related MCP commands (Teams status)
/// </summary>
public static class PresenceCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing presence command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Presence command executed without subcommand");
            Console.WriteLine("❌ Presence command requires subcommand. Try: me, user, set");
            return;
        }
        
        var subCommand = args[0].ToLower();
        string response;

        try
        {
            switch (subCommand)
            {
                case "me":
                    Console.WriteLine("🔄 Getting your presence status...");
                    response = await mcpClient.CallMcpToolAsync("GetMyPresence", null, userToken.Token);
                    Console.WriteLine($"✅ Your Presence:");
                    Console.WriteLine($"   {response}");
                    break;

                case "user":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("❌ User requires email. Usage: presence user <email>");
                        return;
                    }

                    var email = args[1];
                    Console.WriteLine($"🔄 Getting presence for {email}...");
                    response = await mcpClient.CallMcpToolAsync("GetUserPresence", new { email = email },
                        userToken.Token);
                    Console.WriteLine($"✅ User Presence:");
                    Console.WriteLine($"   {response}");
                    break;

                case "set":
                    if (args.Length < 2)
                    {
                        Console.WriteLine(
                            "❌ Set requires status. Usage: presence set <Available|Busy|DoNotDisturb|Away>");
                        return;
                    }

                    var status = args[1];
                    Console.WriteLine($"🔄 Setting presence to {status}...");
                    response = await mcpClient.CallMcpToolAsync("SetPresence", new { availability = status },
                        userToken.Token);
                    Console.WriteLine($"✅ Presence Updated:");
                    Console.WriteLine($"   {response}");
                    break;

                default:
                    Console.WriteLine($"❌ Unknown presence command: {subCommand}");
                    Console.WriteLine("Available: me, user, set");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing presence command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing presence command: {ex.Message}");
            throw;
        }
    }
}
using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Routes commands to appropriate executors and handles common command patterns
/// </summary>
public static class CommandRouter
{
    /// <summary>
    /// Execute a command through the appropriate service handler
    /// </summary>
    public static async Task ExecuteCommandAsync(string command, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        
        Console.WriteLine($"\n🔄 Making real MCP call through Arkana Gateway:");
        Console.WriteLine($"   Command: {command}");
        Console.WriteLine($"   User: {userToken.Username}");
        Console.WriteLine($"   Headers: Authorization: Bearer {userToken.Token[..8]}...");
        Console.WriteLine($"   -> Gateway will exchange token and inject X-ARK-TOKEN for MCP server");
        Console.WriteLine();
        
        var mainCommand = parts[0].ToLower();
        string response;
        
        try
        {
            switch (mainCommand)
            {
                case "list":
                    Console.WriteLine("🔄 Listing all available MCP tools...");
                    response = await mcpClient.ListMcpToolsAsync(userToken.Token);
                    Console.WriteLine($"✅ Available MCP Tools:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                case "help":
                    ConsoleHelpService.ShowDetailedHelp();
                    return;
                    
                // MAIL SERVICE COMMANDS
                case "mail":
                    await MailCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // CALENDAR SERVICE COMMANDS
                case "calendar":
                    await CalendarCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // FILES SERVICE COMMANDS
                case "files":
                    await FilesCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // USER SERVICE COMMANDS
                case "users":
                    await UsersCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // GROUP SERVICE COMMANDS
                case "groups":
                    await GroupsCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // CONTACTS SERVICE COMMANDS
                case "contacts":
                    await ContactsCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // TASKS SERVICE COMMANDS
                case "tasks":
                    await TasksCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // NOTES SERVICE COMMANDS
                case "notes":
                    await NotesCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // PRESENCE SERVICE COMMANDS
                case "presence":
                    await PresenceCommandExecutor.ExecuteAsync(parts.Skip(1).ToArray(), userToken, mcpClient, logger);
                    break;
                    
                // BACKWARD COMPATIBILITY
                case "profile":
                    Console.WriteLine("🔄 Calling GetMyProfile...");
                    response = await mcpClient.CallMcpToolAsync("GetMyProfile", null, userToken.Token);
                    Console.WriteLine($"✅ Real Response from Microsoft Graph:");
                    Console.WriteLine($"   {response}");
                    break;
                    
                default:
                    Console.WriteLine($"❌ Unknown command: {mainCommand}");
                    Console.WriteLine("Available commands: mail, calendar, files, users, groups, contacts, tasks, notes, presence, list, help, exit");
                    Console.WriteLine("Type 'help' for detailed command information.");
                    break;
            }
            
            if (mainCommand != "help")
            {
                Console.WriteLine("   ⚡ Response received via Arkana Gateway proxy");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing command: {Command}", command);
            Console.WriteLine($"❌ Error executing command: {ex.Message}");
        }
    }
}
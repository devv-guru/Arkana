using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes tasks-related MCP commands
/// </summary>
public static class TasksCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing tasks command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Tasks command executed without subcommand");
            Console.WriteLine("❌ Tasks command requires subcommand. Try: list, lists, create, completed");
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
                    Console.WriteLine($"🔄 Getting {count} tasks...");
                    response = await mcpClient.CallMcpToolAsync("GetTasks", new { top = count }, userToken.Token);
                    Console.WriteLine($"✅ Tasks:");
                    Console.WriteLine($"   {response}");
                    break;

                case "lists":
                    Console.WriteLine("🔄 Getting task lists...");
                    response = await mcpClient.CallMcpToolAsync("GetTaskLists", null, userToken.Token);
                    Console.WriteLine($"✅ Task Lists:");
                    Console.WriteLine($"   {response}");
                    break;

                case "create":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("❌ Create requires title. Usage: tasks create <title>");
                        return;
                    }

                    var title = string.Join(" ", args.Skip(1));
                    Console.WriteLine($"🔄 Creating task '{title}'...");
                    response = await mcpClient.CallMcpToolAsync("CreateTask", new { title = title }, userToken.Token);
                    Console.WriteLine($"✅ Task Created:");
                    Console.WriteLine($"   {response}");
                    break;

                case "completed":
                    Console.WriteLine("🔄 Getting completed tasks...");
                    response = await mcpClient.CallMcpToolAsync("GetCompletedTasks", new { top = 10 }, userToken.Token);
                    Console.WriteLine($"✅ Completed Tasks:");
                    Console.WriteLine($"   {response}");
                    break;

                default:
                    Console.WriteLine($"❌ Unknown tasks command: {subCommand}");
                    Console.WriteLine("Available: list, lists, create, completed");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing tasks command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing tasks command: {ex.Message}");
            throw;
        }
    }
}
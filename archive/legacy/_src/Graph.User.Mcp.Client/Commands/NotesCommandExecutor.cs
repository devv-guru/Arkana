using Microsoft.Extensions.Logging;
using Graph.User.Mcp.Client.Models;
using Graph.User.Mcp.Client.Services;
using Serilog.Context;

namespace Graph.User.Mcp.Client.Commands;

/// <summary>
/// Executes notes-related MCP commands (OneNote)
/// </summary>
public static class NotesCommandExecutor
{
    public static async Task ExecuteAsync(string[] args, UserToken userToken, McpApiClient mcpClient, ILogger logger)
    {
        var correlationId = CorrelationService.GetOrCreate();
        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var userScope = LogContext.PushProperty("UserId", userToken.Username);
        
        logger.LogInformation("Executing notes command with args: {Args}", string.Join(" ", args));
        
        if (args.Length == 0)
        {
            logger.LogWarning("Notes command executed without subcommand");
            Console.WriteLine("❌ Notes command requires subcommand. Try: list, sections, pages");
            return;
        }
        
        var subCommand = args[0].ToLower();
        string response;

        try
        {
            switch (subCommand)
            {
                case "list":
                    logger.LogInformation("Getting OneNote notebooks");
                    Console.WriteLine("🔄 Getting OneNote notebooks...");
                    response = await mcpClient.CallMcpToolAsync("GetNotebooks", null, userToken.Token);
                    logger.LogInformation("Successfully retrieved OneNote notebooks");
                    Console.WriteLine($"✅ OneNote Notebooks:");
                    Console.WriteLine($"   {response}");
                    break;

                case "sections":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Sections command executed without notebook ID");
                        Console.WriteLine("❌ Sections requires notebook ID. Usage: notes sections <notebookId>");
                        return;
                    }

                    var notebookId = args[1];
                    logger.LogInformation("Getting sections for notebook: {NotebookId}", notebookId);
                    Console.WriteLine($"🔄 Getting sections for notebook {notebookId}...");
                    response = await mcpClient.CallMcpToolAsync("GetNotebookSections", new { notebookId = notebookId },
                        userToken.Token);
                    logger.LogInformation("Successfully retrieved sections for notebook: {NotebookId}", notebookId);
                    Console.WriteLine($"✅ Notebook Sections:");
                    Console.WriteLine($"   {response}");
                    break;

                case "pages":
                    if (args.Length < 2)
                    {
                        logger.LogWarning("Pages command executed without section ID");
                        Console.WriteLine("❌ Pages requires section ID. Usage: notes pages <sectionId>");
                        return;
                    }

                    var sectionId = args[1];
                    logger.LogInformation("Getting pages for section: {SectionId}", sectionId);
                    Console.WriteLine($"🔄 Getting pages for section {sectionId}...");
                    response = await mcpClient.CallMcpToolAsync("GetSectionPages", new { sectionId = sectionId },
                        userToken.Token);
                    logger.LogInformation("Successfully retrieved pages for section: {SectionId}", sectionId);
                    Console.WriteLine($"✅ Section Pages:");
                    Console.WriteLine($"   {response}");
                    break;

                default:
                    logger.LogWarning("Unknown notes subcommand: {SubCommand}", subCommand);
                    Console.WriteLine($"❌ Unknown notes command: {subCommand}");
                    Console.WriteLine("Available: list, sections, pages");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing notes command {SubCommand}", subCommand);
            Console.WriteLine($"❌ Error executing notes command: {ex.Message}");
            throw;
        }
    }
}
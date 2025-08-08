namespace Graph.User.Mcp.Client.Services;

/// <summary>
/// Service for displaying help information and command documentation
/// </summary>
public static class ConsoleHelpService
{
    public static void ShowWelcomeMessage()
    {
        Console.WriteLine("🤖 Arkana Microsoft Graph MCP Client");
        Console.WriteLine("====================================");
        Console.WriteLine("This client connects to Microsoft Graph MCP server via Arkana Gateway");
        Console.WriteLine();
    }

    public static void ShowServicesOverview()
    {
        Console.WriteLine("\n📋 Available Microsoft Graph Services (65+ Tools - Real API Calls):");
        var services = new[]
        {
            "📧 MAIL (12 tools): messages, send, draft, search, folders, attachments",
            "📅 CALENDAR (8 tools): events, meetings, Teams integration, invitations", 
            "📁 FILES (8 tools): OneDrive + SharePoint files, search, folders",
            "👤 USERS (8 tools): profiles, photos, search, management",
            "👥 GROUPS (6 tools): membership, management, search",
            "📇 CONTACTS (6 tools): contact management, search",
            "✅ TASKS (6 tools): To-Do integration, lists, completion",
            "📝 NOTES (6 tools): OneNote integration, notebooks, pages",
            "🟢 PRESENCE (5 tools): Teams status, availability"
        };
        
        foreach (var service in services)
        {
            Console.WriteLine($"  • {service}");
        }
        
        Console.WriteLine("\n🔧 Interactive Command Execution (Real API Calls):");
        Console.WriteLine("Available commands:");
        Console.WriteLine("  Service commands: mail, calendar, files, users, groups, contacts, tasks, notes, presence");
        Console.WriteLine("  General commands: list, help, exit");
        Console.WriteLine();
    }

    public static void ShowArchitectureInfo()
    {
        Console.WriteLine("\n🔌 MCP Client Architecture:");
        Console.WriteLine("  1. Client authenticates user with Azure AD (Windows Hello/PIN/biometrics)");
        Console.WriteLine("  2. Client sends requests to Gateway with Authorization: Bearer <oidc-token>");
        Console.WriteLine("  3. Gateway validates the OIDC token and user authentication");
        Console.WriteLine("  4. Gateway exchanges OIDC token for Microsoft Graph token (OBO flow)");
        Console.WriteLine("  5. Gateway injects Graph token in X-ARK-TOKEN header for MCP server");
        Console.WriteLine("  6. Gateway adds X-MS-Graph-Proxy: true validation header");
        Console.WriteLine("  7. MCP server receives Graph token and calls Microsoft Graph API");
    }

    /// <summary>
    /// Show detailed help system with all commands and examples
    /// </summary>
    public static void ShowDetailedHelp()
    {
        Console.WriteLine("\n📖 Microsoft Graph MCP Client - Command Reference");
        Console.WriteLine("==================================================");
        
        Console.WriteLine("\n📧 MAIL COMMANDS:");
        Console.WriteLine("  mail list [count]           - List recent emails (default: 10)");
        Console.WriteLine("  mail search <term>          - Search emails by subject/content");
        Console.WriteLine("  mail send <to> <subject>     - Send email (interactive body input)");
        Console.WriteLine("  mail drafts                  - List draft emails");
        Console.WriteLine("  mail folders                 - List mail folders");
        
        Console.WriteLine("\n📅 CALENDAR COMMANDS:");
        Console.WriteLine("  calendar list [count]        - List upcoming events (default: 10)");
        Console.WriteLine("  calendar today               - Show today's events");
        Console.WriteLine("  calendar create              - Create new event (interactive)");
        Console.WriteLine("  calendar meeting <title>     - Create Teams meeting (interactive)");
        
        Console.WriteLine("\n📁 FILES COMMANDS:");
        Console.WriteLine("  files list [count]           - List OneDrive files (default: 10)");
        Console.WriteLine("  files recent [count]         - List recently modified files");
        Console.WriteLine("  files search <term>          - Search files by name");
        Console.WriteLine("  files sharepoint             - List SharePoint sites");
        Console.WriteLine("  files sp-search <term>       - Search SharePoint sites");
        
        Console.WriteLine("\n👤 USERS COMMANDS:");
        Console.WriteLine("  users list [count]           - List organization users (default: 10)");
        Console.WriteLine("  users profile                - Get your profile");
        Console.WriteLine("  users search <term>          - Search users by name");
        Console.WriteLine("  users photo <email>          - Get user photo");
        
        Console.WriteLine("\n👥 GROUPS COMMANDS:");
        Console.WriteLine("  groups list [count]          - List groups (default: 10)");
        Console.WriteLine("  groups my                    - List your groups");
        Console.WriteLine("  groups members <groupId>     - Get group members");
        Console.WriteLine("  groups search <term>         - Search groups");
        
        Console.WriteLine("\n📇 CONTACTS COMMANDS:");
        Console.WriteLine("  contacts list [count]        - List contacts (default: 10)");
        Console.WriteLine("  contacts search <term>       - Search contacts");
        Console.WriteLine("  contacts folders             - List contact folders");
        
        Console.WriteLine("\n✅ TASKS COMMANDS:");
        Console.WriteLine("  tasks list [count]           - List tasks (default: 10)");
        Console.WriteLine("  tasks lists                  - List task lists");
        Console.WriteLine("  tasks create <title>         - Create new task");
        Console.WriteLine("  tasks completed              - List completed tasks");
        
        Console.WriteLine("\n📝 NOTES COMMANDS:");
        Console.WriteLine("  notes list                   - List OneNote notebooks");
        Console.WriteLine("  notes sections <notebookId>  - List sections in notebook");
        Console.WriteLine("  notes pages <sectionId>      - List pages in section");
        
        Console.WriteLine("\n🟢 PRESENCE COMMANDS:");
        Console.WriteLine("  presence me                  - Get your presence status");
        Console.WriteLine("  presence user <email>        - Get user presence");
        Console.WriteLine("  presence set <status>        - Set your presence (Available/Busy/DoNotDisturb/Away)");
        
        Console.WriteLine("\n🔧 GENERAL COMMANDS:");
        Console.WriteLine("  list                         - List all available MCP tools");
        Console.WriteLine("  help                         - Show this help");
        Console.WriteLine("  exit                         - Exit the client");
        
        Console.WriteLine("\n💡 Examples:");
        Console.WriteLine("  mail list 5");
        Console.WriteLine("  calendar today");
        Console.WriteLine("  files search report");
        Console.WriteLine("  users search john");
        Console.WriteLine("  groups my");
    }

    public static void ShowConnectionDiagnostics(string gatewayUrl, bool gatewayHealthy, bool mcpHealthy)
    {
        Console.WriteLine($"Gateway URL: {gatewayUrl}");
        Console.WriteLine();

        Console.WriteLine("\n🔌 Testing Connectivity:");
        Console.WriteLine($"   Testing Gateway health...");
        Console.WriteLine($"   Gateway: {(gatewayHealthy ? "✅ Healthy" : "❌ Unhealthy")}");
        
        Console.WriteLine($"   Testing MCP server health...");
        Console.WriteLine($"   MCP Server: {(mcpHealthy ? "✅ Healthy" : "❌ Unhealthy")}");
        
        if (!gatewayHealthy)
        {
            Console.WriteLine("\n❌ Gateway is not accessible. Please check:");
            Console.WriteLine("   1. Start Aspire: cd C:\\source\\Arkana\\_aspire\\AspireApp1.AppHost && dotnet run");
            Console.WriteLine("   2. Check Aspire dashboard for actual Gateway URL");
            Console.WriteLine("   3. Update Gateway:BaseUrl in appsettings.json if port is different");
            Console.WriteLine($"   4. Currently trying: {gatewayUrl}");
            
            Console.WriteLine("\n🔧 Alternative: Test directly with MCP server (bypass Gateway)");
            Console.WriteLine("   Gateway might be on different port - check Aspire dashboard");
        }
    }
}
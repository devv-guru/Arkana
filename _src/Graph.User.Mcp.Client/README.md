# Microsoft Graph MCP Client

A comprehensive client for interacting with Microsoft Graph services through the Model Context Protocol (MCP) via Arkana Gateway.

## 🏗️ Project Structure

```
Graph.User.Mcp.Client/
├── Program.cs                          # Main application entry point (clean & minimal)
├── appsettings.json                    # Configuration settings
├── DEPLOYMENT.md                       # Complete deployment guide
├── README.md                           # This file
│
├── Authentication/                     # Authentication services
│   └── WindowsHelloAuthService.cs     # Microsoft Entra ID + Windows Hello integration
│
├── Commands/                           # Command execution services
│   ├── CommandRouter.cs               # Routes commands to appropriate executors
│   ├── MailCommandExecutor.cs         # Email service commands
│   ├── CalendarCommandExecutor.cs     # Calendar service commands  
│   ├── FilesCommandExecutor.cs        # OneDrive + SharePoint commands
│   ├── UsersCommandExecutor.cs        # User service commands
│   ├── GroupsCommandExecutor.cs       # Groups service commands
│   ├── ContactsCommandExecutor.cs     # Contacts service commands
│   ├── TasksCommandExecutor.cs        # Tasks service commands
│   ├── NotesCommandExecutor.cs        # Notes service commands
│   └── PresenceCommandExecutor.cs     # Presence service commands
│
├── Models/                             # Data models
│   └── UserToken.cs                   # User authentication token model
│
└── Services/                           # Core services
    ├── McpApiClient.cs                # MCP protocol client for Gateway communication
    └── ConsoleHelpService.cs          # Help and documentation display
```

## ✨ Key Features

### 🔐 **Secure Authentication**
- **Windows Hello Integration**: Biometric authentication (fingerprint, face, iris)
- **Microsoft Entra ID OIDC**: Enterprise-grade identity integration
- **Hardware Security**: TPM-backed credential storage
- **Token Management**: Automatic refresh and caching

### 📡 **Gateway Communication**
- **MCP Protocol**: Standards-compliant Model Context Protocol
- **Token Exchange**: OIDC-to-Graph token exchange via On-Behalf-Of flow
- **Health Monitoring**: Real-time connectivity and service health checks
- **Error Handling**: Comprehensive error recovery and user feedback

### 🎯 **65+ Microsoft Graph Tools**
- **📧 Mail (12 tools)**: Send, search, drafts, folders, attachments
- **📅 Calendar (8 tools)**: Events, meetings, Teams integration
- **📁 Files (8 tools)**: OneDrive + SharePoint file operations
- **👤 Users (8 tools)**: Profiles, photos, search, management
- **👥 Groups (6 tools)**: Membership, management, search
- **📇 Contacts (6 tools)**: Contact management and search
- **✅ Tasks (6 tools)**: To-Do integration and task management
- **📝 Notes (6 tools)**: OneNote notebooks and pages
- **🟢 Presence (5 tools)**: Teams status and availability

### 🏢 **Enterprise Ready**
- **Production Deployment**: Complete deployment scripts and documentation
- **Configuration Management**: Environment-based configuration
- **Logging & Telemetry**: Structured logging with correlation IDs
- **Error Recovery**: Graceful fallbacks and user guidance

## 🚀 Quick Start

### Prerequisites
- Windows 10/11 with Windows Hello
- Microsoft Entra ID account with Microsoft 365
- .NET 9.0 Runtime

### Installation
```bash
# Clone and build
git clone https://github.com/your-org/arkana
cd arkana/_src/Graph.User.Mcp.Client
dotnet restore
dotnet build --configuration Release

# Run the client
dotnet run
```

### Configuration

#### Prerequisites
**🚨 Microsoft Entra ID Setup Required**: Before configuring the client, you must set up Microsoft Entra ID application registrations. Follow the [**Microsoft Entra ID Setup Guide**](../../AZURE_AD_SETUP.md) in the solution root.

#### Local Configuration
Create `appsettings.local.json` with values from your Microsoft Entra ID app registrations:
```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-app-registration-id"
  },
  "Gateway": {
    "ApiScope": "api://your-gateway-app-registration-id/obo"
  }
}
```

> **Note:** `appsettings.local.json` is excluded from source control via `.gitignore` to protect sensitive configuration data. The main `appsettings.json` contains safe defaults and logging configuration.

## 📖 Usage Examples

### Mail Operations
```bash
# List recent emails
mail list 10

# Search emails
mail search "project update"

# Send email with interactive body input
mail send john@company.com "Meeting Tomorrow"

# List draft emails
mail drafts

# View mail folders
mail folders
```

### Calendar Operations
```bash
# Today's events
calendar today

# Create Teams meeting with interactive setup
calendar meeting "Project Review"

# List upcoming events
calendar list 5

# Create regular event
calendar create
```

### File Operations
```bash
# List OneDrive files
files list 10

# Search files
files search "budget report"

# SharePoint sites
files sharepoint

# Search SharePoint
files sp-search "marketing"

# Recent files
files recent 5
```

### Advanced Commands
```bash
# Get your profile
users profile

# Search organization users
users search "john smith"

# Your Teams groups
groups my

# Set presence status
presence set Busy

# Create task
tasks create "Review proposal"

# List OneNote notebooks
notes list
```

### Help System
```bash
# Show all available commands
help

# List all MCP tools
list

# Exit the client
exit
```

## 🔧 Architecture

### Flow Diagram
```
[User] → [Windows Hello] → [Microsoft Entra ID] → [MCP Client] 
   ↓
[Arkana Gateway] → [Token Exchange] → [Graph.User.Mcp.Server]
   ↓
[Microsoft Graph API] → [Microsoft 365 Services]
```

### Security Model
1. **User Authentication**: Windows Hello + Microsoft Entra ID OIDC
2. **Gateway Communication**: Bearer token with OIDC JWT
3. **Token Exchange**: Gateway performs On-Behalf-Of flow
4. **API Access**: Server uses Graph tokens for Microsoft 365
5. **Zero Trust**: Every request authenticated and authorized

## 🎨 Code Organization Principles

### **Separation of Concerns**
- **Authentication**: Isolated in `Authentication/` namespace
- **Commands**: Each service has dedicated executor
- **Models**: Clean data transfer objects
- **Services**: Core business logic separation

### **Dependency Injection**
- **Scoped Services**: Authentication and API clients
- **Configuration**: Environment-based settings
- **Logging**: Structured logging throughout

### **Error Handling**
- **Graceful Degradation**: Fallback authentication modes
- **User Feedback**: Clear error messages and guidance
- **Correlation Tracking**: End-to-end request tracing

### **Extensibility**
- **Command Pattern**: Easy to add new service commands
- **Plugin Architecture**: Service executors are modular
- **Configuration Driven**: Behavior controlled via settings

## 🔍 Troubleshooting

### Common Issues
1. **Authentication Failed**: Check Windows Hello setup and Microsoft Entra ID registration
2. **Gateway Unreachable**: Verify Gateway URL and network connectivity
3. **Permission Denied**: Ensure Microsoft Graph permissions and admin consent
4. **Command Not Found**: Use `help` to see all available commands

### Debug Mode
```bash
# Set environment variable for detailed logging
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

## 📚 Documentation

- **[DEPLOYMENT.md](DEPLOYMENT.md)**: Complete deployment guide
- **Type `help`**: Interactive command reference
- **Microsoft Entra ID Setup**: App registration and permission configuration
- **Gateway Configuration**: Arkana Gateway setup requirements

## 🤝 Contributing

1. Follow existing code organization patterns
2. Add new service executors in `Commands/` folder
3. Update help system for new commands
4. Include comprehensive error handling
5. Test with real Microsoft Graph API calls

## 📄 License

This project is licensed under the MIT License.
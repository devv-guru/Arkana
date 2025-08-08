# Microsoft Graph MCP Client - Deployment Guide

## 🚀 Quick Start for End Users

### Prerequisites
- Windows 10/11 with Windows Hello capability
- Azure AD account with Microsoft 365 access
- PowerShell 5.1 or newer
- .NET 9.0 Runtime (will be auto-installed if missing)

### One-Click Deployment

1. **Download and Run Deployment Script:**
   ```powershell
   # Run as Administrator
   iex ((New-Object System.Net.WebClient).DownloadString('https://your-domain.com/deploy-mcp-client.ps1'))
   ```

2. **Or Manual Installation:**
   ```powershell
   # Clone the repository
   git clone https://github.com/your-org/arkana
   cd arkana\_src\Graph.User.Mcp.Client
   
   # Install dependencies
   dotnet restore
   
   # Build the client
   dotnet build --configuration Release
   
   # Run the client
   dotnet run
   ```

### Configuration

1. **Update `appsettings.json`** with your organization's settings:
   ```json
   {
     "AzureAd": {
       "TenantId": "your-tenant-id",
       "ClientId": "your-app-registration-id"
     },
     "Gateway": {
       "BaseUrl": "https://your-gateway.com",
       "McpEndpoint": "/mcp/msgraph",
       "ApiScope": "api://your-app-id/obo"
     }
   }
   ```

2. **Azure AD App Registration:**
   
   **⚠️ IMPORTANT**: Complete Azure AD setup is required before deployment.
   
   📋 **Follow the comprehensive [Azure AD Setup Guide](../../AZURE_AD_SETUP.md)** which covers:
   - Creating Gateway and Client app registrations
   - Configuring proper permissions and scopes  
   - Setting up On-Behalf-Of (OBO) token exchange
   - Security best practices and troubleshooting
   
   **Quick Reference - Required Permissions:**
   - User.Read, User.ReadBasic.All, User.Read.All
   - Mail.ReadWrite, Mail.Send  
   - Calendars.ReadWrite
   - Files.ReadWrite.All, Sites.Read.All
   - Directory.Read.All, Group.Read.All, GroupMember.Read.All
   - Notes.Read.All, Presence.Read.All
   - Tasks.ReadWrite, Contacts.ReadWrite

## 🎯 Available Services & Commands

### 📧 **Mail Service (12 tools)**
```bash
# List recent emails
mail list 10

# Search emails
mail search "project update"

# Send email
mail send john@company.com "Meeting Tomorrow"
# (interactive body input)

# View drafts
mail drafts

# List mail folders
mail folders
```

### 📅 **Calendar Service (8 tools)**
```bash
# List upcoming events
calendar list 5

# Today's events
calendar today

# Create event (interactive)
calendar create

# Create Teams meeting
calendar meeting "Project Review"
# (interactive attendees and time input)
```

### 📁 **Files Service (8 tools)**
```bash
# List OneDrive files
files list 10

# Recent files
files recent 5

# Search files
files search "budget report"

# SharePoint sites
files sharepoint

# Search SharePoint
files sp-search "marketing"
```

### 👤 **Users Service (8 tools)**
```bash
# List organization users
users list 10

# Your profile
users profile

# Search users
users search "john smith"

# Get user photo
users photo john@company.com
```

### 👥 **Groups Service (6 tools)**
```bash
# List groups
groups list 10

# Your groups
groups my

# Group members
groups members group-id-here

# Search groups
groups search "marketing"
```

### 📇 **Contacts Service (6 tools)**
```bash
# List contacts
contacts list 10

# Search contacts
contacts search "johnson"

# Contact folders
contacts folders
```

### ✅ **Tasks Service (6 tools)**
```bash
# List tasks
tasks list 10

# Task lists
tasks lists

# Create task
tasks create "Review proposal"

# Completed tasks
tasks completed
```

### 📝 **Notes Service (6 tools)**
```bash
# List notebooks
notes list

# Notebook sections
notes sections notebook-id

# Section pages
notes pages section-id
```

### 🟢 **Presence Service (5 tools)**
```bash
# Your presence
presence me

# User presence
presence user john@company.com

# Set presence
presence set Busy
```

## 🔧 Advanced Features

### File Attachments (OneDrive + SharePoint)
The client supports advanced file attachment formats:

```bash
# OneDrive file
mail send john@company.com "Report" --attach "01BYE5RZ56Y2GOVW7725BZO354PWSELRRZ"

# SharePoint drive file
mail send jane@company.com "Docs" --attach "drive:b!abc123:01BYE5RZ123"

# SharePoint site file
mail send team@company.com "Proposal" --attach "site:company.sharepoint.com,def456:01BYE5RZ456"

# Mixed attachments
mail send all@company.com "Resources" --attach "01BYE5RZ123,drive:b!abc:456,site:sp.com,789:012"
```

### Interactive Help
```bash
# General help
help

# Service-specific examples
mail --help
calendar --help
files --help
```

## 🔐 Authentication Modes

### Windows Hello (Recommended)
- Biometric authentication (fingerprint, face, iris)
- Hardware security key support
- Seamless Azure AD integration

### PIN Authentication
- 6+ digit PIN with complexity rules
- Fallback when biometrics unavailable

### Demo Mode (Development Only)
- Username-only authentication
- Mock tokens for testing
- **NOT FOR PRODUCTION USE**

## 🏗️ Architecture

```
[User] → [Graph.User.Mcp.Client] → [Arkana Gateway] → [Graph.User.Mcp.Server] → [Microsoft Graph API]

1. Client authenticates user (Windows Hello + Azure AD)
2. Client sends OIDC token to Gateway
3. Gateway validates token and exchanges for Graph token (OBO flow)
4. Gateway forwards MCP requests with Graph token
5. MCP Server calls Microsoft Graph API
6. Response flows back through Gateway to Client
```

## 🚨 Security Features

- **Zero Trust Architecture**: Every request is authenticated and authorized
- **Token Isolation**: Client never sees Graph tokens directly
- **Hardware Security**: Windows Hello uses TPM/secure hardware
- **Audit Logging**: All API calls are logged and traceable
- **Rate Limiting**: Built-in protection against abuse
- **Correlation Tracking**: End-to-end request tracing

## 📊 Monitoring & Troubleshooting

### Health Checks
```bash
# Test connectivity
list

# View detailed status
help
```

### Common Issues

1. **Authentication Failed**
   - Ensure Windows Hello is configured
   - Check Azure AD app registration
   - Verify tenant ID and client ID

2. **Gateway Unreachable**
   - Check Gateway URL in configuration
   - Verify network connectivity
   - Ensure Gateway is running

3. **Permission Denied**
   - Check Microsoft Graph permissions
   - Verify admin consent granted
   - Ensure user has required licenses

### Logs
- Client logs: Console output with correlation IDs
- Gateway logs: Check Aspire dashboard
- Server logs: Application Insights telemetry

## 🔄 Updates & Maintenance

### Auto-Update (Planned)
- Automatic client updates via Windows Package Manager
- Configuration preservation across updates
- Rollback capability

### Manual Update
```bash
git pull origin main
dotnet build --configuration Release
```

## 🆘 Support

- **Documentation**: https://docs.your-org.com/mcp-client
- **Issues**: https://github.com/your-org/arkana/issues
- **Support**: mcp-support@your-org.com
- **Status**: https://status.your-org.com

## 📋 Deployment Checklist

### For IT Administrators
- [ ] Azure AD app registration configured
- [ ] Graph permissions granted and admin consent provided  
- [ ] Gateway deployed and accessible
- [ ] MCP Server deployed and healthy
- [ ] Network firewall rules configured
- [ ] Monitoring and alerting configured
- [ ] User training materials prepared

### For End Users
- [ ] Windows Hello configured on device
- [ ] Azure AD account with M365 license
- [ ] Client application installed
- [ ] Configuration file updated
- [ ] Initial authentication completed
- [ ] Basic command training completed

## 🎓 Training Resources

### Quick Start Video
- 5-minute walkthrough of basic commands
- Authentication setup demonstration
- Common use case examples

### Command Reference Card
- Printable reference with all commands
- Syntax examples and parameter explanations
- Troubleshooting quick fixes

### Advanced Usage Guide
- File attachment workflows
- Batch operations
- Integration with other tools
# Arkana MCP Bridge

A secure bridge that connects MCP clients (Claude Desktop, VSCode, Cline, Cursor, Windsurf, etc.) to the Arkana Enterprise MCP Gateway, providing enterprise-grade authentication and access control for MCP tools.

## Overview

This bridge serves as an intermediary between any MCP client and the Arkana Gateway, handling:
- **Cross-platform Azure authentication** (Windows Hello, Device Code Flow, etc.)
- **MCP protocol communication** via JSON-RPC over stdin/stdout
- **Tool discovery** filtered by user permissions via gateway RBAC
- **Secure tool execution** with proper token forwarding

## Architecture

```
MCP Client        →  MCP Bridge (this)  →  Arkana Gateway  →  Backend Tools
(Claude/VSCode/      (Stdio/JSON-RPC)      (HTTPS/JWT)      (Graph, etc.)
 Cline/Cursor/etc.)
```

## Features

- ✅ **Cross-platform authentication**: Windows Hello, Azure CLI, Device Code Flow
- ✅ **MCP 2024-11-05 protocol** compliance
- ✅ **Enterprise security**: JWT token handling, secure credential storage  
- ✅ **Dynamic tool discovery**: Only shows tools user is authorized to access
- ✅ **Streaming support**: Pass-through for streaming tool responses
- ✅ **Configuration management**: CLI args, environment variables, config files

## Installation & Deployment

### Enterprise Deployment (Recommended)

Deploy as framework-dependent DLL via Group Policy:

```bash
# Build
dotnet publish -c Release --no-self-contained -o dist/

# Deploy to enterprise machines
# C:\Program Files\CompanyName\ArkanaMcpBridge\
#   ├── Arkana.Mcp.Bridge.dll
#   ├── Arkana.Mcp.Bridge.deps.json  
#   ├── Arkana.Mcp.Bridge.runtimeconfig.json
#   └── appsettings.json (with enterprise gateway URL)
```

### Local Development

```bash
# Clone and build
git clone <repo-url>
cd _src/Arkana.Mcp.Bridge
dotnet restore
dotnet build

# Run
dotnet run -- --gateway-url https://your-gateway.internal
```

## Configuration

### MCP Client Setup

#### Claude Desktop
Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "arkana-bridge": {
      "command": "dotnet",
      "args": [
        "C:\\Program Files\\CompanyName\\ArkanaMcpBridge\\Arkana.Mcp.Bridge.dll"
      ]
    }
  }
}
```

#### VSCode with MCP Extension
Add to VSCode settings or MCP extension config:
```json
{
  "mcp.servers": {
    "arkana-bridge": {
      "command": "dotnet",
      "args": [
        "C:\\Program Files\\CompanyName\\ArkanaMcpBridge\\Arkana.Mcp.Bridge.dll"
      ]
    }
  }
}
```

#### Cline, Cursor, Windsurf
These editors typically use the same MCP configuration pattern:
```json
{
  "mcpServers": {
    "arkana-bridge": {
      "command": "dotnet", 
      "args": ["C:\\Program Files\\CompanyName\\ArkanaMcpBridge\\Arkana.Mcp.Bridge.dll"]
    }
  }
}
```

### Bridge Configuration

#### appsettings.json (Enterprise)
```json
{
  "Gateway": {
    "Url": "https://your-arkana-gateway.internal",
    "Timeout": "00:00:30"
  },
  "Authentication": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "Scopes": ["api://your-gateway-app-id/.default"]
  }
}
```

#### Command Line Options
```bash
dotnet run -- \
  --gateway-url https://gateway.company.com \
  --tenant-id your-tenant-id \
  --client-id your-client-id \
  --verbose
```

#### Environment Variables
```bash
ARKANA_Gateway__Url=https://gateway.company.com
ARKANA_Authentication__TenantId=your-tenant-id
ARKANA_Authentication__ClientId=your-client-id
```

## Authentication Flow

### Windows
1. **Managed Identity** (if running on Azure VM)
2. **Visual Studio/VS Code** (if authenticated)
3. **Azure CLI** (if `az login` completed)
4. **Interactive Browser** (fallback)

### Linux/macOS  
1. **Managed Identity** (if running on Azure)
2. **Azure CLI** (if `az login` completed)
3. **Device Code Flow** (user-friendly fallback)

## Development

### Project Structure
```
Arkana.Mcp.Bridge/
├── Models/                 # MCP protocol models
├── Services/              # Core services
│   ├── AuthenticationService.cs    # Azure auth handling
│   ├── GatewayProxyService.cs     # Gateway communication  
│   └── McpProtocolService.cs      # MCP protocol handler
├── Configuration/         # Configuration models
└── Program.cs            # Entry point & DI setup
```

### Key Dependencies
- **Azure.Identity**: Cross-platform Azure authentication
- **Microsoft.Extensions.Hosting**: Background service host
- **System.CommandLine**: CLI argument parsing
- **System.Text.Json**: JSON serialization

### Building

```bash
# Development build
dotnet build

# Production build (framework-dependent)
dotnet publish -c Release --no-self-contained

# Self-contained executable (if needed)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Troubleshooting

### Authentication Issues

**Problem**: `Failed to obtain access token`  
**Solution**: 
- Ensure Azure CLI login: `az login --tenant your-tenant-id`
- Check tenant/client IDs in configuration
- Verify network connectivity to Azure endpoints

**Problem**: `Device code authentication required`  
**Solution**: Follow the displayed instructions to visit the URL and enter the code

### MCP Protocol Issues

**Problem**: MCP client shows "Server not responding"  
**Solution**:
- Check bridge logs for errors
- Verify gateway URL is accessible
- Test authentication separately: `dotnet run -- --verbose`

**Problem**: No tools visible in MCP client  
**Solution**:
- Check user permissions in Arkana Gateway admin UI
- Verify RBAC configuration
- Check gateway `/tools` endpoint returns data

### Network Issues

**Problem**: `Gateway error (503)`  
**Solution**:
- Check gateway health endpoint
- Verify network connectivity
- Check firewall/proxy settings

## Security Considerations

- **Token Storage**: Tokens are handled in memory only, never persisted
- **Network Security**: All gateway communication uses HTTPS with JWT auth
- **Credential Management**: Uses Azure Identity best practices
- **Audit Logging**: All tool calls logged through gateway audit system

## Support

For issues:
1. Check this README and troubleshooting section
2. Review application logs (`--verbose` flag)
3. Verify gateway connectivity and user permissions
4. Create GitHub issue with logs and configuration (redact secrets)
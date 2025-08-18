# MCP SECURITY GATEWAY

A YARP‑powered, identity‑aware reverse proxy that brokers access between AI agents and Model Context Protocol (MCP) tool servers. The gateway enforces Azure Entra ID authentication, performs per‑tool OAuth 2.0 On‑Behalf‑Of (OBO) token exchange, and exposes a dynamic registry so each user only discovers and invokes the tools they are entitled to.

---

## Table of Contents

1. [Key Features](#key-features)
2. [Architecture](#architecture)
3. [Standards & Design Principles](#standards--design-principles)
4. [Prerequisites](#prerequisites)
5. [Quick Start](#quick-start)
6. [Tool Server Contract](#tool-server-contract)
7. [Access Control](#access-control)
8. [Configuration](#configuration)
9. [Deployment on Azure](#deployment-on-azure)
10. [Operational Guides](#operational-guides)
11. [Extending the Gateway](#extending-the-gateway)
12. [MCP Client Integration](#-mcp-client-integration)
13. [Contributing](#contributing)
14. [License](#license)

---

## Key Features

| Category                   | Capability                                                                                                  |
| -------------------------- | ----------------------------------------------------------------------------------------------------------- |
| **Identity Enforcement**   | Validates incoming Azure AD JWTs; rejects unauthenticated traffic.                                          |
| **Token Mediation**        | Runs OAuth 2.0 OBO to obtain the exact `requiredScope` declared by each tool server.                        |
| **Fine‑Grained Discovery** | `/tools` endpoint returns only the tools the caller is authorised to see.                                   |
| **Dynamic Registry**       | Tool servers self‑register via a signed manifest; routes and clusters reload at runtime—no redeploys.       |
| **Protocol Awareness**     | Parses MCP `tools/list` and `tools/call` payloads to apply policy, transform headers, and stream responses. |
| **Observability**          | Emits OpenTelemetry traces; exposes `/metrics` (Prometheus) and `/healthz`.                                 |
| **Pluggable UI**           | Admin SPA for managing tool metadata, RBAC mappings, and viewing live health.                               |
| **Scalability**            | Stateless app with distributed token cache (Redis) and hot‑reload config store (SQL/Cosmos).                |

---

## Architecture

```
+---------+      JWT       +----------------------+   OBO   +----------------+
|  Agent  | ───────────▶ |  MCP Security Gateway | ───────▶ |  MCP Tool API  |
| (Teams) |               |   (YARP + .NET 8)     |          |  (container)   |
+---------+               +----------------------+          +----------------+
     ▲                           ▲        │                         │
     │ TLS 1.2                   │        │ Redis / SQL             │
     │                           │        ▼                         ▼
User authentication      Azure Entra ID   Token cache        Backend systems
```

**Flow #1 – Delegated user call**

1. Agent obtains AAD access token for the gateway.
2. User calls `/tools/call` via gateway with token.
3. Gateway validates token, looks up `requiredScope` for the tool, executes OBO and caches the new token.
4. Gateway forwards request with `Authorization: Bearer <TokenB>` to tool.
5. Tool validates TokenB and fulfils the request.

---

## Standards & Design Principles

* **OAuth 2.0 & OIDC** – RFC 6749, RFC 8414, RFC 7523.
* **JWT** – RFC 7519 with HS256/RS256; validates `iss`, `aud`, `exp`, `nbf`, `azp`.
* **MCP Tool Contract** – `/tools/list`, `/tools/call`, JSON Schema for manifests.
* **Transport Security** – TLS 1.2+, HSTS.
* **Observability** – OpenTelemetry spans; W3C Trace‑Context headers.
* **API Hardening** – OWASP API Security Top 10 (2023).

---

## Prerequisites

| Requirement             | Version                  |
| ----------------------- | ------------------------ |
| .NET SDK                | 8.0 or later             |
| YARP                    | 3.1.0+                   |
| Azure CLI               | 2.60+                    |
| Node .js (for admin UI) | 20.x                     |
| Redis (token cache)     | 6.x+                     |
| SQL Server / Cosmos DB  | Any supported by EF Core |

---

## Quick Start

```bash
# 1. Clone repository
$ git clone https://github.com/your-org/mcp-gateway.git && cd mcp-gateway

# 2. Setup Microsoft Entra ID application registrations
#   – Follow the Microsoft Entra ID Setup Guide: AZURE_AD_SETUP.md

# 3. Configure local settings (create appsettings.local.json files)  
#   – See "Local Development Setup" section below for details

# 4. Spin up dependencies (Redis + SQL) with docker‑compose
$ docker compose up -d redis sql

# 5. Run the gateway
$ dotnet run --project src/Gateway

# 6. Register a sample tool
$ curl -X POST http://localhost:5000/registry/register \
       -H "Content-Type: application/json" \
       -d '@samples/calculator/manifest.json'
```

Open [http://localhost:5173](http://localhost:5173) to access the Admin UI.

---

## Tool Server Contract

```jsonc
// manifest.json
{
  "toolId": "b7b8f960-5ad3-4ad3-9d3f-17efc7c44145",
  "name": "calculator",
  "description": "Performs basic arithmetic.",
  "version": "1.0.0",
  "requiredScope": "api://CALC_TOOL/.default",
  "transport": "streamable-http",
  "inputSchema": { "$schema": "https://json-schema.org/draft/2020-12/schema", "type": "object", "properties": {"a":{"type":"number"},"b":{"type":"number"},"op":{"enum":["add","sub","mul","div"]}}, "required":["a","b","op"]},
  "outputSchema": { "$schema": "https://json-schema.org/draft/2020-12/schema", "type": "object", "properties": {"result":{"type":"number"}} },
  "tags": ["math"]
}
```

* **`/tools/list`** – returns array of manifest summaries ⬆︎.
* **`/tools/call`** – accepts `{ toolId, input }` and streams `output`.
* **`/healthz`** – 200 OK when healthy, 503 otherwise.

---

## Access Control

| Level               | Mechanism                                                            |
| ------------------- | -------------------------------------------------------------------- |
| **Gateway ingress** | Validates Microsoft Entra ID token; rejects if not in tenant.                  |
| **Discovery**       | Looks up caller’s `roles`/`groups` claim; filters `/tools` response. |
| **Invocation**      | Enforces same RBAC matrix before forwarding.                         |
| **Tool server**     | Verifies scoped TokenB; rejects if missing `requiredScope`.          |

RBAC matrix is stored in `dbo.ToolAccess` (`ToolId`, `AllowedRole`). Manage via Admin UI or SQL scripts.

---

## Configuration

| Name                  | Description                | Example                               |
| --------------------- | -------------------------- | ------------------------------------- |
| `AZURE_TENANT_ID`     | Entra tenant GUID          | `d771...`                             |
| `AZURE_CLIENT_ID`     | Gateway app client ID      | `1f01...`                             |
| `AZURE_CLIENT_SECRET` | (Dev only) client secret   | `xxxyyy`                              |
| `REDIS_URL`           | Token‑cache endpoint       | `redis:6379`                          |
| `SQL_CONNECTION`      | Tool registry & RBAC store | `Server=sql;Database=mcp;User=sa;...` |
| `UI_BASE_URL`         | Admin SPA mount path       | `/admin`                              |

All config keys can also come from Azure Key Vault when deployed.

---

## Deployment on Azure

1. **Create resource group & App Service plan**

   ```bash
   az group create -n mcp-rg -l westeurope
   az appservice plan create -g mcp-rg -n mcp-plan --sku P1v3 --is-linux
   ```
2. **Provision Web App + Managed Identity**

   ```bash
   az webapp create -g mcp-rg -p mcp-plan -n mcp-gateway --runtime "DOTNET|8.0"
   az webapp identity assign -g mcp-rg -n mcp-gateway
   ```
3. **Configure settings** (`AZURE_TENANT_ID`, `REDIS_URL`, etc.)
4. **Deploy** via GitHub Actions or `az webapp deploy`.
5. **Grant MI** permission to Key Vault & tool APIs (`requiredScope`).
6. **Point DNS** or API Management / Front Door to the Web App.

Scaling: Increase instances or move to **Azure Container Apps / AKS**; the gateway is stateless except the distributed Redis cache.

---

## Operational Guides

* **Monitoring** – Enable Application Insights; scrape `/metrics` to Prometheus; set alert rules for 5xx spikes.
* **Rotation** – Use Key Vault auto‑rotation for certificates; gateway picks up via MSI.
* **Backups** – SQL/Cosmos daily; Redis persistence if needed.
* **Incident Response** – Disable a tool by flipping `Enabled=false` in DB; gateway reloads config in <1 s.

---

## Extending the Gateway

| Extension point               | How                                                                 |
| ----------------------------- | ------------------------------------------------------------------- |
| **Custom request transforms** | Implement `ITransformProvider` and register in DI.                  |
| **Alternate IdPs**            | Add additional authentication schemes (`AddJwtBearer("Okta" ...)`). |
| **Policy plugins**            | Use `EndpointFilter` to inject content scans or rate‑limit logic.   |
| **UI additions**              | Admin SPA lives in `src/Admin`; React + Tailwind.                   |

---

## 🖥️ MCP Client Integration

### Overview

To integrate with **MCP clients** (Claude Desktop, VSCode, Cline, Cursor, Windsurf, etc.), you need a lightweight intermediary MCP server that handles local authentication and proxies requests to the Arkana gateway. This approach provides enterprise security while maintaining a simple user experience.

### Architecture

```
MCP Client        →  Local MCP Bridge  →  Arkana Gateway  →  Backend Tools
(Claude/VSCode/      (Single Server)      (RBAC + Auth)     (Graph, etc.)
 Cline/Cursor/etc.)
```

### Why This Approach?

| **Single MCP Bridge Benefits** | **Details** |
|-------------------------------|-------------|
| **Leverages Gateway RBAC** | Gateway's `/tools/list` returns only user-authorized tools |
| **Simplified Client Config** | User configures one MCP server instead of multiple |
| **Centralized Management** | Admin UI manages all tool access from one location |
| **Consistent Audit Logging** | All tool calls logged through gateway |

### Implementation Strategy

Your lightweight MCP bridge server should:

1. **Local Authentication**: Use Windows Hello → Azure Entra ID token exchange
2. **Tool Discovery**: Proxy gateway's `/tools/list` endpoint (filtered by user roles)
3. **Tool Execution**: Forward `/tools/call` requests to gateway with user's token
4. **Response Streaming**: Pass through streaming responses from gateway

### Existing Foundation

Leverage these existing components:

- **Bridge Implementation**: `_src/Arkana.Mcp.Bridge/` (framework-dependent DLL)
- **Cross-Platform Auth**: Windows Hello, Azure CLI, Device Code Flow support
- **Gateway Integration**: JWT token handling and RBAC enforcement

### Configuration Examples

#### Claude Desktop
`claude_desktop_config.json`:
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

#### Other MCP Clients (VSCode, Cline, Cursor, Windsurf)
Most MCP clients use similar configuration patterns:
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

### Enterprise Deployment

Deploy bridge via Group Policy:
```bash
# Build framework-dependent DLL
dotnet publish -c Release --no-self-contained -o dist/

# Deploy to standard location:
# C:\Program Files\CompanyName\ArkanaMcpBridge\
```

This gives you **enterprise security** (via Arkana gateway) with **simple user experience** (single MCP server for any MCP client).

---

## Contributing

1. Fork the repo and create a feature branch.
2. Follow the coding standards (`dotnet format`, ESLint for UI).
3. Write unit/integration tests (`dotnet test`).
4. Submit a PR with a clear description.

We use **Conventional Commits** and **Semantic Versioning**.

---

## 🔧 Local Development Setup

### Prerequisites

1. **Microsoft Entra ID Setup Required**: Follow the [**Microsoft Entra ID Setup Guide**](AZURE_AD_SETUP.md) to create application registrations
2. **Local Configuration**: Create `appsettings.local.json` files as described below

### Configuration Pattern

Each project uses **`appsettings.local.json`** files for sensitive configuration data. These files are excluded from source control via `.gitignore` to protect credentials.

### Required Local Configuration Files

#### 1. **MCP Client** (`_src/Graph.User.Mcp.Client/appsettings.local.json`)
```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id", 
    "ClientId": "your-client-id"
  },
  "Gateway": {
    "ApiScope": "api://your-app-id/obo"
  }
}
```

#### 2. **Gateway** (`_src/Gateway/appsettings.local.json`)
```json
{
  "DataContextOptions": {
    "ConnectionString": "Server=host.docker.internal,1433;Database=Devv.SqlServer.Test;User Id=sa;Password=YourPassword;"
  }
}
```

#### 3. **MCP Server** (`_src/Graph.User.Mcp.Server/appsettings.local.json`)
```json
{
  "Diagnostics": {
    "ApiKey": "your-diagnostics-api-key"
  }
}
```

#### 4. **Aspire AppHost** (`_aspire/AspireApp1.AppHost/appsettings.local.json`)
```json
{
  "DataContextOptions": {
    "ConnectionString": "Server=host.docker.internal,1433;Database=Devv.SqlServer.Test;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

### Configuration Loading Order

Each project loads configuration in this priority order:
1. `appsettings.json` (safe defaults, committed to source control)
2. `appsettings.{Environment}.json` (environment-specific settings)  
3. **`appsettings.local.json`** (local secrets, gitignored)
4. Environment variables (highest precedence)

### Security Benefits

✅ **No secrets in source control**  
✅ **Easy developer onboarding**  
✅ **Environment-specific overrides**  
✅ **Production-ready defaults**

---

## License

MIT – see [`LICENSE`](LICENSE) for details.

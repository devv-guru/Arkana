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
12. [Contributing](#contributing)
13. [License](#license)

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

# 2. Create local environment file
$ cp .env.example .env
#   – fill in AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET

# 3. Spin up dependencies (Redis + SQL) with docker‑compose
$ docker compose up -d redis sql

# 4. Run the gateway
$ dotnet run --project src/Gateway

# 5. Register a sample tool
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
| **Gateway ingress** | Validates Azure AD token; rejects if not in tenant.                  |
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

## Contributing

1. Fork the repo and create a feature branch.
2. Follow the coding standards (`dotnet format`, ESLint for UI).
3. Write unit/integration tests (`dotnet test`).
4. Submit a PR with a clear description.

We use **Conventional Commits** and **Semantic Versioning**.

---

## License

MIT – see [`LICENSE`](LICENSE) for details.

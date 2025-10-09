# MCP Server Standard (HTTP)

This standard defines how our Gateway, UIs, and services interact with MCP servers over HTTP, including OIDC auth, YARP token injection, and user impersonation via the On‑Behalf‑Of (OBO/“OBF”) flow.

## Overview
- Transport: HTTPS preferred. JSON for control; NDJSON for streaming.
- Versioning: `MCP-Version: 1` response header.
- Schemas: JSON Schema (Draft 2020‑12) for tool inputs/outputs.

## Discovery
- `GET /api/mcp` → `{ name, version, capabilities: ["tools","resources","broadcasts"], time }`
- `GET /api/mcp/tools` → Tool descriptors (supports `?skip=&take=`)
- `GET /api/mcp/resources` → Optional resource descriptors

## Tool Descriptor
{
  "name": "namespace.tool",
  "description": "What the tool does",
  "version": "1.0.0",
  "inputSchema": { /* JSON Schema */ },
  "outputSchema": { /* JSON Schema */ },
  "examples": [{"input": {...}, "output": {...}}],
  "limits": { "maxInputBytes": 1048576, "timeoutMs": 60000 }
}

## Invocation
- `POST /api/mcp/tools/{tool}/invoke`
- Request: `Content-Type: application/json`; body must validate against `inputSchema`.
- Response (200): `Content-Type: application/x-ndjson` streaming events:
  - `{ "type":"tool.started", "tool":"...", "id":"run_...", "ts":"..." }`
  - `{ "type":"tool.delta", "id":"run_...", "data":{...} }` (0..N)
  - `{ "type":"tool.completed", "id":"run_...", "result":{...}, "metrics":{ "durationMs":1234 } }`
- Streaming error: `{ "type":"tool.error", "id":"run_...", "error": { "code":"BadInput", "message":"..." } }`
- Non‑streaming error: HTTP 4xx/5xx with `{ "error": { code, message, details? } }`.

## Security & Auth
All requests traverse the Gateway (YARP), which manages tokens.

- M2M service authentication (Gateway → MCP):
  - OIDC client credentials. Gateway middleware injects: `Authorization: Bearer <service_token>`
  - MCP validates token and applies service‑level RBAC.

- User impersonation (On‑Behalf‑Of flow):
  - If a user context exists, Gateway obtains a user token via OBO/“OBF”.
  - Gateway forwards it to MCP as `MCP-User-Assertion: <user_access_token>`.
  - MCP uses this assertion to call underlying services on behalf of the user (OBO), or passes it through as `Authorization: Bearer <user_access_token>` when appropriate.

- Underlying service credentials (non‑OBO backends):
  - When a backend cannot use OBO, Gateway or MCP may supply credentials via explicit headers. Reserved prefix: `MCP-Creds-*`.
  - Example: `MCP-Creds-Azure: <credential-ref-or-token>`; `MCP-Creds-ApiKey: <key>`.
  - Servers must whitelist/strip these headers when not required.

- API key backends (no native RBAC):
  - Two supported modes:
    1) Global credential: Admin configures a single API key for the integration. Gateway injects `MCP-Global-ApiKey: <key>` and MUST enforce tool access policy per user/group. Users must be assigned tools (directly or via groups). If not assigned → `403 { error.code: "ToolNotAssigned" }`.
    2) Per-user credential: Each user stores their own API key. Gateway injects `MCP-Creds-ApiKey: <user_key>`. Gateway MAY still enforce tool assignment; if enabled, the same `ToolNotAssigned` rule applies.
  - Context header: `MCP-Auth-Context: global|user` so servers can log/analyze auth mode if needed.
  - Storage: credentials are stored in the configured secrets provider; keys are never logged and are redacted in traces.

- Required/optional headers (summary):
  - Required: `Authorization: Bearer <service_token>` (M2M)
  - Optional: `MCP-User-Assertion: <user_token>` (impersonation)
  - Optional: `MCP-Creds-*` (backend‑specific creds when OBO not supported)
  - Optional (API key backends): `MCP-Global-ApiKey`, `MCP-Creds-ApiKey`, `MCP-Auth-Context`
  - Response: `MCP-Version: 1`

- Error codes:
  - 401: missing/invalid service token
  - 403: service authenticated but unauthorized for tool, user impersonation denied, or `ToolNotAssigned`
  - 502/504: downstream failures/timeouts; include `{ error.code, error.message }`

## Tool Assignment Policy (Gateway)
- Assignment model: tools can be assigned to users and to groups. Effective permissions = union of user and group assignments minus any explicit denies.
- Global API key mode REQUIRES tool assignment. Per-user mode MAY enforce assignment (configurable).
- Audit: Gateway logs authorization decisions (without secrets) for traceability.

## Redaction & Logging
- Gateway and MCP MUST redact credential values in logs (`***redacted***`).
- Structured logs should include auth context and tool name, not secrets.

## Broadcasts (optional)
- Server‑Sent Events for observers: `GET /api/mcp/stream` (`text/event-stream`)
- Event types may mirror NDJSON (`tool.delta`, `tool.completed`, etc.). Heartbeat every ≤15s.

## Conformance Checklist
- Implements discovery endpoints.
- Valid tool schemas + NDJSON streaming contract.
- Enforces M2M auth; supports user impersonation via `MCP-User-Assertion`.
- Uses reserved `MCP-Creds-*` headers only when necessary.
- Emits structured errors and `MCP-Version: 1`.

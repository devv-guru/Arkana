# MCP Alignment

This project exposes MCP-compatible endpoints for tool discovery and invocation through the Gateway.

## Endpoints
- GET `/api/mcp/tools` — lists available tools with JSON Schemas.
- POST `/api/mcp/tools/{tool}/invoke` — invokes a tool; response is streamed as NDJSON events.
- GET `/api/mcp/resources` — lists available resources by URI.

## Tool Descriptor (example)
```json
{
  "name": "http.fetch",
  "description": "Fetches an HTTP URL",
  "inputSchema": {
    "type": "object",
    "required": ["url"],
    "properties": { "url": { "type": "string", "format": "uri" } }
  },
  "outputSchema": { "type": "object", "properties": { "status": { "type": "integer" }, "body": { "type": "string" } } }
}
```

## Invocation Stream (NDJSON)
- `tool.started` — input accepted
- `tool.delta` — partial output
- `tool.completed` — final result and timing

## Security
- OIDC for UI/API; RBAC per tool; secrets via configured providers (user-secrets for dev; vault in prod).

## Standard
See `docs/mcp/STANDARD.md` for the full HTTP, streaming, and auth (OIDC, OBO/"OBF") specification, including header conventions (`Authorization`, `MCP-User-Assertion`, `MCP-Creds-*`).

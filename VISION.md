# Vision

Build an end‑to‑end, cloud‑neutral product that combines a secure Gateway, an Admin Panel, and a User Chat interface. The system adheres to an MCP-aligned contract for tool access and resource sharing, is simple to run locally (Aspire + Docker) and scales with clear, modular boundaries.

## Who It’s For
- Platform teams and small/mid orgs needing a manageable gateway with a usable admin UI.
- Developers building agentic apps who want standard tool execution via MCP.

## Product Pillars
- Reliability: validated config, safe rollouts, graceful reloads.
- Security: OIDC, RBAC, secret hygiene, TLS/cert management.
- Usability: clear UI flows, streaming responses, sensible defaults.
- Observability: first-class metrics, logs, traces, dashboards.
- Extensibility: pluggable providers for data, secrets, tools.

## MVP Scope
- Gateway with routes/clusters and certs; Admin for CRUD.
- Chat APIs with HTTP streaming (NDJSON) and pub/sub updates.
- Single DB provider fully supported; Redis for pub/sub; Grafana/Prometheus dashboards.

## Milestones (High Level)
- 0.1 Platform Core: Gateway hardening, Admin skeleton, RBAC, observability.
- 0.2 Chat MVP: streaming chat, conversation store, one model provider, one MCP tool.
- 0.3 Orchestration: tool planner/executor, retries, run logs; import/export configs.

## Non‑Goals (Initial)
- Service mesh features; advanced WAF; multi-cloud secrets beyond one documented path.

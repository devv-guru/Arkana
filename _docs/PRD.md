# Product Requirements Document (PRD)

**Product:** MCP Security Gateway & Registry  
**Version:** v0.9‑draft  
**Authors:** Core Platform Team  
**LastUpdated:** 2025‑08‑04

> **📍 Navigation:** [Documentation Index](index.md) | [Implementation Plan](MCP_GATEWAY_IMPLEMENTATION.md) | [Feature Backlog](Backlog.md) | [Architecture Decisions](ARCHITECTURE_DECISIONS.md)

---

## 1Purpose & Vision

Building AI applications that invoke dozens of Model Context Protocol (MCP) tools across the enterprise today requires each client to juggle OAuth tokens, and each tool to implement its own authentication logic. **The MCP Security Gateway** centralises those concerns—acting as an identity‑aware reverse proxy, dynamic registry, and policy enforcement point—so AI agents can safely discover and call only the tools they are authorised to use.

> **Vision:** “One secure door” through which every AI assistant can reach enterprise tools without ever holding more privilege than its user.

---

## 2Background & Problem Statement

| Observed pain point                                                                     | Impact                                                                                    |
| --------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Each MCP tool team re‑implements JWT validation and Azure AD integration.               | Duplicated effort, inconsistent security posture, audit overhead.                         |
| No central place to list available tools or gate them per user/role.                    | Users either see too many tools (confusing) or none at all; hard to meet least‑privilege. |
| Clients (ChatGPT, Claude Desktop) must manage refresh tokens and scopes for every tool. | Complex SDK logic, fragile token leakage risks.                                           |
| Regulators demand audit logs of who accessed which backend with which scope.            | Today logs are siloed across dozens of microservices.                                     |

---

## 3Goals / SuccessCriteria

| ID  | Goal                                        | Success Metric                                                                               |
| --- | ------------------------------------------- | -------------------------------------------------------------------------------------------- |
| G1 | Centralise token exchange via OAuth2.0 OBO | 100% of tool calls carry a gateway‑issued TokenB; zero direct client→tool calls by launch. |
| G2 | Tool discovery filtered by RBAC             | User sees ≤1 false‑positive tool per 1000 lookups (measured in UX tests).                  |
| G3 | <200ms P95 extra latency                  | Gateway overhead ≤200ms for streaming tools under 1000RPS.                               |
| G4 | Single‑click tool onboarding                | 90% of new tool registrations succeed on first attempt.                                     |
| G5 | Audit‑ready                                 | 100% of calls generate `ToolCalled` event with trace‑id in SIEM.                            |

---

## 4Non‑Goals (v1)

* Graphical prompt‑engineering UI.
* Automatic prompt‑safety rewrites (planned post‑GA).
* Non‑HTTP transports (gRPC, AMQP).
* Cross‑tenant marketplace distribution.

---

## 5Target Users & Personas

| Persona                | Needs                                                                   |
| ---------------------- | ----------------------------------------------------------------------- |
| **AI Application Dev** | Simple SDK to list and call tools without touching OAuth flows.         |
| **Tool Service Owner** | Drop‑in manifest + JWT validation library; no custom gateway-side code. |
| **SecurityEngineer**  | Central policy engine; view audit logs; enforce mTLS & scopes.          |
| **PlatformAdmin**     | Register/retire tools; map AAD groups → tool access.                    |

---

## 6User Stories (Prioritised)

1. **As an AI dev** I can obtain a single AAD token for the gateway and call any authorised tool so that I don’t manage dozens of scopes.
2. **As a tool owner** I POST a signed manifest and the tool instantly appears in QA with zero downtime.
3. **As an auditor** I query SIEM for “who called ToolX last week” and receive a full list with timestamps and user IDs.
4. **As a security engineer** I disable a compromised tool via the Admin UI and the gateway blocks calls within one second.
5. **As an external partner** (future) I can auth via my own AzureAD tenant without a separate account.

---

## 7Functional Requirements (FR)

| ID   | Requirement                                          | Priority    |
| ---- | ---------------------------------------------------- | ----------- |
| FR1 | Validate incoming JWT (`iss`, `aud`, `exp`, `azp`)   | MustHave   |
| FR2 | Perform OBO token exchange per `requiredScope`       | MustHave   |
| FR3 | Registry DB stores tool manifest incl. JSONSchema   | MustHave   |
| FR4 | `/tools` endpoint filters by user roles/groups claim | MustHave   |
| FR5 | Admin UI for CRUD on tools & RBAC matrix             | ShouldHave |
| FR6 | Streaming passthrough (SSE / HTTP 2)                 | MustHave   |
| FR7 | Health probes & dynamic YARP reload                  | MustHave   |
| FR8 | Signed manifest onboarding with JOSE JWS             | ShouldHave |
| FR9 | Multi‑tenant token validation (configurable)         | CouldHave  |

---

## 8Non‑Functional Requirements (NFR)

* **Performance:** ≤200ms P95 overhead @1000RPS with 16KB payload.
* **Scalability:** Horizontal scale to 10K RPS (stateless; Redis token cache).
* **Security:** OWASPASVS Level2 compliance; TLS1.2+; mTLS optional.
* **Reliability:** 99.9% monthly gateway uptime; graceful drain on deploy.
* **Observability:** OpenTelemetry traces; Prometheus metrics; JSON logs.
* **Maintainability:** Configurable via DB without redeploy; IaC modules supplied.

---

## 9Assumptions

* All client apps can obtain AzureAD tokens (PublicClient flow).
* Each tool has or can implement JWT validation middleware.
* Redis and SQL/Cosmos are available as managed services in target deployments.

---

## 10Risks & Mitigations

| Risk                   | Likelihood | Impact | Mitigation                                                 |
| ---------------------- | ---------- | ------ | ---------------------------------------------------------- |
| Token cache corruption | Medium     | High   | Use Redis>=6 with AOF persistence and multi‑AZ.          |
| Tool manifest spoofing | Low        | High   | Require detachedJWS with tool’s cert; verify CN in AAD.   |
| Latency budget blow‑up | Medium     | Medium | Perf test; introduce adaptive time‑outs + circuit breaker. |
| Scope creep in v1      | High       | Medium | Enforce MoSCoW priorities; cut non‑goals.                  |

---

## 11MVP Scope

| Workstream   | MVP deliverable                                      |
| ------------ | ---------------------------------------------------- |
| CoreGateway | JWT validation, OBO token mediation, YARP proxy.     |
| Registry DB  | Tool manifest + RBAC tables; REST CRUD API.          |
| Admin UI     | Basic list/create/update tool; map AAD group → tool. |
| SDK (.NET)   | Discovery + call helper with streaming.              |
| AzureDeploy | Terraform module; AppService + Redis + SQL.         |

Launch criteria = **GoalsG1–G4** satisfied in staging + security review passed.

---

## 12Milestones & Timeline (tentative)

| Phase             | Dates            | Key outputs                                      |
| ----------------- | ---------------- | ------------------------------------------------ |
| Design freeze    | Aug18→Aug29 | Finalised manifests, DB schema, API spec         |
| MVPDev Sprint1 | Sep02→Sep27 | Core gateway, JWT validation, registry API       |
| Sprint2         | Sep30→Oct25 | OBO flow, Redis cache, .NET SDK alpha            |
| Sprint3         | Oct28→Nov22 | Admin UI beta, streaming passthrough, load tests |
| Beta release     | Dec02          | Pilot with Finance & HR tools                    |
| GA               | Jan152026     | Production rollout, SOC2 audit kickoff          |

---

## 13Metrics & KPIs

* **GatewayP95 latency** (ms)
* **Token cache hit‑rate** (%)
* **Tool registration successrate** (%)
* **Unauthorized access attempts blocked** (#/day)
* **Mean time to disable a tool** (seconds)

---

## 14Open Questions

1. Do we need gRPC support in 2026 roadmap?
2. Accept external IdPs (Okta) in MVP or post‑GA?
3. Which schema versioning strategy—SemVer vs date‑based?
4. Who owns SOC2 evidence collection after GA?

---

*End of document*

# ADR 0001: Realtime Transport for Chat

## Status
Accepted

## Context
We need realtime UX for user chat and admin viewers. Requirements: simple networking, proxy‑friendly, debuggable streams, and support for replay to late subscribers.

## Decision
Use HTTP chunked streaming with NDJSON for request/response and Server‑Sent Events (SSE) for broadcast updates. Pub/sub is backed by Redis.

## Consequences
- Pros: works over standard HTTP; easy to proxy/record; low client complexity; SSE is broadly supported.
- Cons: no full duplex (compared to WebSockets); need separate channel for broadcast (SSE).
- Mitigations: Combine POST streaming for the caller with SSE for watchers; Redis enables fan‑out and replay via persisted state.

## Alternatives Considered
- SignalR/WebSockets: powerful but adds infra/client complexity; not required for initial needs.
- Polling: simplest but inefficient and poor UX under load.

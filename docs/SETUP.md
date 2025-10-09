# Setup

## Prerequisites
- .NET 9 SDK installed
- .NET Aspire templates/workload
  - dotnet workload update
  - dotnet new install Aspire.Workloads
- Docker Desktop (for Redis and Aspire containers)

## Bootstrap (dotnet CLI)
- Windows (PowerShell):
  - scripts/bootstrap.ps1
- macOS/Linux (bash):
  - chmod +x scripts/bootstrap.sh
  - ./scripts/bootstrap.sh

## Run
- Build: `dotnet build Arkana.sln`
- Aspire AppHost: `dotnet run --project aspire/AppHost`
- Admin UI (pure WASM): `dotnet run --project src/Admin`
- Chat UI (pure WASM): `dotnet run --project src/Chat`
- Chat API: runs under Aspire as `arkana-chatapi` (external HTTP endpoint)

## Notes
- UIs use pure Blazor WebAssembly (no Blazor WebApp/server).
- Chat API (separate from Gateway) exposes `/api/chat/messages` (NDJSON) and `/api/chat/stream` (SSE).

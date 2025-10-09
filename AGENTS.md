# Repository Guidelines

This repository is a .NET Aspire solution built around a Kestrel/YARP gateway with a Blazor WebAssembly UI and supporting data/domain layers. It targets .NET 9 and uses Docker containers for local orchestration (Redis, Grafana, Prometheus, and a selectable database).

## Project Structure & Module Organization
- `_src/` – application code:
  - `Gateway/` (ASP.NET Core + YARP, primary entrypoint)
  - `UI.BlazorWasm/` (client UI)
  - `Data/`, `Domain/`, `Endpoints/`, `Shared/` (core libraries)
- `_aspire/` – Aspire composition:
  - `AspireApp1.AppHost/` (orchestrates services), `AspireApp1.ServiceDefaults/`
  - `grafana/`, `prometheus/` (observability configs)
- `_test/` – test projects: `Gateway.Tests/` (NUnit), `AspireApp1.Tests/` (xUnit)
- Solution: `Arkana.sln`

## Build, Test, and Development Commands
- Restore/build: `dotnet restore` then `dotnet build Arkana.sln`
- Run full stack (requires Docker): `dotnet run --project _aspire/AspireApp1.AppHost`
- Run gateway only: `dotnet run --project _src/Gateway`
- Run UI only: `dotnet run --project _src/UI.BlazorWasm`
- Tests (all): `dotnet test`
- Optional watch: `dotnet watch run --project _src/Gateway`

## Coding Style & Naming Conventions
- Language: C# with nullable enabled and implicit usings.
- Indentation: 4 spaces; file‑scoped namespaces preferred.
- Naming: PascalCase for types/namespaces; camelCase for locals/fields; Async methods end with `Async`.
- Logging: use Serilog abstractions in web projects.
- Formatting: `dotnet format` (run at solution root) before PRs when available.

## Testing Guidelines
- Frameworks: NUnit (`_test/Gateway.Tests`) and xUnit (`_test/AspireApp1.Tests`).
- Naming: `MethodName_State_ExpectedResult` for tests; mirror source folder structure.
- Coverage: `dotnet test --collect:"XPlat Code Coverage"` (coverlet collector included in Aspire tests).

## Commit & Pull Request Guidelines
- Commits: imperative mood, concise subject (≤72 chars). Conventional prefixes encouraged: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`.
- PRs: include summary, linked issues (e.g., `Closes #123`), test evidence, and screenshots/GIFs for UI changes. Note any config or migration steps.

## Security & Configuration Tips
- Secrets: use `dotnet user-secrets` in `Gateway` during development (see `UserSecretsId`), or environment variables in containers.
- Database: selected via `DataContextOptions:Provider` (SqlServer, PostgreSql, MySql/MariaDb) in AppHost configuration; ensure Docker is running.


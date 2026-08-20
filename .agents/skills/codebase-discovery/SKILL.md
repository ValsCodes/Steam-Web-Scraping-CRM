---
name: codebase-discovery
description: Discover SteamApp architecture, symbols, callers, routes, dependencies, implementation paths, and change impact with codebase-memory-mcp and scoped source verification. Use for repository discovery or dependency analysis; skip broad discovery when the user already identified a locally understandable file or symbol.
---

# SteamApp Codebase Discovery

## Choose the smallest inspection

Inspect source directly when the user names the exact file, class, method, endpoint, or component and the task is locally understandable.

Use codebase-memory-mcp as the primary navigation index when correctness requires repository discovery, including natural-language codebase questions, symbol discovery, callers, dependencies, route tracing, architectural placement, or change impact.

Do not route a question to Graphify merely because `graphify-out/graph.json` exists. Use Graphify when the user explicitly requests it, asks for graph-specific analysis or outputs, or when codebase-memory-mcp is unavailable or insufficient and a scoped Graphify query can materially help. Do not run both graph systems routinely.

## Procedure

1. Run `list_projects` and select the project whose root matches the current SteamApp workspace.
2. Run `index_status`.
3. Run `detect_changes` when the task concerns the current diff.
4. Re-index only when the index is missing or stale.
5. Use the narrowest operation:
   - `search_graph`: locate functions, classes, interfaces, routes, fields, or variables.
   - `trace_path`: trace callers, callees, and dependencies.
   - `get_code_snippet`: inspect a targeted symbol.
   - `query_graph`: answer a specific structural relationship.
   - `get_architecture`: perform broad layer/package/hotspot analysis only.
   - `search_code`: locate text or patterns when structural search is unsuitable.
6. Verify implementation-sensitive, negative, exhaustive, or security-sensitive conclusions against source and project files.

## SteamApp orientation

- Client: Angular under `SteamApp.Client/src/app`.
- Domain: `SteamApp.Server/SteamApp.Models`.
- DTOs/mapping/application helpers: `SteamApp.Application`.
- Contracts: `SteamApp.Interfaces`.
- EF Core and external integrations: `SteamApp.Infrastructure`.
- API, jobs, RabbitMQ, Redis composition, and security: `SteamApp.WebAPI`.
- Tests: `SteamApp.Tests`, `SteamApp.IntegrationTests`, `SteamApp.E2ETests`, and client unit/integration/E2E suites.

Treat this map as navigation, not proof. Inspect affected `.csproj`, source, configuration, and tests before making architectural claims.

## Fallback

Use `rg` or direct file inspection for string literals, errors, configuration, HTML/SCSS, generated assets, or gaps in indexed results.

If codebase-memory-mcp is unavailable, state that briefly and continue with scoped source inspection. Use Graphify as a secondary fallback only when the existing graph can materially help. Never invent graph results.

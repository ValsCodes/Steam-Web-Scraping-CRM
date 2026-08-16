# SteamApp repository instructions

## Command execution

- Run Windows commands with PowerShell.
- Invoke explicit PowerShell commands through `pwsh -NoProfile -Command "<command>"`; if `pwsh` is unavailable, use `powershell -NoProfile -Command "<command>"`.
- Use `rg` or `rg --files` for search and file discovery.
- Do not expose user secrets, connection strings, tokens, or local configuration in command output or reports.

## Work style

- For a small, identified change, inspect the named file or symbol and make the smallest complete change.
- Expand discovery only when correctness requires callers, dependencies, architecture, or change-impact analysis.
- Preserve unrelated user changes and avoid unrelated refactoring.
- Prefer direct, readable control flow and existing framework capabilities over new helper layers or dependencies.
- Treat source files and project files as authoritative; generated graphs are navigation aids.

## Project map

- `SteamApp.Client`: Angular 21 standalone client using Angular Material, RxJS, Jasmine/Karma unit tests, Jest integration tests, and Playwright E2E tests.
- `SteamApp.Server/SteamApp.Models`: `SteamApp.Domain` entities, enums, value objects, and domain constants.
- `SteamApp.Server/SteamApp.Application`: DTOs, mapping profiles, operation results, cache keys, JSON models, and application utilities.
- `SteamApp.Server/SteamApp.Interfaces`: service and repository contracts.
- `SteamApp.Server/SteamApp.Infrastructure`: EF Core context, ASP.NET Identity user, repositories, email, Steam/Selenium, encryption, retry, and wishlist services.
- `SteamApp.Server/SteamApp.WebAPI`: ASP.NET Core API composition, controllers, Minimal APIs, security, jobs, manual/scrape orchestration, RabbitMQ, Redis caching, and migrations.
- `SteamApp.Server/SteamApp.WebApiClient`: typed .NET API-client managers.
- `SteamApp.Server/SteamApp.Tests`, `SteamApp.IntegrationTests`, and `SteamApp.E2ETests`: NUnit test projects.

Respect the current project-reference graph rather than imposing a generic clean-architecture template:

- Application references Domain.
- Interfaces references Application and Domain.
- Infrastructure references Application, Interfaces, and Domain.
- WebAPI references Infrastructure.
- WebApiClient references Application and Domain.

Inspect the affected `.csproj` files before changing these boundaries.

## Code conventions

- Use one new top-level C# type per file and name the file after that type unless an established local grouping is clearer.
- Do not add local functions inside methods; extract focused private members when extraction materially improves clarity.
- Prefer primary constructors when they remain easy to read with validation, inheritance, and dependency injection.
- Put domain entities and enums under `SteamApp.Models`, DTOs and mappings under `SteamApp.Application`, contracts under `SteamApp.Interfaces`, and infrastructure implementations under `SteamApp.Infrastructure`.
- Put API-only orchestration, controllers, Minimal APIs, hosted workers, RabbitMQ messages/handlers/consumers, security policies, and API-specific services under `SteamApp.WebAPI`.
- Extend existing services, endpoints, and message-broker infrastructure instead of creating parallel paths.
- Follow the existing standalone Angular component/service style and existing import-barrel conventions in the touched area.

## Data access and dependency injection

- `ApplicationDbContext` is the single EF Core context for both ASP.NET Identity and SteamApp business data.
- In repositories, reusable services, message handlers, and background work, inject `IDbContextFactory<ApplicationDbContext>`, create a context per operation, and dispose it with `using` or `await using`.
- Request-bound Minimal API handlers may inject `ApplicationDbContext` directly, matching existing endpoint conventions.
- Never run concurrent operations on one `DbContext` or let it escape its owning request, scope, or operation.
- Register ordinary WebAPI services in `Program.cs` beside comparable registrations. Keep provider-specific registrations in their existing extension, such as `AddRabbitMqMessageBroker`.
- Singletons must not capture scoped services. Hosted services and RabbitMQ consumers must create and dispose a scope per unit of scoped work.
- Do not dispose dependencies supplied by DI. Dispose resources created and owned by the current component exactly once.

## Async, workers, brokers, and caching

- Await async work and propagate `CancellationToken` through cancellable I/O, EF Core, HTTP, and long-running operations.
- Catch failures inside hosted-service iteration and message-consumer boundaries so one operation does not silently terminate processing.
- Treat RabbitMQ payloads as untrusted input. Validate identifiers and state before persistence or external calls, keep correlation IDs in logs/history, and acknowledge messages only after the intended outcome is durable.
- Preserve retry, cancellation, partial-result, status-transition, and history behavior in scrape and manual-check workflows.
- Keep Redis and memory-cache keys consistent with `CacheKeys`; include user/tenant identity whenever cached data is user-specific.

## Security

- Preserve JWT validation, Identity lockout/password rules, authorization policies, user ownership checks, rate limiting, CORS, HSTS, and security headers unless the task explicitly changes them.
- Never place real secrets in tracked `appsettings*.json`, launch settings, compose files, logs, tests, or examples.
- Treat user-controlled URLs, Steam links, scrape targets, message payloads, and serialized JSON as untrusted.
- The product intentionally permits users to open unverified external URLs after disclosure. Preserve the warning/consent flow and `noopener,noreferrer`; do not silently convert it into a blocklist.
- Admin, role, ownership, authentication, authorization, external URL, scraping, broker, deployment, and secret-handling changes are high risk and require the security-review workflow.

## Discovery and generated graphs

This repository has a Graphify graph at `graphify-out/` and a codebase-memory-mcp index.

- For an exact, locally understandable change, inspect source directly.
- For symbol, caller, dependency, route, or change-impact discovery, use the `codebase-discovery` skill and the narrowest codebase-memory-mcp operation.
- For natural-language codebase questions when `graphify-out/graph.json` exists, run `graphify query "<question>"` first. Use `graphify path` for relationships and `graphify explain` for a focused concept.
- Use `graphify-out/wiki/index.md` for broad navigation when it exists. Read `GRAPH_REPORT.md` only for broad architecture work or when scoped queries are insufficient.
- Do not use Graphify and codebase-memory-mcp for the same routine task unless one result is insufficient or cross-validation is requested.
- Dirty `graphify-out/` files are expected. After modifying source code, run `graphify update .`.

## Repository workflows

Use the repository-local skills under `.agents/skills`:

- Discovery or change impact: `codebase-discovery`
- `/unit-tests` or focused test work: `unit-tests`
- `/pr-review` or standard pre-PR review: `pr-review`
- `/pr-review-refactor` or behavior-preserving refactor review: `pr-review-refactor`
- `/security-review` or security-sensitive review: `security-review`
- `/graphify` or explicit graph analysis: `graphify`

Versioned directives under `.agents/directives` are authoritative for the workflows they govern:

- `Directive.Code.PR_v4.md`
- `Directive.Code.Refactoring_v1.md`
- `Directive.UnitTest_v1.md`
- `Directive.Security_v2.md`

When a workflow directive conflicts with general guidance here, follow the workflow directive. Source files remain authoritative for implementation behavior.

## Validation

- Server build: `dotnet build SteamApp.Server\SteamApp.WebAPI\SteamApp.sln -v:m`
- Server unit tests: `dotnet test SteamApp.Server\SteamApp.Tests\SteamApp.Tests.csproj -v:m -m:1`
- Server integration tests: run only when explicitly requested or required by the changed boundary; otherwise report `Not Verified`.
- Server E2E tests: run only when explicitly requested.
- Client build: from `SteamApp.Client`, run `npm.cmd run build`.
- Client unit tests: from `SteamApp.Client`, run `npm.cmd run test:unit`.
- Client integration tests: run `npm.cmd run test:integration` only when requested or required.
- Client E2E tests: run `npm.cmd run test:e2e` only when requested.

Run only validation relevant to the changed scope. Report skipped or unavailable checks as `Not Verified`; never imply they passed.

## Completion

Before reporting completion:

- Confirm the requested behavior or configuration exists.
- Inspect the final diff for readability, scope, security, and accidental changes.
- Run the relevant validation.
- Update Graphify after source-code changes.
- Report tests and commands actually executed, remaining risk, and every `Not Verified` check.

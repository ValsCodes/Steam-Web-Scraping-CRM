---
name: csharp-conventions
description: Apply SteamApp C#/.NET structure, architecture, EF/DI lifetime, async, worker, broker, and cache conventions. Required when creating, modifying, moving, refactoring, or reviewing C# code or .NET project references, including test code.
---

# SteamApp C# conventions

Read and follow this skill before governed C#/.NET work. Apply only relevant sections; testing mechanics remain in `unit-tests`, specialized relation writes in `bulk-relations`, and security assessment criteria in `security-review`. Read those skills when triggered.

## Readability and framework usage

- Prefer clear, simple, human-readable code, direct control flow, and the smallest complete solution.
- Extend existing implementations rather than adding parallel infrastructure. Do not introduce abstractions, wrappers, factories, managers, helper layers, dependencies, projects, or folders without a concrete benefit.
- Follow surrounding conventions. Prefer primary constructors when validation, inheritance, and dependency injection remain easy to read.
- Do not add local functions inside methods; extract focused private members only when extraction materially improves clarity.
- Prefer framework-native capabilities appropriate to the actual target framework and platform. Verify uncertain framework behavior or APIs against current official documentation before implementing. For Fluent UI Blazor, use official Fluent UI Blazor documentation.

## Architecture and ownership

The existing repository structure is authoritative. Before creating, moving, or substantially modifying code, inspect the affected project, neighboring folders, namespaces, and comparable implementations. For structurally significant work, inspect comparable source rather than inferring architecture from generic .NET conventions.

All paths below are under `SteamApp.Server`.

| Project | Ownership |
|---|---|
| `SteamApp.Models` (namespace `SteamApp.Domain`) | Domain entities, enums, value objects, and domain constants |
| `SteamApp.Application` | DTOs, mappings, operation results, cache keys, JSON models, and application utilities |
| `SteamApp.Interfaces` | Service and repository contracts |
| `SteamApp.Infrastructure` | EF Core persistence, Identity user, repositories, Steam/Selenium, email, encryption, retry, wishlist, and external integrations |
| `SteamApp.WebAPI` | API composition, controllers, Minimal APIs, API-specific services/orchestration, hosted workers/jobs, RabbitMQ messages/handlers/consumers, Redis composition, security, and migrations |
| `SteamApp.WebApiClient` | Typed .NET API-client managers |
| `SteamApp.Tests`, `SteamApp.IntegrationTests`, `SteamApp.E2ETests` | Existing NUnit test projects; boundary selection is in `unit-tests` |

Preserve the actual project-reference graph:

- Application references Domain.
- Interfaces references Application and Domain.
- Infrastructure references Application, Interfaces, and Domain.
- WebAPI references Infrastructure.
- WebApiClient references Application and Domain.

Inspect affected `.csproj` files before changing dependencies. Source and project files establish actual references; project names alone do not prove a violation.

- Place code in the existing project/domain/folder owning equivalent behavior; namespaces must match surrounding source.
- Do not impose a generic clean-architecture layout, alternative domain structure, shared module, cross-domain abstraction, or parallel service hierarchy.
- Do not move responsibilities between projects unless the requested task explicitly requires it. Do not choose a different layer merely because it is easier to reference.
- Follow the nearest established implementation pattern. When a proposed location conflicts with established structure, the repository structure wins.

## Mandatory: one C# type per file

Each class, record, struct, interface, enum, and delegate MUST have its own source file named after the type. No local-grouping exception.

- A `.cs` file MUST NOT contain multiple top-level types or a second helper, DTO, options, message, request/response, configuration, result, or implementation type for convenience.
- When materially modifying an existing multi-type file, split the touched types into correctly named files unless this changes generated code or violates an explicitly documented framework requirement.
- Nested types are allowed only when genuinely implementation-private and consistent with the local design. Do not use nesting to bypass this rule.
- Generated sources are exempt unless the task explicitly modifies their generation strategy.

## Data access, DI, and resources

- `ApplicationDbContext` is the single EF Core context for Identity and business data.
- Repositories, reusable services, handlers, and background work inject `IDbContextFactory<ApplicationDbContext>`, create one context per operation, and dispose it with `using` or `await using`.
- Request-bound Minimal API handlers may inject `ApplicationDbContext` directly, matching established request-scoped conventions. Any other direct usage must be an explicit short-lived scope, not a reusable service bypass of factory ownership.
- Never run concurrent operations on one context or retain it beyond its owning request, scope, or operation.
- Register ordinary WebAPI services in `Program.cs` beside comparable registrations; retain provider-specific extensions such as `AddRabbitMqMessageBroker`.
- Singletons must not capture scoped services or mutable transient dependencies whose ownership/lifetime is unsafe. Hosted services and consumers create and dispose a scope per unit of scoped work.
- Never dispose DI-supplied dependencies. Dispose directly created/owned resources exactly once, asynchronously when required.

## Async, workers, brokers, and caching

- Await tasks or intentionally supervise them; `async void` is limited to valid UI event handlers.
- Propagate `CancellationToken` through cancellable EF, HTTP, queue, and long-running I/O. Do not convert cancellation into an ordinary failure.
- Catch failures at hosted-service iteration and message-consumer boundaries so one operation does not silently terminate processing.
- Validate untrusted RabbitMQ payload shape, identifiers, ownership, and state before persistence or external calls. Preserve correlation IDs in logs/history and acknowledge only after the intended outcome is durable.
- Preserve retry, cancellation, partial results, error traces, rerun inputs, ordering, status transitions, and history in scrape/manual-check workflows.
- Shared mutable state, queues, connections, and caches must be thread-safe for their registered lifetime.
- Keep keys consistent with `CacheKeys`; user-specific data needs user-specific keys and invalidation.

## Error and external boundaries

- Translate validation and expected failures at controllers/endpoints. Do not swallow exceptions or use them as ordinary control flow.
- Handle HTTP, Selenium, parsing, serialization, email, Redis, RabbitMQ, and persistence failures at their owning boundary.
- Preserve consistency of partial operations or an explicit recovery/status path.
- Use structured logging with correlation/trace properties; omit secrets, credentials, JWTs, personal data, and sensitive payloads. Avoid redundant cross-layer logging without operational value.
- Preserve API/client contracts and authenticated ownership unless the task explicitly changes them. Security-sensitive changes additionally require `security-review`.

## Completion checks

Inspect every added/modified `.cs` file for one correctly named top-level type, subject to documented exceptions. Verify placement, namespaces, nearest implementation patterns, project references, resource ownership, and applicable async/broker/cache requirements. Inspect the final diff for structural drift and unnecessary layers, folders, or abstractions. Run validation required by `AGENTS.md` and applicable workflows; do not claim checks that were not performed.

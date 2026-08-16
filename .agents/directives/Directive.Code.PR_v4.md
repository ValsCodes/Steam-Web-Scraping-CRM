# SteamApp Pre-PR Validation Directive

## Objective

Before creating, recommending, or approving a pull request, review the exact changed commit and working-tree diff for architectural fit, required behavior, failure handling, maintainability, resource ownership, dependency-injection lifetime safety, asynchronous execution, concurrency, security, and validation evidence.

Base findings on SteamApp source, project files, tests, and reachable execution paths. Do not substitute generic architectural preferences for the repository's current design.

## Review scope

1. Record the reviewed commit SHA and whether uncommitted changes are included.
2. Inspect the diff, affected unchanged code, callers, consumers, configuration, and relevant tests.
3. Use scoped graph discovery only when dependencies or impact are not evident from the diff.
4. Re-check the commit before the final recommendation. A changed commit invalidates the prior recommendation.
5. Apply `.agents/directives/Directive.Security_v2.md` and include its required Security Review report.

## Architecture alignment

Verify placement and dependency changes against the current project graph:

- Domain entities, enums, and value objects: `SteamApp.Server/SteamApp.Models`.
- DTOs, mapping profiles, operation results, cache keys, and application utilities: `SteamApp.Application`.
- Service and repository contracts: `SteamApp.Interfaces`.
- EF Core, Identity user, repositories, Steam/Selenium, email, encryption, retry, and infrastructure services: `SteamApp.Infrastructure`.
- Controllers, Minimal APIs, API services, jobs, RabbitMQ, Redis composition, security, and migrations: `SteamApp.WebAPI`.
- Angular presentation, models, and HTTP services: `SteamApp.Client`.

Confirm new project references follow the existing `.csproj` graph. Do not assert an architecture violation from project names alone.

## Required behavior

Verify:

- The stated requirement is complete, including normal, empty, invalid, boundary, cancellation, and failure paths.
- API status codes, payloads, state transitions, database writes, history records, cache behavior, ordering, and external effects are deliberate.
- User-owned queries and mutations use the authenticated user ID unless an explicit admin/internal policy authorizes broader access.
- Client models, routes, and service contracts remain synchronized with server DTOs and endpoints.
- Scrape/manual-check flows preserve partial results, error traces, status transitions, cancellation, correlation IDs, and rerun inputs.
- Breaking changes and assumptions are explicit.

## Errors, logging, and external boundaries

Verify:

- Validation and expected failures are translated at the controller or endpoint boundary.
- Exceptions are not swallowed or used as ordinary control flow.
- Logs use structured properties, preserve correlation/trace identifiers, and omit secrets, credentials, JWTs, personal data, and sensitive payloads.
- HTTP, Selenium, parsing, serialization, email, Redis, RabbitMQ, and persistence failures are handled at their owning boundary.
- Partial operations preserve consistency or have an explicit recovery/status path.
- The same failure is not redundantly logged across layers without operational value.

## Data access, DI, and resources

Verify:

- `ApplicationDbContext` continues to serve both Identity and business data.
- Repositories, reusable services, handlers, and background work use `IDbContextFactory<ApplicationDbContext>` per operation and dispose created contexts.
- Direct `ApplicationDbContext` injection is limited to request-bound Minimal API handlers or an explicit short-lived scope.
- No `DbContext` is used concurrently or retained beyond its scope.
- Singleton services do not capture scoped services; hosted workers and consumers create a scope for scoped work.
- DI registrations are present in `Program.cs` or the existing provider-specific extension.
- DI-owned dependencies are not manually disposed. Directly owned resources are disposed exactly once, asynchronously when required.

## Async, workers, brokers, and caching

Verify:

- Tasks are awaited or intentionally supervised; `async void` is limited to valid UI event handlers.
- Cancellation is accepted and propagated through EF Core, HTTP, queues, and long-running work, and is not converted into an ordinary error.
- Hosted-service iteration catches failures at the iteration boundary.
- RabbitMQ consumers validate untrusted messages, create/dispose scopes, preserve correlation IDs, and acknowledge only after the intended durable outcome.
- Shared mutable state, queues, connections, and caches are thread-safe for their registered lifetime.
- User-specific cached data has user-specific keys and invalidation behavior.

## Angular client

Verify:

- Standalone component, service, RxJS, Material, and routing conventions in the touched area are preserved.
- Subscriptions, timers, event listeners, and cancellation resources are cleaned up.
- State changes after asynchronous work remain valid for the component lifecycle.
- Manual change detection is used only where the current async boundary requires it; avoid masking state-flow defects with repeated `detectChanges()`.
- External links preserve disclosure/warning behavior and use `noopener,noreferrer`.
- JWT/session handling and interceptors do not expose tokens or create authorization bypasses.

## Tests and evidence

- Apply `Directive.UnitTest_v1.md`.
- Run affected .NET and/or Angular unit tests.
- Run builds or focused static validation appropriate to the changed surface.
- Do not run integration or E2E suites unless explicitly requested; record them as `Not Verified` when relevant.
- Do not weaken or delete meaningful coverage without explicit justification.
- Report the exact commands and outcomes. Failed or unavailable validation cannot be described as passed.

## Finding severity

### Blocking

Use Blocking for reachable defects involving incorrect required behavior, broken contracts, architecture or ownership violations, unsafe error handling, DI lifetime mismatch, invalid disposal, unsafe async/concurrency, user-isolation failure, material security exposure, failing required validation, or changes that cannot be reviewed safely.

### Non-Blocking

Use Non-Blocking only for an improvement that does not affect correctness, contracts, safety, resource ownership, test confidence, or maintainability enough to impede review.

## Required report

## Pre-PR Validation Report

### Summary

- Commit:
- Uncommitted changes included:
- Requirement reviewed:
- Files and execution paths reviewed:
- Architecture or patterns inspected:
- Commands executed:
- Result: Ready for PR / Not Ready for PR

### Blocking Findings

For each finding:

- ID and title:
- Location:
- Reachable path:
- Evidence:
- Impact:
- Required fix:

If none, state `None identified`.

### Non-Blocking Findings

For each finding:

- ID and title:
- Location:
- Evidence:
- Recommendation:

If none, state `None identified`.

### Validation Notes

- Architecture alignment: Pass / Fail / Not Verified
- Required behavior: Pass / Fail / Not Verified
- Error handling and logging: Pass / Fail / Not Verified
- Data ownership and isolation: Pass / Fail / Not Verified / N/A
- DbContext ownership and concurrency: Pass / Fail / Not Verified / N/A
- DI lifetime and disposal: Pass / Fail / Not Verified / N/A
- Async and cancellation: Pass / Fail / Not Verified / N/A
- Worker, broker, and cache safety: Pass / Fail / Not Verified / N/A
- Angular lifecycle and contract alignment: Pass / Fail / Not Verified / N/A
- Unit tests and build: Pass / Fail / Not Verified
- Integration tests: Pass / Fail / Not Verified / N/A
- E2E tests: Pass / Fail / Not Verified / N/A

### Residual Risks

- List remaining risks or state `None identified`.

### Decision

- Ready for PR: Yes / No
- Blocking findings unresolved: Yes / No
- Human approval required: Yes / No
- Required actions:

Append the Security Review report required by `Directive.Security_v2.md`.

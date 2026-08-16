# SteamApp Refactoring Validation Directive

## Objective

Improve structure, readability, maintainability, testability, or performance without changing intended business behavior or externally observable outcomes. Treat behavior changes and defect fixes as separate work unless the user explicitly approves them.

Apply only checks relevant to the changed files. Mark unrelated sections `N/A`.

## Preserve the existing contract

Before editing or reviewing, establish the current contract from executable behavior, tests, callers, configuration, and affected integration paths:

- Inputs, outputs, validation, exception/error responses, and API/client contracts.
- Authorization, user ownership, and security behavior.
- EF Core queries, tracking/materialization, transaction boundaries, writes, and side-effect order.
- Cache keys, invalidation, RabbitMQ messages, acknowledgements, history/status transitions, and retries.
- Async ordering, cancellation, concurrency, partial-result behavior, and external calls.
- Angular visible state, route behavior, dialogs, notifications, and change-detection outcomes.

Add characterization tests first when important behavior is not adequately protected.

Do not silently fix a discovered defect. Preserve it, record it under Discovered Existing Defects, and propose a separate fix.

## Scope

Accept focused method/type extraction, duplication removal, clearer naming, reduced nesting, explicit side effects, improved dependency boundaries, proven dead-code removal, and behavior-preserving performance work.

Separate feature additions, business-rule changes, API/DTO/schema changes, authorization changes, broad formatting, speculative abstractions, and unrelated redesign.

Database-schema and migration creation/execution are outside this workflow unless explicitly requested.

## Architecture and placement

Preserve SteamApp's current project responsibilities:

- Domain in `SteamApp.Models`.
- DTOs/mapping/application utilities in `SteamApp.Application`.
- contracts in `SteamApp.Interfaces`.
- EF Core and external integrations in `SteamApp.Infrastructure`.
- API orchestration, jobs, brokers, and security in `SteamApp.WebAPI`.
- presentation and browser behavior in `SteamApp.Client`.

Validate project references from the affected `.csproj` files. Do not move code solely to satisfy a theoretical layering model.

## Data, DI, and resource ownership

When touched, verify:

- Query filtering, ordering, eager loading, tracking, materialization timing, writes, and transaction behavior remain equivalent.
- User ownership filters are preserved.
- Repositories/services/handlers/background work create per-operation contexts through `IDbContextFactory<ApplicationDbContext>`.
- Request-bound Minimal API context injection remains request scoped.
- No context is shared concurrently or retained beyond its operation.
- Service lifetimes remain valid; singletons do not capture scoped or mutable transient dependencies.
- DI-supplied dependencies are not disposed by consumers. Factory-created/owned resources are disposed exactly once.

## Async, workers, brokers, and client state

When touched, verify:

- Awaiting, exception observability, cancellation propagation, and execution order are preserved.
- Hosted workers and RabbitMQ consumers retain their scope, retry, acknowledgement, correlation, and failure semantics.
- Cache behavior and invalidation remain equivalent.
- Angular subscriptions/listeners/timers are cleaned up as before.
- State changes after `await` and manual change-detection calls do not introduce repeated or disposed-component updates.
- External-link disclosure and opening behavior remain unchanged.

## Readability gate

The result must be materially easier to understand:

- Responsibilities and names are clearer.
- Nesting, branching, coupling, or duplication is reduced.
- Side effects and ownership are more explicit.
- New abstractions have a concrete responsibility and reduce real complexity.
- Manual review does not require tracing additional unnecessary layers.

If the new version is not easier to reason about, the refactoring has not met its objective.

## Verification

- Apply `Directive.UnitTest_v1.md`.
- Run the existing relevant tests before and after when feasible.
- Add behavior-focused characterization/regression coverage where needed.
- Run relevant builds and focused analyzers.
- Do not run integration or E2E suites unless explicitly requested; mark relevant unexecuted suites `Not Verified`.
- Do not weaken tests merely because private implementation changed.

## Blocking findings

Block when the refactoring changes business behavior, public/client/API contracts, validation, errors, authorization, ownership, persistence, message/cache behavior, async/cancellation/concurrency, resource ownership, or when behavioral equivalence for a high-risk path is not established.

Do not block solely because EF migrations are absent or unverified; migration work is outside this workflow.

## Required report

## Code Refactoring Validation Report

### Objective

- Commit:
- Refactoring goal:
- Changed files or areas:
- Existing behavior reviewed:
- Applicable sections:

### Behavioral Contract

- Inputs preserved: Yes / No / Not Verified
- Outputs preserved: Yes / No / Not Verified
- Business rules preserved: Yes / No / Not Verified
- Errors and status codes preserved: Yes / No / Not Verified
- Authorization and ownership preserved: Yes / No / Not Verified / N/A
- Persistence and side effects preserved: Yes / No / Not Verified / N/A
- Async, cancellation, broker, and cache behavior preserved: Yes / No / Not Verified / N/A
- Angular visible behavior preserved: Yes / No / Not Verified / N/A
- Public contracts preserved: Yes / No / Not Verified

### Structural Improvements

- Duplication reduced:
- Responsibilities clarified:
- Coupling reduced:
- Readability improved:
- Complexity reduced:

### Blocking Findings

For each finding provide location, behavior at risk, evidence, issue, and required fix. If none, state `None identified`.

### Non-Blocking Findings

For each finding provide location, evidence, and recommendation. If none, state `None identified`.

### Validation Status

- Architecture and placement: Pass / Fail / Not Verified / N/A
- Business behavior equivalence: Pass / Fail / Not Verified
- DbContext and user-ownership behavior: Pass / Fail / Not Verified / N/A
- DI lifetime and disposal: Pass / Fail / Not Verified / N/A
- Async, worker, broker, and cache behavior: Pass / Fail / Not Verified / N/A
- Angular lifecycle behavior: Pass / Fail / Not Verified / N/A
- Unit tests and build: Pass / Fail / Not Verified
- Integration/E2E: Pass / Fail / Not Verified / N/A
- Migration checks: N/A unless explicitly requested

### Discovered Existing Defects

- List separately or state `None identified`.

### Residual Risks

- List risks or state `None identified`.

### Decision

- Behavior preserved: Yes / No / Not Verified
- Maintainability improved: Yes / No
- Ready for PR: Yes / No
- Required actions:

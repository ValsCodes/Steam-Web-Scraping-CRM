# Pre-PR Validation Directive for AI Agents

## Objective

Before creating, recommending, or approving a pull request, review the proposed changes for architectural fit, correctness, failure handling, maintainability, resource ownership, dependency injection lifetime safety, asynchronous execution safety, and validation evidence.

The review must be based on the project's existing architecture and conventions, not on assumptions or generic preferences.

## Context Environment
- **Platform:** .NET
- **Language:** C#
- **Data Access:** Entity Framework Core
- **Architecture:** Service-Repository Pattern

---

## Required Review

### 1. Architecture Alignment

Verify that the change follows the project's existing domain boundaries, dependency direction, layer responsibilities, naming, and placement conventions.

Check for:

- Domain and business logic placed in the correct domain or application layer.
- Infrastructure, persistence, API, UI, and external integration concerns kept outside the domain unless explicitly intended by the architecture.
- Existing abstractions and architectural patterns reused instead of duplicated, bypassed, or contradicted.
- New classes, interfaces, services, handlers, repositories, models, and validators placed in the correct project, folder, or namespace.
- Dependencies pointing in the correct architectural direction.
- Intentional architectural changes explicitly documented and justified.

---

### 2. Required Change Logic

Verify that the implementation fully and correctly satisfies the stated requirement.

Check for:

- Required behavior implemented completely.
- Business rules represented explicitly and consistently.
- Edge cases, invalid input, null handling, empty input, and state transitions handled appropriately.
- Unrelated behavior not changed unintentionally.
- Breaking changes identified and documented when applicable.
- Implementation assumptions supported by the task, project behavior, or existing domain rules.

---

### 3. Error Handling

Verify that failures are deliberate, consistent, safe, and surfaced at the correct boundary.

Check for:

- Validation failures and expected failure conditions handled explicitly.
- Exceptions not silently swallowed.
- Exceptions not used for normal control flow.
- External calls, persistence operations, serialization, parsing, file access, and asynchronous workflows handling failure scenarios.
- Error messages providing useful context without exposing sensitive information.
- Logging performed at the appropriate application boundary and severity level.
- Errors not logged repeatedly across layers without a specific reason.
- API or UI-facing failures translated into appropriate responses when applicable.
- Rollback, consistency, or compensating behavior considered for partially completed operations where applicable.

---

### 4. Maintainability, SOLID, Resource Ownership, and Lifetime Safety

Verify that the implementation remains understandable, focused, decoupled, testable, and safe under its configured lifetime.

Check for:

- Classes and methods having clear, focused responsibilities.
- Business logic, validation, persistence, external integration, and presentation concerns not combined unnecessarily.
- Minimal coupling and appropriate use of abstractions.
- Interfaces introduced only when they provide meaningful separation, substitution, testability, or architectural alignment.
- Direct and readable control flow without excessive nesting, hidden side effects, or unnecessary indirection.
- Clear names that communicate business intent and technical purpose.
- Comments used only to explain non-obvious decisions or constraints.
- Scope limited to the required change unless associated refactoring is necessary and justified.
- Reusable business behavior extracted where appropriate without creating unnecessary abstraction.

#### Resource Ownership and Disposal

Check for:

- A service implements `IDisposable` or `IAsyncDisposable` only when it directly owns a resource requiring deterministic cleanup.
- A service does not implement disposal only because it receives or uses an injected disposable dependency.
- A service does not dispose dependencies supplied by dependency injection, a factory, or a caller unless ownership is explicitly transferred by contract.
- The dependency injection container remains responsible for disposing container-created disposable services.
- Resources created and owned by a service are disposed correctly and exactly once.
- `IAsyncDisposable` is used when owned cleanup requires asynchronous operations.
- Ownership transfer, where applicable, is explicit and understandable from the API or implementation.
- Resource ownership, disposal responsibility, and dependency injection lifetime are consistent and reviewable.
- Disposable transient resources are not created in a way that leaves disposal responsibility unclear.

#### Dependency Injection Lifetime Safety

Check for:

- Singleton services do not directly or indirectly capture scoped dependencies.
- Scoped services and request-owned resources do not escape their valid scope.
- Service lifetimes are appropriate for their state and dependencies.
- Factories or scopes are used correctly when a longer-lived service must perform scoped work.
- The implementation does not introduce hidden lifetime mismatches or premature disposal risks.

---

### 5. Asynchronous Execution, Cancellation, and Concurrency

Verify that asynchronous work is awaited, cancellable where required, and safe for the lifetimes and thread-safety constraints of its dependencies.

Check for:

- `async void` not used except for valid UI event handlers.
- Asynchronous operations awaited or otherwise intentionally observed.
- Exceptions from asynchronous work remaining observable and handled at the correct boundary.
- `CancellationToken` accepted and propagated for cancellable operations, including I/O, persistence, external API calls, and long-running work.
- Cancellation not incorrectly converted into an ordinary failure or swallowed without reason.
- Shared mutable state not accessed concurrently unless explicitly safe.
- Scoped dependencies not used concurrently unless explicitly thread-safe.
- Database contexts and other known non-thread-safe dependencies not used concurrently.
- Background, queued, or fire-and-forget work not outliving scoped dependencies, request-owned resources, or disposed services.
- Blazor or UI state mutations occurring within the correct lifecycle and synchronization context when applicable.

---

### 6. Tests and Verification Evidence

Verify the changed behavior through available validation and record what was actually inspected or executed.

Check for:

- Tests covering new business logic where applicable.
- Tests covering failure paths, validation paths, and important edge cases where applicable.
- Tests covering disposal, lifetime, cancellation, or concurrency behavior when the change affects those areas.
- Existing tests not removed, bypassed, or weakened without documented justification.
- Tests verifying observable behavior rather than unnecessary implementation detail.
- Build, relevant test suite, analyzers, format checks, or static validation executed when available.
- Unexecuted validation recorded as `Not Verified`.
- Residual risks documented without presenting assumptions as confirmed results.

---

## Finding Severity

Classify every identified issue as one of the following:

### Blocking

A finding is **Blocking** when it involves:

- Incorrect or incomplete required behavior.
- Domain architecture violation.
- Unsafe or inconsistent error handling.
- Invalid resource ownership or disposal behavior.
- Incorrect implementation of `IDisposable` or `IAsyncDisposable`.
- Disposal of a dependency not owned by the service.
- Dependency injection lifetime mismatch.
- Capturing scoped dependencies in singleton services.
- Unsafe asynchronous execution or unobserved task failures.
- Missing cancellation propagation where cancellation is required.
- Concurrent use of non-thread-safe dependencies, including database contexts.
- Security, privacy, or sensitive data exposure risk.
- Broken build, failing required tests, or weakened test protection.
- Code that cannot be reviewed, maintained, or validated safely.

### Non-Blocking

A finding is **Non-Blocking** only when it is an improvement that does not affect correctness, architecture, safety, resource management, lifetime behavior, test confidence, or manual reviewability.

---

## Required Pre-PR Validation Report

Before creating, recommending, or approving a pull request, produce the following report:

```markdown
## Pre-PR Validation Report

### Summary

- Requirement reviewed: ...
- Files or areas reviewed: ...
- Architecture or patterns inspected: ...
- Build/tests/analyzers executed: ...
- Result: Ready for PR / Not Ready for PR

### Blocking Findings

1. **[Category] Finding Title**
   - Location: `path/to/file`
   - Issue: ...
   - Impact: ...
   - Required fix: ...

If none:

- None identified.

### Non-Blocking Findings

1. **[Category] Finding Title**
   - Location: `path/to/file`
   - Recommendation: ...

If none:

- None identified.

### Validation Notes

- Architecture alignment: Pass / Fail / Not Verified
- Required change logic: Pass / Fail / Not Verified
- Error handling: Pass / Fail / Not Verified
- Maintainability and SOLID: Pass / Fail / Not Verified
- Resource ownership and disposal: Pass / Fail / Not Verified
- Dependency injection lifetime safety: Pass / Fail / Not Verified
- Async execution and cancellation: Pass / Fail / Not Verified
- Concurrency safety: Pass / Fail / Not Verified
- Tests and verification: Pass / Fail / Not Verified

### Residual Risks

- ...

If none:

- None identified.

### Decision

- Ready for PR: Yes / No
- Blocking reasons, if any: ...
- Required actions before PR, if any: ...
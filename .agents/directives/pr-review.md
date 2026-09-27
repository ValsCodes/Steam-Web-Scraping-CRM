# SteamApp Pre-PR Validation Directive

## Objective

Before creating, recommending, or approving a pull request, review the exact changed commit and working-tree diff for architectural fit, required behavior, failure handling, maintainability, resource ownership, dependency-injection lifetime safety, asynchronous execution, concurrency, security, and validation evidence.

Base findings on SteamApp source, project files, tests, and reachable execution paths. Do not substitute generic architectural preferences for the repository's current design.

## Review scope

1. Record the commit SHA, whether uncommitted changes are included, and a snapshot of the reviewed scope (staged/unstaged diff plus included untracked file contents). Re-check both the commit and scope snapshot immediately before the recommendation. A changed commit invalidates the prior recommendation; changed working-tree content requires review and validation of the changed portions before a new recommendation.
2. Inspect the diff, affected unchanged code, callers, consumers, configuration, and relevant tests.
3. Use scoped graph discovery only when dependencies or impact are not evident from the diff.
4. Re-check the reviewed commit and scope snapshot before the final recommendation as required above.
5. Apply `.agents/directives/security-review.md` and include its required Security Review report.

## Required conventions

Read and follow `.agents/skills/csharp-conventions/SKILL.md` for affected C#/.NET code or project references and `.agents/skills/ui-conventions/SKILL.md` for affected Angular code, including tests. Apply both for cross-stack work. Load other applicable skills as required by `AGENTS.md`.

## Architecture alignment

Verify compliance with the applicable conventions skill, including structural completion checks. Establish actual dependencies from source and project files.

## Required behavior

Verify:

- The stated requirement is complete, including normal, empty, invalid, boundary, cancellation, and failure paths.
- API status codes, payloads, state transitions, database writes, history records, cache behavior, ordering, and external effects are deliberate.
- User-owned queries and mutations use the authenticated user ID unless an explicit admin/internal policy authorizes broader access.
- Client models, routes, and service contracts remain synchronized with server DTOs and endpoints.
- Scrape/manual-check flows preserve partial results, error traces, status transitions, cancellation, correlation IDs, and rerun inputs.
- Breaking changes and assumptions are explicit.

## Implementation safety

Verify relevant C# conventions for errors/logging, external boundaries, data access, DI/resources, async/workers/brokers, and caching. Verify UI conventions for structure, lifecycle, async state, contracts, session handling, and external links. Record evidence and reachable defects using the finding gates below; do not duplicate the conventions here.

## Tests and evidence

- Apply `unit-tests.md`.
- Run affected .NET and/or Angular unit tests.
- Run builds or focused static validation appropriate to the changed surface.
- Run integration tests when explicitly requested or necessary to prove the affected boundary. Run E2E only when explicitly requested. Mark applicable skipped/unavailable checks `Not Verified` and unrelated checks `N/A`.
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
- Reviewed scope snapshot:
- Skills followed and unmet requirements:
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

Append the Security Review report required by `security-review.md`.

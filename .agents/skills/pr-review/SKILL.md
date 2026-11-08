---
name: pr-review
description: Review SteamApp changes for PR readiness, including behavior-preserving refactors. Use for /pr-review, /pr-review-refactor, requests for a code/refactor readiness review, or before creating or recommending a PR.
---

# SteamApp review

## Scope and evidence

1. Read AGENTS.md and applicable conventions. Use unit-tests for validation selection and codebase-discovery only when callers or impact are unclear.
2. Record HEAD, included staged/unstaged changes, and relevant untracked files. Inspect the diff plus affected callers, contracts, configuration, and tests. Recheck that scope before the recommendation; review/validate any changed portion.
3. Review without editing unless implementation was also authorized. Do not create a PR, merge, or deploy merely because review was requested.

## Review

- Verify required behavior, validation/errors, client/server contracts, persistence and side-effect order, user ownership, lifetime/disposal, cancellation/concurrency, and visible Angular state only where affected.
- Check placement and dependencies against source and the local conventions; do not impose a different architecture.
- Perform the security triage in AGENTS.md. Invoke security-review only for a relevant boundary or explicit assessment; otherwise report brief triage without a separate security report.
- Select focused evidence under AGENTS.md and unit-tests. Do not run every suite for a small change.

## Behavior-preserving mode

For /pr-review-refactor or refactor-only readiness, establish the before/after contract: inputs/outputs, errors, authorization, persistence, messages/cache, async ordering, and UI state where affected. Use installed csharp-refactoring for C# mechanics; preserve Angular and cross-stack behavior locally. Add characterization coverage when important behavior is unprotected. Record existing defects separately; do not silently fix them. Require a concrete readability/maintenance benefit. Do not demand migrations outside the authorized scope.

## Findings and decision

Lead with reachable defects. For each, provide severity, path/line, evidence, impact, and required correction. Distinguish introduced issues from existing defects and uncertainty from verified behavior. Treat incorrect behavior, broken contracts, ownership/security/lifetime failures, and necessary failed/unverified checks as blocking; omit speculative findings and unrelated checklists.

Finish with reviewed scope, exact checks/results, unresolved risks, and Ready / Not Ready / Not Verified. State behavior-preservation evidence for refactor mode. Use one combined report; include detailed security findings only when that workflow applies.

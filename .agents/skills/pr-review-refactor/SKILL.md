---
name: pr-review-refactor
description: Validate a SteamApp refactoring against its existing observable behavior, architecture, tests, async/lifetime rules, and security controls. Use when the user invokes /pr-review-refactor, requests refactoring validation, or asks whether a behavior-preserving refactor is PR-ready.
---

# SteamApp Refactoring PR Review

## Resolve directives

Read these files completely:

- `.agents/directives/Directive.Code.Refactoring_v1.md`
- `.agents/directives/Directive.UnitTest_v1.md`
- `.agents/directives/Directive.Security_v2.md` when the refactor touches a security trigger

If an exact version is missing, use the highest available version with the same stem. Stop when a required directive cannot be found.

## Establish behavior first

1. Record the commit and changed files.
2. Identify the public/API/client contract, callers, tests, database/message/cache side effects, error behavior, authorization/ownership, async ordering, and cancellation that must remain unchanged.
3. Use characterization tests when important behavior lacks coverage.
4. Use scoped Graphify and `codebase-discovery` only where callers or dependencies are unclear.
5. Verify behavior against source rather than names or comments alone.

## Review

Apply only relevant directive sections. Confirm the refactor:

- Preserves business, API, persistence, message, cache, security, and Angular-visible behavior.
- Respects SteamApp project placement and actual project references.
- Preserves DbContext ownership, DI lifetimes, cancellation, worker/consumer scope, and resource disposal.
- Is materially easier to understand, test, or maintain.
- Does not hide a bug fix, feature, schema change, or unrelated redesign.

Record discovered existing defects separately. Do not demand migration validation unless the user explicitly included migration work.

## Validation and output

Run relevant unit tests and builds. Skip integration/E2E unless explicitly requested and report applicable suites as `Not Verified`.

Use the Code Refactoring Validation Report from the refactoring directive. Add the Security Review report when the security directive applies. Do not recommend the PR while a Blocking finding remains.


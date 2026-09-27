---
name: pr-review-refactor
description: Validate a SteamApp refactoring against its existing observable behavior, architecture, tests, async/lifetime rules, and security controls. Use when the user invokes /pr-review-refactor, requests refactoring validation, or asks whether a behavior-preserving refactor is PR-ready.
---

# SteamApp Refactoring PR Review

## Resolve directives

Read these files completely:

- `.agents/directives/refactoring-review.md`
- `.agents/directives/unit-tests.md`
- `.agents/directives/security-review.md` when the refactor touches a security trigger

If any required directive is missing, report its exact path and pause the affected review. Do not substitute another directive.

## Required conventions

Read and follow `.agents/skills/csharp-conventions/SKILL.md` for affected C#/.NET code or project references and `.agents/skills/ui-conventions/SKILL.md` for affected Angular code, including tests. Apply both for cross-stack work. Load other applicable skills as required by `AGENTS.md`.

## Establish behavior first

1. Record the commit SHA, whether uncommitted changes are included, and a snapshot of the reviewed scope (staged/unstaged diff plus included untracked file contents). Re-check both the commit and scope snapshot immediately before the recommendation. A changed commit invalidates the prior recommendation; changed working-tree content requires review and validation of the changed portions before a new recommendation.
2. Identify the public/API/client contract, callers, tests, database/message/cache side effects, error behavior, authorization/ownership, async ordering, and cancellation that must remain unchanged.
3. Use characterization tests when important behavior lacks coverage.
4. Use `codebase-discovery` and codebase-memory-mcp as the primary indexed discovery path where callers or dependencies are unclear.
5. Use scoped Graphify only when explicitly requested or when codebase-memory-mcp is unavailable or insufficient and the existing graph can materially help.
6. Verify behavior against source rather than names or comments alone.

## Review

Apply only relevant directive sections. Confirm the refactor:

- Preserves business, API, persistence, message, cache, security, and Angular-visible behavior.
- Follows applicable conventions skills and preserves resource ownership and lifetime behavior.
- Is materially easier to understand, test, or maintain.
- Does not hide a bug fix, feature, schema change, or unrelated redesign.

Record discovered existing defects separately. Do not demand migration validation unless the user explicitly included migration work.

## Validation and output

Identify applicable skills followed and any unmet mandatory requirements.

Run relevant unit tests and builds. Run integration tests when explicitly requested or necessary to prove the affected boundary. Run E2E only when explicitly requested. Mark applicable skipped/unavailable checks `Not Verified` and unrelated checks `N/A`.

Use the Code Refactoring Validation Report from the refactoring directive. Add the Security Review report when the security directive applies. Do not recommend the PR while a Blocking finding remains.

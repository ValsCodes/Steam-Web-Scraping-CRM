---
name: pr-review
description: Perform SteamApp's commit-bound pre-PR review across server, Angular client, tests, architecture, async/lifetime safety, and security. Use when the user invokes /pr-review, asks whether changes are ready for a PR, or requests the repository pre-PR validation workflow.
---

# SteamApp PR Review

## Resolve directives

Read these files completely before review:

- `.agents/directives/pr-review.md`
- `.agents/directives/unit-tests.md`
- `.agents/directives/security-review.md`

If any required directive is missing, report its exact path and pause the affected review. Do not substitute another directive.

## Required conventions

Read and follow `.agents/skills/csharp-conventions/SKILL.md` for affected C#/.NET code or project references and `.agents/skills/ui-conventions/SKILL.md` for affected Angular code, including tests. Apply both for cross-stack work. Load other applicable skills as required by `AGENTS.md`.

## Bind the review

Record the commit SHA, whether uncommitted changes are included, and a snapshot of the reviewed scope (staged/unstaged diff plus included untracked file contents). Re-check both the commit and scope snapshot immediately before the recommendation. A changed commit invalidates the prior recommendation; changed working-tree content requires review and validation of the changed portions before a new recommendation. Review the diff plus relevant unchanged callers, consumers, configuration, and tests.

Do not modify code, create a PR, merge, deploy, or accept security risk unless the user explicitly requests that separate action.

## Locate affected paths

Use direct source inspection when the diff is locally clear. Otherwise:

1. Use `codebase-discovery` and the narrowest codebase-memory-mcp operation for type-aware callers, dependencies, routes, architecture, or change impact.
2. Use scoped Graphify queries only when explicitly requested or when codebase-memory-mcp is unavailable or insufficient and the existing graph can materially help.
3. Verify findings against source and project files.

Avoid repository-wide browsing when a narrower path establishes the behavior.

## Review and validate

1. Apply the PR directive to changed production code and affected paths.
2. Apply the security directive, classify risk, and select relevant SteamApp playbooks.
3. Apply the unit-test directive to relevant tests.
4. Run affected server/client unit tests and builds when available.
5. Run integration tests when explicitly requested or necessary to prove the affected boundary. Run E2E only when explicitly requested. Mark applicable skipped/unavailable checks `Not Verified` and unrelated checks `N/A`.
6. Treat unresolved Blocking findings as `Not Ready for PR`.

## Output

Identify applicable skills followed and any unmet mandatory requirements.

Use the Pre-PR Validation Report format from the PR directive, followed by the Security Review report. Include precise locations, evidence, reachable paths, commands, outcomes, and every `Not Verified` check.

---
name: pr-review
description: Perform SteamApp's commit-bound pre-PR review across server, Angular client, tests, architecture, async/lifetime safety, and security. Use when the user invokes /pr-review, asks whether changes are ready for a PR, or requests the repository pre-PR validation workflow.
---

# SteamApp PR Review

## Resolve directives

Read these files completely before review:

- `.agents/directives/Directive.Code.PR_v4.md`
- `.agents/directives/Directive.UnitTest_v1.md`
- `.agents/directives/Directive.Security_v2.md`

If an exact version is absent, locate the highest available version with the same directive stem. Stop rather than perform a partial review when any required directive is missing.

## Bind the review

Record the current commit and working-tree scope. Review the diff plus relevant unchanged callers, consumers, configuration, and tests. Re-check the commit immediately before the recommendation.

Do not modify code, create a PR, merge, deploy, or accept security risk unless the user explicitly requests that separate action.

## Locate affected paths

Use direct source inspection when the diff is locally clear. Otherwise:

1. Use scoped Graphify queries because this repository maintains `graphify-out/graph.json`.
2. Use `codebase-discovery` for type-aware callers, dependencies, routes, or change impact when Graphify is insufficient.
3. Verify findings against source and project files.

Avoid repository-wide browsing when a narrower path establishes the behavior.

## Review and validate

1. Apply the PR directive to changed production code and affected paths.
2. Apply the security directive, classify risk, and select relevant SteamApp playbooks.
3. Apply the unit-test directive to relevant tests.
4. Run affected server/client unit tests and builds when available.
5. Skip integration and E2E suites unless explicitly requested; mark relevant skipped suites `Not Verified`.
6. Treat unresolved Blocking findings as `Not Ready for PR`.

## Output

Use the Pre-PR Validation Report format from the PR directive, followed by the Security Review report. Include precise locations, evidence, reachable paths, commands, outcomes, and every `Not Verified` check.


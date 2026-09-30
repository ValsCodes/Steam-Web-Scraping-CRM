# SteamApp repository instructions

## Command execution and scope

- Run Windows commands through `pwsh -NoProfile -Command "<command>"`; use `powershell` if `pwsh` is unavailable.
- Use `rg` or `rg --files` for search and file discovery.
- Never expose secrets, connection strings, tokens, private data, or sensitive local configuration in output or reports.
- Preserve unrelated user changes. Make the smallest complete change and avoid unrelated refactoring.
- For an identified, locally understandable task, inspect the specified files directly. Expand investigation only when correctness requires callers, dependencies, architecture, or change impact.
- Source and project files are authoritative; generated graphs are navigation aids.

## Mandatory skill enforcement

Before implementation or review, identify every applicable repository skill in the table below. Explicit skill invocations and matching automatic triggers MUST load and follow the skill.

- Read the applicable `SKILL.md` and every directive or reference it requires before performing the governed work. Conditional references are required only when their condition applies.
- Follow required procedures, validation, gates, and report formats. Do not substitute a generic checklist or claim compliance without performing the required work.
- Apply overlapping skills together. Cross-stack changes require both conventions skills; relation mutations may also require bulk, testing, and security workflows.
- Skill enforcement does not authorize unrelated discovery, refactoring, external actions, or broader validation.
- If a required skill, directive, or reference is missing, report its exact path and pause the affected workflow. Continue independent authorized work; do not silently substitute another policy.
- Existing user authorization remains valid. Do not request it again merely because a skill applies.
- At completion, identify skills followed, validation performed, and unmet mandatory requirements. Do not recommend readiness while applicable blocking gates remain unmet.

All skill paths below are relative to `.agents/skills/`.

| Trigger | Required skill | Required directive |
|---|---|---|
| Create, modify, move, refactor, or review C# code or .NET project references, including tests | `csharp-conventions` | None |
| Create, modify, move, refactor, or review Angular components, templates, styles, services, models, routes, or client tests | `ui-conventions` | None |
| Repository discovery, callers, dependencies, routes, architecture, or change-impact analysis | `codebase-discovery` | None |
| Direct SQL Server/LocalDB inspection or mutation, including preset creation and catalog data operations | `steamapp-database-operations` | None |
| Join-table selection or batch relation mutations | `bulk-relations` | None |
| Explicit test work or required characterization/regression coverage | `unit-tests` | `unit-tests.md` |
| `/pr-review`, standard pre-PR readiness review, or creating/recommending/approving a PR | `pr-review` | `pr-review.md`, `unit-tests.md`, `security-review.md` |
| `/pr-review-refactor` or behavior-preserving refactor readiness review | `pr-review-refactor` | `refactoring-review.md`, `unit-tests.md`; security directive when triggered |
| `/security-review`, security assessment, or documented high-risk change | `security-review` | `security-review.md` |
| `/graphify`, explicit Graphify analysis/maintenance, graph-specific outputs, selected discovery fallback, or required source-change update | `graphify` | None |

For a refactor-only readiness review, use `pr-review-refactor` instead of standard `pr-review`; mixed behavior changes use standard review. Apply other matching skills in either case.

Directives live under `.agents/directives/` and are authoritative for their workflow checks. They do not override higher-priority instructions or existing user authorization. Git history records directive versions; load the named file, never a version fallback.

## Project orientation

- Angular client: `SteamApp.Client`.
- Server projects and .NET tests: `SteamApp.Server`.
- Server ownership and project dependencies: [C# conventions](.agents/skills/csharp-conventions/SKILL.md).
- Angular conventions and browser behavior: [UI conventions](.agents/skills/ui-conventions/SKILL.md).

## Security workflow triggers

Preserve security controls unless the authorized task explicitly changes them. Never put real secrets in tracked configuration, logs, tests, or examples.

High-risk changes require `security-review`: authentication/Identity/JWT/roles/admin, authorization/ownership/internal-job trust, secrets/cryptography, user-controlled URLs/scraping/parsing/outbound HTTP, RabbitMQ, sensitive caches or personal data, files/paths/serialization/code execution, public API controls, migrations/infrastructure/deployment, security tests/controls/audit logging, and externally opened URLs. The security directive defines the complete risk classification and gates.

## Discovery and generated graphs

- Exact, locally understandable edits permit direct inspection without invoking discovery.
- For repository discovery, use `codebase-discovery` and codebase-memory-mcp first. Check freshness and re-index only when missing or stale.
- Verify implementation-sensitive, negative, exhaustive, and security-sensitive conclusions against source and project files.
- Use Graphify for explicit graph work or a selected fallback when the primary index is unavailable or insufficient. Do not routinely use both graph systems.
- Read `graphify-out/wiki/index.md` for broad Graphify navigation; read `GRAPH_REPORT.md` only for broad graph analysis or insufficient scoped queries.
- Dirty `graphify-out/` files are expected. After source-code changes, follow the Graphify update procedure and run `graphify update .` once per task. If that update still fails after any sandbox escalation required by the host, report it as `Not Verified` and do not invoke Graphify again during the task unless the user explicitly requests a retry. Do not update for read-only reviews or instruction/configuration-only changes unless requested.

## Validation policy

Run only validation relevant to the affected scope, plus checks required by applicable workflows.

- Server build, repository root: `dotnet build SteamApp.Server\SteamApp.WebAPI\SteamApp.sln -v:m`.
- Server unit tests, repository root: `dotnet test SteamApp.Server\SteamApp.Tests\SteamApp.Tests.csproj -v:m -m:1`.
- Client build, from `SteamApp.Client`: `npm.cmd run build`.
- Client unit tests, from `SteamApp.Client`: `npm.cmd run test:unit`.
- Integration tests run when explicitly requested or necessary to prove the affected boundary; commands are in the testing directive.
- Server/client E2E tests run only when explicitly requested.
- Report failed checks as failed, applicable skipped/unavailable checks as `Not Verified`, and unrelated checks as `N/A`. Never imply unexecuted validation passed.

## Completion

Confirm the requested behavior or configuration exists. Inspect the final diff for readability, scope, structure, security, and accidental changes. Complete applicable skill checks and relevant validation; update Graphify after source changes. Report skills followed, exact commands and outcomes, unmet requirements, applicable `Not Verified` checks, and residual risk.

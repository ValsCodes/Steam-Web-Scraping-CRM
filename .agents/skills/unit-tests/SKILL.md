---
name: unit-tests
description: Add, update, or validate focused SteamApp tests using the repository's NUnit/Moq and Angular Jasmine/Jest conventions. Use when the user invokes /unit-tests, requests tests for a change, asks for regression coverage, or requests the repository unit-test workflow.
---

# SteamApp Unit Tests

## Resolve the directive

Read `.agents/directives/unit-tests.md` completely before governed work. If it is missing, report its exact path and pause that workflow; do not substitute another directive.

Treat the resolved directive as authoritative.

## Required conventions

Read and follow `.agents/skills/csharp-conventions/SKILL.md` for affected C#/.NET code or project references and `.agents/skills/ui-conventions/SKILL.md` for affected Angular code, including tests. Apply both for cross-stack work. Load other applicable skills as required by `AGENTS.md`.

## Procedure

1. Identify the changed observable behavior and the closest existing tests.
2. Use direct source inspection for an already identified target. Use `codebase-discovery` only when callers, endpoints, or impact are unclear.
3. Select the existing test boundary:
   - .NET unit/TestServer: `SteamApp.Tests`
   - .NET integration: `SteamApp.IntegrationTests`
   - .NET E2E: `SteamApp.E2ETests`
   - Angular unit: `*.unit.spec.ts` through Jasmine/Karma
   - Angular integration: existing Jest configuration
   - Angular E2E: Playwright
4. Add the smallest behavior-focused coverage required by the directive.
5. Follow the surrounding file's fixtures and style. Use NUnit + Moq + `Assert.That` for .NET and Jasmine spies/testing utilities for Angular unit tests.
6. Keep tests deterministic and isolate external HTTP, Selenium, email, RabbitMQ, Redis, time, and secrets.
7. Run the relevant unit-test command.
8. Run integration tests when explicitly requested or necessary to prove the affected boundary. Run E2E only when explicitly requested. Mark applicable skipped/unavailable checks `Not Verified` and unrelated checks `N/A`.

## Output

Identify applicable skills followed and any unmet mandatory requirements.

Report the tests changed, behaviors covered, exact commands, results, `Not Verified` suites, and remaining untested risk.

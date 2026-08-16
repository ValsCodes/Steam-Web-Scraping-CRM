---
name: unit-tests
description: Add, update, or validate focused SteamApp tests using the repository's NUnit/Moq and Angular Jasmine/Jest conventions. Use when the user invokes /unit-tests, requests tests for a change, asks for regression coverage, or requests the repository unit-test workflow.
---

# SteamApp Unit Tests

## Resolve the directive

Read `.agents/directives/Directive.UnitTest_v1.md` completely before changing tests. If it is absent, locate the highest available `Directive.UnitTest_v*.md`. Stop and report the missing directive when no version exists.

Treat the resolved directive as authoritative.

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
8. Run integration/E2E only when explicitly requested or required by the chosen boundary; otherwise report it as `Not Verified`.

## Output

Report the tests changed, behaviors covered, exact commands, results, `Not Verified` suites, and remaining untested risk.


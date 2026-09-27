# SteamApp Testing Directive

## Objective

When production behavior changes, add or update the smallest deterministic tests that prove the changed contract, normal path, important edge cases, and regression scenario.

Use the existing test stack. Do not introduce another test framework without explicit approval.

## Required conventions

Read and follow `.agents/skills/csharp-conventions/SKILL.md` for affected C#/.NET code or project references and `.agents/skills/ui-conventions/SKILL.md` for affected Angular code, including tests. Apply both for cross-stack work. Load other applicable skills as required by `AGENTS.md`.

## Coverage

For each changed behavior, cover applicable cases:

- Expected successful flow.
- Invalid input and validation failure.
- Null, empty, default, and boundary values.
- Error/exception mapping.
- Authentication, authorization, admin-role, and user-ownership failure.
- Async completion and cancellation without blocking calls.
- Retry, partial-result, status/history, cache, or message behavior.
- The exact regression that motivated a bug fix.

Test observable behavior rather than private implementation structure.

## .NET tests

- Use NUnit attributes and NUnit assertions (`Assert.That`, `Assert.Multiple`).
- Use Moq for mocks when a test double is needed.
- Follow Arrange / Act / Assert while matching the surrounding test file.
- Name tests `Method_StateUnderTest_ExpectedBehavior`.
- Use `async Task`, never `async void`.
- Reuse `SteamApp.Tests/TestSupport/TestDb.cs`, `MinimalApiTestApp`, and existing factories/fixtures when applicable.
- Do not share one `ApplicationDbContext` across concurrent operations.
- Mock HTTP, Selenium, email, RabbitMQ, Redis, clock, and other external dependencies for unit tests.
- Keep tests independent of real time, randomness, network access, machine state, execution order, and developer secrets.

Choose the test project by boundary:

- `SteamApp.Tests`: isolated unit and in-memory/TestServer endpoint tests.
- `SteamApp.IntegrationTests`: database, ASP.NET host, HTTP, message/job, or infrastructure integration behavior.
- `SteamApp.E2ETests`: end-to-end server scenarios only when explicitly requested.

## Angular tests

- Use Jasmine/Karma for `*.unit.spec.ts` tests.
- Use Jasmine spies and Angular testing utilities such as `fakeAsync`/`tick` when they match the async behavior.
- Use Jest only for the existing integration-test configuration.
- Use Playwright only for explicitly requested E2E validation.
- Test component-visible state, emitted requests/actions, cleanup, change-detection effects, external-link disclosure, and error outcomes rather than private implementation.
- Keep RxJS and timer tests deterministic; avoid real network and wall-clock dependencies.

## Commands

Run from the repository root unless stated otherwise:

- Server unit tests: `dotnet test SteamApp.Server\SteamApp.Tests\SteamApp.Tests.csproj -v:m -m:1`
- Server integration tests: `dotnet test SteamApp.Server\SteamApp.IntegrationTests\SteamApp.IntegrationTests.csproj -v:m -m:1`
- Server E2E tests: `dotnet test SteamApp.Server\SteamApp.E2ETests\SteamApp.E2ETests.csproj -v:m -m:1`
- Client unit tests, from `SteamApp.Client`: `npm.cmd run test:unit`
- Client integration tests, from `SteamApp.Client`: `npm.cmd run test:integration`
- Client E2E tests, from `SteamApp.Client`: `npm.cmd run test:e2e`

Run only relevant unit tests by default. Run integration tests when explicitly requested or necessary to prove the affected boundary. Run E2E only when explicitly requested. Mark applicable skipped/unavailable checks `Not Verified` and unrelated checks `N/A`.

## Completion report

Report:

- Skills followed and unmet mandatory requirements.
- Tests added or changed.
- Behaviors and edge cases covered.
- Exact commands executed.
- Pass/fail result.
- Relevant suites not run as `Not Verified`.
- Remaining untested risk.

Never claim an unexecuted test passed.

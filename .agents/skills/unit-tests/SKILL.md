---
name: unit-tests
description: Add or run focused SteamApp regression tests using existing NUnit/Moq, Jasmine/Karma, Jest, and Playwright boundaries. Use for explicit test work or meaningful behavior changes needing characterization or regression coverage.
---

# SteamApp testing

Follow AGENTS.md for proportional validation and applicable C#/UI conventions for test code. Inspect the closest existing fixtures before editing. Do not introduce or migrate test frameworks without explicit authorization.

## Boundaries

| Boundary | Existing stack and location |
|---|---|
| .NET unit/TestServer | NUnit + Moq; SteamApp.Server/SteamApp.Tests |
| .NET infrastructure integration | SteamApp.Server/SteamApp.IntegrationTests |
| .NET E2E | SteamApp.Server/SteamApp.E2ETests; explicit request only |
| Angular unit | Jasmine/Karma; *.unit.spec.ts |
| Angular integration | Existing Jest configuration |
| Angular E2E | Playwright; explicit request only |

Use Assert.That/Assert.Multiple and Method_StateUnderTest_ExpectedBehavior for .NET. Reuse TestDb, MinimalApiTestApp, and nearby factories when appropriate. Keep async tests Task-returning and DbContext operations sequential. Use relational integration tests when transaction rollback or bulk-provider behavior must be proved; EF InMemory is insufficient.

## Coverage and execution

Test observable changed behavior and its regression, selecting relevant normal, invalid, boundary, error, ownership, cancellation, retry, and side-effect cases. Isolate HTTP, Selenium, email, RabbitMQ, Redis, time, randomness, and secrets. Follow existing Jasmine async/spying patterns; plugin Vitest examples do not authorize changing the runner.

Run the smallest affected selection. For .NET append --filter FullyQualifiedName~RelevantFixture; for Angular unit run npm.cmd exec -- ng test --watch=false --browsers=ChromeHeadless --include=src/app/path/example.unit.spec.ts from SteamApp.Client instead of appending to the broad test:unit include; for Jest append -- --runTestsByPath src/app/path/example.spec.ts. Use actual existing test paths. Broaden only for meaningful cross-cutting impact or an explicit request.

Available commands from the repository root:

- dotnet build SteamApp.Server/SteamApp.WebAPI/SteamApp.sln -v:m
- dotnet test SteamApp.Server/SteamApp.Tests/SteamApp.Tests.csproj -v:m -m:1
- dotnet test SteamApp.Server/SteamApp.IntegrationTests/SteamApp.IntegrationTests.csproj -v:m -m:1
- dotnet test SteamApp.Server/SteamApp.E2ETests/SteamApp.E2ETests.csproj -v:m -m:1

From SteamApp.Client: npm.cmd run build, npm.cmd run test:unit, npm.cmd run test:integration, and npm.cmd run test:e2e.

For agent-configuration scripts use python scripts/validate_agent_configuration.py and python -m unittest discover -s scripts/tests -p test_agent_configuration.py; these use only the standard library.

Do not weaken meaningful tests to conceal a behavior change. Report covered behavior, exact commands/results, and necessary unverified checks; use the completion policy in AGENTS.md instead of a separate report template.

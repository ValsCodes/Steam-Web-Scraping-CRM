# SteamApp security playbooks

Apply only sections relevant to the reviewed boundary. All server paths start under SteamApp.Server.

## Authentication and ownership

Inspect SteamApp.WebAPI/Program.cs, Controllers/AuthController.cs, Security/SecurityPolicies.cs, Security/ClaimsPrincipalExtensions.cs, MinimalAPIs/AdminUserEndpoints.cs, SteamApp.Infrastructure/Identity/ApplicationUser.cs, and relevant Angular auth/interceptors/guards as needed.

Verify JWT issuer/audience/signing/lifetime, roles/policies, anonymous boundaries, lockout/password controls, and error redaction. Trace authenticated user IDs through services, EF filters, mutations, cache keys, and history. Admin/internal policies must be explicit and unavailable through caller-selected identifiers. Client filtering never replaces server ownership checks.

## External URLs, scraping, parsing, and files

Treat URLs, Steam URIs, redirects, HTML/JSON, product data, paths, and serialized payloads as untrusted. Check destination/protocol restrictions, redirects, SSRF reachability, traversal, parsing/size limits, timeouts, cancellation, browser execution, and redacted failures where applicable.

SteamApp intentionally permits unverified external URLs after disclosure/consent. Preserve the warning/agreement and noopener,noreferrer; absence of a blocklist alone is not a finding.

## RabbitMQ, workers, and sensitive caches

Verify payload shape, identifiers, authenticated/established ownership and state, correlation, idempotency, retry/dead-letter behavior, cancellation, and per-work scope lifetimes. Acknowledge only after durable intended outcomes. Prevent payloads from selecting arbitrary users, destinations, or privileged actions.

Check user-specific Redis/memory keys, safe serialization, invalidation, concurrency, and sensitive-data retention. Preserve partial results, history/status transitions, recovery inputs, and failure observability.

## API and browser controls

Review changed authorization/ownership, CORS origins/methods/headers and QUERY preflight, rate limits, host filtering, security headers/HTTPS where configured, ProblemDetails redaction/trace IDs, session handling, and external-window opener protections. Compare server and client contracts.

## Secrets, data integrity, and infrastructure

Inspect only relevant tracked configuration/examples/logging/tests for exposed real secrets; never echo their values. Verify fail-closed handling of required JWT, database, Redis, broker, and email settings when affected.

For schema/data/bulk changes verify exact ownership, constraints, atomicity/rollback, idempotency, and recovery. For CI/hooks/infrastructure review permissions, untrusted inputs, executable sources, credential exposure, and external effects. Configuration validators should parse data without executing file contents. Do not invent production requirements for this development project.

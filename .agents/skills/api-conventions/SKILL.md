---
name: api-conventions
description: Apply SteamApp HTTP method, generic result, ProblemDetails, and global exception-handling conventions. Required for API endpoints, HTTP clients or contracts, expected-error flows, exception handling, and custom HTTP methods.
---

# SteamApp API conventions

Apply this skill together with `csharp-conventions` for server work and `ui-conventions` for Angular HTTP-client work. Use `unit-tests` when behavior changes and `security-review` when the public API, CORS, authorization, or error exposure changes.

## HTTP method selection

- Use GET for ordinary resource retrieval and filters that fit naturally in the query string.
- Use QUERY only for a safe, idempotent read whose structured, voluminous, or sensitive criteria genuinely need request content. Do not convert existing GET endpoints without an explicit compatibility plan.
- Never use QUERY for an operation that changes requested application state. A QUERY handler must remain safe and idempotent even when retried.
- QUERY requests require content and a matching `Content-Type`. Map Minimal API handlers with `MapMethods` and `ApiHttpMethods.Query`; Angular clients use `HttpClient.request<T>("QUERY", url, { body })`.
- Keep CORS methods, authorization, ownership checks, rate limits, endpoint metadata, API documentation, clients, and tests synchronized. Browser QUERY requests require CORS preflight.

## Expected errors

- Return `Result<T>` from application or service operations when failure is an expected outcome the caller can act on. Do not throw exceptions for validation, not-found, conflict, authorization, or temporary-unavailability outcomes.
- Keep `Result<T>` transport-neutral. Use `Error` and `ErrorType`; do not store HTTP status codes or ASP.NET types in application results.
- Map failures at the API boundary to ProblemDetails: Validation `400`, NotFound `404`, Conflict `409`, Unauthorized `401`, Forbidden `403`, and Unavailable `503`.
- Framework model validation can continue to return ValidationProblemDetails directly. Do not wrap a result merely to replace framework-native validation.
- Preserve cancellation by propagating `OperationCanceledException` when the relevant token is canceled. Exceptions remain appropriate for programmer errors, broken invariants, and genuinely unexpected failures.

## Unexpected exceptions

- Keep `GlobalExceptionHandler` registered through `AddExceptionHandler`, `AddProblemDetails`, and `UseExceptionHandler` early in the request pipeline.
- Return a redacted 500 ProblemDetails response with the request path and trace ID. Never expose exception messages, stack traces, credentials, tokens, personal data, request bodies, or external payloads.
- Avoid duplicate logging. On .NET 9, rely on the exception-handler middleware diagnostics unless a concrete operational requirement justifies separate logging and its duplication is configured away.
- The HTTP handler covers only the request pipeline. Startup, hosted workers, message consumers, and external integration boundaries retain their own cancellation, retry, recovery, and logging behavior.

## Completion checks

Verify result invariants, error-to-status mapping, redaction, trace correlation, cancellation, CORS preflight for QUERY, and preservation of authorization and rate limiting. Run the validation required by `AGENTS.md` and report applicable unverified checks.

---
name: ui-conventions
description: Apply SteamApp Angular structure, Material/RxJS patterns, lifecycle, client contracts, session safeguards, and external-link behavior. Required when creating, modifying, moving, refactoring, or reviewing Angular components, templates, styles, services, models, routes, or client test code.
---

# SteamApp UI conventions

Read and follow this skill before governed Angular work, including client tests. Testing mechanics remain in `unit-tests`; security assessment criteria remain in `security-review`. Read those skills when triggered. Cross-stack changes also require `csharp-conventions`.

## Structure and implementation

- The client is Angular 21 standalone under `SteamApp.Client`, using Angular Material and RxJS. Follow the existing component, service, routing, model, template, style, and import-barrel conventions in the touched area.
- Before creating, moving, or substantially modifying code, inspect neighboring folders and comparable implementations. Extend the existing feature owning the behavior.
- Preserve existing architecture. Do not add alternative shared modules, top-level structures, parallel services, abstractions, or dependencies merely for organization.
- Prefer clear, direct code and the smallest complete solution. Do not perform unrelated refactoring or introduce a new design system or visual redesign policy.
- Prefer framework-native capabilities appropriate to the actual installed versions. Verify uncertain framework behavior against current official documentation.

## Lifecycle and asynchronous state

- Clean up subscriptions, timers, event listeners, and cancellation resources according to surrounding lifecycle patterns.
- Ensure state changes after asynchronous work remain valid for the component lifecycle; avoid updates to disposed components.
- Preserve request/action ordering, cancellation, component-visible state, route behavior, dialogs, notifications, and error outcomes unless the task intentionally changes them.
- Use manual change detection only where the existing async boundary requires it. Do not mask state-flow defects with repeated `detectChanges()` or introduce repeated/disposed-component updates after `await`.
- Preserve existing RxJS patterns and resource ownership instead of adding unnecessary indirection.

## Contracts and browser safeguards

- Keep client models, routes, and HTTP service contracts synchronized with server DTOs, endpoints, status codes, and payloads.
- Preserve JWT/session handling, interceptor behavior, and route guards; never expose tokens or introduce an authorization bypass. Client filtering does not replace server ownership validation.
- SteamApp intentionally permits opening unverified external URLs after disclosure/consent. Preserve warning/agreement semantics and `noopener,noreferrer`; do not silently replace that flow with a blocklist.
- Treat user-controlled URLs and external content as untrusted. Security-sensitive UI changes require `security-review` in addition to this skill.

## Completion checks

Inspect the final diff for established feature placement, import conventions, contract alignment, lifecycle cleanup, async state, and applicable browser/session safeguards. Confirm visible behavior matches the request without unrelated redesign. Follow `AGENTS.md` validation policy and applicable testing/review workflows; report applicable unexecuted checks as `Not Verified`.

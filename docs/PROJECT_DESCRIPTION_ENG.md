# SteamApp — Extended Project Description

## Project overview

**SteamApp** is a full-stack Steam market intelligence and workflow automation platform. It connects market context, reusable matching rules, live listing checks, automated queues, monitoring targets, and historical evidence in one authenticated workspace.

The product is not a traditional CRM. It does not manage customers, leads, or a sales pipeline. Its operational model is closer to a market-research workbench: users describe the Steam items they care about, capture the rules behind a decision, execute those rules consistently, and keep enough history to explain the result later.

The application combines:

- an Angular client in `SteamApp.Client`;
- a .NET Web API under `SteamApp.Server`;
- SQL Server persistence through Entity Framework Core;
- RabbitMQ-backed and hosted background processing;
- memory and Redis caching;
- authenticated, user-scoped access to saved definitions and runs.

> **Disclaimer:** SteamApp is not affiliated with, endorsed by, or sponsored by Valve Software.

## Product direction

SteamApp has moved beyond its original scraper-oriented CRM description. Its active direction has four pillars:

1. **Connected market context** — games, Steam market URLs, products, tags, item groups, stock history, wish-list conditions, and watch-list targets.
2. **Reusable decision logic** — expression-based manual-check presets and ordered preset combinations.
3. **Observable execution** — live Pending, Matched, No match, and Failed outcomes; run controls; persisted setup and history.
4. **Workflow automation** — automatic queues built from check blocks and delays, with current references resolved at run start and frozen snapshots retained afterward.

Standalone Web Scraper and Pixel client routes are deprecated. Their underlying server code and contracts remain for compatibility, but they are no longer promoted as core product workflows.

## The problem it solves

Steam market research becomes difficult to reproduce when URLs, item definitions, quality rules, price thresholds, and outcomes live in separate spreadsheets or one-off scripts. Even when an individual check is correct, the reasoning can be lost when the same work is repeated later.

SteamApp turns those moving parts into durable records and explicit workflows:

- catalog records provide shared market context;
- presets capture a reusable matching expression;
- preset combinations express broader decisions without copying or changing the original presets;
- manual runs expose live progress and retain their resolved setup;
- automatic queues sequence checks and delays;
- historical snapshots preserve what a run actually evaluated.

## Core capabilities

### Catalog, stock, and monitoring context

- **Games** anchor market-specific records.
- **Game URLs** identify the Steam listing sources used by checks.
- **Products** represent tracked items and their operational metadata.
- **Tags** provide reusable taxonomy.
- **Item groups** collect related items for matching criteria.
- **Current stock and stock history** record inventory context for game URL/product pairs.
- **Wish List** records target conditions such as price thresholds.
- **Watch List** keeps priority market targets visible for ongoing review.

### Manual checks and presets

A manual check evaluates listings from one selected game URL against a criteria expression. Users can:

- construct grouped expressions from supported condition operators;
- save and maintain reusable presets;
- select the products included in a run;
- apply a listing limit and cooldown;
- follow progress while work is running;
- filter results by Pending, Matched, No match, or Failed;
- pause, continue, cancel, inspect, and rerun eligible checks.

### Preset combinations

Combination mode applies two to ten unique presets from the same game. The first term has no operator; later terms use top-level `AND` or `OR`. Evaluation is left-to-right against the same listing asset, while every preset retains its own internal expression grouping.

A combination is a run or queue recipe, not a new saved preset. Its listing limit and cooldown are independent overrides. The server resolves every referenced preset within the authenticated user's scope, validates aggregate criterion and group limits, and stores the resolved recipe with the run.

### Automated queues

An automatic queue is an ordered workflow composed from:

- saved-preset check blocks;
- preset-combination check blocks;
- private-template check blocks;
- delay blocks.

Queue definitions retain live references to saved presets. When a run starts, the server revalidates ownership and compatibility, resolves the latest accessible preset versions, and freezes the resulting block setup into the queue-run snapshot. A missing or inaccessible reference prevents a partial run from being created.

### History and analysis

Manual and queue runs persist status, progress, setup, and results. Frozen snapshots keep historical outcomes understandable even after saved presets change. Supported catalog tables provide filter-first review and Excel export for offline reporting.

## Typical user flows

### Build market context

1. Create a game.
2. Add the Steam market URL used for listing checks.
3. Create products and connect tags or item groups.
4. Maintain stock, wish-list conditions, and watch-list targets as needed.

### Run repeatable listing analysis

1. Choose a game and listing source.
2. Select or create a saved preset, or enter combination mode.
3. Configure products, listing limit, and cooldown.
4. Start the check and filter live outcomes as they arrive.
5. Inspect the frozen setup and results from run history.

### Automate a sequence

1. Create an automatic queue for the relevant market context.
2. Add check and delay blocks in execution order.
3. Save the queue definition and start a run.
4. Follow block-level progress and manage the run from the queue workspace.
5. Use the persisted snapshot to explain or revisit the completed execution.

## System architecture

```text
Angular client
    |
    | HTTPS + bearer JWT
    v
ASP.NET Core API
    |---- EF Core ----> SQL Server
    |---- cache ------> memory / Redis
    |---- messages ---> RabbitMQ consumers and hosted workers
    `---- external ---> Steam listing sources and SMTP
```

The server projects keep explicit ownership boundaries:

| Project | Responsibility |
| --- | --- |
| `SteamApp.Models` | Domain entities, enums, value objects, and constants |
| `SteamApp.Application` | DTOs, mappings, operation results, cache keys, and JSON models |
| `SteamApp.Interfaces` | Repository and service contracts |
| `SteamApp.Infrastructure` | EF Core persistence and external integrations |
| `SteamApp.WebAPI` | HTTP composition, security, orchestration, messages, consumers, workers, and caching |
| `SteamApp.WebApiClient` | Typed .NET API-client managers |

The Angular client uses standalone components and lazy-loaded routes. Route-level screens live under `pages/`; shared dialogs and controls under `components/`; typed HTTP access under `services/`; and client contracts under `models/`.

## Security and reliability

- ASP.NET Core Identity and JWT bearer authentication protect private workflows.
- Policies distinguish authenticated users, administrators, and internal jobs.
- User ownership is checked for saved presets, runs, queue definitions, and referenced IDs.
- General and expensive API operations use separate rate-limit policies.
- External Steam data and user-supplied URLs are treated as untrusted input.
- Long-running work is detached from initiating HTTP requests and persists observable state.
- Cancellation, pause, failure, and partial progress are represented explicitly.
- Configuration and secrets are supplied through environment-specific settings, user secrets, or environment variables rather than tracked production credentials.

## Testing and delivery

The repository contains server unit, integration, and end-to-end projects plus Angular unit, integration, and Playwright suites. Local development can run directly through the .NET and Angular toolchains or with containerized SQL Server, Redis, and optional mail infrastructure.

See [Getting Started](GETTING_STARTED.md), [Architecture Overview](ARCHITECTURE.md), and [API Reference](API_REFERENCE.md) for operational detail.

## Project position

SteamApp is best categorized as a **Steam market intelligence and workflow automation platform**. Its differentiator is not generic record keeping or raw scraping. It is the combination of connected market context, explicit reusable rules, observable execution, queue automation, user-scoped history, and frozen evidence in one maintainable system.

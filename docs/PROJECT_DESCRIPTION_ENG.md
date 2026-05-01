```md
# Detailed Project Description

## 1) What the Project Is

**Steam Web Scraping CRM** is a full-stack system for:

- collecting (scraping) and processing data from Steam Community Market,
- managing catalog data (games, URLs, products, pixels, tags),
- monitoring price conditions (watch/wish logic),
- operational work through an administrative UI.

The project combines:

- Angular client (`SteamApp.Client`),
- .NET Web API (`SteamApp.Server/SteamApp.WebAPI`),
- SQL Server database through EF Core.

---

## 2) Business Goal and Problem Solved

The system reduces manual work when monitoring market offers on Steam through:

1. standardized sources (Game URL configurations),
2. filtering and categorization (Tag/Pixel/Product rules),
3. automated checks (wishlist/watchlist scenarios),
4. centralized administration and data export.

---

## 3) Main Functional Areas

## 3.1 Catalog and Configuration

- **Games**: main games/contexts.
- **GameUrls**: URL templates/settings for scraping:
  - batch paging,
  - public API mode,
  - pixel scraping parameters.
- **Products**: items to monitor.
- **Pixels**: color signatures (RGB/paint values).
- **Tags**: taxonomy for product categorization.

## 3.2 Relations (M2M)

The system supports and manages the following many-to-many relations directly from the main forms:

- GameUrl ↔ Product
- GameUrl ↔ Pixel
- Product ↔ Tag

This is a key UX decision: the operator configures connectivity in the context of the main object instead of using separate “join” screens.

## 3.3 Operational Monitoring

- **WatchList**: active rules for market monitoring.
- **WishList**: target conditions for price notification/checking.

## 3.4 Scraping and Analysis

- scraping of listing pages,
- scraping through public API,
- pixel-based validation of item attributes,
- helper functionality for URL encoding/formatting.

## 3.5 Export and Reporting

The key tables in the UI support export to Excel (XLSX) for offline analysis/reporting.

---

## 4) Roles and Usage (Typical Users)

## 4.1 Operator / Analyst

- prepares Game URL configurations,
- links products/pixels/tags,
- starts scraping scenarios,
- reviews filtered tables,
- exports current results.

## 4.2 Administrator

- maintains the base catalog,
- configures auth clients and secrets,
- manages deployment configurations,
- monitors worker processes.

---

## 5) Key Business Objects and Responsibilities

## 5.1 Game

Contextual “container” for URLs, products, pixels, and tags.

## 5.2 GameUrl

Defines how market data is accessed for a given game/scenario:

- partial URL,
- batch/public API flags,
- pixel coordinates and image size,
- linked products and pixels.

## 5.3 Product

Unit for monitoring/filtering:

- name,
- activity,
- rating,
- tags,
- links to game URLs.

## 5.4 Pixel

Represents a color signature for paint/visual attribute:

- RGB values,
- link to game,
- usage in pixel scraping.

## 5.5 Tag

Categorization layer for products; used for filtering, grouping, and rules.

## 5.6 WatchList / WishList

- WatchList: current targets for monitoring.
- WishList: conditions/threshold for checking and notification.

---

## 6) Critical User Flows

## 6.1 Onboarding a New Game

1. Create a Game.
2. Create a GameUrl (mode + paging + pixel settings).
3. Create Products and Pixels.
4. Link:
   - GameUrl ↔ Products,
   - GameUrl ↔ Pixels,
   - Product ↔ Tags.

## 6.2 Configuring a Scraping Scenario

1. Select Game/GameUrl.
2. Run scraping (page/API).
3. Review/filter in tables.
4. Export to Excel for analysis.

## 6.3 Monitoring Maintenance

1. Add/edit WishList/WatchList.
2. Worker/scheduled checks.
3. Check results and adjust thresholds.

---

## 7) Architectural Structure (Technical)

## 7.1 Frontend (Angular)

- `pages/`: route-level screens.
- `services/`: API clients.
- `models/`: type/DTO representations.
- `components/`: reusable UI elements.

Important UX principles:

- inline relation management,
- filter-first tables,
- export in grid screens.

## 7.2 Backend (.NET)

- Minimal APIs + controllers,
- JWT authentication/authorization,
- EF Core + SQL Server,
- background worker for periodic tasks,
- Swagger (dev), CORS policy.

## 7.3 Data Access

- repository/service separation,
- cache usage for read-heavy operations,
- DTO-based API contracts.

---

## 8) Configuration Model

Startup requires valid keys (connection string, JWT, internal secret, etc.).

Loading environment:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. env vars
4. user secrets (development)

This allows secure separation between dev/test/prod.

---

## 9) Security and Risk Areas

## 9.1 Auth

- JWT Bearer tokens for protected endpoints.
- Need for strict token lifecycle and XSS protection in the client.

## 9.2 CORS/Swagger

- CORS must be restricted outside dev.
- Swagger must be controlled according to environment/access.

## 9.3 Scraping

- external dependencies (Steam DOM/API) are unstable;
- rate limiting, retries, and error handling are mandatory.

---

## 10) Non-Functional Requirements

- **Reliability**: graceful handling of timeout/HTTP errors.
- **Observability**: logs for scraping/worker/auth.
- **Performance**: cache and optimized read operations.
- **Maintainability**: clear boundaries between UI, services, API, data.

---

## 11) What the “Detailed Documentation” Should Include (Recommendation)

To build complete product documentation, it is recommended to add the following sections as separate documents:

1. **Domain glossary** (terms and definitions).
2. **Use-case catalog** (scenarios with pre/post conditions).
3. **ER diagram** (including M2M tables).
4. **Sequence diagrams** for scraping and relation sync.
5. **Runbooks** (incident/restart/config reset).
6. **Security checklist** (token, CORS, secrets, logs).
7. **Release checklist** (migration, smoke tests, rollback).

---

## 12) Brief Summary

The project is an operational platform for Steam market intelligence focused on:

- flexible configuration of data sources,
- rich relation-driven administration,
- filtering + export for analysis,
- backend automation and a secured API layer.

This document can serve as a **base canonical description** for building detailed internal/external documentation.
```

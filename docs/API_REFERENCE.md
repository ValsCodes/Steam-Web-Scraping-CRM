# API Reference (Quick)

Auth: Bearer JWT (global for protected groups). User-scoped resources return only records owned by the authenticated user.

> Tip: use backend Swagger UI for live schemas/examples.

## Auth

| Method | Path | Description |
|---|---|---|
| POST | `/api/Auth/token` | Get access token |

## Games

| Method | Path |
|---|---|
| GET | `/api/games` |
| POST | `/api/games` |
| GET | `/api/games/{id}` |
| PUT | `/api/games/{id}` |
| DELETE | `/api/games/{id}` |

## Game URLs

| Method | Path |
|---|---|
| GET | `/api/game-urls` |
| POST | `/api/game-urls` |
| GET | `/api/game-urls/{id}` |
| PUT | `/api/game-urls/{id}` |
| DELETE | `/api/game-urls/{id}` |

## Products

| Method | Path |
|---|---|
| GET | `/api/products` |
| POST | `/api/products` |
| GET | `/api/products/{id}` |
| PUT | `/api/products/{id}` |
| DELETE | `/api/products/{id}` |

## Pixels

> **Legacy/deprecated:** retained for compatibility. The standalone Pixel client workflow is no longer part of active navigation.

| Method | Path |
|---|---|
| GET | `/api/pixels` |
| POST | `/api/pixels` |
| GET | `/api/pixels/{id}` |
| PUT | `/api/pixels/{id}` |
| DELETE | `/api/pixels/{id}` |

## Tags

| Method | Path |
|---|---|
| GET | `/api/tags` |
| POST | `/api/tags` |
| GET | `/api/tags/{id}` |
| PUT | `/api/tags/{id}` |
| DELETE | `/api/tags/{id}` |
| GET | `/api/tags/game/{gameId}` |

## Product Tags (M2M)

| Method | Path |
|---|---|
| GET | `/api/product-tags` |
| POST | `/api/product-tags` |
| GET | `/api/product-tags/{productId}/{tagId}` |
| DELETE | `/api/product-tags/{productId}/{tagId}` |
| GET | `/api/product-tags/product/{productId}` |

## Game URL Products (M2M)

| Method | Path |
|---|---|
| GET | `/api/game-url-products` |
| POST | `/api/game-url-products` |
| GET | `/api/game-url-products/{productId}/{gameUrlId}` |
| DELETE | `/api/game-url-products/{productId}/{gameUrlId}` |
| GET | `/api/game-url-products/{gameUrlId}` |

## Game URL Pixels (M2M)

> **Legacy/deprecated:** retained for compatibility with the Pixel data model.

| Method | Path |
|---|---|
| GET | `/api/game-url-pixels` |
| POST | `/api/game-url-pixels` |
| GET | `/api/game-url-pixels/{pixelId}/{gameUrlId}` |
| DELETE | `/api/game-url-pixels/{pixelId}/{gameUrlId}` |
| GET | `/api/game-url-pixels/{gameUrlId}` |

## Watch List

| Method | Path |
|---|---|
| GET | `/api/watch-list` |
| POST | `/api/watch-list` |
| GET | `/api/watch-list/{id}` |
| PUT | `/api/watch-list/{id}` |
| DELETE | `/api/watch-list/{id}` |

## Wish List

| Method | Path |
|---|---|
| GET | `/api/wish-list` |
| POST | `/api/wish-list` |
| GET | `/api/wish-list/{id}` |
| PUT | `/api/wish-list/{id}` |
| DELETE | `/api/wish-list/{id}` |

## Item Groups

| Method | Path | Description |
|---|---|---|
| GET | `/api/item-groups/game/{gameId}` | List groups for a game |
| POST | `/api/item-groups` | Create an item group |

## Product Stock

| Method | Path | Description |
|---|---|---|
| PUT | `/api/game-url-products/{productId}/{gameUrlId}/current-stock` | Record current stock |
| GET | `/api/game-url-products/{productId}/{gameUrlId}/current-stock/history` | Read stock history |

## Manual Checks

| Method | Path | Description |
|---|---|---|
| GET | `/api/manual-checks/condition-operators` | List supported criterion operators |
| GET | `/api/manual-checks/presets?gameId={gameId}` | List accessible presets, optionally by game |
| POST | `/api/manual-checks/presets` | Create a preset |
| PUT | `/api/manual-checks/presets/{id}` | Update an owned preset |
| DELETE | `/api/manual-checks/presets/{id}` | Delete an owned preset |
| POST | `/api/manual-checks/runs` | Start a preset or preset-combination run |
| GET | `/api/manual-checks/runs?gameId={gameId}&take={take}` | List run history |
| GET | `/api/manual-checks/runs/{id}` | Read run setup, progress, and results |
| POST | `/api/manual-checks/runs/{id}/pause` | Pause a standalone run |
| POST | `/api/manual-checks/runs/{id}/continue` | Continue a paused standalone run |
| POST | `/api/manual-checks/runs/{id}/cancel` | Cancel a standalone run |
| POST | `/api/manual-checks/runs/{id}/rerun` | Replay a historical frozen setup |

`POST /api/manual-checks/runs` accepts exactly one of `presetId` or `presetCombination`. Combination terms are ordered, use only top-level `And`/`Or`, and must reference unique presets from the selected game. The server resolves and validates every reference within the authenticated user's scope.

## Automatic Check Queues

| Method | Path | Description |
|---|---|---|
| GET | `/api/automatic-check-queues` | List owned queue definitions |
| GET | `/api/automatic-check-queues/{id}` | Read a queue definition |
| POST | `/api/automatic-check-queues` | Create a queue definition |
| PUT | `/api/automatic-check-queues/{id}` | Update a queue definition |
| DELETE | `/api/automatic-check-queues/{id}` | Delete a queue definition |
| POST | `/api/automatic-check-queues/{id}/runs` | Resolve references and start a queue run |
| GET | `/api/automatic-check-queues/runs?queueId={queueId}&take={take}` | List queue-run history |
| GET | `/api/automatic-check-queues/runs/{id}` | Read a queue run and block progress |
| POST | `/api/automatic-check-queues/runs/{id}/pause` | Pause a queue run |
| POST | `/api/automatic-check-queues/runs/{id}/continue` | Continue a paused queue run |
| POST | `/api/automatic-check-queues/runs/{id}/cancel` | Cancel a queue run |

Queue definitions may contain saved-preset, preset-combination, private-template, and delay blocks. Saved references are revalidated and resolved at run start; the resolved recipe is then frozen into the run snapshot.

## Legacy direct scraping endpoints

> **Legacy/deprecated:** these endpoints remain documented because they still exist on the server, but the standalone Web Scraper and Pixel routes are not active product workflows.

| Method | Path |
|---|---|
| GET | `/steam/scrape-page/gameUrl/{gamerUrlId}/page/{page}` |
| GET | `/steam/scrape-public-api/gameUrl/{gameUrlId}/page/{page}` |
| GET | `/steam/pixel-info/gameUrl/{gameUrlId}` |
| GET | `/steam/scrape-pixels/gameUrl/{gamerUrlId}/page/{page}` |
| GET | `/steam/check-wishlist/{wishlistId}` |

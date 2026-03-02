# API Reference (Quick)

Auth: Bearer JWT (global for protected groups).

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

## Steam scraping endpoints

| Method | Path |
|---|---|
| GET | `/steam/scrape-page/gameUrl/{gamerUrlId}/page/{page}` |
| GET | `/steam/scrape-public-api/gameUrl/{gameUrlId}/page/{page}` |
| GET | `/steam/pixel-info/gameUrl/{gameUrlId}` |
| GET | `/steam/scrape-pixels/gameUrl/{gamerUrlId}/page/{page}` |
| GET | `/steam/check-wishlist/{wishlistId}` |

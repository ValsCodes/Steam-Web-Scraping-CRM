# Web Scraping CRM for Steam Community market

**An Angular 21 client integrated with a .NET 9 Web API backend**. This application helps you find items listed on the Steam Community Market at favorable price ranges—whether you’re investing or simply adding to your personal collection.

> **Disclaimer**: This project is **not** affiliated with, endorsed, or sponsored by Valve Software. It simply consumes publicly available endpoints provided by Valve.

---

## 🚀 Features
- 💰 **Price Scanning**: Quickly search for listings on the Steam Community Market within your desired price range.
- 🏷️ **Minimalistic UI**: A sleek interface built with Angular, Tailwind, and SCSS for a focused user experience.
- ⏱️ **Real-Time Updates**: Fetch live listing and pricing data directly from the Steam Community Market.
- 📈 **Investment Insights**: Identify undervalued items that might be profitable for collectors or traders.

## Backend

- **In-memory caching** (IMemoryCache)
- **JWT authentication**
- **.NET Minimal APIs**
- **Email integration** (Mailtrap)
- **Background jobs**
- **AutoMapper**
- **Logging**
- **User Secrets**

## Frontend

- **Angular**
- **Tailwind CSS + SCSS**
- **Auth guard** (route protection)
- **HTTP interceptor**
- **RxJS**
- **Angular Material tables**
- **Environment variables**

---

## 📥 Installation

1. **Clone the Repository**:
   ```bash
   git clone *repo*
   ```
2. **Install Angular Client Dependencies**:
    ```bash
    npm install
    ```
3. **Restore & Build the .NET 9 Web API**:
    ```bash
    dotnet restore
    dotnet build
    ```
4. **Setup User Secrets + Database**:
    - Request User Secrets from the owener of the repo
   ```bash
   update-database
   ```
5. **Run the Application**:
Terminal 1 (Web API):
   ```bash
   dotnet run
   ```
6. **Terminal 2 (Angular client)**:
   ```bash
    npm start
   ```
Open your browser at http://localhost:4200.

## 📌 Requirements
- 🏷️ Node.js (latest LTS recommended)
- 🎯 .NET 9 SDK
- Mailtrap setup guide https://github.com/mailtrap/mailtrap-dotnet


# SteamApp API — Endpoints

Auth: Bearer JWT (global)

## Auth

| Method | Path | Body | Notes |
|---|---|---|---|
| POST | `/api/Auth/token` | `TokenRequest` | Returns `200 OK` |

## Games

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/games` | — | `GetAllGames` → `200 OK` (`GameDto[]`) |
| POST | `/api/games` | `GameCreateDto` | `CreateGame` → `201 Created` (`GameDto`) |
| GET | `/api/games/{id}` | — | `GetGameById` → `200 OK` (`GameDto`), `404 Not Found` |
| PUT | `/api/games/{id}` | `GameUpdateDto` | `UpdateGame` → `204 No Content`, `404 Not Found` |
| DELETE | `/api/games/{id}` | — | `DeleteGame` → `204 No Content`, `404 Not Found` |

## GameUrlPixels

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/game-url-pixels` | — | `200 OK` |
| POST | `/api/game-url-pixels` | `GameUrlPixelCreateDto` | `200 OK` |
| GET | `/api/game-url-pixels/{pixelId}/{gameUrlId}` | — | `pixelId:int64`, `gameUrlId:int64` → `200 OK` |
| DELETE | `/api/game-url-pixels/{pixelId}/{gameUrlId}` | — | `pixelId:int64`, `gameUrlId:int64` → `200 OK` |

## GameUrlProducts

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/game-url-products` | — | `200 OK` |
| POST | `/api/game-url-products` | `GameUrlProductCreateDto` | `200 OK` |
| GET | `/api/game-url-products/{productId}/{gameUrlId}` | — | `productId:int64`, `gameUrlId:int64` → `200 OK` |
| DELETE | `/api/game-url-products/{productId}/{gameUrlId}` | — | `productId:int64`, `gameUrlId:int64` → `200 OK` |
| GET | `/api/game-url-products/{gameUrlId}` | — | `gameUrlId:int64` → `200 OK` |

## GameUrls

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/game-urls` | — | `GetAllGameUrls` → `200 OK` |
| POST | `/api/game-urls` | `GameUrlCreateDto` | `CreateGameUrl` → `201 Created` (`GameUrlDto`), `400 Bad Request` |
| GET | `/api/game-urls/{id}` | — | `id:int64` → `GetGameUrlById` (`200 OK` / `404 Not Found`) |
| PUT | `/api/game-urls/{id}` | `GameUrlUpdateDto` | `id:int64` → `UpdateGameUrl` (`204 No Content` / `404 Not Found`) |
| DELETE | `/api/game-urls/{id}` | — | `id:int64` → `DeleteGameUrl` (`204 No Content` / `404 Not Found`) |

## Pixels

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/pixels` | — | `GetAllPixels` → `200 OK` |
| POST | `/api/pixels` | `PixelCreateDto` | `CreatePixel` → `201 Created` (`PixelDto`), `400 Bad Request` |
| GET | `/api/pixels/{id}` | — | `id:int64` → `GetPixelById` (`200 OK` / `404 Not Found`) |
| PUT | `/api/pixels/{id}` | `PixelUpdateDto` | `id:int64` → `UpdatePixel` (`204 No Content` / `404 Not Found`) |
| DELETE | `/api/pixels/{id}` | — | `id:int64` → `DeletePixel` (`204 No Content` / `404 Not Found`) |

## Products

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/products` | — | `GetAllProducts` → `200 OK` |
| POST | `/api/products` | `ProductCreateDto` | `CreateProduct` → `201 Created` (`ProductDto`), `400 Bad Request` |
| GET | `/api/products/{id}` | — | `id:int64` → `GetProductById` (`200 OK` / `404 Not Found`) |
| PUT | `/api/products/{id}` | `ProductUpdateDto` | `id:int64` → `UpdateProduct` (`204 No Content` / `404 Not Found`) |
| DELETE | `/api/products/{id}` | — | `id:int64` → `DeleteProduct` (`204 No Content` / `404 Not Found`) |

## ProductTags

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/product-tags` | — | `200 OK` |
| POST | `/api/product-tags` | `ProductTagCreateDto` | `200 OK` |
| GET | `/api/product-tags/{productId}/{tagId}` | — | `productId:int64`, `tagId:int64` → `200 OK` |
| DELETE | `/api/product-tags/{productId}/{tagId}` | — | `productId:int64`, `tagId:int64` → `200 OK` |
| GET | `/api/product-tags/product/{productId}` | — | `productId:int64` → `200 OK` |

## Steam

| Method | Path | Params | Notes |
|---|---|---|---|
| GET | `/steam/scrape-page/gameUrl/{gamerUrlId}/page/{page}` | `gamerUrlId:int64`, `page:int32` | `200 OK` |
| GET | `/steam/scrape-public-api/gameUrl/{gameUrlId}/page/{page}` | `gameUrlId:int64`, `page:int32` | `200 OK` |
| GET | `/steam/pixel-info/gameUrl/{gameUrlId}` | `gameUrlId:int64`, query `srcUrl:string?` | `200 OK` |
| GET | `/steam/scrape-pixels/gameUrl/{gamerUrlId}/page/{page}` | path `gamerUrlId:string`, path `page:int32`, query `gameUrlId:int64?` | `200 OK` |
| GET | `/steam/check-wishlist/{wishlistId}` | `wishlistId:int64` | `200 OK` |

## Tags

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/tags` | — | `200 OK` |
| POST | `/api/tags` | `TagCreateDto` | `200 OK` |
| GET | `/api/tags/{id}` | — | `id:int64` → `200 OK` |
| PUT | `/api/tags/{id}` | `TagUpdateDto` | `id:int64` → `200 OK` |
| DELETE | `/api/tags/{id}` | — | `id:int64` → `200 OK` |
| GET | `/api/tags/game/{gameId}` | — | `gameId:int64` → `200 OK` |

## WatchList

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/watch-list` | — | `GetAllWatchList` → `200 OK` |
| POST | `/api/watch-list` | `WatchListCreateDto` | `CreateWatchListItem` → `201 Created` (`WatchListDto`), `400 Bad Request` |
| GET | `/api/watch-list/{id}` | — | `id:int64` → `GetWatchListItemById` (`200 OK` / `404 Not Found`) |
| PUT | `/api/watch-list/{id}` | `WatchListUpdateDto` | `id:int64` → `UpdateWatchListItem` (`204 No Content` / `404 Not Found`) |
| DELETE | `/api/watch-list/{id}` | — | `id:int64` → `DeleteWatchList` (`204 No Content` / `404 Not Found`) |

## WishList

| Method | Path | Body | Notes |
|---|---|---|---|
| GET | `/api/wish-list` | — | `GetAllWishList` → `200 OK` (`WishListDto[]`) |
| POST | `/api/wish-list` | `WishListCreateDto` | `CreateWishListItem` → `201 Created` (`WishListDto`), `400 Bad Request` |
| GET | `/api/wish-list/{id}` | — | `id:int64` → `GetWishListById` (`200 OK` / `404 Not Found`) |
| PUT | `/api/wish-list/{id}` | `WishListUpdateDto` | `id:int64` → `UpdateWishListItem` (`204 No Content` / `404 Not Found`) |
| DELETE | `/api/wish-list/{id}` | — | `id:int64` → `DeleteWishList` (`204 No Content` / `404 Not Found`) |

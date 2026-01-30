using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.GameUrlProduct;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class GameUrlProductsEndpoints
{
    public static WebApplication MapGameUrlProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/game-url-products")
                       .WithTags("GameUrlProducts")
                       .RequireAuthorization();

        // GET: /api/game-url-products
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var items = await db.GameUrlsProducts
                .AsNoTracking()
                .Select(x => new
                {
                    x.ProductId,
                    ProductName = x.Product.Name,
                    x.GameUrlId,
                    GameUrlName = x.GameUrl.Name,
                    x.GameUrl.IsBatchUrl,
                    FullUrl = x.GameUrl.Game.BaseUrl + x.GameUrl.PartialUrl + Uri.EscapeDataString(x.Product.Name
                    .Replace("’", "'")
                    .Replace("'", "%27")
            )
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        // GET: /api/game-url-products/{productId}/{gameUrlId}
        group.MapGet("/{productId:long}/{gameUrlId:long}", async (
            long productId,
            long gameUrlId,
            ApplicationDbContext db) =>
        {
            var exists = await db.GameUrlsProducts
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ProductId == productId &&
                    x.GameUrlId == gameUrlId);

            return exists
                ? Results.Ok()
                : Results.NotFound();
        });

        // GET: /api/game-url-products/{gameUrlId}
        group.MapGet("{gameUrlId:long}", async (
            long gameUrlId,
            ApplicationDbContext db) =>
        {
            var exists = await db.GameUrlsProducts
                .AsNoTracking()
                .Where(x => x.GameUrlId == gameUrlId)
                .Select(x => new
                {
                    x.ProductId,
                    ProductName = x.Product.Name,
                    x.GameUrlId,
                    GameUrlName = x.GameUrl.Name,
                    FullUrl = x.GameUrl.PartialUrl + Uri.EscapeDataString(x.Product.Name)
                })
                .ToListAsync();

            return  Results.Ok(exists);
        });

        // POST: /api/game-url-products
        group.MapPost("/", async (
            GameUrlProductCreateDto input,
            ApplicationDbContext db) =>
        {
            var productExists = await db.Products
                .AnyAsync(p => p.Id == input.ProductId);

            var gameUrlExists = await db.GameUrls
                .AnyAsync(g => g.Id == input.GameUrlId);

            if (!productExists || !gameUrlExists)
            {
                return Results.BadRequest("Invalid ProductId or GameUrlId");
            }

            var alreadyExists = await db.GameUrlsProducts
                .AnyAsync(x =>
                    x.ProductId == input.ProductId &&
                    x.GameUrlId == input.GameUrlId);

            if (alreadyExists)
            {
                return Results.Conflict("Relation already exists");
            }

            var entity = new GameUrlProducts
            {
                ProductId = input.ProductId,
                GameUrlId = input.GameUrlId
            };

            db.GameUrlsProducts.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/game-url-products/{input.ProductId}/{input.GameUrlId}",
                null);
        });

        // DELETE: /api/game-url-products/{productId}/{gameUrlId}
        group.MapDelete("/{productId:long}/{gameUrlId:long}", async (
            long productId,
            long gameUrlId,
            ApplicationDbContext db) =>
        {
            var entity = await db.GameUrlsProducts.FindAsync(
                productId,
                gameUrlId);

            if (entity is null)
            {
                return Results.NotFound();
            }

            db.GameUrlsProducts.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return app;
    }
}

using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.GameUrlPixel;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class GameUrlPixelsEndpoints
{
    public static WebApplication MapGameUrlPixelsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/game-url-pixels")
                       .WithTags("GameUrlPixels")
                       .RequireAuthorization();

        // GET: /api/game-url-pixels
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var items = await db.GameUrlsPixels
                .AsNoTracking()
                .Select(x => new
                {
                    x.PixelId,
                    PixelName = x.Pixel.Name,
                    x.GameUrlId,
                    GameUrlName = x.GameUrl.Name,
                    GameName = x.GameUrl.Game.Name,
                    GameUrlPixelLocationX = x.GameUrl.PixelX,
                    GameUrlPixelLocationY = x.GameUrl.PixelY,
                    GameUrlImageWidth = x.GameUrl.PixelImageWidth,
                    GameUrlImageHeight = x.GameUrl.PixelImageHeight,
                    x.Pixel.RedValue,
                    x.Pixel.BlueValue,
                    x.Pixel.GreenValue
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        // GET: /api/game-url-pixels/{pixelId}/{gameUrlId}
        group.MapGet("/{pixelId:long}/{gameUrlId:long}", async (
            long pixelId,
            long gameUrlId,
            ApplicationDbContext db) =>
        {
            var exists = await db.GameUrlsPixels
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PixelId == pixelId &&
                    x.GameUrlId == gameUrlId);

            return exists
                ? Results.Ok()
                : Results.NotFound();
        });

        // POST: /api/game-url-pixels
        group.MapPost("/", async (
            GameUrlPixelCreateDto input,
            ApplicationDbContext db) =>
        {
            var pixelExists = await db.Pixels
                .AnyAsync(p => p.Id == input.PixelId);

            var gameUrlExists = await db.GameUrls
                .AnyAsync(g => g.Id == input.GameUrlId);

            if (!pixelExists || !gameUrlExists)
            {
                return Results.BadRequest("Invalid PixelId or GameUrlId");
            }

            var alreadyExists = await db.GameUrlsPixels
                .AnyAsync(x =>
                    x.PixelId == input.PixelId &&
                    x.GameUrlId == input.GameUrlId);

            if (alreadyExists)
            {
                return Results.Conflict("Relation already exists");
            }

            var entity = new GameUrlPixels
            {
                PixelId = input.PixelId,
                GameUrlId = input.GameUrlId
            };

            db.GameUrlsPixels.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/game-url-pixels/{input.PixelId}/{input.GameUrlId}",
                null);
        });

        // DELETE: /api/game-url-pixels/{pixelId}/{gameUrlId}
        group.MapDelete("/{pixelId:long}/{gameUrlId:long}", async (
            long pixelId,
            long gameUrlId,
            ApplicationDbContext db) =>
        {
            var entity = await db.GameUrlsPixels.FindAsync(
                pixelId,
                gameUrlId);

            if (entity is null)
            {
                return Results.NotFound();
            }

            db.GameUrlsPixels.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return app;
    }
}

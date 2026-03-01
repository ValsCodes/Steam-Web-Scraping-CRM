using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class GameUrlEndpoints
    {
        public static WebApplication MapGameUrlEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/game-urls")
                           .WithTags("GameUrls")
                           .RequireAuthorization();

            // GET: /api/game-urls
            group.MapGet("/", async (ApplicationDbContext db) =>
            {
                var entities = await db.GameUrls
                    .AsNoTracking()
                    .Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        GameId = x.GameId,
                        GameName = x.Game.Name,
                        PartialUrl = x.PartialUrl,
                        IsBatchUrl = x.IsBatchUrl,
                        StartPage = x.StartPage,
                        EndPage = x.EndPage,
                        IsPixelScrape = x.IsPixelScrape,
                        IsPublicApi = x.IsPublicApi
                    })
                    .ToListAsync();

                return Results.Ok(entities);
            })
            .WithName("GetAllGameUrls")
            .Produces<List<object>>(StatusCodes.Status200OK);

            // GET: /api/game-urls/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.GameUrls.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<GameUrlDto>(entity));
            })
            .WithName("GetGameUrlById")
            .Produces<GameUrlDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/game-urls
            group.MapPost("/", async (
                GameUrlCreateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var gameExists = await db.Games
                    .AsNoTracking()
                    .AnyAsync(g => g.Id == input.GameId);

                if (!gameExists)
                {
                    return Results.BadRequest("Invalid GameId");
                }

                var entity = mapper.Map<GameUrl>(input);

                db.GameUrls.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<GameUrlDto>(entity);
                return Results.Created($"/api/game-urls/{entity.Id}", dto);
            })
            .WithName("CreateGameUrl")
            .Accepts<GameUrlCreateDto>("application/json")
            .Produces<GameUrlDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT: /api/game-urls/{id}
            group.MapPut("/{id:long}", async (
                long id,
                GameUrlUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper,
                IMemoryCache cache) =>
            {
                var entity = await db.GameUrls.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);

                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.GameUrl, id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdateGameUrl")
            .Accepts<GameUrlUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/game-urls/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var entity = await db.GameUrls.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.GameUrls.Remove(entity);
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.GameUrl, id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("DeleteGameUrl")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

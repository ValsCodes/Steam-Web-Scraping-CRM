using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.Game;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class GameEndpoints
    {
        public static WebApplication MapGameEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/games")
               .WithTags("Games")
               .RequireAuthorization();

            // GET: /api/games
            group.MapGet("/", async (
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var games = await db.Games.AsNoTracking().ToListAsync();
                return Results.Ok(mapper.Map<List<GameDto>>(games));
            })
            .WithName("GetAllGames")
            .Produces<List<GameDto>>(StatusCodes.Status200OK);

            // GET: /api/games/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var game = await db.Games.FindAsync(id);
                if (game is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<GameDto>(game));
            })
            .WithName("GetGameById")
            .Produces<GameDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/games
            group.MapPost("/", async (
                GameCreateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = mapper.Map<Game>(input);

                db.Games.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<GameDto>(entity);
                return Results.Created($"/api/games/{entity.Id}", dto);
            })
            .WithName("CreateGame")
            .Accepts<GameCreateDto>("application/json")
            .Produces<GameDto>(StatusCodes.Status201Created);

            // PUT: /api/games/{id}
            group.MapPut("/{id:long}", async (
                long id,
                GameUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper, 
                IMemoryCache cache) =>
            {
                var entity = await db.Games.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);

                var cacheKey = string.Format(CacheKeys.Game, id);
                cache.Remove(cacheKey);

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateGame")
            .Accepts<GameUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/games/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db, IMemoryCache cache) =>
            {
                var entity = await db.Games.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.Games.Remove(entity);
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.Game, id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("DeleteGame")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

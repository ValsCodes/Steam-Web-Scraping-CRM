using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs.WatchList;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class WatchListEndpoints
    {
        public static WebApplication MapWatchListEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/watch-list")
                           .WithTags("WatchList")
                           .RequireAuthorization();

            // GET: /api/watch-list
            group.MapGet("/", async (
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entities = await db.WatchListItems
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(mapper.Map<List<WatchListDto>>(entities));
            })
            .WithName("GetAllWatchListItems")
            .Produces<List<WatchListDto>>(StatusCodes.Status200OK);

            // GET: /api/watch-list/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.WatchListItems.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<WatchListDto>(entity));
            })
            .WithName("GetWatchListItemById")
            .Produces<WatchListDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/watch-list
            group.MapPost("/", async (
                WatchListCreateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                // Rule: at least one FK required
                if (input.GameId is null && input.GameUrlId is null)
                {
                    return Results.BadRequest("Either GameId or GameUrlId must be provided");
                }

                if (input.GameId.HasValue)
                {
                    var gameExists = await db.Games
                        .AsNoTracking()
                        .AnyAsync(g => g.Id == input.GameId.Value);

                    if (!gameExists) { return Results.BadRequest("Invalid GameId"); }
                }

                if (input.GameUrlId.HasValue)
                {
                    var gameUrlExists = await db.GameUrls
                        .AsNoTracking()
                        .AnyAsync(g => g.Id == input.GameUrlId.Value);

                    if (!gameUrlExists) { return Results.BadRequest("Invalid GameUrlId"); }
                }

                var entity = mapper.Map<WatchListItem>(input);

                db.WatchListItems.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<WatchListDto>(entity);
                return Results.Created($"/api/watch-list/{entity.Id}", dto);
            })
            .WithName("CreateWatchListItem")
            .Accepts<WatchListCreateDto>("application/json")
            .Produces<WatchListDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT: /api/watch-list/{id}
            group.MapPut("/{id:long}", async (
                long id,
                WatchListUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.WatchListItems.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateWatchListItem")
            .Accepts<WatchListUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/watch-list/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db) =>
            {
                var entity = await db.WatchListItems.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.WatchListItems.Remove(entity);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteWatchListItem")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

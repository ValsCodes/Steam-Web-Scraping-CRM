using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class WishListEndpoints
    {
        public static WebApplication MapWishListEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/wish-list")
                           .WithTags("WishList")
                           .RequireAuthorization();
                           //.RequireAuthorization("InternalJob");

            // GET: /api/wish-list
            group.MapGet("/", async (
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entities = await db.WishLists
                    .AsNoTracking()
                    .Select(x=> new
                    {
                        Id = x.Id,
                        GameName = x.Game.Name,
                        x.Name,
                        x.GameId,
                        x.Game.PageUrl,
                        x.Price,
                        x.IsActive
                    })
                    .ToListAsync();

                return Results.Ok(entities);
            })
            .WithName("GetAllWishList")
            .Produces<List<WishListDto>>(StatusCodes.Status200OK);

            // GET: /api/wish-list/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.WishLists.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<WishListDto>(entity));
            })
            .WithName("GetWishListById")
            .Produces<WishListDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/wish-list
            group.MapPost("/", async (
                WishListCreateDto input,
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

                var entity = mapper.Map<WishList>(input);

                db.WishLists.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<WishListDto>(entity);
                return Results.Created($"/api/wish-list/{entity.Id}", dto);
            })
            .WithName("CreateWishListItem")
            .Accepts<WishListCreateDto>("application/json")
            .Produces<WishListDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT: /api/wish-list/{id}
            group.MapPut("/{id:long}", async (
                long id,
                WishListUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper, 
                IMemoryCache cache) =>
            {
                var entity = await db.WishLists.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);

                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.WishListItem, entity.Id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdateWishListItem")
            .Accepts<WishListUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/wish-list/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var entity = await db.WishLists.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.WishLists.Remove(entity);
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.WishListItem, entity.Id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("DeleteWishList")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

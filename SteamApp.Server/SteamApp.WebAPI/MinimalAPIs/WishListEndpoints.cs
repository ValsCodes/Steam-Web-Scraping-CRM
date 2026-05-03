using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.Pagination;

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
                    .Select(x => new
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

            // GET: /api/wish-list/paged
            group.MapGet("/paged", async (
                ApplicationDbContext db,
                [AsParameters] WishListsPageQuery request,
                CancellationToken ct) =>
            {
                var query = db.WishLists.AsNoTracking();

                if (request.GameId.HasValue)
                {
                    query = query.Where(x => x.GameId == request.GameId.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var nameFilter = request.Name.Trim();
                    query = query.Where(x => x.Name != null && x.Name.Contains(nameFilter));
                }

                query = request.SortBy switch
                {
                    "gameName" => request.IsDescending
                        ? query.OrderByDescending(x => x.Game.Name).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.Game.Name).ThenBy(x => x.Id),
                    "name" => request.IsDescending
                        ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.Name).ThenBy(x => x.Id),
                    "pageUrl" => request.IsDescending
                        ? query.OrderByDescending(x => x.Game.PageUrl).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.Game.PageUrl).ThenBy(x => x.Id),
                    "price" => request.IsDescending
                        ? query.OrderByDescending(x => x.Price).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.Price).ThenBy(x => x.Id),
                    "isActive" => request.IsDescending
                        ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
                    _ => query.OrderBy(x => x.Id),
                };

                var totalCount = await query.CountAsync(ct);
                var pageWindow = request.ToPageWindow(totalCount);

                var items = await query
                    .ApplyPage(pageWindow)
                    .Select(x => new
                    {
                        Id = x.Id,
                        GameName = x.Game.Name,
                        x.Name,
                        x.GameId,
                        PageUrl = x.Game.PageUrl,
                        x.Price,
                        x.IsActive,
                    })
                    .ToListAsync(ct);

                return Results.Ok(pageWindow.ToPagedResponse(items));
            })
            .WithName("GetPagedWishList")
            .Produces(StatusCodes.Status200OK);

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

    public sealed record WishListsPageQuery : PagedQuery
    {
        public long? GameId { get; init; }
        public string? Name { get; init; }
    }
}

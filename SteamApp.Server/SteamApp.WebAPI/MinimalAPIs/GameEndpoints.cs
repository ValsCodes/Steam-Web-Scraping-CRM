using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.Game;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.Pagination;

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

            // GET: /api/games/paged
            group.MapGet("/paged", async (
                ApplicationDbContext db,
                [AsParameters] GamesPageQuery request,
                CancellationToken ct) =>
            {
                var query = db.Games.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var nameFilter = request.Name.Trim();
                    query = query.Where(x => x.Name != null && x.Name.Contains(nameFilter));
                }

                query = request.SortBy switch
                {
                    "name" => request.IsDescending
                        ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.Name).ThenBy(x => x.Id),
                    "pageUrl" => request.IsDescending
                        ? query.OrderByDescending(x => x.PageUrl).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.PageUrl).ThenBy(x => x.Id),
                    "internalId" => request.IsDescending
                        ? query.OrderByDescending(x => x.InternalId).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.InternalId).ThenBy(x => x.Id),
                    "isActive" => request.IsDescending
                        ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
                    _ => query.OrderBy(x => x.Id),
                };

                var totalCount = await query.CountAsync(ct);
                var pageWindow = request.ToPageWindow(totalCount);

                var items = await query
                    .ApplyPage(pageWindow)
                    .Select(x => new GameDto
                    {
                        Id = x.Id,
                        Name = x.Name ?? string.Empty,
                        BaseUrl = x.BaseUrl ?? string.Empty,
                        PageUrl = x.PageUrl,
                        InternalId = x.InternalId,
                        IsActive = x.IsActive,
                    })
                    .ToListAsync(ct);

                return Results.Ok(pageWindow.ToPagedResponse(items));
            })
            .WithName("GetPagedGames")
            .Produces(StatusCodes.Status200OK);

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
                var exists = await db.Games
                   .Where(x => x.Id == id)
                   .Select(x => new
                   {
                       HasDependencies =
                           x.GameUrls.Any() ||
                           x.Products.Any() ||
                           x.WishLists.Any() ||
                           x.Tags.Any() ||
                           x.Pixels.Any()
                   })
                   .FirstOrDefaultAsync();

                if (exists is null)
                {
                    return Results.NotFound();
                }

                if (exists.HasDependencies)
                {
                    return Results.BadRequest(new { message = "Game cannot be deleted" });
                }

                db.Games.Remove(new Game { Id = id });
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.Game, id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("DeleteGame")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // PATCH: /api/games/{id}
            group.MapPatch("/{id:long}", async (
                GameUpdateStatusDto input,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var entity = await db.Games.FindAsync(input.Id);
                if (entity is null) { return Results.NotFound(); }

                if (entity.IsActive != input.IsActive)
                {
                    entity.IsActive = input.IsActive;
                    await db.SaveChangesAsync();
                }

                var cacheKey = string.Format(CacheKeys.Game, input.Id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdateGameStatus")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }

    public sealed record GamesPageQuery : PagedQuery
    {
        public string? Name { get; init; }
    }
}

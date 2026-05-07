using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.Pagination;

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
                        ScrapingModeId = x.ScrapingModeId,
                        ScrapingModeName = x.ScrapingMode != null ? x.ScrapingMode.Name : null,
                        PartialUrl = x.PartialUrl,
                        StartPage = x.StartPage,
                        EndPage = x.EndPage,
                        PixelX = x.PixelX,
                        PixelY = x.PixelY,
                        PixelImageWidth = x.PixelImageWidth,
                        PixelImageHeight = x.PixelImageHeight,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();

                return Results.Ok(entities);
            })
            .WithName("GetAllGameUrls")
            .Produces<List<object>>(StatusCodes.Status200OK);

            // GET: /api/game-urls/paged
            group.MapGet("/paged", async (
                ApplicationDbContext db,
                [AsParameters] GameUrlsPageQuery request,
                CancellationToken ct) =>
            {
                var query = db.GameUrls.AsNoTracking();

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
                    "scrapingModeName" => request.IsDescending
                        ? query.OrderByDescending(x => x.ScrapingMode != null ? x.ScrapingMode.Name : null).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.ScrapingMode != null ? x.ScrapingMode.Name : null).ThenBy(x => x.Id),
                    "startPage" => request.IsDescending
                        ? query.OrderByDescending(x => x.StartPage).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.StartPage).ThenBy(x => x.Id),
                    "endPage" => request.IsDescending
                        ? query.OrderByDescending(x => x.EndPage).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.EndPage).ThenBy(x => x.Id),
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
                        Name = x.Name,
                        GameId = x.GameId,
                        GameName = x.Game.Name,
                        ScrapingModeId = x.ScrapingModeId,
                        ScrapingModeName = x.ScrapingMode != null ? x.ScrapingMode.Name : null,
                        PartialUrl = x.PartialUrl,
                        StartPage = x.StartPage,
                        EndPage = x.EndPage,
                        PixelX = x.PixelX,
                        PixelY = x.PixelY,
                        PixelImageWidth = x.PixelImageWidth,
                        PixelImageHeight = x.PixelImageHeight,
                        IsActive = x.IsActive
                    })
                    .ToListAsync(ct);

                return Results.Ok(pageWindow.ToPagedResponse(items));
            })
            .WithName("GetPagedGameUrls")
            .Produces(StatusCodes.Status200OK);

            // GET: /api/game-urls/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db) =>
            {
                var dto = await ProjectGameUrlDtos(db.GameUrls
                    .AsNoTracking()
                    .Where(x => x.Id == id))
                    .FirstOrDefaultAsync();

                if (dto is null) { return Results.NotFound(); }

                return Results.Ok(dto);
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

                var scrapingModeExists = await ScrapingModeExistsAsync(db, input.ScrapingModeId);
                if (!scrapingModeExists)
                {
                    return Results.BadRequest("Invalid ScrapingModeId");
                }

                var entity = mapper.Map<GameUrl>(input);

                db.GameUrls.Add(entity);
                await db.SaveChangesAsync();

                var dto = await ProjectGameUrlDtos(db.GameUrls
                    .AsNoTracking()
                    .Where(x => x.Id == entity.Id))
                    .FirstAsync();

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

                var scrapingModeExists = await ScrapingModeExistsAsync(db, input.ScrapingModeId);
                if (!scrapingModeExists)
                {
                    return Results.BadRequest("Invalid ScrapingModeId");
                }

                mapper.Map(input, entity);

                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.GameUrl, id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdateGameUrl")
            .Accepts<GameUrlUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/game-urls/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var entity = await db.GameUrls
                .Include(x => x.GameUrlsProducts)
                .Include(x => x.GameUrlsPixels)
                .FirstOrDefaultAsync(x => x.Id == id);

                if (entity is null) { return Results.NotFound(); }

                db.GameUrlsProducts.RemoveRange(entity.GameUrlsProducts);
                db.GameUrlsPixels.RemoveRange(entity.GameUrlsPixels);
                db.GameUrls.Remove(entity);

                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.GameUrl, id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("DeleteGameUrl")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // PATCH: /api/game-urls/{id}
            group.MapPatch("/{id:long}", async (
                GameUrlUpdateStatusDto input,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var entity = await db.GameUrls.FindAsync(input.Id);
                if (entity is null) { return Results.NotFound(); }

                if (entity.IsActive != input.IsActive)
                {
                    entity.IsActive = input.IsActive;
                    await db.SaveChangesAsync();
                }

                var cacheKey = string.Format(CacheKeys.GameUrl, input.Id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdateGameUrlStatus")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }

        private static IQueryable<GameUrlDto> ProjectGameUrlDtos(IQueryable<GameUrl> query)
        {
            return query.Select(x => new GameUrlDto
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty,
                GameId = x.GameId,
                ScrapingModeId = x.ScrapingModeId,
                ScrapingModeName = x.ScrapingMode != null ? x.ScrapingMode.Name : null,
                PartialUrl = x.PartialUrl ?? string.Empty,
                StartPage = x.StartPage,
                EndPage = x.EndPage,
                PixelX = x.PixelX,
                PixelY = x.PixelY,
                PixelImageWidth = x.PixelImageWidth,
                PixelImageHeight = x.PixelImageHeight,
                IsActive = x.IsActive
            });
        }

        private static Task<bool> ScrapingModeExistsAsync(ApplicationDbContext db, long? scrapingModeId)
        {
            return scrapingModeId.HasValue
                ? db.ScrapingModes.AsNoTracking().AnyAsync(x => x.Id == scrapingModeId.Value)
                : Task.FromResult(true);
        }
    }

    public sealed record GameUrlsPageQuery : PagedQuery
    {
        public long? GameId { get; init; }
        public string? Name { get; init; }
    }
}

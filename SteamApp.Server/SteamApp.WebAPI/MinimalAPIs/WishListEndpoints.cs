using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.Pagination;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class WishListEndpoints
    {
        public static WebApplication MapWishListEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/wish-list")
                           .WithTags("WishList")
                           .RequireAuthorization(SecurityPolicies.ApiUser)
                           .RequireRateLimiting(SecurityPolicies.ApiRateLimit);
                           //.RequireAuthorization("InternalJob");

            // GET: /api/wish-list
            group.MapGet("/", async (
                HttpContext httpContext,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var entities = await db.WishLists
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
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
                HttpContext httpContext,
                ApplicationDbContext db,
                [AsParameters] WishListsPageQuery request,
                CancellationToken ct) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var query = db.WishLists
                    .AsNoTracking()
                    .Where(x => x.UserId == userId);

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
                HttpContext httpContext,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var entity = await db.WishLists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<WishListDto>(entity));
            })
            .WithName("GetWishListById")
            .Produces<WishListDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:long}/checks", async (
                long id,
                HttpContext httpContext,
                IWishlistCheckExecutionService checkExecution,
                CancellationToken ct) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var result = await checkExecution.ExecuteManualAsync(
                    id,
                    userId,
                    httpContext.TraceIdentifier,
                    ct);
                if (result.IsFailure)
                {
                    return ToProblem(result.Error!);
                }

                var outcome = result.Value!;
                if (outcome.CheckError is not null)
                {
                    return Results.Problem(
                        title: "Price check failed.",
                        detail: outcome.CheckError.Description,
                        statusCode: ToStatusCode(outcome.CheckError.Type),
                        extensions: new Dictionary<string, object?>
                        {
                            ["checkHistoryId"] = outcome.Trace.Id,
                            ["correlationId"] = outcome.Trace.CorrelationId
                        });
                }

                return Results.Ok(outcome.Trace);
            })
            .WithName("CheckWishListItem")
            .RequireRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)
            .Produces<WishListCheckHistoryDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable);

            group.MapGet("/{id:long}/checks", async (
                long id,
                HttpContext httpContext,
                [AsParameters] WishListCheckHistoryPageQuery request,
                IWishlistCheckExecutionService checkExecution,
                CancellationToken ct) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var result = await checkExecution.GetHistoryAsync(id, userId, request, ct);
                return result.Match<IResult>(Results.Ok, ToProblem);
            })
            .WithName("GetWishListCheckHistory")
            .Produces<WishListCheckHistoryPageDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

            // POST: /api/wish-list
            group.MapPost("/", async (
                WishListCreateDto input,
                HttpContext httpContext,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var gameExists = await db.Games
                    .AsNoTracking()
                    .AnyAsync(g => g.Id == input.GameId && (g.UserId == null || g.UserId == userId));

                if (!gameExists)
                {
                    return Results.BadRequest("Invalid GameId");
                }

                var entity = mapper.Map<WishList>(input);
                entity.UserId = userId;

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
                HttpContext httpContext,
                ApplicationDbContext db,
                IMapper mapper,
                IMemoryCache cache) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var entity = await db.WishLists.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
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
                HttpContext httpContext,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var entity = await db.WishLists.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
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

            // PATCH: /api/wish-list/{id}
            group.MapPatch("/{id:long}", async (
                WishListUpdateStatusDto input,
                HttpContext httpContext,
                ApplicationDbContext db,
                IMemoryCache cache) =>
            {
                var userId = httpContext.User.GetUserId();
                if (userId is null) { return Results.Unauthorized(); }

                var entity = await db.WishLists.FirstOrDefaultAsync(x => x.Id == input.Id && x.UserId == userId);
                if (entity is null) { return Results.NotFound(); }

                if (entity.IsActive != input.IsActive)
                {
                    entity.IsActive = input.IsActive;
                    await db.SaveChangesAsync();
                }

                var cacheKey = string.Format(CacheKeys.WishListItem, entity.Id);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdateWishListStatus")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }

        private static IResult ToProblem(Error error)
        {
            return Results.Problem(
                title: error.Type switch
                {
                    ErrorType.NotFound => "Price alert not found.",
                    ErrorType.Validation => "Invalid price alert.",
                    ErrorType.Conflict => "Price alert conflict.",
                    ErrorType.Unauthorized => "Authentication required.",
                    ErrorType.Forbidden => "Access denied.",
                    ErrorType.Unavailable => "Price check unavailable.",
                    _ => "Request failed."
                },
                detail: error.Description,
                statusCode: ToStatusCode(error.Type));
        }

        private static int ToStatusCode(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}

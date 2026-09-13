using System.Data;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.GameUrlProduct;
using SteamApp.Application.Utilities;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.GameUrlProduct;
using SteamApp.WebAPI.Contracts.Pagination;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class GameUrlProductsEndpoints
{
    private const int MaxStockUpdateAttempts = 5;

    public static IEnumerable<string> Tags { get; private set; } = [];

    public static WebApplication MapGameUrlProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/game-url-products")
            .WithTags("GameUrlProducts")
            .RequireAuthorization(SecurityPolicies.ApiUser)
            .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        group.MapGet("/", async (HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var items = await ProjectProducts(db.GameUrlsProducts.AsNoTracking()
                    .Where(x => x.Product.UserId == userId && x.GameUrl.UserId == userId))
                .ToListAsync(ct);
            return Results.Ok(items);
        });

        group.MapGet("/{productId:long}/{gameUrlId:long}", async (
            long productId, long gameUrlId, HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var exists = await db.GameUrlsProducts.AsNoTracking().AnyAsync(x =>
                x.ProductId == productId && x.GameUrlId == gameUrlId &&
                x.Product.UserId == userId && x.GameUrl.UserId == userId, ct);
            return exists ? Results.Ok() : Results.NotFound();
        });

        group.MapGet("{gameUrlId:long}", async (
            long gameUrlId, HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var items = await ProjectProducts(db.GameUrlsProducts.AsNoTracking().Where(x =>
                    x.GameUrlId == gameUrlId &&
                    x.Product.UserId == userId && x.GameUrl.UserId == userId))
                .ToListAsync(ct);
            return Results.Ok(items);
        });

        group.MapPost("/", async (
            GameUrlProductCreateDto input, HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var productExists = await db.Products.AnyAsync(p => p.Id == input.ProductId && p.UserId == userId, ct);
            var gameUrlExists = await db.GameUrls.AnyAsync(g => g.Id == input.GameUrlId && g.UserId == userId, ct);
            if (!productExists || !gameUrlExists)
            {
                return Results.BadRequest("Invalid ProductId or GameUrlId");
            }

            var alreadyExists = await db.GameUrlsProducts.AnyAsync(x =>
                x.ProductId == input.ProductId && x.GameUrlId == input.GameUrlId &&
                x.Product.UserId == userId && x.GameUrl.UserId == userId, ct);
            if (alreadyExists)
            {
                return Results.Conflict("Relation already exists");
            }

            db.GameUrlsProducts.Add(new GameUrlProducts
            {
                ProductId = input.ProductId,
                GameUrlId = input.GameUrlId,
            });
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/game-url-products/{input.ProductId}/{input.GameUrlId}", null);
        });

        // The IDs are the complete desired selection, including relations that already exist.
        group.MapPut("/{gameUrlId:long}/bulk", async (
            long gameUrlId, GameUrlProductBulkSyncDto input, HttpContext httpContext,
            ApplicationDbContext db, ILoggerFactory loggerFactory, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }
            if (gameUrlId <= 0 || input.ProductIds is null || input.ProductIds.Length > 10_000 ||
                input.ProductIds.Any(id => id <= 0))
            {
                return Results.BadRequest("Provide valid ProductIds (at most 10000).");
            }

            var productIds = input.ProductIds.Distinct().Order().ToArray();
            var strategy = db.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(() => BulkSyncProductsAsync(gameUrlId, productIds, userId, db, ct));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                loggerFactory.CreateLogger(nameof(GameUrlProductsEndpoints))
                    .LogError(exception, "Bulk product relation sync failed for Game URL {GameUrlId}", gameUrlId);
                return Results.Problem("Could not save product relations.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("BulkSyncGameUrlProducts")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{productId:long}/{gameUrlId:long}/current-stock", async (
            long productId, long gameUrlId, GameUrlProductCurrentStockUpdateDto input,
            HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            return await UpdateCurrentStockAsync(productId, gameUrlId, userId,
                GameUrlProductStockOperationEnum.Assigned, input.CurrentStock, db, ct);
        })
        .WithName("AssignGameUrlProductCurrentStock")
        .Produces<GameUrlProductCurrentStockDto>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{productId:long}/{gameUrlId:long}/current-stock/increment", async (
            long productId, long gameUrlId, HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            return await UpdateCurrentStockAsync(productId, gameUrlId, userId,
                GameUrlProductStockOperationEnum.Incremented, null, db, ct);
        })
        .WithName("IncrementGameUrlProductCurrentStock")
        .Produces<GameUrlProductCurrentStockDto>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{productId:long}/{gameUrlId:long}/current-stock/decrement", async (
            long productId, long gameUrlId, HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            return await UpdateCurrentStockAsync(productId, gameUrlId, userId,
                GameUrlProductStockOperationEnum.Decremented, null, db, ct);
        })
        .WithName("DecrementGameUrlProductCurrentStock")
        .Produces<GameUrlProductCurrentStockDto>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{productId:long}/{gameUrlId:long}/current-stock/history", async (
            long productId, long gameUrlId, HttpContext httpContext, ApplicationDbContext db,
            [AsParameters] GameUrlProductStockHistoryQuery request, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var relationExists = await db.GameUrlsProducts.AsNoTracking().AnyAsync(x =>
                x.ProductId == productId && x.GameUrlId == gameUrlId &&
                x.Product.UserId == userId && x.GameUrl.UserId == userId, ct);
            if (!relationExists) { return Results.NotFound(); }

            var query = db.GameUrlProductStockHistories.AsNoTracking()
                .Where(x => x.ProductId == productId && x.GameUrlId == gameUrlId && x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id);
            var totalCount = await query.CountAsync(ct);
            var pageSize = request.NormalizedPageSize;
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            var page = totalPages == 0 ? 1 : Math.Min(request.NormalizedPage, totalPages);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new GameUrlProductStockHistoryDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    GameUrlId = x.GameUrlId,
                    PreviousStock = x.PreviousStock,
                    NewStock = x.NewStock,
                    Operation = x.Operation,
                    CreatedAtUtc = x.CreatedAtUtc,
                })
                .ToListAsync(ct);

            return Results.Ok(new PagedResponse<GameUrlProductStockHistoryDto>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
            });
        })
        .WithName("GetGameUrlProductCurrentStockHistory")
        .Produces<PagedResponse<GameUrlProductStockHistoryDto>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{productId:long}/{gameUrlId:long}", async (
            long productId, long gameUrlId, HttpContext httpContext, ApplicationDbContext db, CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.GameUrlsProducts.FirstOrDefaultAsync(x =>
                x.ProductId == productId && x.GameUrlId == gameUrlId &&
                x.Product.UserId == userId && x.GameUrl.UserId == userId, ct);
            if (entity is null)
            {
                return Results.NotFound();
            }

            db.GameUrlsProducts.Remove(entity);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return app;
    }

    private static async Task<IResult> BulkSyncProductsAsync(
        long gameUrlId, long[] productIds, string userId, ApplicationDbContext db, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var gameId = await db.GameUrls.AsNoTracking()
                .Where(url => url.Id == gameUrlId && url.UserId == userId)
                .Select(url => (long?)url.GameId)
                .FirstOrDefaultAsync(ct);
            if (gameId is null) { return Results.NotFound(); }

            var validCount = await db.Products.AsNoTracking().CountAsync(product =>
                productIds.Contains(product.Id) && product.UserId == userId && product.GameId == gameId.Value, ct);
            if (validCount != productIds.Length)
            {
                return Results.BadRequest("Every product must belong to you and to the Game URL's game.");
            }

            var existing = await db.GameUrlsProducts.AsNoTracking()
                .Where(relation => relation.GameUrlId == gameUrlId)
                .ToListAsync(ct);
            var desiredIds = productIds.ToHashSet();
            var existingIds = existing.Select(relation => relation.ProductId).ToHashSet();
            var deletes = existing.Where(relation => !desiredIds.Contains(relation.ProductId)).ToList();
            var inserts = productIds.Where(id => !existingIds.Contains(id))
                .Select(id => new GameUrlProducts { GameUrlId = gameUrlId, ProductId = id })
                .ToList();

            if (deletes.Count > 0)
            {
                await db.BulkDeleteAsync(deletes, cancellationToken: ct);
            }
            if (inserts.Count > 0)
            {
                await db.BulkInsertAsync(inserts, new BulkConfig
                {
                    SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers,
                }, cancellationToken: ct);
            }
            await transaction.CommitAsync(ct);
            return Results.NoContent();
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static IQueryable<object> ProjectProducts(IQueryable<GameUrlProducts> query)
    {
        return query.Select(x => new
        {
            x.ProductId,
            ProductName = x.Product.Name,
            x.GameUrlId,
            GameUrlName = x.GameUrl.Name,
            x.GameUrl.ScrapingModeId,
            ScrapingModeName = x.GameUrl.ScrapingMode != null ? x.GameUrl.ScrapingMode.Name : null,
            Tags = x.Product.ProductTags.Select(y => y.Tag.Name),
            FullUrl = x.Product.Name != null
                ? x.GameUrl.PartialUrl + UrlUtilities.UrlEncode(x.Product.Name)
                : x.GameUrl.PartialUrl,
            x.Product.IsActive,
            x.Product.Rating,
            x.CurrentStock,
        });
    }

    private static async Task<IResult> UpdateCurrentStockAsync(
        long productId, long gameUrlId, string userId, GameUrlProductStockOperationEnum operation,
        int? assignedStock, ApplicationDbContext db, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => UpdateCurrentStockWithRetriesAsync(
            productId,
            gameUrlId,
            userId,
            operation,
            assignedStock,
            db,
            ct));
    }

    private static async Task<IResult> UpdateCurrentStockWithRetriesAsync(
        long productId, long gameUrlId, string userId, GameUrlProductStockOperationEnum operation,
        int? assignedStock, ApplicationDbContext db, CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxStockUpdateAttempts; attempt++)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var currentStock = await db.GameUrlsProducts.AsNoTracking()
                .Where(x => x.ProductId == productId && x.GameUrlId == gameUrlId &&
                    x.Product.UserId == userId && x.GameUrl.UserId == userId)
                .Select(x => (int?)x.CurrentStock)
                .FirstOrDefaultAsync(ct);
            if (currentStock is null)
            {
                return Results.NotFound();
            }

            var nextStock = operation switch
            {
                GameUrlProductStockOperationEnum.Assigned => assignedStock!.Value,
                GameUrlProductStockOperationEnum.Incremented when currentStock.Value < int.MaxValue => currentStock.Value + 1,
                GameUrlProductStockOperationEnum.Decremented when currentStock.Value > int.MinValue => currentStock.Value - 1,
                _ => currentStock.Value,
            };
            if (nextStock == currentStock.Value)
            {
                await transaction.CommitAsync(ct);
                return Results.Ok(new GameUrlProductCurrentStockDto { CurrentStock = currentStock.Value });
            }

            var updated = await db.GameUrlsProducts
                .Where(x => x.ProductId == productId && x.GameUrlId == gameUrlId &&
                    x.CurrentStock == currentStock.Value)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CurrentStock, nextStock), ct);
            if (updated == 0)
            {
                await transaction.RollbackAsync(ct);
                continue;
            }

            db.GameUrlProductStockHistories.Add(new GameUrlProductStockHistory
            {
                ProductId = productId,
                GameUrlId = gameUrlId,
                PreviousStock = currentStock.Value,
                NewStock = nextStock,
                Operation = operation,
                CreatedAtUtc = DateTime.UtcNow,
                UserId = userId,
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Results.Ok(new GameUrlProductCurrentStockDto { CurrentStock = nextStock });
        }

        return Results.Conflict("Current stock changed concurrently. Try again.");
    }
}

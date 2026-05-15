using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.Product;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.Pagination;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ProductEndpoints
{
    public static WebApplication MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/products")
                       .WithTags("Products")
                       .RequireAuthorization(SecurityPolicies.ApiUser)
                       .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        // GET: /api/products
        group.MapGet("/", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entities = await db.Products
                .Include(x => x.Game)
                .Include(x => x.ProductTags)
                .ThenInclude(x => x.Tag)
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return Results.Ok(mapper.Map<List<ProductDto>>(entities));
        })
        .WithName("GetAllProducts")
        .Produces<List<object>>(StatusCodes.Status200OK);

        // GET: /api/products/paged
        group.MapGet("/paged", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper,
            [AsParameters] ProductsPageQuery request,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var query = db.Products
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

            if (request.MinRating.HasValue)
            {
                query = query.Where(x => x.Rating >= request.MinRating.Value);
            }

            if (request.TagIds is { Length: > 0 })
            {
                foreach (var tagId in request.TagIds.Distinct())
                {
                    query = query.Where(x => x.ProductTags.Any(t => t.TagId == tagId));
                }
            }

            query = request.SortBy switch
            {
                "gameName" => request.IsDescending
                    ? query.OrderByDescending(x => x.Game.Name).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.Game.Name).ThenBy(x => x.Id),
                "name" => request.IsDescending
                    ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.Name).ThenBy(x => x.Id),
                "tags" => request.IsDescending
                    ? query.OrderByDescending(x => x.ProductTags
                            .OrderBy(t => t.Tag.Name)
                            .Select(t => t.Tag.Name)
                            .FirstOrDefault())
                        .ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.ProductTags
                            .OrderBy(t => t.Tag.Name)
                            .Select(t => t.Tag.Name)
                            .FirstOrDefault())
                        .ThenBy(x => x.Id),
                "rating" => request.IsDescending
                    ? query.OrderByDescending(x => x.Rating).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.Rating).ThenBy(x => x.Id),
                "isActive" => request.IsDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
                _ => query.OrderBy(x => x.Id),
            };

            var totalCount = await query.CountAsync(ct);
            var pageWindow = request.ToPageWindow(totalCount);

            var items = await query
                .Include(x => x.Game)
                .Include(x => x.ProductTags)
                .ThenInclude(x => x.Tag)
                .ApplyPage(pageWindow)
                .ToListAsync(ct);

            var dto = mapper.Map<List<ProductDto>>(items);

            return Results.Ok(pageWindow.ToPagedResponse(dto));
        })
        .WithName("GetPagedProducts")
        .Produces(StatusCodes.Status200OK);

        // GET: /api/products/{id}
        group.MapGet("/{id:long}", async (
            long id,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.Products
                .Include(x => x.Game)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (entity is null) { return Results.NotFound(); }

            return Results.Ok(mapper.Map<ProductDto>(entity));
        })
        .WithName("GetProductById")
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST: /api/products
        group.MapPost("/", async (
            ProductCreateDto input,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var gameExists = await db.Games
                .AsNoTracking()
                .AnyAsync(g => g.Id == input.GameId && g.UserId == userId);

            if (!gameExists)
            {
                return Results.BadRequest("Invalid GameId");
            }

            var normalizedInputName = (input.Name ?? string.Empty).Trim().ToLower();

            var productExists = await db.Products
                .AsNoTracking()
                .AnyAsync(g =>
                    g.UserId == userId &&
                    (g.Name ?? string.Empty).Trim().ToLower() == normalizedInputName);

            if (productExists)
            {
                return Results.BadRequest("Product Exists");
            }

            var entity = mapper.Map<Product>(input);
            entity.UserId = userId;

            db.Products.Add(entity);
            await db.SaveChangesAsync();

            var dto = mapper.Map<ProductDto>(entity);
            return Results.Created($"/api/products/{entity.Id}", dto);
        })
        .WithName("CreateProduct")
        .Accepts<ProductCreateDto>("application/json")
        .Produces<ProductDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // PUT: /api/products/{id}
        group.MapPut("/{id:long}", async (
            long id,
            ProductUpdateDto input,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (entity is null) { return Results.NotFound(); }

            mapper.Map(input, entity);

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateProduct")
        .Accepts<ProductUpdateDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /api/products/{id}
        group.MapDelete("/{id:long}", async (
            long id,
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (entity is null) { return Results.NotFound(); }

            db.Products.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // PATCH: /api/products/{id}
        group.MapPatch("/{id:long}", async (
            ProductUpdateStatusDto input,
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.Products.FirstOrDefaultAsync(x => x.Id == input.Id && x.UserId == userId);
            if (entity is null) { return Results.NotFound(); }

            if (entity.IsActive != input.IsActive)
            {
                entity.IsActive = input.IsActive;

                await db.SaveChangesAsync();
            }

            return Results.NoContent();
        })
        .WithName("UpdateProductStatus")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

public sealed record ProductsPageQuery : PagedQuery
{
    public long? GameId { get; init; }
    public string? Name { get; init; }
    public int? MinRating { get; init; }
    public long[]? TagIds { get; init; }
}

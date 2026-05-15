using Microsoft.EntityFrameworkCore;
using SteamApp.Domain.Entities;
using SteamApp.Application.DTOs;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ProductTagsEndpoints
{
    public static WebApplication MapProductTagsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/product-tags")
                       .WithTags("ProductTags")
                       .RequireAuthorization(SecurityPolicies.ApiUser)
                       .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        // GET: /api/product-tags
        group.MapGet("/", async (
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var items = await db.ProductTags
                .AsNoTracking()
                .Where(x => x.Product.UserId == userId && x.Tag.UserId == userId)
                .Select(x => new
                {
                    x.ProductId,
                    ProductName = x.Product.Name,
                    x.TagId,
                    TagName = x.Tag.Name
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        // GET: /api/product-tags/{productId}/{tagId}
        group.MapGet("/{productId:long}/{tagId:long}", async (
            long productId,
            long tagId,
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var exists = await db.ProductTags
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ProductId == productId &&
                    x.TagId == tagId &&
                    x.Product.UserId == userId &&
                    x.Tag.UserId == userId);

            return exists
                ? Results.Ok()
                : Results.NotFound();
        });

        // GET: /api/product-tags/product/{productId}
        group.MapGet("/product/{productId:long}", async (
            long productId,
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var tags = await db.ProductTags
                .AsNoTracking()
                .Where(x => x.ProductId == productId && x.Product.UserId == userId && x.Tag.UserId == userId)
                .Select(x => new
                {
                    x.TagId,
                    TagName = x.Tag.Name
                })
                .ToListAsync();

            return Results.Ok(tags);
        });

        // POST: /api/product-tags
        group.MapPost("/", async (
    ProductTagCreateDto input,
    HttpContext httpContext,
    ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var productExists = await db.Products
                .AnyAsync(p => p.Id == input.ProductId && p.UserId == userId);

            var tagExists = await db.Tags
                .AnyAsync(t => t.Id == input.TagId && t.UserId == userId);

            if (!productExists || !tagExists)
            {
                return Results.BadRequest("Invalid ProductId or TagId");
            }

            var exists = await db.ProductTags.AnyAsync(x =>
                x.ProductId == input.ProductId &&
                x.TagId == input.TagId &&
                x.Product.UserId == userId &&
                x.Tag.UserId == userId);

            if (exists)
            {
                return Results.Conflict("Relation already exists");
            }

            var entity = new ProductTags
            {
                ProductId = input.ProductId,
                TagId = input.TagId
            };

            db.ProductTags.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/product-tags/{input.ProductId}/{input.TagId}",
                null);
        });

        // DELETE: /api/product-tags/{productId}/{tagId}
        group.MapDelete("/{productId:long}/{tagId:long}", async (
            long productId,
            long tagId,
            HttpContext httpContext,
            ApplicationDbContext db) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.ProductTags.FirstOrDefaultAsync(x =>
                x.ProductId == productId &&
                x.TagId == tagId &&
                x.Product.UserId == userId &&
                x.Tag.UserId == userId);

            if (entity is null)
            {
                return Results.NotFound();
            }

            db.ProductTags.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return app;
    }
}

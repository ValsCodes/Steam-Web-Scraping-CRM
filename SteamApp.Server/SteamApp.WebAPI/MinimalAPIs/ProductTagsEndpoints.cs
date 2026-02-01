using Microsoft.EntityFrameworkCore;
using SteamApp.WebAPI.Context;
using SteamApp.Domain.Entities;
using SteamApp.Application.DTOs;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ProductTagsEndpoints
{
    public static WebApplication MapProductTagsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/product-tags")
                       .WithTags("ProductTags")
                       .RequireAuthorization();

        // GET: /api/product-tags
        group.MapGet("/", async (ApplicationDbContext db) =>
        {
            var items = await db.ProductTags
                .AsNoTracking()
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
            ApplicationDbContext db) =>
        {
            var exists = await db.ProductTags
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ProductId == productId &&
                    x.TagId == tagId);

            return exists
                ? Results.Ok()
                : Results.NotFound();
        });

        // GET: /api/product-tags/product/{productId}
        group.MapGet("/product/{productId:long}", async (
            long productId,
            ApplicationDbContext db) =>
        {
            var tags = await db.ProductTags
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
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
    ApplicationDbContext db) =>
        {
            var productExists = await db.Products
                .AnyAsync(p => p.Id == input.ProductId);

            var tagExists = await db.Tags
                .AnyAsync(t => t.Id == input.TagId);

            if (!productExists || !tagExists)
            {
                return Results.BadRequest("Invalid ProductId or TagId");
            }

            var exists = await db.ProductTags.AnyAsync(x =>
                x.ProductId == input.ProductId &&
                x.TagId == input.TagId);

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
            ApplicationDbContext db) =>
        {
            var entity = await db.ProductTags.FindAsync(productId, tagId);

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
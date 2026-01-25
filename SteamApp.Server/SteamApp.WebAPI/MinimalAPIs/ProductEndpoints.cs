using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.Product;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ProductEndpoints
{
    public static WebApplication MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/products")
                       .WithTags("Products")
                       .RequireAuthorization();

        // GET: /api/products
        group.MapGet("/", async (
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entities = await db.Products
                .AsNoTracking()
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    GameId = x.GameId,
                    GameName = x.Game.Name,
                    x.IsActive,
                })
                .ToListAsync();

            return Results.Ok(entities);
        })
        .WithName("GetAllProducts")
        .Produces<List<object>>(StatusCodes.Status200OK);

        // GET: /api/products/{id}
        group.MapGet("/{id:long}", async (
            long id,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entity = await db.Products.FindAsync(id);
            if (entity is null) { return Results.NotFound(); }

            return Results.Ok(mapper.Map<ProductDto>(entity));
        })
        .WithName("GetProductById")
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST: /api/products
        group.MapPost("/", async (
            ProductCreateDto input,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var productExists = await db.Products
                .AsNoTracking()
                .AnyAsync(g => g.Name.Trim().ToLower() == input.Name.Trim().ToLower());

            if (productExists)
            {
                return Results.BadRequest("Product Exists");
            }

            var entity = mapper.Map<Product>(input);

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
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entity = await db.Products.FindAsync(id);
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
            ApplicationDbContext db) =>
        {
            var entity = await db.Products.FindAsync(id);
            if (entity is null) { return Results.NotFound(); }

            db.Products.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteProduct")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

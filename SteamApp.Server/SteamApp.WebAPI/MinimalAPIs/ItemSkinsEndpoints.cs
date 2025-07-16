using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ItemSkinsEndpoints
{
    public static WebApplication MapItemSkinsEndpoints(this WebApplication app)
    {
        // Apply authorization to all ItemSkins endpoints
        var group = app.MapGroup("api/itemskins")
            .WithTags("ItemSkins")
                       .RequireAuthorization();

        // GET: /itemskins
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.ItemSkins
                    .Include(x => x.ManualSearchItem)
                    .Include(x => x.Skin)
                    .Include(x => x.Grade)
                    .ToListAsync())
            .WithName("GetAllItemSkins")
            .Produces<List<ItemSkins>>(StatusCodes.Status200OK);

        // GET: /itemskins/{id}
        group.MapGet("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var itemSkin = await db.ItemSkins
                .Include(x => x.ManualSearchItem)
                .Include(x => x.Skin)
                .Include(x => x.Grade)
                .FirstOrDefaultAsync(x => x.Id == id);

            return itemSkin is not null
                ? Results.Ok(itemSkin)
                : Results.NotFound();
        })
        .WithName("GetItemSkinById")
        .Produces<ItemSkins>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST: /itemskins
        group.MapPost("/", async (ItemSkins input, ApplicationDbContext db) =>
        {
            db.ItemSkins.Add(input);
            await db.SaveChangesAsync();
            return Results.Created($"/itemskins/{input.Id}", input);
        })
        .WithName("CreateItemSkin")
        .Accepts<ItemSkins>("application/json")
        .Produces<ItemSkins>(StatusCodes.Status201Created);

        // PATCH: /itemskins
        group.MapPatch("/", async (UpdateItemSkinsDto input, ApplicationDbContext db) =>
        {
            var existing = await db.ItemSkins.FindAsync(input.Id);
            if (existing is null) return Results.NotFound();

            if(input.ItemId.HasValue) existing.ItemId = (long)input.ItemId;
            if (input.SkinId.HasValue) existing.SkinId = (long)input.SkinId;
            if (input.GradeId.HasValue) existing.GradeId = input.GradeId;

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateItemSkin")
        .Accepts<ItemSkins>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /itemskins/{id}
        group.MapDelete("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var existing = await db.ItemSkins.FindAsync(id);
            if (existing is null) return Results.NotFound();

            db.ItemSkins.Remove(existing);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteItemSkin")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

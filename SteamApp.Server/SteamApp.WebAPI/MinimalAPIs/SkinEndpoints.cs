using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class SkinEndpoints
{
    public static WebApplication MapSkinEndpoints(this WebApplication app)
    {
        // Apply authorization to all Skin endpoints
        var group = app.MapGroup("api/skins")
            .WithTags("Skins")
                       .RequireAuthorization();

        // GET: /skins
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Skins.Include(s => s.Quality).ToListAsync())
            .WithName("GetAllSkins")
            .Produces<List<Skin>>(StatusCodes.Status200OK);

        // GET: /skins/{id}
        group.MapGet("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var skin = await db.Skins.Include(s => s.Quality).FirstOrDefaultAsync(s => s.Id == id);
            return skin is not null ? Results.Ok(skin) : Results.NotFound();
        })
        .WithName("GetSkinById")
        .Produces<Skin>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST: /skins
        group.MapPost("/", async (Skin skin, ApplicationDbContext db) =>
        {
            db.Skins.Add(skin);
            await db.SaveChangesAsync();
            return Results.Created($"/skins/{skin.Id}", skin);
        })
        .WithName("CreateSkin")
        .Accepts<Skin>("application/json")
        .Produces<Skin>(StatusCodes.Status201Created);

        // PUT: /skins/{id}
        group.MapPut("/{id}", async (long id, UpdateSkinDto input, ApplicationDbContext db) =>
        {
            var skin = await db.Skins.FindAsync(id);
            if (skin is null) return Results.NotFound();

            if (input.Name != null) skin.Name = input.Name;
            if (input.IsWarPaint.HasValue) skin.IsWarPaint = (bool)input.IsWarPaint;
            if (input.QualityId.HasValue) skin.QualityId = input.QualityId;
            if (input.IsActive.HasValue) skin.IsActive = (bool)input.IsActive;

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateSkin")
        .Accepts<Skin>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /skins/{id}
        group.MapDelete("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var skin = await db.Skins.FindAsync(id);
            if (skin is null) return Results.NotFound();

            db.Skins.Remove(skin);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteSkin")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

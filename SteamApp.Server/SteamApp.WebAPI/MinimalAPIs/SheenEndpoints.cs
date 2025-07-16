using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class SheenEndpoints
{
    public static WebApplication MapSheenEndpoints(this WebApplication app)
    {
        // Apply authorization to all Sheen endpoints
        var group = app.MapGroup("api/sheens")
            .WithTags("Sheens")
                       .RequireAuthorization();

        // GET: /sheens
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Sheens.ToListAsync())
            .WithName("GetAllSheens")
            .Produces<List<Sheen>>(StatusCodes.Status200OK);

        // GET: /sheens/{id}
        group.MapGet("/{id}", async (short id, ApplicationDbContext db) =>
            await db.Sheens.FindAsync(id)
                is Sheen sheen
                    ? Results.Ok(sheen)
                    : Results.NotFound())
            .WithName("GetSheenById")
            .Produces<Sheen>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /sheens
        group.MapPost("/", async (Sheen sheen, ApplicationDbContext db) =>
        {
            db.Sheens.Add(sheen);
            await db.SaveChangesAsync();
            return Results.Created($"/sheens/{sheen.Id}", sheen);
        })
        .WithName("CreateSheen")
        .Accepts<Sheen>("application/json")
        .Produces<Sheen>(StatusCodes.Status201Created);

        // PATCH: /sheens
        group.MapPatch("/", async (UpdateSheenDto input, ApplicationDbContext db) =>
        {
            var sheen = await db.Sheens.FindAsync(input.Id);
            if (sheen is null) return Results.NotFound();

            if (input.Name != null) sheen.Name = input.Name;
            if (input.IsGoodSheen.HasValue) sheen.IsGoodSheen = (bool)input.IsGoodSheen;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateSheen")
        .Accepts<Sheen>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /sheens/{id}
        group.MapDelete("/{id}", async (short id, ApplicationDbContext db) =>
        {
            var sheen = await db.Sheens.FindAsync(id);
            if (sheen is null) return Results.NotFound();

            db.Sheens.Remove(sheen);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteSheen")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

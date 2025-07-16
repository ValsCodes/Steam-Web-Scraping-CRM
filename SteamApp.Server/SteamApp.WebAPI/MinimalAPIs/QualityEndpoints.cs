using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class QualityEndpoints
{
    public static WebApplication MapQualityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/qualities")
            .WithTags("Qualities")
            .RequireAuthorization();

        // GET: /qualities
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Qualities.ToListAsync())
            .WithName("GetAllQualities")           
            .Produces<List<Quality>>(StatusCodes.Status200OK);

        // GET: /qualities/{id}
        group.MapGet("/{id}", async (short id, ApplicationDbContext db) =>
            await db.Qualities.FindAsync(id)
                is Quality quality
                    ? Results.Ok(quality)
                    : Results.NotFound())
            .WithName("GetQualityById")
            .Produces<Quality>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /qualities
        group.MapPost("/", async (Quality quality, ApplicationDbContext db) =>
        {
            db.Qualities.Add(quality);
            await db.SaveChangesAsync();
            return Results.Created($"/qualities/{quality.Id}", quality);
        })
        .WithName("CreateQuality")
        .Accepts<Quality>("application/json")
        .Produces<Quality>(StatusCodes.Status201Created);

        // PUT: /qualities/{id}
        group.MapPut("/{id}", async (short id, BaseUpdateDto inputQuality, ApplicationDbContext db) =>
        {
            var quality = await db.Qualities.FindAsync(id);
            if (quality is null)
            {
                return Results.NotFound();
            }

            quality.Name = inputQuality.Name;

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateQuality")
        .Accepts<Quality>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /qualities/{id}
        group.MapDelete("/{id}", async (short id, ApplicationDbContext db) =>
        {
            var quality = await db.Qualities.FindAsync(id);
            if (quality is null) return Results.NotFound();

            db.Qualities.Remove(quality);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteQuality")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class PaintEndpoints
{
    public static WebApplication MapPaintEndpoints(this WebApplication app)
    {
        // Apply authorization to all Paint endpoints
        var group = app.MapGroup("api/paints")
            .WithTags("Paints")
                       .RequireAuthorization();

        // GET: /paints
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Paints.ToListAsync())
            .WithName("GetAllPaints")
            .Produces<List<Paint>>(StatusCodes.Status200OK);

        // GET: /paints/{id}
        group.MapGet("/{id}", async (short id, ApplicationDbContext db) =>
            await db.Paints.FindAsync(id)
                is Paint paint
                    ? Results.Ok(paint)
                    : Results.NotFound())
            .WithName("GetPaintById")
            .Produces<Paint>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /paints
        group.MapPost("/", async (Paint paint, ApplicationDbContext db) =>
        {
            db.Paints.Add(paint);
            await db.SaveChangesAsync();
            return Results.Created($"/paints/{paint.Id}", paint);
        })
        .WithName("CreatePaint")
        .Accepts<Paint>("application/json")
        .Produces<Paint>(StatusCodes.Status201Created);

        // PATCH: /paints
        group.MapPatch("/", async (UpdatePaintDto input, ApplicationDbContext db) =>
        {
            var paint = await db.Paints.FindAsync(input.Id);
            if (paint is null) return Results.NotFound();

            if (input.Name != null) paint.Name = input.Name;
            if (input.R != null) paint.R = (byte)input.R;
            if (input.G != null) paint.G = (byte)input.G;
            if (input.B != null) paint.B = (byte)input.B;
            if (input.IsGoodPaint != null) paint.IsGoodPaint = (bool)input.IsGoodPaint;

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdatePaint")
        .Accepts<Paint>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /paints/{id}
        group.MapDelete("/{id}", async (short id, ApplicationDbContext db) =>
        {
            var paint = await db.Paints.FindAsync(id);
            if (paint is null) return Results.NotFound();

            db.Paints.Remove(paint);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeletePaint")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

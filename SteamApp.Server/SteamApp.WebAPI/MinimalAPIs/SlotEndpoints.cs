using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class SlotEndpoints
{
    public static WebApplication MapSlotEndpoints(this WebApplication app)
    {
        // Apply authorization to all Slot endpoints
        var group = app.MapGroup("api/slots")
            .WithTags("Slots")
                       .RequireAuthorization();

        // GET: /slots
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Slots.ToListAsync())
            .WithName("GetAllSlots")
            .Produces<List<Slot>>(StatusCodes.Status200OK);

        // GET: /slots/{id}
        group.MapGet("/{id}", async (long id, ApplicationDbContext db) =>
            await db.Slots.FindAsync(id)
                is Slot slot
                    ? Results.Ok(slot)
                    : Results.NotFound())
            .WithName("GetSlotById")
            .Produces<Slot>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /slots
        group.MapPost("/", async (Slot slot, ApplicationDbContext db) =>
        {
            db.Slots.Add(slot);
            await db.SaveChangesAsync();
            return Results.Created($"/slots/{slot.Id}", slot);
        })
        .WithName("CreateSlot")
        .Accepts<Slot>("application/json")
        .Produces<Slot>(StatusCodes.Status201Created);

        // PUT: /slots/{id}
        group.MapPut("/{id}", async (long id, BaseUpdateDto input, ApplicationDbContext db) =>
        {
            var slot = await db.Slots.FindAsync(id);
            if (slot is null) return Results.NotFound();

            slot.Name = input.Name;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateSlot")
        .Accepts<Slot>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /slots/{id}
        group.MapDelete("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var slot = await db.Slots.FindAsync(id);
            if (slot is null) return Results.NotFound();

            db.Slots.Remove(slot);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteSlot")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

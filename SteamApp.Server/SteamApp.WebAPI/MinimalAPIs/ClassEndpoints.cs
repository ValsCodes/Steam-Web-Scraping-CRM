using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ClassEndpoints
{
    public static WebApplication MapClassEndpoints(this WebApplication app)
    {
        // Apply authorization to all Class endpoints
        var group = app.MapGroup("api/classes")
                       .WithTags("Classes")
                       .RequireAuthorization();

        // GET: /classes
        group.MapGet("/", async (ApplicationDbContext db) =>
            await db.Classes.ToListAsync())
            .WithName("GetAllClasses")
            .Produces<List<Class>>(StatusCodes.Status200OK);

        // GET: /classes/{id}
        group.MapGet("/{id}", async (long id, ApplicationDbContext db) =>
            await db.Classes.FindAsync(id)
                is Class entity
                    ? Results.Ok(entity)
                    : Results.NotFound())
            .WithName("GetClassById")
            .Produces<Class>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /classes
        group.MapPost("/", async (Class input, ApplicationDbContext db) =>
        {
            db.Classes.Add(input);
            await db.SaveChangesAsync();
            return Results.Created($"/classes/{input.Id}", input);
        })
        .WithName("CreateClass")
        .Accepts<Class>("application/json")
        .Produces<Class>(StatusCodes.Status201Created);

        // PUT: /classes/{id}
        group.MapPut("/{id}", async (long id, BaseUpdateDto input, ApplicationDbContext db) =>
        {
            var entity = await db.Classes.FindAsync(id);
            if (entity is null) return Results.NotFound();

            entity.Name = input.Name;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateClass")
        .Accepts<Class>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE: /classes/{id}
        group.MapDelete("/{id}", async (long id, ApplicationDbContext db) =>
        {
            var entity = await db.Classes.FindAsync(id);
            if (entity is null) return Results.NotFound();

            db.Classes.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteClass")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs;
using SteamApp.Models.DTOs.Class;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;
using SteamApp.WebAPI.Mapper;

namespace SteamApp.WebAPI.MinimalAPIs
{
    // TODO NOT DONE: Implement Game endpoints similar to ClassEndpoints.cs
    public static class GameEndpoints
    {
        public static WebApplication MapClassEndpoints(this WebApplication app)
        {
            // Apply authorization to all Class endpoints
            var group = app.MapGroup("api/classes")
                           .WithTags("Classes")
                           .RequireAuthorization();

            // GET: /games

            group.MapGet("/", static async (ApplicationDbContext db) => await db.Games.Select(c => c.ToDto<Game, BaseDto>()).ToListAsync())
            .WithName("GetAllGames")
            .WithDisplayName("Game")
            .Produces<List<BaseDto>>(StatusCodes.Status200OK);

            // GET: /games/{id}
            group.MapGet("/{id}", async (long id, ApplicationDbContext db) =>
                await db.Games.FindAsync(id)
                    is Game entity
                        ? Results.Ok(entity.ToDto<Game, BaseDto>())
                        : Results.NotFound())
                .WithName("GetGameById")

                .Produces<ClassDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            // POST: /games
            group.MapPost("/", async (ClassCreateDto input, ApplicationDbContext db) =>
            {
                var entity = input.ToEntity();

                db.Classes.Add(entity);
                await db.SaveChangesAsync();
                return Results.Created($"/classes/{entity.Id}", input);
            })
            .WithName("CreateClass")
            .Accepts<ClassCreateDto>("application/json")
            .Produces<ClassCreateDto>(StatusCodes.Status201Created);

            // PUT: /classes/{id}
            group.MapPut("/{id}", async (ClassUpdateDto input, ApplicationDbContext db) =>
            {
                var entity = await db.Classes.FindAsync(input.Id);
                if (entity is null) return Results.NotFound();

                if (input.Name != null) entity.Name = input.Name;
                if (input.GameId != null) entity.GameId = input.GameId.Value;

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateClass")
            .Accepts<ClassUpdateDto>("application/json")
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
}

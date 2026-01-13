using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Models.DTOs.ExtraPixel;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class ExtraPixelEndpoints
    {
        public static WebApplication MapExtraPixelEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/extra-pixels")
                           .WithTags("ExtraPixels")
                           .RequireAuthorization();

            // GET: /api/extra-pixels
            group.MapGet("/", async (
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entities = await db.ExtraPixels
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(mapper.Map<List<ExtraPixelDto>>(entities));
            })
            .WithName("GetAllExtraPixels")
            .Produces<List<ExtraPixelDto>>(StatusCodes.Status200OK);

            // GET: /api/extra-pixels/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.ExtraPixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<ExtraPixelDto>(entity));
            })
            .WithName("GetExtraPixelById")
            .Produces<ExtraPixelDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/extra-pixels
            group.MapPost("/", async (
                ExtraPixelCreateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var gameUrlExists = await db.GameUrls
                    .AsNoTracking()
                    .AnyAsync(g => g.Id == input.GameUrlId);

                if (!gameUrlExists)
                {
                    return Results.BadRequest("Invalid GameUrlId");
                }

                var entity = mapper.Map<ExtraPixel>(input);

                db.ExtraPixels.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<ExtraPixelDto>(entity);
                return Results.Created($"/api/extra-pixels/{entity.Id}", dto);
            })
            .WithName("CreateExtraPixel")
            .Accepts<ExtraPixelCreateDto>("application/json")
            .Produces<ExtraPixelDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT: /api/extra-pixels/{id}
            group.MapPut("/{id:long}", async (
                long id,
                ExtraPixelUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.ExtraPixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateExtraPixel")
            .Accepts<ExtraPixelUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/extra-pixels/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db) =>
            {
                var entity = await db.ExtraPixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.ExtraPixels.Remove(entity);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteExtraPixel")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

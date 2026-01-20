using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.Pixel;
using SteamApp.Models.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class PixelEndpoints
    {
        public static WebApplication MapExtraPixelEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/pixels")
                           .WithTags("ExtraPixels")
                           .RequireAuthorization();

            // GET: /api/pixels
            group.MapGet("/", async (
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entities = await db.Pixels
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(mapper.Map<List<PixelDto>>(entities));
            })
            .WithName("GetAllPixels")
            .Produces<List<PixelDto>>(StatusCodes.Status200OK);

            // GET: /api/pixels/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.Pixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<PixelDto>(entity));
            })
            .WithName("GetPixelById")
            .Produces<PixelDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/pixels
            group.MapPost("/", async (
                PixelCreateDto input,
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

                var entity = mapper.Map<Pixel>(input);

                db.Pixels.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<PixelDto>(entity);
                return Results.Created($"/api/extra-pixels/{entity.Id}", dto);
            })
            .WithName("CreatePixel")
            .Accepts<PixelCreateDto>("application/json")
            .Produces<PixelDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT: /api/pixels/{id}
            group.MapPut("/{id:long}", async (
                long id,
                PixelUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.Pixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdatePixel")
            .Accepts<PixelUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/pixels/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db) =>
            {
                var entity = await db.Pixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.Pixels.Remove(entity);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeletePixel")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

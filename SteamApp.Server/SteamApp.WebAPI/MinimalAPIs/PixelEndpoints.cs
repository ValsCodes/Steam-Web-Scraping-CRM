using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.Pixel;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class PixelEndpoints
    {
        public static WebApplication MapPixelEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/pixels")
                           .WithTags("Pixels")
                           .RequireAuthorization();

            // GET: /api/pixels
            group.MapGet("/", async (ApplicationDbContext db) =>
            {
                var entities = await db.Pixels
                    .AsNoTracking()
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.RedValue,
                        e.GreenValue,
                        e.BlueValue,
                        e.GameId,
                        GameName = e.Game.Name
                    })
                    .ToListAsync();

                return Results.Ok(entities);
            })
            .WithName("GetAllPixels")
            .Produces<List<object>>(StatusCodes.Status200OK);

            // GET: /api/pixels/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.Pixels
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

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
                IMapper mapper, IMemoryCache cache) =>
            {
                var gameExists = await db.Games
                    .AsNoTracking()
                    .AnyAsync(g => g.Id == input.GameId);

                if (!gameExists)
                {
                    return Results.BadRequest("Invalid GameId");
                }

                var entity = mapper.Map<Pixel>(input);

                db.Pixels.Add(entity);
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.Game, entity.Id);
                cache.Remove(cacheKey);

                var dto = mapper.Map<PixelDto>(entity);
                return Results.Created($"/api/pixels/{entity.Id}", dto);
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
                IMapper mapper, IMemoryCache cache) =>
            {
                var entity = await db.Pixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                mapper.Map(input, entity);
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.Game, entity.GameId);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("UpdatePixel")
            .Accepts<PixelUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/pixels/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db, IMemoryCache cache) =>
            {
                var entity = await db.Pixels.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.Pixels.Remove(entity);
                await db.SaveChangesAsync();

                var cacheKey = string.Format(CacheKeys.Game, entity.GameId);
                cache.Remove(cacheKey);

                return Results.NoContent();
            })
            .WithName("DeletePixel")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

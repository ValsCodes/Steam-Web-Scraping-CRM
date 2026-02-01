using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.Tag;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class TagsEndpoints
{
    public static WebApplication MapTagsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/tags")
                       .WithTags("Tags")
                       .RequireAuthorization();

        // GET: /api/tags
        group.MapGet("/", async (
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var tags = await db.Tags
                .AsNoTracking()
                .Include(x => x.Game)
                .ToListAsync();

            var dto = mapper.Map<IEnumerable<TagDto>>(tags);
            return Results.Ok(dto);
        });

        // GET: /api/tags/{id}
        group.MapGet("/{id:long}", async (
            long id,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var tag = await db.Tags
                .AsNoTracking()
                .Include(x => x.Game)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tag is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(mapper.Map<TagDto>(tag));
        });

        // GET: /api/tags/game/{gameId}
        group.MapGet("/game/{gameId:long}", async (
            long gameId,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var tags = await db.Tags
                .AsNoTracking()
                .Where(x => x.GameId == gameId)
                .Include(x => x.Game)
                .ToListAsync();

            return Results.Ok(mapper.Map<IEnumerable<TagDto>>(tags));
        });

        // POST: /api/tags
        group.MapPost("/", async (
            TagCreateDto input,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var gameExists = await db.Games
                .AnyAsync(x => x.Id == input.GameId);

            if (!gameExists)
            {
                return Results.BadRequest("Invalid GameId");
            }

            var entity = mapper.Map<Tag>(input);

            db.Tags.Add(entity);
            await db.SaveChangesAsync();

            var dto = mapper.Map<TagDto>(entity);

            return Results.Created($"/api/tags/{entity.Id}", dto);
        });

        // PUT: /api/tags/{id}
        group.MapPut("/{id:long}", async (
            long id,
            TagUpdateDto input,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entity = await db.Tags.FindAsync(id);

            if (entity is null)
            {
                return Results.NotFound();
            }

            mapper.Map(input, entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE: /api/tags/{id}
        group.MapDelete("/{id:long}", async (
            long id,
            ApplicationDbContext db) =>
        {
            var entity = await db.Tags.FindAsync(id);

            if (entity is null)
            {
                return Results.NotFound();
            }

            db.Tags.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return app;
    }
}
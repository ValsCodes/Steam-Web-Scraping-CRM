using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.WatchList;
using SteamApp.Application.DTOs.WatchListItem;
using SteamApp.Domain.Entities;
using SteamApp.WebAPI.Context;

namespace SteamApp.WebAPI.MinimalAPIs
{
    public static class WatchListEndpoints
    {
        public static WebApplication MapWatchListEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("api/watch-list")
                           .WithTags("WatchList")
                           .RequireAuthorization();

            // GET: /api/watch-list
            group.MapGet("/", async (
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entities = await db.WatchList
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(mapper.Map<List<WatchListDto>>(entities));
            })
            .WithName("GetAllWatchList")
            .Produces<List<object>>(StatusCodes.Status200OK);

            // GET: /api/watch-list/{id}
            group.MapGet("/{id:long}", async (
                long id,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.WatchList
                    .FirstAsync(w => w.Id == id);

                if (entity is null) { return Results.NotFound(); }

                return Results.Ok(mapper.Map<WatchListDto>(entity));
            })
            .WithName("GetWatchListItemById")
            .Produces<WatchListDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST: /api/watch-list
            group.MapPost("/", async (
                WatchListCreateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                input.RegistrationDate ??= new DateOnly();
                var entity = mapper.Map<WatchList>(input);             
              
                db.WatchList.Add(entity);
                await db.SaveChangesAsync();

                var dto = mapper.Map<WatchListDto>(entity);
                return Results.Created($"/api/watch-list/{entity.Id}", dto);
            })
            .WithName("CreateWatchListItem")
            .Accepts<WatchListCreateDto>("application/json")
            .Produces<WatchListDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // PUT: /api/watch-list/{id}
            group.MapPut("/{id:long}", async (
                long id,
                WatchListUpdateDto input,
                ApplicationDbContext db,
                IMapper mapper) =>
            {
                var entity = await db.WatchList.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                input.RegistrationDate ??= new DateOnly();

 
                mapper.Map(input, entity);

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateWatchListItem")
            .Accepts<WatchListUpdateDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE: /api/watch-list/{id}
            group.MapDelete("/{id:long}", async (
                long id,
                ApplicationDbContext db) =>
            {
                var entity = await db.WatchList.FindAsync(id);
                if (entity is null) { return Results.NotFound(); }

                db.WatchList.Remove(entity);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteWatchList")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}

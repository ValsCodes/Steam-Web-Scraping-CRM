using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.WatchList;
using SteamApp.Application.DTOs.WatchListItem;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Contracts.Pagination;

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

            // GET: /api/watch-list/paged
            group.MapGet("/paged", async (
                ApplicationDbContext db,
                [AsParameters] WatchListPageQuery request,
                CancellationToken ct) =>
            {
                var query = db.WatchList.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var nameFilter = request.Name.Trim();
                    query = query.Where(x => x.Name != null && x.Name.Contains(nameFilter));
                }

                query = request.SortBy switch
                {
                    "name" => request.IsDescending
                        ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.Name).ThenBy(x => x.Id),
                    "registrationDate" => request.IsDescending
                        ? query.OrderByDescending(x => x.RegistrationDate).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.RegistrationDate).ThenBy(x => x.Id),
                    "isActive" => request.IsDescending
                        ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id)
                        : query.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
                    _ => query.OrderBy(x => x.Id),
                };

                var totalCount = await query.CountAsync(ct);
                var pageWindow = request.ToPageWindow(totalCount);

                var items = await query
                    .ApplyPage(pageWindow)
                    .Select(x => new WatchListDto
                    {
                        Id = x.Id,
                        Name = x.Name ?? string.Empty,
                        Url = x.Url ?? string.Empty,
                        RegistrationDate = x.RegistrationDate,
                        IsActive = x.IsActive,
                    })
                    .ToListAsync(ct);

                return Results.Ok(pageWindow.ToPagedResponse(items));
            })
            .WithName("GetPagedWatchList")
            .Produces(StatusCodes.Status200OK);

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

    public sealed record WatchListPageQuery : PagedQuery
    {
        public string? Name { get; init; }
    }
}

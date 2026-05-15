using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.ScrapingMode;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ScrapingModeEndpoints
{
    public static WebApplication MapScrapingModeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/scraping-modes")
                       .WithTags("ScrapingModes")
                       .RequireAuthorization(SecurityPolicies.ApiUser)
                       .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        group.MapGet("/", async (
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entities = await db.ScrapingModes
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            return Results.Ok(mapper.Map<IEnumerable<ScrapingModeDto>>(entities));
        })
        .WithName("GetAllScrapingModes")
        .Produces<IEnumerable<ScrapingModeDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:long}", async (
            long id,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entity = await db.ScrapingModes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(mapper.Map<ScrapingModeDto>(entity));
        })
        .WithName("GetScrapingModeById")
        .Produces<ScrapingModeDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            ScrapingModeCreateDto input,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entity = mapper.Map<ScrapingMode>(input);

            db.ScrapingModes.Add(entity);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/scraping-modes/{entity.Id}",
                mapper.Map<ScrapingModeDto>(entity));
        })
        .WithName("CreateScrapingMode")
        .Accepts<ScrapingModeCreateDto>("application/json")
        .Produces<ScrapingModeDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:long}", async (
            long id,
            ScrapingModeUpdateDto input,
            ApplicationDbContext db,
            IMapper mapper) =>
        {
            var entity = await db.ScrapingModes.FindAsync(id);

            if (entity is null)
            {
                return Results.NotFound();
            }

            mapper.Map(input, entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateScrapingMode")
        .Accepts<ScrapingModeUpdateDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:long}", async (
            long id,
            ApplicationDbContext db) =>
        {
            var entity = await db.ScrapingModes.FindAsync(id);

            if (entity is null)
            {
                return Results.NotFound();
            }

            db.ScrapingModes.Remove(entity);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteScrapingMode")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

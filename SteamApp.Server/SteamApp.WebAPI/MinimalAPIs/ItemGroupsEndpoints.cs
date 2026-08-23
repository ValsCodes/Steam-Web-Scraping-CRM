using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.ItemGroup;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class ItemGroupsEndpoints
{
    public static WebApplication MapItemGroupsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/item-groups")
            .WithTags("Item Groups")
            .RequireAuthorization(SecurityPolicies.ApiUser)
            .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        group.MapGet("/game/{gameId:long}", async (
            long gameId,
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var gameExists = await db.Games
                .AsNoTracking()
                .AnyAsync(x => x.Id == gameId && x.UserId == userId, ct);

            if (!gameExists)
            {
                return Results.NotFound();
            }

            var itemGroups = await db.ItemGroups
                .AsNoTracking()
                .Where(x => x.GameId == gameId && x.UserId == userId)
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .Select(x => new ItemGroupDto
                {
                    Id = x.Id,
                    GameId = x.GameId,
                    Name = x.Name,
                })
                .ToListAsync(ct);

            return Results.Ok(itemGroups);
        });

        group.MapPost("/", async (
            ItemGroupCreateDto input,
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var name = input.Name?.Trim();
            if (string.IsNullOrEmpty(name) || name.Length > ItemGroup.NameMaxLength)
            {
                return Results.BadRequest(new { message = $"Name must be between 1 and {ItemGroup.NameMaxLength} characters." });
            }

            var gameExists = await db.Games
                .AsNoTracking()
                .AnyAsync(x => x.Id == input.GameId && x.UserId == userId, ct);

            if (!gameExists)
            {
                return Results.BadRequest(new { message = "Invalid GameId" });
            }

            var duplicateExists = await db.ItemGroups
                .AsNoTracking()
                .AnyAsync(
                    x => x.GameId == input.GameId &&
                         x.UserId == userId &&
                         x.Name == name,
                    ct);

            if (duplicateExists)
            {
                return Results.Conflict(new { message = "An item group with this name already exists for the selected game." });
            }

            var entity = new ItemGroup
            {
                GameId = input.GameId,
                Name = name,
                UserId = userId,
            };

            db.ItemGroups.Add(entity);
            await db.SaveChangesAsync(ct);

            var dto = new ItemGroupDto
            {
                Id = entity.Id,
                GameId = entity.GameId,
                Name = entity.Name,
            };

            return Results.Created($"/api/item-groups/{entity.Id}", dto);
        });

        return app;
    }
}

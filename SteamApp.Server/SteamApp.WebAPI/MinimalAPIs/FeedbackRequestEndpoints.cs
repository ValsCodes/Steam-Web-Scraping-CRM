using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.FeedbackRequest;
using SteamApp.Application.Utilities;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.MinimalAPIs;

public static class FeedbackRequestEndpoints
{
    public static WebApplication MapFeedbackRequestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/feedback-requests")
                       .WithTags("FeedbackRequests")
                       .RequireAuthorization(SecurityPolicies.ApiUser)
                       .RequireRateLimiting(SecurityPolicies.ApiRateLimit);

        group.MapGet("/", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entities = await db.FeedbackRequests
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
                .ToListAsync(ct);

            return Results.Ok(mapper.Map<List<FeedbackRequestDto>>(entities));
        })
        .WithName("GetAllFeedbackRequests")
        .Produces<List<FeedbackRequestDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:long}", async (
            long id,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var entity = await db.FeedbackRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
            if (entity is null) { return Results.NotFound(); }

            return Results.Ok(mapper.Map<FeedbackRequestDto>(entity));
        })
        .WithName("GetFeedbackRequestById")
        .Produces<FeedbackRequestDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/reference/{referenceId}", async (
            string referenceId,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            if (!FeedbackRequestReference.TryParse(referenceId, out var id))
            {
                return Results.BadRequest("Invalid reference ID.");
            }

            var entity = await db.FeedbackRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
            if (entity is null) { return Results.NotFound(); }

            return Results.Ok(mapper.Map<FeedbackRequestDto>(entity));
        })
        .WithName("GetFeedbackRequestByReference")
        .Produces<FeedbackRequestDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:long}/history", async (
            long id,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            var requestExists = await db.FeedbackRequests
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.UserId == userId, ct);
            if (!requestExists) { return Results.NotFound(); }

            var history = await db.FeedbackRequestHistories
                .AsNoTracking()
                .Where(x => x.FeedbackRequestId == id && x.UserId == userId)
                .OrderBy(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id)
                .ToListAsync(ct);

            return Results.Ok(mapper.Map<List<FeedbackRequestHistoryDto>>(history));
        })
        .WithName("GetFeedbackRequestHistory")
        .Produces<List<FeedbackRequestHistoryDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            FeedbackRequestCreateDto input,
            HttpContext httpContext,
            ApplicationDbContext db,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            if (!Validate(input, out var error))
            {
                return Results.BadRequest(error);
            }

            var now = DateTime.UtcNow;
            var entity = new FeedbackRequest
            {
                Type = input.Type,
                Title = input.Title!.Trim(),
                Description = input.Description!.Trim(),
                Area = NormalizeOptional(input.Area),
                Status = FeedbackRequestStatusEnum.Active,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                StatusChangedAtUtc = now,
                UserId = userId
            };

            db.FeedbackRequests.Add(entity);
            db.FeedbackRequestHistories.Add(new FeedbackRequestHistory
            {
                FeedbackRequest = entity,
                Action = FeedbackRequestHistoryActionEnum.Created,
                CreatedAtUtc = now,
                NewType = entity.Type,
                NewTitle = entity.Title,
                NewDescription = entity.Description,
                NewArea = entity.Area,
                NewStatus = entity.Status,
                UserId = userId
            });
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/api/feedback-requests/{entity.Id}",
                mapper.Map<FeedbackRequestDto>(entity));
        })
        .WithName("CreateFeedbackRequest")
        .Accepts<FeedbackRequestCreateDto>("application/json")
        .Produces<FeedbackRequestDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:long}", async (
            long id,
            FeedbackRequestUpdateDto input,
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            if (!Validate(input, out var error))
            {
                return Results.BadRequest(error);
            }

            var entity = await db.FeedbackRequests
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
            if (entity is null) { return Results.NotFound(); }

            var nextTitle = input.Title!.Trim();
            var nextDescription = input.Description!.Trim();
            var nextArea = NormalizeOptional(input.Area);
            var now = DateTime.UtcNow;
            var statusChanged = entity.Status != input.Status;
            var hasTrackedChange =
                entity.Type != input.Type ||
                entity.Title != nextTitle ||
                entity.Description != nextDescription ||
                entity.Area != nextArea ||
                statusChanged;

            if (hasTrackedChange)
            {
                db.FeedbackRequestHistories.Add(new FeedbackRequestHistory
                {
                    FeedbackRequestId = entity.Id,
                    Action = statusChanged && !HasDetailChange(entity, input.Type, nextTitle, nextDescription, nextArea)
                        ? FeedbackRequestHistoryActionEnum.StatusChanged
                        : FeedbackRequestHistoryActionEnum.Updated,
                    CreatedAtUtc = now,
                    PreviousType = entity.Type,
                    NewType = input.Type,
                    PreviousTitle = entity.Title,
                    NewTitle = nextTitle,
                    PreviousDescription = entity.Description,
                    NewDescription = nextDescription,
                    PreviousArea = entity.Area,
                    NewArea = nextArea,
                    PreviousStatus = entity.Status,
                    NewStatus = input.Status,
                    UserId = userId
                });

                entity.Type = input.Type;
                entity.Title = nextTitle;
                entity.Description = nextDescription;
                entity.Area = nextArea;
                entity.Status = input.Status;
                entity.UpdatedAtUtc = now;

                if (statusChanged)
                {
                    entity.StatusChangedAtUtc = now;
                }
            }

            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateFeedbackRequest")
        .Accepts<FeedbackRequestUpdateDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:long}/status", async (
            long id,
            FeedbackRequestUpdateStatusDto input,
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetUserId();
            if (userId is null) { return Results.Unauthorized(); }

            if (!IsDefined(input.Status))
            {
                return Results.BadRequest("Invalid status.");
            }

            var entity = await db.FeedbackRequests
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
            if (entity is null) { return Results.NotFound(); }

            if (entity.Status != input.Status)
            {
                var now = DateTime.UtcNow;
                db.FeedbackRequestHistories.Add(new FeedbackRequestHistory
                {
                    FeedbackRequestId = entity.Id,
                    Action = FeedbackRequestHistoryActionEnum.StatusChanged,
                    CreatedAtUtc = now,
                    PreviousStatus = entity.Status,
                    NewStatus = input.Status,
                    UserId = userId
                });

                entity.Status = input.Status;
                entity.UpdatedAtUtc = now;
                entity.StatusChangedAtUtc = now;
                await db.SaveChangesAsync(ct);
            }

            return Results.NoContent();
        })
        .WithName("UpdateFeedbackRequestStatus")
        .Accepts<FeedbackRequestUpdateStatusDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static bool Validate(FeedbackRequestCreateDto input, out string error)
    {
        if (!IsDefined(input.Type))
        {
            error = "Invalid type.";
            return false;
        }

        return ValidateText(input.Title, input.Description, input.Area, out error);
    }

    private static bool Validate(FeedbackRequestUpdateDto input, out string error)
    {
        if (!IsDefined(input.Type))
        {
            error = "Invalid type.";
            return false;
        }

        if (!IsDefined(input.Status))
        {
            error = "Invalid status.";
            return false;
        }

        return ValidateText(input.Title, input.Description, input.Area, out error);
    }

    private static bool HasDetailChange(
        FeedbackRequest entity,
        FeedbackRequestTypeEnum nextType,
        string nextTitle,
        string nextDescription,
        string? nextArea)
    {
        return entity.Type != nextType ||
               entity.Title != nextTitle ||
               entity.Description != nextDescription ||
               entity.Area != nextArea;
    }

    private static bool ValidateText(
        string? title,
        string? description,
        string? area,
        out string error)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            error = "Title is required.";
            return false;
        }

        if (title.Trim().Length > FeedbackRequest.TitleMaxLength)
        {
            error = $"Title must be {FeedbackRequest.TitleMaxLength} characters or fewer.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            error = "Description is required.";
            return false;
        }

        if (description.Trim().Length > FeedbackRequest.DescriptionMaxLength)
        {
            error = $"Description must be {FeedbackRequest.DescriptionMaxLength} characters or fewer.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(area) &&
            area.Trim().Length > FeedbackRequest.AreaMaxLength)
        {
            error = $"Area must be {FeedbackRequest.AreaMaxLength} characters or fewer.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool IsDefined<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return Enum.IsDefined(value);
    }
}

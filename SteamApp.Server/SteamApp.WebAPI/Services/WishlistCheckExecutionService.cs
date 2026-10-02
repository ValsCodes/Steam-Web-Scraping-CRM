using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Contracts.Pagination;

namespace SteamApp.WebAPI.Services;

public sealed class WishlistCheckExecutionService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IWishlistService wishlistService,
    TimeProvider timeProvider,
    ILogger<WishlistCheckExecutionService> logger) : IWishlistCheckExecutionService
{
    private static readonly Error NotFoundError = new(
        "WishlistCheck.NotFound",
        "The price alert was not found.",
        ErrorType.NotFound);

    public Task<Result<WishlistCheckExecutionOutcome>> ExecuteManualAsync(
        long wishListId,
        string userId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            wishListId,
            userId,
            requireActive: false,
            WishListCheckSourceEnum.Manual,
            timeProvider.GetUtcNow().UtcDateTime,
            correlationId,
            cancellationToken);
    }

    public Task<Result<WishlistCheckExecutionOutcome>> ExecuteScheduledAsync(
        long wishListId,
        DateTime requestedAtUtc,
        string correlationId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            wishListId,
            requiredUserId: null,
            requireActive: true,
            WishListCheckSourceEnum.Scheduled,
            requestedAtUtc,
            correlationId,
            cancellationToken);
    }

    public async Task<Result<WishListCheckHistoryPageDto>> GetHistoryAsync(
        long wishListId,
        string userId,
        WishListCheckHistoryPageQuery request,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var ownsAlert = await db.WishLists
            .AsNoTracking()
            .AnyAsync(x => x.Id == wishListId && x.UserId == userId, cancellationToken);
        if (!ownsAlert)
        {
            return Result<WishListCheckHistoryPageDto>.Failure(NotFoundError);
        }

        var query = db.Set<WishListCheckHistory>()
            .AsNoTracking()
            .Where(x => x.WishListId == wishListId && x.UserId == userId)
            .OrderByDescending(x => x.StartedAtUtc)
            .ThenByDescending(x => x.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageWindow = request.ToPageWindow(totalCount);
        var histories = await query
            .ApplyPage(pageWindow)
            .ToListAsync(cancellationToken);

        return Result<WishListCheckHistoryPageDto>.Success(new WishListCheckHistoryPageDto
        {
            Items = histories.Select(ToDto).ToList(),
            PageNumber = pageWindow.PageNumber,
            PageSize = pageWindow.PageSize,
            TotalCount = pageWindow.TotalCount,
            TotalPages = pageWindow.TotalPages
        });
    }

    private async Task<Result<WishlistCheckExecutionOutcome>> ExecuteAsync(
        long wishListId,
        string? requiredUserId,
        bool requireActive,
        WishListCheckSourceEnum source,
        DateTime requestedAtUtc,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var history = await StartHistoryAsync(
            wishListId,
            requiredUserId,
            requireActive,
            source,
            requestedAtUtc,
            correlationId,
            cancellationToken);
        if (history is null)
        {
            return Result<WishlistCheckExecutionOutcome>.Failure(NotFoundError);
        }

        logger.LogInformation(
            "Wishlist check {HistoryId} started for wishlist item {WishlistId} with correlation {CorrelationId} and source {Source}.",
            history.Id,
            wishListId,
            history.CorrelationId,
            source);

        try
        {
            var checkResult = await wishlistService.CheckWishlistItem(wishListId, cancellationToken);
            if (checkResult.IsFailure)
            {
                var error = checkResult.Error!;
                var failed = await CompleteHistoryAsync(
                    history.Id,
                    WishListCheckStatusEnum.Failed,
                    response: null,
                    error,
                    cancellationToken);

                logger.LogWarning(
                    "Wishlist check {HistoryId} failed with code {ErrorCode} and correlation {CorrelationId}.",
                    history.Id,
                    error.Code,
                    history.CorrelationId);

                return Result<WishlistCheckExecutionOutcome>.Success(
                    new WishlistCheckExecutionOutcome(failed, error));
            }

            var succeeded = await CompleteHistoryAsync(
                history.Id,
                WishListCheckStatusEnum.Succeeded,
                checkResult.Value,
                error: null,
                cancellationToken);

            logger.LogInformation(
                "Wishlist check {HistoryId} completed with correlation {CorrelationId}; price reached: {IsPriceReached}.",
                history.Id,
                history.CorrelationId,
                succeeded.IsPriceReached);

            return Result<WishlistCheckExecutionOutcome>.Success(
                new WishlistCheckExecutionOutcome(succeeded, CheckError: null));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await CompleteHistoryAsync(
                history.Id,
                WishListCheckStatusEnum.Canceled,
                response: null,
                error: null,
                CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            var error = new Error(
                "WishlistCheck.Unexpected",
                "The price check could not be completed.",
                ErrorType.Unavailable);

            await CompleteHistoryAsync(
                history.Id,
                WishListCheckStatusEnum.Failed,
                response: null,
                error,
                CancellationToken.None);

            logger.LogError(
                exception,
                "Wishlist check {HistoryId} failed unexpectedly with correlation {CorrelationId}.",
                history.Id,
                history.CorrelationId);
            throw;
        }
    }

    private async Task<WishListCheckHistory?> StartHistoryAsync(
        long wishListId,
        string? requiredUserId,
        bool requireActive,
        WishListCheckSourceEnum source,
        DateTime requestedAtUtc,
        string correlationId,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = db.WishLists
            .AsNoTracking()
            .Where(x => x.Id == wishListId && x.UserId != null);

        if (requiredUserId is not null)
        {
            query = query.Where(x => x.UserId == requiredUserId);
        }

        if (requireActive)
        {
            query = query.Where(x => x.IsActive);
        }

        var snapshot = await query
            .Select(x => new
            {
                x.Id,
                UserId = x.UserId!,
                x.Price,
                GameName = x.Game.Name ?? "Unknown game"
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        var startedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var history = new WishListCheckHistory
        {
            WishListId = snapshot.Id,
            UserId = snapshot.UserId,
            GameName = Truncate(snapshot.GameName, WishListCheckHistory.GameNameMaxLength),
            Source = source,
            Status = WishListCheckStatusEnum.Running,
            TargetPrice = snapshot.Price,
            RequestedAtUtc = requestedAtUtc,
            StartedAtUtc = startedAtUtc,
            CorrelationId = Truncate(correlationId, WishListCheckHistory.CorrelationIdMaxLength)
        };

        db.Set<WishListCheckHistory>().Add(history);
        await db.SaveChangesAsync(cancellationToken);
        return history;
    }

    private async Task<WishListCheckHistoryDto> CompleteHistoryAsync(
        long historyId,
        WishListCheckStatusEnum status,
        WhishListResponse? response,
        Error? error,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var history = await db.Set<WishListCheckHistory>()
            .SingleAsync(x => x.Id == historyId, cancellationToken);

        history.Status = status;
        history.CurrentPrice = response?.CurrentPrice;
        history.IsPriceReached = response?.IsPriceReached;
        history.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        history.ErrorCode = error is null
            ? null
            : Truncate(error.Code, WishListCheckHistory.ErrorCodeMaxLength);
        history.ErrorText = error is null
            ? null
            : Truncate(error.Description, WishListCheckHistory.ErrorTextMaxLength);

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(history);
    }

    private static WishListCheckHistoryDto ToDto(WishListCheckHistory history)
    {
        return new WishListCheckHistoryDto
        {
            Id = history.Id,
            WishListId = history.WishListId,
            GameName = history.GameName,
            Source = history.Source.ToString(),
            Status = history.Status.ToString(),
            TargetPrice = history.TargetPrice,
            CurrentPrice = history.CurrentPrice,
            IsPriceReached = history.IsPriceReached,
            RequestedAtUtc = history.RequestedAtUtc,
            StartedAtUtc = history.StartedAtUtc,
            CompletedAtUtc = history.CompletedAtUtc,
            DurationMilliseconds = history.CompletedAtUtc.HasValue
                ? Math.Max(0, (long)(history.CompletedAtUtc.Value - history.StartedAtUtc).TotalMilliseconds)
                : null,
            CorrelationId = history.CorrelationId,
            ErrorCode = history.ErrorCode,
            ErrorText = history.ErrorText
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

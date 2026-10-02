using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.WebAPI.Contracts.Pagination;

namespace SteamApp.WebAPI.Services;

public interface IWishlistCheckExecutionService
{
    Task<Result<WishlistCheckExecutionOutcome>> ExecuteManualAsync(
        long wishListId,
        string userId,
        string correlationId,
        CancellationToken cancellationToken);

    Task<Result<WishlistCheckExecutionOutcome>> ExecuteScheduledAsync(
        long wishListId,
        DateTime requestedAtUtc,
        string correlationId,
        CancellationToken cancellationToken);

    Task<Result<WishListCheckHistoryPageDto>> GetHistoryAsync(
        long wishListId,
        string userId,
        WishListCheckHistoryPageQuery request,
        CancellationToken cancellationToken);
}

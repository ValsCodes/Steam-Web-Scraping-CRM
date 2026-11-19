using SteamApp.Application.Common.Pagination;
using SteamApp.Contracts.WishListItem;
using SteamApp.Application.Common.Results;
using SteamApp.Contracts.Pagination;

namespace SteamApp.Application.Features.Wishlist;

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

using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;

namespace SteamApp.Interfaces.Services;

public interface IWishlistService
{
    Task<Result<WhishListResponse>> CheckWishlistItem(
        long id,
        CancellationToken cancellationToken);

    Task<IEnumerable<WishListDto>> GetAllAsync(CancellationToken ct);

    Task<WishListDto> GetAsync(long id, CancellationToken ct);
}

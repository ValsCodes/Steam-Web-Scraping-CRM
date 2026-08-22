using SteamApp.Application.DTOs.WishListItem;

namespace SteamApp.Interfaces.Services
{
    public interface IWishlistService
    {
        Task<WhishListResponse> CheckWishlistItem(long id);

        Task<IEnumerable<WishListDto>> GetAllAsync(CancellationToken ct);

        Task<WishListDto> GetAsync(long id, CancellationToken ct);
    }
}

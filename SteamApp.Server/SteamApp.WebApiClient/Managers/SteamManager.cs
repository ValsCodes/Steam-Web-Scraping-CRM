using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Application.DTOs.WishListItem;

namespace SteamApp.WebApiClient.Managers
{
    public class SteamManager(BaseApiClient api)
    {
        public async Task<WhishListResponse?> CheckWishlistItem(long wishlistId, CancellationToken ct = default)
        {
            return await api.GetAsync<WhishListResponse?>($"api/steam/check-wishlist/{wishlistId}", ct);
        }
    }
}

using SteamApp.Contracts.WishListItem;
using SteamApp.Application.DTOs.WishListItem;

namespace SteamApp.Application.Abstractions.Persistence;

public interface IWishlistNotificationRecipientService
{
    Task<IReadOnlyList<WishlistNotificationRecipient>> GetActiveRecipientsAsync(
        CancellationToken cancellationToken);

    Task<WishlistNotificationRecipient?> GetActiveRecipientAsync(
        long wishlistId,
        CancellationToken cancellationToken);
}

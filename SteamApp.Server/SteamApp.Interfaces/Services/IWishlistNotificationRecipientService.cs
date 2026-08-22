using SteamApp.Application.DTOs.WishListItem;

namespace SteamApp.Interfaces.Services;

public interface IWishlistNotificationRecipientService
{
    Task<IReadOnlyList<WishlistNotificationRecipient>> GetActiveRecipientsAsync(
        CancellationToken cancellationToken);
}

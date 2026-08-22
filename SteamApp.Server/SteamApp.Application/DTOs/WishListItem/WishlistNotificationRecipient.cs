namespace SteamApp.Application.DTOs.WishListItem;

public sealed record WishlistNotificationRecipient(
    long WishlistId,
    string? WishlistName,
    string Email);

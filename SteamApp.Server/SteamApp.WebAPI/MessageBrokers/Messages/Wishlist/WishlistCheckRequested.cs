namespace SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;

public sealed record WishlistCheckRequested(
    long WishlistId,
    string? WishlistName,
    string Email,
    DateTime RequestedAtUtc,
    string CorrelationId);

namespace SteamApp.Contracts.Events.Wishlist;

public sealed record WishlistCheckRequested(
    long WishlistId,
    DateTime RequestedAtUtc,
    string CorrelationId);

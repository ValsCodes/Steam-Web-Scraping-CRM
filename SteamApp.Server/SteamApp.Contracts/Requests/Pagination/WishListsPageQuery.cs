namespace SteamApp.Contracts.Pagination;

public sealed record WishListsPageQuery : PagedQuery
{
    public long? GameId { get; init; }
    public string? Name { get; init; }
}

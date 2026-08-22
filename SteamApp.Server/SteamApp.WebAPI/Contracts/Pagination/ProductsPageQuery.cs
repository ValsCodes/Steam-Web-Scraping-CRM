namespace SteamApp.WebAPI.Contracts.Pagination;

public sealed record ProductsPageQuery : PagedQuery
{
    public long? GameId { get; init; }
    public string? Name { get; init; }
    public int? MinRating { get; init; }
    public long[]? TagIds { get; init; }
}

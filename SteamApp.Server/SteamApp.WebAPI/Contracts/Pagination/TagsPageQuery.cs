namespace SteamApp.WebAPI.Contracts.Pagination;

public sealed record TagsPageQuery : PagedQuery
{
    public long? GameId { get; init; }
    public string? Name { get; init; }
}

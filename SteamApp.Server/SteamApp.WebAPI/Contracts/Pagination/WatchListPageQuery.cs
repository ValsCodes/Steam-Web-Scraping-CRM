namespace SteamApp.WebAPI.Contracts.Pagination;

public sealed record WatchListPageQuery : PagedQuery
{
    public string? Name { get; init; }
}

namespace SteamApp.WebAPI.Contracts.Pagination;

public sealed record GamesPageQuery : PagedQuery
{
    public string? Name { get; init; }
}

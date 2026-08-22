namespace SteamApp.WebAPI.Contracts.Pagination;

public sealed record PixelsPageQuery : PagedQuery
{
    public long? GameId { get; init; }
    public string? Name { get; init; }
}

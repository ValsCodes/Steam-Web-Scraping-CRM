namespace SteamApp.WebAPI.Contracts.GameUrlProduct;

public sealed class GameUrlProductStockHistoryQuery
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public int NormalizedPage => Math.Max(Page ?? 1, 1);

    public int NormalizedPageSize => PageSize.GetValueOrDefault(DefaultPageSize) switch
    {
        <= 0 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => PageSize.GetValueOrDefault(DefaultPageSize),
    };
}

namespace SteamApp.WebAPI.Contracts.Pagination;

public abstract record PagedQuery
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }

    public int NormalizedPageNumber => Math.Max(PageNumber ?? 1, 1);
    public int NormalizedPageSize => PageSize.GetValueOrDefault(DefaultPageSize) switch
    {
        <= 0 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => PageSize.GetValueOrDefault(DefaultPageSize),
    };

    public bool IsDescending =>
        string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}

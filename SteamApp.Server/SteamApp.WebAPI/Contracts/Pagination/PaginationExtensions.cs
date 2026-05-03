namespace SteamApp.WebAPI.Contracts.Pagination;

public readonly record struct PageWindow(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public int Skip => (PageNumber - 1) * PageSize;
}

public static class PaginationExtensions
{
    public static PageWindow ToPageWindow(this PagedQuery request, int totalCount)
    {
        var pageSize = request.NormalizedPageSize;
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        var pageNumber = totalPages == 0
            ? 1
            : Math.Min(request.NormalizedPageNumber, totalPages);

        return new PageWindow(pageNumber, pageSize, totalCount, totalPages);
    }

    public static IQueryable<T> ApplyPage<T>(
        this IQueryable<T> query,
        PageWindow pageWindow)
    {
        return query
            .Skip(pageWindow.Skip)
            .Take(pageWindow.PageSize);
    }

    public static PagedResponse<T> ToPagedResponse<T>(
        this PageWindow pageWindow,
        IReadOnlyCollection<T> items)
    {
        return new PagedResponse<T>
        {
            Items = items,
            PageNumber = pageWindow.PageNumber,
            PageSize = pageWindow.PageSize,
            TotalCount = pageWindow.TotalCount,
            TotalPages = pageWindow.TotalPages,
        };
    }
}

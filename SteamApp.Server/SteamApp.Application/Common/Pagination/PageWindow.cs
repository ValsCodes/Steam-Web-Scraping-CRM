using SteamApp.Contracts.Pagination;
namespace SteamApp.Application.Common.Pagination;

public readonly record struct PageWindow(
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public int Skip => (PageNumber - 1) * PageSize;
}

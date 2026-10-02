namespace SteamApp.Application.DTOs.WishListItem;

public sealed class WishListCheckHistoryPageDto
{
    public IReadOnlyCollection<WishListCheckHistoryDto> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

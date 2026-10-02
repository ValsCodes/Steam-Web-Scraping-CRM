namespace SteamApp.Application.DTOs.WishListItem;

public sealed class WishListCheckHistoryDto
{
    public long Id { get; set; }
    public long WishListId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double? TargetPrice { get; set; }
    public double? CurrentPrice { get; set; }
    public bool? IsPriceReached { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public long? DurationMilliseconds { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ErrorText { get; set; }
}

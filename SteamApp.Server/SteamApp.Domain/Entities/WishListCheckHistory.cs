using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class WishListCheckHistory
{
    public const int CorrelationIdMaxLength = 128;
    public const int ErrorCodeMaxLength = 100;
    public const int ErrorTextMaxLength = 500;
    public const int GameNameMaxLength = 256;

    public long Id { get; set; }

    public long WishListId { get; set; }

    public WishList WishList { get; set; } = null!;

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(GameNameMaxLength)]
    public string GameName { get; set; } = string.Empty;

    public WishListCheckSourceEnum Source { get; set; }

    public WishListCheckStatusEnum Status { get; set; }

    public double? TargetPrice { get; set; }

    public double? CurrentPrice { get; set; }

    public bool? IsPriceReached { get; set; }

    public DateTime RequestedAtUtc { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    [MaxLength(CorrelationIdMaxLength)]
    public string CorrelationId { get; set; } = string.Empty;

    [MaxLength(ErrorCodeMaxLength)]
    public string? ErrorCode { get; set; }

    [MaxLength(ErrorTextMaxLength)]
    public string? ErrorText { get; set; }
}

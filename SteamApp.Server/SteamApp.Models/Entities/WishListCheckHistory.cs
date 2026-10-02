using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("wish_list_check_history")]
public sealed class WishListCheckHistory
{
    public const int CorrelationIdMaxLength = 128;
    public const int ErrorCodeMaxLength = 100;
    public const int ErrorTextMaxLength = 500;
    public const int GameNameMaxLength = 256;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("wish_list_id")]
    public long WishListId { get; set; }

    [ForeignKey(nameof(WishListId))]
    public WishList WishList { get; set; } = null!;

    [MaxLength(450)]
    [Column("user_id")]
    public string? UserId { get; set; }

    [MaxLength(GameNameMaxLength)]
    [Column("game_name")]
    public string GameName { get; set; } = string.Empty;

    [Column("source")]
    public WishListCheckSourceEnum Source { get; set; }

    [Column("status")]
    public WishListCheckStatusEnum Status { get; set; }

    [Column("target_price")]
    public double? TargetPrice { get; set; }

    [Column("current_price")]
    public double? CurrentPrice { get; set; }

    [Column("is_price_reached")]
    public bool? IsPriceReached { get; set; }

    [Column("requested_at_utc")]
    public DateTime RequestedAtUtc { get; set; }

    [Column("started_at_utc")]
    public DateTime StartedAtUtc { get; set; }

    [Column("completed_at_utc")]
    public DateTime? CompletedAtUtc { get; set; }

    [MaxLength(CorrelationIdMaxLength)]
    [Column("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;

    [MaxLength(ErrorCodeMaxLength)]
    [Column("error_code")]
    public string? ErrorCode { get; set; }

    [MaxLength(ErrorTextMaxLength)]
    [Column("error_text")]
    public string? ErrorText { get; set; }
}

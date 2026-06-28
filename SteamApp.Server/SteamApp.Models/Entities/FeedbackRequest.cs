using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("feedback_request")]
public sealed class FeedbackRequest
{
    public const int TitleMaxLength = 140;
    public const int AreaMaxLength = 120;
    public const int DescriptionMaxLength = 4000;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("type")]
    public FeedbackRequestTypeEnum Type { get; set; }

    [MaxLength(TitleMaxLength)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(DescriptionMaxLength)]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(AreaMaxLength)]
    [Column("area")]
    public string? Area { get; set; }

    [Column("status")]
    public FeedbackRequestStatusEnum Status { get; set; } = FeedbackRequestStatusEnum.Active;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    [Column("status_changed_at_utc")]
    public DateTime StatusChangedAtUtc { get; set; }

    [MaxLength(450)]
    [Column("user_id")]
    public string? UserId { get; set; }
}

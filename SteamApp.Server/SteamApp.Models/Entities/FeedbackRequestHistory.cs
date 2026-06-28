using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("feedback_request_history")]
public sealed class FeedbackRequestHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("feedback_request_id")]
    public long FeedbackRequestId { get; set; }

    public FeedbackRequest FeedbackRequest { get; set; } = null!;

    [Column("action")]
    public FeedbackRequestHistoryActionEnum Action { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("previous_type")]
    public FeedbackRequestTypeEnum? PreviousType { get; set; }

    [Column("new_type")]
    public FeedbackRequestTypeEnum? NewType { get; set; }

    [MaxLength(FeedbackRequest.TitleMaxLength)]
    [Column("previous_title")]
    public string? PreviousTitle { get; set; }

    [MaxLength(FeedbackRequest.TitleMaxLength)]
    [Column("new_title")]
    public string? NewTitle { get; set; }

    [MaxLength(FeedbackRequest.DescriptionMaxLength)]
    [Column("previous_description")]
    public string? PreviousDescription { get; set; }

    [MaxLength(FeedbackRequest.DescriptionMaxLength)]
    [Column("new_description")]
    public string? NewDescription { get; set; }

    [MaxLength(FeedbackRequest.AreaMaxLength)]
    [Column("previous_area")]
    public string? PreviousArea { get; set; }

    [MaxLength(FeedbackRequest.AreaMaxLength)]
    [Column("new_area")]
    public string? NewArea { get; set; }

    [Column("previous_status")]
    public FeedbackRequestStatusEnum? PreviousStatus { get; set; }

    [Column("new_status")]
    public FeedbackRequestStatusEnum? NewStatus { get; set; }

    [MaxLength(450)]
    [Column("user_id")]
    public string? UserId { get; set; }
}

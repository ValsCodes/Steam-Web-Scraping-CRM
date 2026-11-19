using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class FeedbackRequestHistory
{
    public long Id { get; set; }

    public long FeedbackRequestId { get; set; }

    public FeedbackRequest FeedbackRequest { get; set; } = null!;

    public FeedbackRequestHistoryActionEnum Action { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public FeedbackRequestTypeEnum? PreviousType { get; set; }

    public FeedbackRequestTypeEnum? NewType { get; set; }

    [MaxLength(FeedbackRequest.TitleMaxLength)]
    public string? PreviousTitle { get; set; }

    [MaxLength(FeedbackRequest.TitleMaxLength)]
    public string? NewTitle { get; set; }

    [MaxLength(FeedbackRequest.DescriptionMaxLength)]
    public string? PreviousDescription { get; set; }

    [MaxLength(FeedbackRequest.DescriptionMaxLength)]
    public string? NewDescription { get; set; }

    [MaxLength(FeedbackRequest.AreaMaxLength)]
    public string? PreviousArea { get; set; }

    [MaxLength(FeedbackRequest.AreaMaxLength)]
    public string? NewArea { get; set; }

    public FeedbackRequestStatusEnum? PreviousStatus { get; set; }

    public FeedbackRequestStatusEnum? NewStatus { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }
}

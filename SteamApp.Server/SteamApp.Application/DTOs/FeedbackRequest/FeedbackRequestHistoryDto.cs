using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.FeedbackRequest;

public sealed class FeedbackRequestHistoryDto
{
    public long Id { get; set; }
    public long FeedbackRequestId { get; set; }
    public FeedbackRequestHistoryActionEnum Action { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public FeedbackRequestTypeEnum? PreviousType { get; set; }
    public FeedbackRequestTypeEnum? NewType { get; set; }
    public string? PreviousTitle { get; set; }
    public string? NewTitle { get; set; }
    public string? PreviousDescription { get; set; }
    public string? NewDescription { get; set; }
    public string? PreviousArea { get; set; }
    public string? NewArea { get; set; }
    public FeedbackRequestStatusEnum? PreviousStatus { get; set; }
    public FeedbackRequestStatusEnum? NewStatus { get; set; }
}

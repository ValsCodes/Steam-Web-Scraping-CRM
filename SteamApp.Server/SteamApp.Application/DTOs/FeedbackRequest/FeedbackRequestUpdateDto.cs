using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.FeedbackRequest;

public sealed class FeedbackRequestUpdateDto
{
    public FeedbackRequestTypeEnum Type { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }
    public FeedbackRequestStatusEnum Status { get; set; }
}

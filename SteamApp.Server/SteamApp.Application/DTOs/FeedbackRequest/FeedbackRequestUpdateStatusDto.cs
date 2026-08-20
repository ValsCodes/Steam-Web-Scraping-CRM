using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.FeedbackRequest;

public sealed class FeedbackRequestUpdateStatusDto
{
    public FeedbackRequestStatusEnum Status { get; set; }
}

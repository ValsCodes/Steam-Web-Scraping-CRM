using SteamApp.Contracts.Enums;

namespace SteamApp.Contracts.FeedbackRequest;

public sealed class FeedbackRequestUpdateStatusDto
{
    public FeedbackRequestStatusEnum Status { get; set; }
}

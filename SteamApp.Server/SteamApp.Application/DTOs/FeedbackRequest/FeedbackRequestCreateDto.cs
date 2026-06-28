using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.FeedbackRequest;

public sealed class FeedbackRequestCreateDto
{
    public FeedbackRequestTypeEnum Type { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }
}

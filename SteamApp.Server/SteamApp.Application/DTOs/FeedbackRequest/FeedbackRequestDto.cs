using SteamApp.Domain.Enums;

namespace SteamApp.Application.DTOs.FeedbackRequest;

public sealed class FeedbackRequestDto
{
    public long Id { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public FeedbackRequestTypeEnum Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Area { get; set; }
    public FeedbackRequestStatusEnum Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime StatusChangedAtUtc { get; set; }
}

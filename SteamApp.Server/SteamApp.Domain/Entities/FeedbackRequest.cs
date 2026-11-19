using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class FeedbackRequest
{
    public const int TitleMaxLength = 140;
    public const int AreaMaxLength = 120;
    public const int DescriptionMaxLength = 4000;
    public const int TargetResourceTypeMaxLength = 50;

    public long Id { get; set; }

    public FeedbackRequestTypeEnum Type { get; set; }

    [MaxLength(TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(DescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(AreaMaxLength)]
    public string? Area { get; set; }

    [MaxLength(TargetResourceTypeMaxLength)]
    public string? TargetResourceType { get; set; }

    public long? TargetResourceId { get; set; }

    public FeedbackRequestStatusEnum Status { get; set; } = FeedbackRequestStatusEnum.Active;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime StatusChangedAtUtc { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }
}

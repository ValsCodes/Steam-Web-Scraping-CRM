using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class ItemGroup
{
    public const int NameMaxLength = 255;

    public long Id { get; set; }

    public long GameId { get; set; }

    public Game Game { get; set; } = null!;

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = null!;

    [MaxLength(450)]
    public string? UserId { get; set; }

    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<GameUrl> GameUrls { get; set; } = [];
    public ICollection<ManualCheckPreset> ManualCheckPresets { get; set; } = [];
}

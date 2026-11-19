using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class ManualCheckPreset
{
    public const int NameMaxLength = 100;
    public const int DefaultListingLimit = 10;

    public long Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    public long GameId { get; set; }

    public Game Game { get; set; } = null!;

    public long? ItemGroupId { get; set; }

    public ItemGroup? ItemGroup { get; set; }

    [Required]
    [MaxLength(NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    public int ListingLimit { get; set; } = DefaultListingLimit;

    public ManualCheckPriceRangeModeEnum? PriceRangeMode { get; set; }

    public long? MinimumPriceMinorUnits { get; set; }

    public long? MaximumPriceMinorUnits { get; set; }

    public int? CooldownMinutes { get; set; }

    public int? CooldownSeconds { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<ManualCheckRun> Runs { get; set; } = [];
    public ICollection<ManualCheckCriterion> Criteria { get; set; } = [];
    public ICollection<ManualCheckPresetGameUrl> GameUrls { get; set; } = [];
}

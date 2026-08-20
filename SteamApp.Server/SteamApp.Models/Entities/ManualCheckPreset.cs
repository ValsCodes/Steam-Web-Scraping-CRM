using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("manual_check_preset")]
public sealed class ManualCheckPreset
{
    public const int NameMaxLength = 100;
    public const int DefaultListingLimit = 10;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("game_id")]
    [ForeignKey(nameof(Game))]
    public long GameId { get; set; }

    public Game Game { get; set; } = null!;

    [Required]
    [MaxLength(NameMaxLength)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("listing_limit")]
    public int ListingLimit { get; set; } = DefaultListingLimit;

    [Column("cooldown_minutes")]
    public int? CooldownMinutes { get; set; }

    [Column("cooldown_seconds")]
    public int? CooldownSeconds { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<ManualCheckRun> Runs { get; set; } = [];
    public ICollection<ManualCheckCriterion> Criteria { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("manual_check_preset")]
public sealed class ManualCheckPreset
{
    public const int NameMaxLength = 100;

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

    [Column("match_mode")]
    public ManualCheckMatchModeEnum MatchMode { get; set; }

    [Required]
    [Column("criteria_json")]
    public string CriteriaJson { get; set; } = "[]";

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<ManualCheckRun> Runs { get; set; } = [];
}

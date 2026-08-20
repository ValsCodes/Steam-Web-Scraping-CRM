using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("manual_check_run")]
public sealed class ManualCheckRun
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("manual_check_preset_id")]
    public long? ManualCheckPresetId { get; set; }

    public ManualCheckPreset? ManualCheckPreset { get; set; }

    [Column("game_id")]
    public long GameId { get; set; }

    public Game Game { get; set; } = null!;

    [Column("game_url_id")]
    public long GameUrlId { get; set; }

    public GameUrl GameUrl { get; set; } = null!;

    [Required]
    [MaxLength(ManualCheckPreset.NameMaxLength)]
    [Column("preset_name")]
    public string PresetName { get; set; } = string.Empty;

    [Required]
    [Column("setup_json")]
    public string SetupJson { get; set; } = "{}";

    [Column("results_json")]
    public string? ResultsJson { get; set; }

    [Column("total_products")]
    public int TotalProducts { get; set; }

    [Column("checked_products")]
    public int CheckedProducts { get; set; }

    [Column("matched_products")]
    public int MatchedProducts { get; set; }

    [Column("failed_products")]
    public int FailedProducts { get; set; }

    [Column("status")]
    public ManualCheckRunStatusEnum Status { get; set; } = ManualCheckRunStatusEnum.Queued;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("started_at_utc")]
    public DateTime? StartedAtUtc { get; set; }

    [Column("completed_at_utc")]
    public DateTime? CompletedAtUtc { get; set; }

    [Column("error_text")]
    public string? ErrorText { get; set; }

    [MaxLength(64)]
    [Column("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;
}

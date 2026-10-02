using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("manual_check_preset_game_url")]
public sealed class ManualCheckPresetGameUrl
{
    [Column("manual_check_preset_id")]
    public long ManualCheckPresetId { get; set; }

    public ManualCheckPreset ManualCheckPreset { get; set; } = null!;

    [Column("game_url_id")]
    public long GameUrlId { get; set; }

    public GameUrl GameUrl { get; set; } = null!;
}

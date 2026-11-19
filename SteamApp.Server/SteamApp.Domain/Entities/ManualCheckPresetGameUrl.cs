namespace SteamApp.Domain.Entities;

public sealed class ManualCheckPresetGameUrl
{
    public long ManualCheckPresetId { get; set; }

    public ManualCheckPreset ManualCheckPreset { get; set; } = null!;

    public long GameUrlId { get; set; }

    public GameUrl GameUrl { get; set; } = null!;
}

namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckRunRequestDto
{
    public long GameUrlId { get; set; }
    public long PresetId { get; set; }
    public bool BypassCache { get; set; }
}

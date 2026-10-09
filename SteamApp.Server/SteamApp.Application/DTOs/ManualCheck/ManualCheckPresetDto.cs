namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckPresetDto : ManualCheckPresetWriteDto
{
    public long Id { get; set; }
    public string? GameName { get; set; }
    public string? ItemGroupName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string Scope { get; set; } = "personal";
    public bool CanEdit { get; set; }
    public bool CanClone { get; set; }
    public bool CanRun { get; set; } = true;
}

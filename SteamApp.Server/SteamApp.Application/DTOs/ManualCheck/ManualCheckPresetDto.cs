namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckPresetDto : ManualCheckPresetWriteDto
{
    public long Id { get; set; }
    public string? GameName { get; set; }
    public string? ItemGroupName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

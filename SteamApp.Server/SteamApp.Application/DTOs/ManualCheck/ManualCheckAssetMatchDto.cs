namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckAssetMatchDto
{
    public string AppId { get; set; } = string.Empty;
    public string ContextId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public string MarketName { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public List<ManualCheckDescriptionMatchDto> Descriptions { get; set; } = [];
}

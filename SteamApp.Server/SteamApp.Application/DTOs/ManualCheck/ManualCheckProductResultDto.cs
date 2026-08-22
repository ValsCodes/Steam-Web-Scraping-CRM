namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckProductResultDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long GameUrlId { get; set; }
    public string GameUrlName { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public int? Rating { get; set; }
    public List<ManualCheckAssetMatchDto> MatchedAssets { get; set; } = [];
}

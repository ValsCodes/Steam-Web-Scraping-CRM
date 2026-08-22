namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckProductTraceDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public bool MatchEvaluated { get; set; }
    public bool Matched { get; set; }
    public int MatchedAssetCount { get; set; }
    public string? SteamApiResultJson { get; set; }
    public long? DurationMilliseconds { get; set; }
}

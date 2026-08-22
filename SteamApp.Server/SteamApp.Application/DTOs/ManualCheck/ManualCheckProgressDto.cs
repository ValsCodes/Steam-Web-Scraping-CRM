namespace SteamApp.Application.DTOs.ManualCheck;

public sealed class ManualCheckProgressDto
{
    public int TotalProducts { get; set; }
    public int CheckedProducts { get; set; }
    public int MatchedProducts { get; set; }
    public int FailedProducts { get; set; }
}

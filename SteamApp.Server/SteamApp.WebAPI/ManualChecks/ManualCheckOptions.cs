namespace SteamApp.WebAPI.ManualChecks;

public sealed class ManualCheckOptions
{
    public const string SectionName = "ManualChecks";
    public const string HttpClientName = "ManualChecks";

    public TimeSpan DelayBetweenRequests { get; set; } = TimeSpan.FromSeconds(3);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan RateLimitCooldown { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RateLimitJitter { get; set; } = TimeSpan.FromMinutes(2);
    public int MaxAttempts { get; set; } = 4;
    public List<string> UserAgents { get; set; } =
    [
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0"
    ];
}

namespace SteamApp.WebAPI.ManualChecks;

public sealed class ManualCheckOptions
{
    public const string SectionName = "ManualChecks";
    public const string HttpClientName = "ManualChecks";

    public TimeSpan DelayBetweenRequests { get; set; } = TimeSpan.FromSeconds(3);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public int MaxAttempts { get; set; } = 4;
}

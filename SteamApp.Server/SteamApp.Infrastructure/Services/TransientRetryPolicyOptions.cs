namespace SteamApp.Infrastructure.Services;

public sealed class TransientRetryPolicyOptions
{
    public const string SectionName = "TransientRetryPolicy";

    public int MaxAttempts { get; set; } = 3;
    public int BaseDelayMilliseconds { get; set; } = 200;
    public int MaxDelayMilliseconds { get; set; } = 2_000;
    public bool UseJitter { get; set; } = true;
}

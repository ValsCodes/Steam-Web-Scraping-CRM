namespace SteamApp.WebAPI.Security;

internal sealed class MfaFlow(string userId, string securityStamp, DateTimeOffset expiresAtUtc)
{
    public string UserId { get; } = userId;
    public string SecurityStamp { get; } = securityStamp;
    public DateTimeOffset ExpiresAtUtc { get; } = expiresAtUtc;
    public string? ExternalProvider { get; init; }
    public ExternalProfileNames? ProfileNames { get; init; }
    public string? SetupKey { get; init; }
    public SemaphoreSlim Gate { get; } = new(1);
    public bool Consumed { get; set; }
    public int AttemptsRemaining { get; set; } = 5;
}

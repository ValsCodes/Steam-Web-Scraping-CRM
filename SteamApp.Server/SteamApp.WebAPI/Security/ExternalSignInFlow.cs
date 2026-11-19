using Microsoft.AspNetCore.Identity;

namespace SteamApp.WebAPI.Security;

internal sealed class ExternalSignInFlow(string provider, string? userId, DateTimeOffset expiresAt)
{
    public string Provider { get; } = provider;
    public string? UserId { get; } = userId;
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
    public SemaphoreSlim Gate { get; } = new(1);
    public bool Started { get; set; }
    public bool Consumed { get; set; }
    public UserLoginInfo? Login { get; set; }
    public string? VerifiedEmail { get; set; }
    public ExternalProfileNames? ProfileNames { get; set; }
    public string? PendingEmail { get; set; }
    public byte[]? EmailCode { get; set; }
    public int AttemptsRemaining { get; set; } = 5;
}

namespace SteamApp.WebAPI.Security;

internal sealed record MfaState(string ProtectedKey, string[] RecoveryHashes, long LastUsedStep);

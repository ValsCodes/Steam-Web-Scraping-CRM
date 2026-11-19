using System.ComponentModel.DataAnnotations;

namespace SteamApp.Contracts.Requests;

public sealed record MfaVerificationRequest(
    [Required, StringLength(64, MinimumLength = 64)] string ChallengeId,
    [Required, RegularExpression("^(authenticator|recovery)$")] string Method,
    [Required, StringLength(64)] string Code);

using System.ComponentModel.DataAnnotations;

namespace SteamApp.Contracts.Requests;

public sealed record MfaEnableRequest(
    [Required, StringLength(64, MinimumLength = 64)] string SetupId,
    [Required, RegularExpression("^[0-9]{6}$")] string Code);

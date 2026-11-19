using System.ComponentModel.DataAnnotations;

namespace SteamApp.WebAPI.Contracts;

public sealed record MfaEnableRequest(
    [Required, StringLength(64, MinimumLength = 64)] string SetupId,
    [Required, RegularExpression("^[0-9]{6}$")] string Code);

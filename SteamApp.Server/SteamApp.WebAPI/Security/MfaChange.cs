using SteamApp.Infrastructure.Identity;

namespace SteamApp.WebAPI.Security;

internal sealed record MfaChange(ApplicationUser User, string[]? RecoveryCodes = null);

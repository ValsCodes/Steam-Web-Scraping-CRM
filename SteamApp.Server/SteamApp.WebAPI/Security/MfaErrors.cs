using SteamApp.Application.OperationResults;

namespace SteamApp.WebAPI.Security;

internal static class MfaErrors
{
    public static readonly Error InvalidCode = new(
        "mfa_invalid_code", "The verification code is incorrect or has already been used.", ErrorType.Validation);
    public static readonly Error InvalidSetupCode = new(
        "mfa_invalid_setup_code", "Enter a fresh code from the authenticator entry for this setup key. If it still fails, check that your device sets its time automatically.", ErrorType.Validation);
    public static readonly Error Expired = new(
        "mfa_expired", "Verification expired. Start again.", ErrorType.NotFound);
    public static readonly Error Conflict = new(
        "mfa_conflict", "The MFA state changed. Refresh and try again.", ErrorType.Conflict);
}

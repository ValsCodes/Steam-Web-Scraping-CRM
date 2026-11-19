using SteamApp.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.Application.Common.Results;
using SteamApp.Contracts.Authentication;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Security;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace SteamApp.WebAPI.Endpoints.Controllers;

public partial class AuthController
{
    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyMfa(
        MfaVerificationRequest request, [FromServices] MfaFlows flows, [FromServices] AuthenticatorMfa mfa)
    {
        MarkNoStore();
        if (flows.Get(request.ChallengeId) is not { SetupKey: null } challenge)
        {
            return MfaFailure(MfaErrors.Expired);
        }

        var ct = HttpContext.RequestAborted;
        await challenge.Gate.WaitAsync(ct);
        try
        {
            if (flows.Get(request.ChallengeId) != challenge)
            {
                return MfaFailure(MfaErrors.Expired);
            }

            var user = await userManager.FindByIdAsync(challenge.UserId);
            if (user == null || !await signInManager.CanSignInAsync(user))
            {
                flows.Consume(request.ChallengeId, challenge);
                return MfaFailure(MfaErrors.Expired);
            }

            var result = await mfa.RedeemAsync(challenge, request, ct);
            if (result.IsSuccess || result.Error == MfaErrors.Expired)
            {
                flows.Consume(request.ChallengeId, challenge);
            }

            return result.Value is { } completed
                ? Ok(await CreateMfaTokenResponseAsync(
                    completed.User, timeProvider.GetUtcNow(), challenge.ExternalProvider, challenge.ProfileNames))
                : MfaFailure(result.Error!);
        }
        finally
        {
            challenge.Gate.Release();
        }
    }

    [HttpGet("profile/mfa")]
    [EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public async Task<IActionResult> GetMfaStatus([FromServices] AuthenticatorMfa mfa)
    {
        MarkNoStore();
        if (await GetCurrentUserAsync() is not { } user)
        {
            return Unauthorized();
        }

        var count = user.TwoFactorEnabled
            ? await mfa.CountRecoveryCodesAsync(user.Id, HttpContext.RequestAborted)
            : 0;
        return Ok(new { enabled = user.TwoFactorEnabled, recoveryCodesRemaining = count });
    }

    [HttpPost("profile/mfa/setup")]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public async Task<IActionResult> SetupMfa([FromServices] MfaFlows flows)
    {
        MarkNoStore();
        if (await GetRecentMfaUserAsync() is not { } user)
        {
            return MfaReauthenticationRequired();
        }
        if (user.TwoFactorEnabled)
        {
            return MfaFailure(MfaErrors.Conflict);
        }

        if (flows.GetOrCreateSetup(user.Id, user.SecurityStamp!, GetAuthenticationTime()!.Value) is not { } setup)
        {
            return MfaUnavailable();
        }

        var (setupId, flow) = setup;
        var key = flow.SetupKey!;
        var label = Uri.EscapeDataString(user.Email ?? user.UserName ?? user.Id);
        return Ok(new
        {
            setupId,
            flow.ExpiresAtUtc,
            manualKey = key,
            otpAuthUri = $"otpauth://totp/SteamApp:{label}?secret={key}&issuer=SteamApp"
        });
    }

    [HttpPost("profile/mfa/enable")]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public async Task<IActionResult> EnableMfa(
        MfaEnableRequest request, [FromServices] MfaFlows flows, [FromServices] AuthenticatorMfa mfa)
    {
        MarkNoStore();
        if (await GetRecentMfaUserAsync() is not { } user)
        {
            return MfaReauthenticationRequired();
        }
        if (flows.Get(request.SetupId) is not { SetupKey: not null } setup ||
            setup.UserId != user.Id || setup.SecurityStamp != user.SecurityStamp)
        {
            return MfaFailure(MfaErrors.Expired);
        }

        var ct = HttpContext.RequestAborted;
        await setup.Gate.WaitAsync(ct);
        try
        {
            if (flows.Get(request.SetupId) != setup)
            {
                return MfaFailure(MfaErrors.Expired);
            }

            var result = await mfa.EnableAsync(setup, request.Code, ct);
            if (result.Error != MfaErrors.InvalidSetupCode || --setup.AttemptsRemaining == 0)
            {
                flows.Consume(request.SetupId, setup);
            }

            return result.Value is { } updated
                ? Ok(new { session = await ReplaceMfaSessionAsync(updated.User), updated.RecoveryCodes })
                : MfaFailure(setup.AttemptsRemaining == 0 ? MfaErrors.Expired : result.Error!);
        }
        finally
        {
            setup.Gate.Release();
        }
    }

    [HttpPost("profile/mfa/recovery-codes")]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public Task<IActionResult> ReplaceMfaRecoveryCodes([FromServices] AuthenticatorMfa mfa) =>
        UpdateMfaAsync(mfa, disable: false);

    [HttpDelete("profile/mfa")]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public Task<IActionResult> DisableMfa([FromServices] AuthenticatorMfa mfa) =>
        UpdateMfaAsync(mfa, disable: true);

    private async Task<IActionResult> UpdateMfaAsync(AuthenticatorMfa mfa, bool disable)
    {
        MarkNoStore();
        if (await GetRecentMfaUserAsync() is not { } user)
        {
            return MfaReauthenticationRequired();
        }

        var result = await mfa.UpdateAsync(user, disable, HttpContext.RequestAborted);
        if (result.Value is not { } updated)
        {
            return MfaFailure(result.Error!);
        }

        return disable
            ? Ok(await ReplaceMfaSessionAsync(updated.User))
            : Ok(new { updated.RecoveryCodes });
    }

    private async Task<SignInResult> CheckMfaPasswordAsync(ApplicationUser user, string password)
    {
        if (!await signInManager.CanSignInAsync(user))
        {
            return SignInResult.NotAllowed;
        }
        if (await userManager.IsLockedOutAsync(user))
        {
            return SignInResult.LockedOut;
        }
        if (await userManager.CheckPasswordAsync(user, password))
        {
            return SignInResult.Success;
        }

        var failure = await userManager.AccessFailedAsync(user);
        return failure.Succeeded && await userManager.IsLockedOutAsync(user)
            ? SignInResult.LockedOut
            : SignInResult.Failed;
    }

    private async Task<IActionResult> FinishPrimarySignInAsync(
        ApplicationUser user, string? provider = null, ExternalProfileNames? profileNames = null)
    {
        MarkNoStore();
        var now = timeProvider.GetUtcNow();
        if (!user.TwoFactorEnabled)
        {
            await FillMissingNamesAsync(user, profileNames, userManager);
            return Ok(await CreateUserTokenResponseAsync(user, now, provider));
        }

        var flows = HttpContext.RequestServices.GetRequiredService<MfaFlows>();
        var challenge = new MfaFlow(user.Id, user.SecurityStamp!, now.Add(MfaFlows.Lifetime))
        {
            ExternalProvider = provider,
            ProfileNames = profileNames
        };
        return flows.Add(challenge) is { } challengeId
            ? Accepted(new { requiresMfa = true, challengeId, challenge.ExpiresAtUtc })
            : MfaUnavailable();
    }

    private bool HasMfaProof() => User.HasClaim(UserSessionClaims.Mfa, "true");

    private async Task<ApplicationUser?> GetRecentMfaUserAsync() =>
        HasRecentUserSignIn() && await GetCurrentUserAsync() is { } user &&
        UserSessionClaims.Matches(User, user) && !await userManager.IsLockedOutAsync(user)
            ? user
            : null;

    private Task<AuthResponse> ReplaceMfaSessionAsync(ApplicationUser user) =>
        CreateMfaTokenResponseAsync(user, GetAuthenticationTime()!.Value, User.FindFirstValue(ExternalProviderClaim));

    private async Task<AuthResponse> CreateMfaTokenResponseAsync(
        ApplicationUser user, DateTimeOffset authenticationTime, string? provider, ExternalProfileNames? profileNames = null)
    {
        // The MFA transaction returned a fresh user snapshot. Use a new store scope for role provisioning.
        await using var scope = HttpContext.RequestServices.CreateAsyncScope();
        var sessionUsers = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (profileNames != null && await sessionUsers.FindByIdAsync(user.Id) is { } current &&
            current.SecurityStamp == user.SecurityStamp)
        {
            await FillMissingNamesAsync(current, profileNames, sessionUsers);
            user = current;
        }
        return await CreateUserTokenResponseAsync(user, authenticationTime, provider, user.TwoFactorEnabled, sessionUsers);
    }

    private void MarkNoStore()
    {
        if (HttpContext is { } context)
        {
            context.Response.Headers.CacheControl = "no-store";
        }
    }

    private ObjectResult MfaFailure(Error error) => Problem(statusCode: error.Type switch
    {
        ErrorType.Validation => 400,
        ErrorType.NotFound => 410,
        _ => 409
    }, title: error.Description);

    private ObjectResult MfaReauthenticationRequired() =>
        Problem(statusCode: 403, type: "urn:steamapp:mfa:recent-sign-in-required", title: "Sign in again to change MFA.");

    private ObjectResult MfaUnavailable() =>
        Problem(statusCode: 503, title: "Verification is temporarily unavailable. Try again.");
}

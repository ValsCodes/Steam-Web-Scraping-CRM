using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.OperationResults;
using SteamApp.Interfaces.Services;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Contracts;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;

namespace SteamApp.WebAPI.Controllers;

public partial class AuthController
{
    private static readonly TimeSpan RecentSignInWindow = TimeSpan.FromMinutes(5);

    [HttpGet("external/providers")]
    [EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
    [AllowAnonymous]
    public IActionResult GetExternalProviders([FromServices] ExternalAuthentication external) =>
        Ok(external.Providers);

    [HttpPost("external/{provider}/prepare")]
    [EnableCors(ExternalAuthentication.FrontendCors)]
    [AllowAnonymous]
    public async Task<IActionResult> PrepareExternalSignIn(
        string provider, bool link, [FromServices] ExternalAuthentication external)
    {
        if (!external.IsFrontendRequest(Request) || !external.Providers.Contains(provider, StringComparer.Ordinal))
        {
            return NotFound();
        }

        string? userId = null;
        if (link)
        {
            if (!HasRecentUserSignIn() || await GetCurrentUserAsync() is not { } user ||
                await userManager.IsLockedOutAsync(user))
            {
                return RecentSignInRequired();
            }
            userId = user.Id;
        }

        Response.Headers.CacheControl = "no-store";
        external.Prepare(HttpContext, provider, userId);
        return NoContent();
    }

    [HttpGet("external/start")]
    [AllowAnonymous]
    public async Task<IActionResult> ChallengeExternalProvider([FromServices] ExternalAuthentication external)
    {
        if (!Request.IsHttps || external.GetFlow(HttpContext) is not { } flow)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "no-store";
        await flow.Gate.WaitAsync(HttpContext.RequestAborted);
        try
        {
            if (flow.Started || flow.Consumed || !external.IsCurrent(flow))
            {
                return Redirect(external.ReturnUrl(false));
            }
            flow.Started = true;
            return Challenge(external.GetChallenge(HttpContext), flow.Provider);
        }
        finally
        {
            flow.Gate.Release();
        }
    }

    [HttpPost("external/complete")]
    [EnableCors(ExternalAuthentication.FrontendCors)]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteExternalSignIn(
        ExternalSignInCompletion request,
        [FromServices] ExternalAuthentication external,
        [FromServices] ExternalAccountRegistration registration,
        [FromServices] IEmailService emailService)
    {
        if (!external.IsFrontendRequest(Request) || external.GetFlow(HttpContext) is not { } flow)
        {
            return ExternalFlowExpired();
        }

        Response.Headers.CacheControl = "no-store";
        await flow.Gate.WaitAsync(HttpContext.RequestAborted);
        try
        {
            if (flow.Consumed || !external.IsCurrent(flow) || flow.Login is not { } login)
            {
                return ExternalFlowExpired();
            }

            var existing = await userManager.FindByLoginAsync(login.LoginProvider, login.ProviderKey);
            if (flow.UserId != null)
            {
                external.Consume(HttpContext, flow);
                return await CompleteExternalLinkAsync(flow.UserId, login, existing?.Id);
            }
            if (existing != null)
            {
                external.Consume(HttpContext, flow);
                if (await userManager.IsLockedOutAsync(existing) || !await signInManager.CanSignInAsync(existing))
                {
                    return Problem(statusCode: 403, title: "This account cannot currently sign in.");
                }

                return await FinishPrimarySignInAsync(existing, flow.Provider, flow.ProfileNames);
            }
            if (!RegistrationIsEnabled())
            {
                external.Consume(HttpContext, flow);
                return Problem(statusCode: 403, title: "SteamApp registration is closed.");
            }

            var verifiedEmail = flow.VerifiedEmail;
            if (verifiedEmail == null)
            {
                var verificationError = await VerifyExternalEmailAsync(flow, request, emailService);
                if (verificationError != null)
                {
                    if (flow.AttemptsRemaining == 0)
                    {
                        external.Consume(HttpContext, flow);
                    }
                    return verificationError;
                }
                verifiedEmail = flow.PendingEmail!;
            }

            external.Consume(HttpContext, flow);
            var result = await registration.RegisterAsync(login, verifiedEmail, HttpContext.RequestAborted);
            if (result.IsFailure)
            {
                return MapExternalError(result.Error!);
            }

            var registered = await userManager.FindByIdAsync(result.Value!);
            return registered != null
                ? await FinishPrimarySignInAsync(registered, flow.Provider, flow.ProfileNames)
                : ExternalFlowExpired();
        }
        finally
        {
            flow.Gate.Release();
        }
    }

    private static async Task FillMissingNamesAsync(
        ApplicationUser user, ExternalProfileNames? names, UserManager<ApplicationUser> users)
    {
        if (names == null)
        {
            return;
        }

        var firstName = user.FirstName;
        var lastName = user.LastName;
        user.FirstName = string.IsNullOrWhiteSpace(firstName) ? names.FirstName ?? firstName : firstName;
        user.LastName = string.IsNullOrWhiteSpace(lastName) ? names.LastName ?? lastName : lastName;
        if (user.FirstName == firstName && user.LastName == lastName)
        {
            return;
        }

        if (!(await users.UpdateAsync(user)).Succeeded)
        {
            user.FirstName = firstName;
            user.LastName = lastName;
        }
    }

    [HttpGet("profile/sign-in-methods")]
    [EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public async Task<IActionResult> GetSignInMethods()
    {
        if (await GetCurrentUserAsync() is not { } user)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            hasPassword = await userManager.HasPasswordAsync(user),
            providers = (await userManager.GetLoginsAsync(user)).Select(login => login.LoginProvider).Distinct()
        });
    }

    [HttpDelete("profile/external")]
    [Authorize(Policy = SecurityPolicies.UserSession)]
    public async Task<IActionResult> DeleteExternalProfile()
    {
        var provider = User.FindFirstValue(ExternalProviderClaim);
        if (provider == null || !HasRecentUserSignIn() || await GetCurrentUserAsync() is not { } user ||
            await userManager.HasPasswordAsync(user) || await userManager.IsLockedOutAsync(user) ||
            !(await userManager.GetLoginsAsync(user)).Any(login => login.LoginProvider == provider))
        {
            return RecentSignInRequired();
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded ? NoContent() : ValidationProblem(CreateModelState(result));
    }

    private async Task<IActionResult> CompleteExternalLinkAsync(string userId, UserLoginInfo login, string? ownerId)
    {
        if (!HasRecentUserSignIn() || await GetCurrentUserAsync() is not { } user ||
            user.Id != userId || await userManager.IsLockedOutAsync(user))
        {
            return RecentSignInRequired();
        }
        if (ownerId == userId)
        {
            return NoContent();
        }
        if (ownerId != null || (await userManager.GetLoginsAsync(user)).Any(x => x.LoginProvider == login.LoginProvider))
        {
            return ExternalLinkConflict();
        }

        try
        {
            var result = await userManager.AddLoginAsync(user, login);
            return result.Succeeded ? NoContent() : ExternalLinkConflict();
        }
        catch (DbUpdateException exception) when (ExternalAccountRegistration.IsUniqueConflict(exception))
        {
            return ExternalLinkConflict();
        }
    }

    private async Task<IActionResult?> VerifyExternalEmailAsync(
        ExternalSignInFlow flow, ExternalSignInCompletion request, IEmailService emailService)
    {
        if (request.Code is { } code && flow.EmailCode is { } expected)
        {
            if (CryptographicOperations.FixedTimeEquals(expected, Encoding.ASCII.GetBytes(code)))
            {
                return null;
            }

            flow.AttemptsRemaining--;
            return flow.AttemptsRemaining == 0
                ? ExternalFlowExpired()
                : Problem(statusCode: 400, title: "The verification code is incorrect.");
        }

        if (flow.EmailCode != null || request.Email is not { } email)
        {
            return Accepted(new { requiresEmail = true, codeSent = flow.EmailCode != null });
        }

        var generated = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var sent = await emailService.TrySendVerificationAsync(new EmailMessage(
            email, "Your SteamApp verification code",
            $"Your SteamApp verification code is {generated}. It expires when your sign-in attempt expires."),
            HttpContext.RequestAborted);
        if (!sent)
        {
            return Problem(statusCode: 503, title: "Verification email could not be sent. Please try again.");
        }

        flow.PendingEmail = email;
        flow.EmailCode = Encoding.ASCII.GetBytes(generated);
        return Accepted(new { requiresEmail = true, codeSent = true });
    }

    private bool HasRecentUserSignIn()
    {
        var now = timeProvider.GetUtcNow();
        return User.Identity?.IsAuthenticated == true &&
            User.HasClaim("scope", SecurityPolicies.UserScope) &&
            GetAuthenticationTime() is { } signedIn &&
            signedIn <= now.Add(MaximumClockSkew) && signedIn.Add(RecentSignInWindow) > now;
    }

    private IActionResult RecentSignInRequired() =>
        Problem(statusCode: 403, title: "Sign in to your SteamApp account again before continuing.");

    private IActionResult ExternalFlowExpired() =>
        Problem(statusCode: 410, title: "This sign-in attempt has expired. Start again.");

    private IActionResult ExternalLinkConflict() =>
        Problem(statusCode: 409, title: "This provider cannot be linked to your SteamApp account.");

    private IActionResult MapExternalError(Error error) =>
        Problem(statusCode: error.Type switch
        {
            ErrorType.Conflict => 409,
            ErrorType.Unavailable => 503,
            _ => 400
        }, title: error.Description);
}

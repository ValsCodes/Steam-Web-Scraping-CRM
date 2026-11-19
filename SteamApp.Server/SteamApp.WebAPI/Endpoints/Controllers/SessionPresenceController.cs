using SteamApp.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.Application.Abstractions.Services;
using SteamApp.Application.Common.Exceptions;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Endpoints.Controllers;

[ApiController]
[Route("api/session-presence")]
[Authorize(Policy = SecurityPolicies.UserSession)]
[EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
public sealed class SessionPresenceController(IAutomationAccessService automationAccessService) : ControllerBase
{
    [HttpPut("{tabId:guid}")]
    public async Task<IActionResult> Heartbeat(Guid tabId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await automationAccessService.UpsertPresenceAsync(userId, tabId, cancellationToken);
            return NoContent();
        }
        catch (AutomationAccessException exception)
        {
            return Problem(statusCode: exception.StatusCode, title: "Session presence request failed", detail: exception.Message);
        }
    }

    [HttpDelete("{tabId:guid}")]
    public async Task<IActionResult> Release(Guid tabId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await automationAccessService.ReleasePresenceAsync(userId, tabId, cancellationToken);
        return NoContent();
    }
}

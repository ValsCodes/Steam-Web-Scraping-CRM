using SteamApp.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.Application.Abstractions.Services;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Endpoints.Controllers;

[ApiController]
[Route("api/automation-usage")]
[Authorize(Policy = SecurityPolicies.UserSession)]
[EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
public sealed class AutomationUsageController(IAutomationAccessService automationAccessService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await automationAccessService.GetUsageAsync(userId, cancellationToken));
    }
}

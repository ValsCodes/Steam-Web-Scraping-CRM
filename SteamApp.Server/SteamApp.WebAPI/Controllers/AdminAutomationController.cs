using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.Application.DTOs.Automation;
using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = SecurityPolicies.AdminOnly)]
[EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
public sealed class AdminAutomationController(
    IAutomationAccessService automationAccessService,
    IManualCheckDataService manualCheckDataService,
    IAutomaticQueueDataService automaticQueueDataService) : ControllerBase
{
    [HttpGet("automation-policy")]
    public async Task<IActionResult> GetPolicy(CancellationToken cancellationToken) =>
        Ok(await automationAccessService.GetPolicyAsync(cancellationToken));

    [HttpPut("automation-policy")]
    public async Task<IActionResult> UpdatePolicy(
        [FromBody] AutomationPolicyUpdateDto input,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await automationAccessService.UpdatePolicyAsync(userId, input, cancellationToken));
        }
        catch (AutomationAccessException exception)
        {
            return Problem(statusCode: exception.StatusCode, title: "Automation policy update failed", detail: exception.Message);
        }
    }

    [HttpPost("automation-policy/reset-usage")]
    public async Task<IActionResult> ResetUsage(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return userId is null
            ? Unauthorized()
            : Ok(await automationAccessService.ResetUsageAsync(userId, cancellationToken));
    }

    [HttpGet("automation-usage")]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken) =>
        Ok(await automationAccessService.GetAllUsageAsync(cancellationToken));

    [HttpPost("manual-check-presets")]
    public async Task<IActionResult> CreateGlobalPreset(
        [FromBody] ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken) =>
        await HandleTemplateMutationAsync(async userId =>
        {
            var result = await manualCheckDataService.CreateGlobalPresetAsync(userId, input, cancellationToken);
            return Created($"/api/manual-checks/presets/{result.Id}", result);
        });

    [HttpPut("manual-check-presets/{id:long}")]
    public async Task<IActionResult> UpdateGlobalPreset(
        long id,
        [FromBody] ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken) =>
        await HandleTemplateMutationAsync(async userId =>
            Ok(await manualCheckDataService.UpdateGlobalPresetAsync(userId, id, input, cancellationToken)));

    [HttpDelete("manual-check-presets/{id:long}")]
    public async Task<IActionResult> DeleteGlobalPreset(long id, CancellationToken cancellationToken) =>
        await HandleTemplateMutationAsync(async _ =>
        {
            await manualCheckDataService.DeleteGlobalPresetAsync(id, cancellationToken);
            return NoContent();
        });

    [HttpPost("automatic-queue-templates")]
    public async Task<IActionResult> CreateGlobalQueueTemplate(
        [FromBody] AutomaticQueueWriteDto input,
        CancellationToken cancellationToken) =>
        await HandleTemplateMutationAsync(async userId =>
        {
            var result = await automaticQueueDataService.CreateGlobalDefinitionAsync(userId, input, cancellationToken);
            return Created($"/api/automatic-check-queues/{result.Id}", result);
        });

    [HttpPut("automatic-queue-templates/{id:long}")]
    public async Task<IActionResult> UpdateGlobalQueueTemplate(
        long id,
        [FromBody] AutomaticQueueWriteDto input,
        CancellationToken cancellationToken) =>
        await HandleTemplateMutationAsync(async userId =>
            Ok(await automaticQueueDataService.UpdateGlobalDefinitionAsync(id, userId, input, cancellationToken)));

    [HttpDelete("automatic-queue-templates/{id:long}")]
    public async Task<IActionResult> DeleteGlobalQueueTemplate(long id, CancellationToken cancellationToken) =>
        await HandleTemplateMutationAsync(async _ =>
        {
            await automaticQueueDataService.DeleteGlobalDefinitionAsync(id, cancellationToken);
            return NoContent();
        });

    private async Task<IActionResult> HandleTemplateMutationAsync(Func<string, Task<IActionResult>> action)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            return await action(userId);
        }
        catch (ManualCheckRequestException exception)
        {
            return Problem(statusCode: exception.StatusCode, title: "Global preset update failed", detail: exception.Message);
        }
        catch (AutomaticQueueRequestException exception)
        {
            return Problem(statusCode: exception.StatusCode, title: "Global queue template update failed", detail: exception.Message);
        }
    }
}

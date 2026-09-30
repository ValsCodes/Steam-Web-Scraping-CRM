using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/manual-checks")]
[Authorize(Policy = SecurityPolicies.ApiUser)]
[EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
public sealed class ManualChecksController(
    IManualCheckDataService dataService,
    IManualCheckQueue queue,
    ILogger<ManualChecksController> logger) : ControllerBase
{
    [HttpGet("condition-operators")]
    public async Task<IActionResult> GetConditionOperators(CancellationToken cancellationToken = default)
    {
        return Ok(await dataService.GetConditionOperatorsAsync(cancellationToken));
    }

    [HttpGet("presets")]
    public async Task<IActionResult> GetPresets(
        [FromQuery] long? gameId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            return Ok(await dataService.GetPresetsAsync(userId, gameId, cancellationToken));
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check preset lookup failed for game {GameId}.", gameId);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPost("presets")]
    public async Task<IActionResult> CreatePreset(
        [FromBody] ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            if (!await dataService.UserOwnsGameAsync(userId, input.GameId, cancellationToken)) return NotFound();
            var preset = await dataService.CreatePresetAsync(input, cancellationToken);
            return CreatedAtAction(nameof(GetPresets), new { gameId = preset.GameId }, preset);
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check preset creation failed for game {GameId}.", input.GameId);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPut("presets/{id:long}")]
    public async Task<IActionResult> UpdatePreset(
        long id,
        [FromBody] ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            if (!await dataService.UserOwnsPresetAsync(userId, id, cancellationToken) ||
                !await dataService.UserOwnsGameAsync(userId, input.GameId, cancellationToken)) return NotFound();
            return Ok(await dataService.UpdatePresetAsync(id, input, cancellationToken));
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check preset {PresetId} update failed.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpDelete("presets/{id:long}")]
    public async Task<IActionResult> DeletePreset(
        long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            if (!await dataService.UserOwnsPresetAsync(userId, id, cancellationToken)) return NotFound();
            await dataService.DeletePresetAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check preset {PresetId} deletion failed.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPost("runs")]
    [EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
    public async Task<IActionResult> CreateRun(
        [FromBody] ManualCheckRunRequestDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            var run = await dataService.CreateRunAsync(
                userId,
                input.GameUrlId,
                input.PresetId,
                input.PresetCombination,
                input.BypassCache,
                input.ProductIds,
                cancellationToken);
            await queue.EnqueueAsync(run.Id, cancellationToken);
            return AcceptedAtAction(nameof(GetRun), new { id = run.Id }, new ManualCheckRunAcceptedDto
            {
                RunId = run.Id,
                Run = run
            });
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(
                exception,
                "Manual-check run creation failed for source {GameUrlId} and preset {PresetId}.",
                input.GameUrlId,
                input.PresetId);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns(
        [FromQuery] long? gameId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            return Ok(await dataService.GetRunsAsync(userId, gameId, take, cancellationToken));
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check history lookup failed for game {GameId}.", gameId);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpGet("runs/{id:long}")]
    public async Task<IActionResult> GetRun(
        long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();
            if (!await dataService.UserOwnsRunAsync(userId, id, cancellationToken)) return NotFound();
            var run = await dataService.GetRunAsync(id, cancellationToken);
            return run is null ? NotFound() : Ok(run);
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check run {RunId} lookup failed.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPost("runs/{id:long}/cancel")]
    public async Task<IActionResult> CancelRun(
        long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureStandaloneRunAsync(id, cancellationToken);
            var run = await dataService.CancelAsync(id, cancellationToken);
            queue.TryCancel(id);
            logger.LogInformation(
                "Manual-check run {RunId} was canceled at {CheckedProducts}/{TotalProducts} products.",
                id,
                run.CheckedProducts,
                run.TotalProducts);
            return Ok(run);
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check run {RunId} cancellation failed.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPost("runs/{id:long}/pause")]
    public async Task<IActionResult> PauseRun(
        long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureStandaloneRunAsync(id, cancellationToken);
            var run = await dataService.PauseAsync(id, cancellationToken);
            queue.TryPause(id);
            logger.LogInformation(
                "Manual-check run {RunId} pause requested at {CheckedProducts}/{TotalProducts} products.",
                id,
                run.CheckedProducts,
                run.TotalProducts);
            return Ok(run);
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check run {RunId} pause failed.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPost("runs/{id:long}/continue")]
    [EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
    public async Task<IActionResult> ContinueRun(
        long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureStandaloneRunAsync(id, cancellationToken);
            var run = await dataService.ContinueAsync(id, cancellationToken);
            try
            {
                await queue.EnqueueAsync(run.Id, cancellationToken);
            }
            catch
            {
                await dataService.PauseAsync(id, CancellationToken.None);
                queue.TryPause(id);
                throw;
            }

            return AcceptedAtAction(nameof(GetRun), new { id = run.Id }, new ManualCheckRunAcceptedDto
            {
                RunId = run.Id,
                Run = run
            });
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check run {RunId} continuation failed.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    [HttpPost("runs/{id:long}/rerun")]
    [EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
    public async Task<IActionResult> Rerun(
        long id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureStandaloneRunAsync(id, cancellationToken);
            var run = await dataService.RerunAsync(id, cancellationToken);
            await queue.EnqueueAsync(run.Id, cancellationToken);
            return AcceptedAtAction(nameof(GetRun), new { id = run.Id }, new ManualCheckRunAcceptedDto
            {
                RunId = run.Id,
                Run = run
            });
        }
        catch (ManualCheckRequestException exception)
        {
            logger.LogWarning(exception, "Manual-check rerun failed for historical run {RunId}.", id);
            return Problem(
                statusCode: exception.StatusCode,
                title: "Manual check request failed",
                detail: exception.Message);
        }
    }

    private async Task EnsureStandaloneRunAsync(long id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            throw new ManualCheckRequestException(StatusCodes.Status401Unauthorized, "A signed-in user is required.");
        }
        if (!await dataService.UserOwnsRunAsync(userId, id, cancellationToken))
        {
            throw new ManualCheckRequestException(StatusCodes.Status404NotFound, "Run was not found.");
        }
        if (await dataService.IsQueueOwnedRunAsync(id, cancellationToken))
        {
            throw new ManualCheckRequestException(
                StatusCodes.Status409Conflict,
                "Queue-owned checks must be managed from their automatic queue run.");
        }
    }
}

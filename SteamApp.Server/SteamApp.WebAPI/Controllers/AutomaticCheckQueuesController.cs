using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Controllers;

[ApiController]
[Route("api/automatic-check-queues")]
[Authorize(Policy = SecurityPolicies.ApiUser)]
[EnableRateLimiting(SecurityPolicies.ApiRateLimit)]
public sealed class AutomaticCheckQueuesController(
    IAutomaticQueueDataService dataService,
    ILogger<AutomaticCheckQueuesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDefinitions(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return userId is null ? Unauthorized() : Ok(await dataService.GetDefinitionsAsync(userId, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetDefinition(long id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await dataService.GetDefinitionAsync(id, userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDefinition([FromBody] AutomaticQueueWriteDto input, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var result = await dataService.CreateDefinitionAsync(userId, input, cancellationToken);
            return CreatedAtAction(nameof(GetDefinition), new { id = result.Id }, result);
        }
        catch (AutomaticQueueRequestException exception)
        {
            return RequestProblem(exception);
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateDefinition(long id, [FromBody] AutomaticQueueWriteDto input, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            return Ok(await dataService.UpdateDefinitionAsync(id, userId, input, cancellationToken));
        }
        catch (AutomaticQueueRequestException exception)
        {
            return RequestProblem(exception);
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteDefinition(long id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            await dataService.DeleteDefinitionAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (AutomaticQueueRequestException exception)
        {
            return RequestProblem(exception);
        }
    }

    [HttpPost("{id:long}/runs")]
    [EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
    public async Task<IActionResult> StartRun(long id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var run = await dataService.StartRunAsync(id, userId, cancellationToken);
            return AcceptedAtAction(nameof(GetRun), new { id = run.Id }, new AutomaticQueueRunAcceptedDto
            {
                RunId = run.Id,
                Run = run
            });
        }
        catch (AutomaticQueueRequestException exception)
        {
            return RequestProblem(exception);
        }
    }

    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns([FromQuery] long? queueId, [FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        return userId is null ? Unauthorized() : Ok(await dataService.GetRunsAsync(userId, queueId, take, cancellationToken));
    }

    [HttpGet("runs/{id:long}")]
    public async Task<IActionResult> GetRun(long id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        var result = await dataService.GetRunAsync(id, userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("runs/{id:long}/pause")]
    public Task<IActionResult> PauseRun(long id, CancellationToken cancellationToken) =>
        ChangeRunAsync(id, dataService.PauseRunAsync, cancellationToken);

    [HttpPost("runs/{id:long}/continue")]
    [EnableRateLimiting(SecurityPolicies.ExpensiveApiRateLimit)]
    public Task<IActionResult> ContinueRun(long id, CancellationToken cancellationToken) =>
        ChangeRunAsync(id, dataService.ContinueRunAsync, cancellationToken);

    [HttpPost("runs/{id:long}/cancel")]
    public Task<IActionResult> CancelRun(long id, CancellationToken cancellationToken) =>
        ChangeRunAsync(id, dataService.CancelRunAsync, cancellationToken);

    private async Task<IActionResult> ChangeRunAsync(
        long id,
        Func<long, string, CancellationToken, Task<AutomaticQueueRunDto>> operation,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            return Ok(await operation(id, userId, cancellationToken));
        }
        catch (AutomaticQueueRequestException exception)
        {
            return RequestProblem(exception);
        }
    }

    private IActionResult RequestProblem(AutomaticQueueRequestException exception)
    {
        logger.LogWarning(exception, "Automatic queue request failed with status {StatusCode}.", exception.StatusCode);
        return Problem(statusCode: exception.StatusCode, title: "Automatic queue request failed", detail: exception.Message);
    }
}

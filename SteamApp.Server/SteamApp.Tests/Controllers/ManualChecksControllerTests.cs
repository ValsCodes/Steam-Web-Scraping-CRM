using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Enums;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Controllers;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Security;

namespace SteamApp.Tests.Controllers;

[TestFixture]
public sealed class ManualChecksControllerTests
{
    [Test]
    public async Task GetConditionOperators_ReturnsSeededLookupValues()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.GetConditionOperatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ManualCheckConditionOperatorDto { Id = 1, Name = "AND" }]);
        var controller = Controller(data, queue);

        var result = await controller.GetConditionOperators();

        var values = (result as OkObjectResult)?.Value as IReadOnlyList<ManualCheckConditionOperatorDto>;
        Assert.That(values?.Single().Name, Is.EqualTo("AND"));
    }

    [Test]
    public async Task CreateRunReturnsAcceptedAndEnqueuesTheNewSharedRun()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.CreateRunAsync(
                8,
                4,
                true,
                It.Is<IReadOnlyList<long>?>(ids => ids != null && ids.SequenceEqual(new long[] { 2, 3 })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Summary(55));
        queue.Setup(x => x.EnqueueAsync(55, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        var controller = Controller(data, queue);

        var result = await controller.CreateRun(
            new ManualCheckRunRequestDto
            {
                GameUrlId = 8,
                PresetId = 4,
                BypassCache = true,
                ProductIds = [2, 3]
            });

        var accepted = result as AcceptedAtActionResult;
        Assert.Multiple(() =>
        {
            Assert.That(accepted, Is.Not.Null);
            Assert.That((accepted!.Value as ManualCheckRunAcceptedDto)?.RunId, Is.EqualTo(55));
        });
        queue.Verify(x => x.EnqueueAsync(55, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PresetCrudDoesNotRequireOrPassAUserIdentifier()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        var input = new ManualCheckPresetWriteDto
        {
            GameId = 440,
            Name = "Shared",
            Criteria =
            [
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = null,
                    ValueContains = "Sheen"
                }
            ]
        };
        data.Setup(x => x.CreatePresetAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckPresetDto { Id = 9, GameId = 440, Name = "Shared" });
        data.Setup(x => x.GetPresetsAsync(440, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ManualCheckPresetDto { Id = 9, GameId = 440, Name = "Shared" }]);
        var controller = Controller(data, queue);

        var create = await controller.CreatePreset(input);
        var list = await controller.GetPresets(440);

        Assert.Multiple(() =>
        {
            Assert.That(create, Is.TypeOf<CreatedAtActionResult>());
            Assert.That((list as OkObjectResult)?.Value, Is.AssignableTo<IReadOnlyList<ManualCheckPresetDto>>());
            Assert.That(typeof(ManualCheckPresetWriteDto).GetProperty("UserId"), Is.Null);
        });
    }

    [Test]
    public async Task DetailAndRerunUseTheHistoryRunId()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.GetRunAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckRunDetailDto { Id = 12, PresetName = "Historical" });
        data.Setup(x => x.RerunAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Summary(13));
        queue.Setup(x => x.EnqueueAsync(13, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        var controller = Controller(data, queue);

        var detail = await controller.GetRun(12);
        var rerun = await controller.Rerun(12);

        Assert.Multiple(() =>
        {
            Assert.That(detail, Is.TypeOf<OkObjectResult>());
            Assert.That(rerun, Is.TypeOf<AcceptedAtActionResult>());
        });
        queue.Verify(x => x.EnqueueAsync(13, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CancelMarksTheRunCanceledAndSignalsItsQueueToken()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.CancelAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckRunDetailDto
            {
                Id = 12,
                PresetName = "Historical",
                Status = ManualCheckRunStatusEnum.Canceled,
                CheckedProducts = 2,
                TotalProducts = 5
            });
        queue.Setup(x => x.TryCancel(12)).Returns(true);
        var controller = Controller(data, queue);

        var result = await controller.CancelRun(12);

        Assert.That((result as OkObjectResult)?.Value, Is.TypeOf<ManualCheckRunDetailDto>());
        queue.Verify(x => x.TryCancel(12), Times.Once);
    }

    [Test]
    public async Task PauseAndContinue_UseTheSameRunAndSignalTheQueue()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.PauseAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckRunDetailDto
            {
                Id = 12,
                PresetName = "Historical",
                Status = ManualCheckRunStatusEnum.PauseRequested,
                CheckedProducts = 2,
                TotalProducts = 5
            });
        data.Setup(x => x.ContinueAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Summary(12));
        queue.Setup(x => x.TryPause(12)).Returns(true);
        queue.Setup(x => x.EnqueueAsync(12, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        var controller = Controller(data, queue);

        var pauseResult = await controller.PauseRun(12);
        var continueResult = await controller.ContinueRun(12);

        Assert.Multiple(() =>
        {
            Assert.That(pauseResult, Is.TypeOf<OkObjectResult>());
            Assert.That(continueResult, Is.TypeOf<AcceptedAtActionResult>());
            Assert.That(
                ((continueResult as AcceptedAtActionResult)!.Value as ManualCheckRunAcceptedDto)!.RunId,
                Is.EqualTo(12));
        });
        queue.Verify(x => x.TryPause(12), Times.Once);
        queue.Verify(x => x.EnqueueAsync(12, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void ContinueRun_KeepsTheExpensiveApiRateLimit()
    {
        var rateLimit = typeof(ManualChecksController)
            .GetMethod(nameof(ManualChecksController.ContinueRun))!
            .GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: true)
            .Cast<EnableRateLimitingAttribute>()
            .Single();

        Assert.That(rateLimit.PolicyName, Is.EqualTo(SecurityPolicies.ExpensiveApiRateLimit));
    }

    [Test]
    public void ControllerKeepsSharedEndpointsBehindTheAuthenticatedApiPolicy()
    {
        var authorize = typeof(ManualChecksController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.That(authorize.Policy, Is.EqualTo(SecurityPolicies.ApiUser));
    }

    [Test]
    public async Task EndpointHandlesRequestErrorInlineWithoutHandleAsyncWrapper()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        var input = new ManualCheckPresetWriteDto { GameId = 440, Name = "Duplicate" };
        data.Setup(x => x.CreatePresetAsync(input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ManualCheckRequestException(409, "Preset already exists."));
        var controller = Controller(data, queue);

        var result = await controller.CreatePreset(input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<ObjectResult>());
            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(409));
            Assert.That(
                typeof(ManualChecksController).GetMethod(
                    "HandleAsync",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic),
                Is.Null);
        });
    }

    private static ManualChecksController Controller(
        Mock<IManualCheckDataService> data,
        Mock<IManualCheckQueue> queue)
    {
        return new ManualChecksController(
            data.Object,
            queue.Object,
            NullLogger<ManualChecksController>.Instance);
    }

    private static ManualCheckRunSummaryDto Summary(long id)
    {
        return new ManualCheckRunSummaryDto
        {
            Id = id,
            PresetName = "Shared",
            GameId = 440,
            GameUrlId = 8,
            Status = ManualCheckRunStatusEnum.Queued,
            CorrelationId = "test"
        };
    }
}

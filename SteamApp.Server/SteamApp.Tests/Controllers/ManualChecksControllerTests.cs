using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
                "test-user",
                8,
                4,
                null,
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
    public async Task CreateRun_PresetCombination_PassesAuthenticatedUserAndRecipeToTheService()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        var combination = new ManualCheckPresetCombinationWriteDto
        {
            ListingLimit = 20,
            Terms =
            [
                new ManualCheckPresetCombinationTermWriteDto { PresetId = 4 },
                new ManualCheckPresetCombinationTermWriteDto
                {
                    PresetId = 5,
                    Operator = ManualCheckPresetCombinationOperatorEnum.And
                }
            ]
        };
        data.Setup(x => x.CreateRunAsync(
                "test-user",
                8,
                null,
                combination,
                false,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Summary(56));
        queue.Setup(x => x.EnqueueAsync(56, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        var controller = Controller(data, queue);

        var result = await controller.CreateRun(new ManualCheckRunRequestDto
        {
            GameUrlId = 8,
            PresetCombination = combination
        });

        Assert.That(result, Is.TypeOf<AcceptedAtActionResult>());
        data.Verify(x => x.CreateRunAsync(
            "test-user",
            8,
            null,
            combination,
            false,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
        queue.Verify(x => x.EnqueueAsync(56, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PresetCrudScopesOwnershipThroughTheAuthenticatedGame()
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
        data.Setup(x => x.CreatePresetAsync("test-user", input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckPresetDto { Id = 9, GameId = 440, Name = "Shared" });
        data.Setup(x => x.GetPresetsAsync("test-user", 440, null, It.IsAny<CancellationToken>()))
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
        data.Verify(x => x.GetPresetsAsync("test-user", 440, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeletePreset_AuthenticatedOwner_PassesUserScopeToCascade()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.UserOwnsPresetAsync("test-user", 9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = Controller(data, queue);

        var result = await controller.DeletePreset(9);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        data.Verify(x => x.DeletePresetAsync("test-user", 9, It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task UpdateListingLimit_UpdatesOnlyAnOwnedStandaloneRun()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        data.Setup(x => x.UpdateListingLimitAsync(12, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckRunDetailDto
            {
                Id = 12,
                PresetName = "Historical",
                Status = ManualCheckRunStatusEnum.Paused,
                Setup = new ManualCheckSetupDto { ListingLimit = 25 }
            });
        var controller = Controller(data, queue);

        var result = await controller.UpdateListingLimit(
            12,
            new ManualCheckListingLimitUpdateDto { ListingLimit = 25 });

        Assert.That((result as OkObjectResult)?.Value, Is.TypeOf<ManualCheckRunDetailDto>());
        data.Verify(x => x.UserOwnsRunAsync("test-user", 12, It.IsAny<CancellationToken>()), Times.Once);
        data.Verify(x => x.UpdateListingLimitAsync(12, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateListingLimit_RejectsAQueueOwnedRun()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        var controller = Controller(data, queue);
        data.Setup(x => x.IsQueueOwnedRunAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await controller.UpdateListingLimit(
            12,
            new ManualCheckListingLimitUpdateDto { ListingLimit = 25 });

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<ObjectResult>());
            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(409));
        });
        data.Verify(
            x => x.UpdateListingLimitAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task QueueOwnedRunRejectsDirectControlOperations()
    {
        var data = new Mock<IManualCheckDataService>();
        var queue = new Mock<IManualCheckQueue>();
        var controller = Controller(data, queue);
        data.Setup(x => x.IsQueueOwnedRunAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await controller.CancelRun(12);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<ObjectResult>());
            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(409));
        });
        data.Verify(x => x.CancelAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        queue.Verify(x => x.TryCancel(It.IsAny<long>()), Times.Never);
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
        data.Setup(x => x.CreatePresetAsync("test-user", input, It.IsAny<CancellationToken>()))
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
        data.Setup(x => x.UserOwnsGameAsync("test-user", It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        data.Setup(x => x.UserOwnsGameUrlAsync("test-user", It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        data.Setup(x => x.UserOwnsPresetAsync("test-user", It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        data.Setup(x => x.UserOwnsRunAsync("test-user", It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        data.Setup(x => x.IsQueueOwnedRunAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var controller = new ManualChecksController(
            data.Object,
            queue.Object,
            NullLogger<ManualChecksController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "test-user")],
                    "Test"))
            }
        };
        return controller;
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

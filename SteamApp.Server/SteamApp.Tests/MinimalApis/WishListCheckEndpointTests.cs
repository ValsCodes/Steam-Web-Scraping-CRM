using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Contracts.Pagination;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class WishListCheckEndpointTests
{
    [Test]
    public async Task CheckWishListItem_OwnedAlert_ReturnsFreshTrace()
    {
        var trace = CreateTrace();
        var execution = new Mock<IWishlistCheckExecutionService>();
        execution
            .Setup(x => x.ExecuteManualAsync(
                1,
                TestDb.TestUserId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WishlistCheckExecutionOutcome>.Success(
                new WishlistCheckExecutionOutcome(trace, CheckError: null)));
        await using var app = await MinimalApiTestApp.CreateAsync(
            wishlistCheckExecution: execution.Object);

        var response = await app.Client.PostAsJsonAsync("/api/wish-list/1/checks", new { });
        var body = await response.Content.ReadFromJsonAsync<WishListCheckHistoryDto>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body?.Id, Is.EqualTo(10));
            Assert.That(body?.CurrentPrice, Is.EqualTo(4.5));
        });
    }

    [Test]
    public async Task CheckWishListItem_ExpectedSteamFailure_ReturnsRedactedProblemWithTraceIds()
    {
        var trace = CreateTrace();
        trace.Status = "Failed";
        trace.ErrorCode = "WishlistCheck.PriceUnavailable";
        trace.ErrorText = "Steam did not provide a readable price for this game.";
        var error = new Error(
            trace.ErrorCode,
            trace.ErrorText,
            ErrorType.Unavailable);
        var execution = new Mock<IWishlistCheckExecutionService>();
        execution
            .Setup(x => x.ExecuteManualAsync(
                1,
                TestDb.TestUserId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WishlistCheckExecutionOutcome>.Success(
                new WishlistCheckExecutionOutcome(trace, error)));
        await using var app = await MinimalApiTestApp.CreateAsync(
            wishlistCheckExecution: execution.Object);

        var response = await app.Client.PostAsJsonAsync("/api/wish-list/1/checks", new { });
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(problem, Is.Not.Null);
            Assert.That(problem?.Detail, Is.EqualTo(trace.ErrorText));
            Assert.That(Convert.ToString(problem!.Extensions["checkHistoryId"]), Does.Contain("10"));
            Assert.That(Convert.ToString(problem.Extensions["correlationId"]), Does.Contain("trace-10"));
            Assert.That(problem?.Detail, Does.Not.Contain("stack"));
        });
    }

    [Test]
    public async Task GetWishListCheckHistory_ReturnsPagedHistory()
    {
        var page = new WishListCheckHistoryPageDto
        {
            Items = [CreateTrace()],
            PageNumber = 2,
            PageSize = 25,
            TotalCount = 30,
            TotalPages = 2
        };
        var execution = new Mock<IWishlistCheckExecutionService>();
        execution
            .Setup(x => x.GetHistoryAsync(
                1,
                TestDb.TestUserId,
                It.Is<WishListCheckHistoryPageQuery>(q => q.PageNumber == 2 && q.PageSize == 25),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WishListCheckHistoryPageDto>.Success(page));
        await using var app = await MinimalApiTestApp.CreateAsync(
            wishlistCheckExecution: execution.Object);

        var response = await app.Client.GetAsync(
            "/api/wish-list/1/checks?pageNumber=2&pageSize=25");
        var body = await response.Content.ReadFromJsonAsync<WishListCheckHistoryPageDto>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body?.Items, Has.Count.EqualTo(1));
            Assert.That(body?.PageNumber, Is.EqualTo(2));
            Assert.That(body?.TotalCount, Is.EqualTo(30));
        });
    }

    private static WishListCheckHistoryDto CreateTrace()
    {
        return new WishListCheckHistoryDto
        {
            Id = 10,
            WishListId = 1,
            GameName = "Alpha Game",
            Source = "Manual",
            Status = "Succeeded",
            TargetPrice = 5,
            CurrentPrice = 4.5,
            IsPriceReached = true,
            RequestedAtUtc = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc),
            StartedAtUtc = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc),
            CompletedAtUtc = new DateTime(2026, 10, 2, 12, 0, 1, DateTimeKind.Utc),
            DurationMilliseconds = 1000,
            CorrelationId = "trace-10"
        };
    }
}

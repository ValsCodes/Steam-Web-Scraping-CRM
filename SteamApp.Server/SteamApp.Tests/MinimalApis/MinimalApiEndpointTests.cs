using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.FeedbackRequest;
using SteamApp.Application.DTOs.Game;
using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Application.DTOs.Pixel;
using SteamApp.Application.DTOs.Product;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class MinimalApiEndpointTests
{
    [TestCase("/api/games/")]
    [TestCase("/api/game-urls/")]
    [TestCase("/api/scraping-modes/")]
    [TestCase("/api/products/")]
    [TestCase("/api/pixels/")]
    [TestCase("/api/watch-list/")]
    [TestCase("/api/wish-list/")]
    [TestCase("/api/feedback-requests/")]
    [TestCase("/api/feedback-requests/1/history")]
    [TestCase("/api/game-url-products/")]
    [TestCase("/api/tags/")]
    [TestCase("/api/product-tags/")]
    [TestCase("/api/game-url-pixels/")]
    public async Task GetAllEndpoints_ReturnOk(string path)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.GetAsync(path);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), path);
    }

    [TestCase("/api/games/1")]
    [TestCase("/api/game-urls/1")]
    [TestCase("/api/scraping-modes/1")]
    [TestCase("/api/products/1")]
    [TestCase("/api/pixels/1")]
    [TestCase("/api/watch-list/1")]
    [TestCase("/api/wish-list/1")]
    [TestCase("/api/feedback-requests/1")]
    [TestCase("/api/feedback-requests/reference/FB-000001")]
    [TestCase("/api/tags/1")]
    public async Task GetByIdEndpoints_ReturnOkWhenFound(string path)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.GetAsync(path);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), path);
    }

    [TestCase("/api/games/404")]
    [TestCase("/api/game-urls/404")]
    [TestCase("/api/scraping-modes/404")]
    [TestCase("/api/products/404")]
    [TestCase("/api/pixels/404")]
    [TestCase("/api/watch-list/404")]
    [TestCase("/api/wish-list/404")]
    [TestCase("/api/feedback-requests/404")]
    [TestCase("/api/feedback-requests/reference/FB-000404")]
    [TestCase("/api/tags/404")]
    public async Task GetByIdEndpoints_ReturnNotFoundWhenMissing(string path)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.GetAsync(path);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), path);
    }

    [Test]
    public async Task PostGame_ReturnsCreatedWithLocation()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PostAsJsonAsync("/api/games/", new GameCreateDto
        {
            Name = "Created Game",
            BaseUrl = "https://steam.example/created",
            PageUrl = "https://steam.example/app/3",
            InternalId = 30,
            IsActive = true
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var created = db.Games.SingleOrDefault(x => x.Name == "Created Game");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location?.ToString(), Does.StartWith("/api/games/"));
            Assert.That(created?.UserId, Is.EqualTo(TestDb.TestUserId));
        });
    }

    [Test]
    public async Task GetGameById_ReturnsNotFoundForAnotherUsersRecord()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.Games.Add(new Game
            {
                Id = 99,
                Name = "Other User Game",
                IsActive = true,
                UserId = "other-user"
            });
            db.SaveChanges();
        });

        var response = await app.Client.GetAsync("/api/games/99");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PostWishList_RejectsInvalidGameId()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PostAsJsonAsync("/api/wish-list/", new WishListCreateDto
        {
            GameId = 404,
            Name = "Invalid",
            Price = 1,
            IsActive = true
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task FeedbackRequests_ReturnOnlyCurrentUsersRequests()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.FeedbackRequests.Add(new FeedbackRequest
            {
                Id = 99,
                Type = FeedbackRequestTypeEnum.Bug,
                Title = "Other user bug",
                Description = "This belongs to a different user.",
                Status = FeedbackRequestStatusEnum.Active,
                CreatedAtUtc = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                StatusChangedAtUtc = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                UserId = "other-user"
            });
            db.SaveChanges();
        });

        var response = await app.Client.GetAsync("/api/feedback-requests/");
        var items = await app.ReadJsonAsync<List<FeedbackRequestDto>>(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(items.Select(x => x.Id), Is.EquivalentTo(new[] { 1L, 2L }));
            Assert.That(items.Single(x => x.Id == 1).ReferenceId, Is.EqualTo("FB-000001"));
            Assert.That(items.Any(x => x.Id == 99), Is.False);
        });
    }

    [Test]
    public async Task PostFeedbackRequest_DefaultsToActiveAndCurrentUser()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PostAsJsonAsync("/api/feedback-requests/", new FeedbackRequestCreateDto
        {
            Type = FeedbackRequestTypeEnum.Bug,
            Title = "  Broken status menu  ",
            Description = "  The status menu does not update after save.  ",
            Area = "  Feedback  "
        });
        var dto = await app.ReadJsonAsync<FeedbackRequestDto>(response);

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var created = db.FeedbackRequests.SingleOrDefault(x => x.Title == "Broken status menu");
        var history = created is null
            ? null
            : db.FeedbackRequestHistories.SingleOrDefault(x => x.FeedbackRequestId == created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location?.ToString(), Does.StartWith("/api/feedback-requests/"));
            Assert.That(dto.ReferenceId, Does.Match("^FB-\\d{6}$"));
            Assert.That(created, Is.Not.Null);
            Assert.That(created?.UserId, Is.EqualTo(TestDb.TestUserId));
            Assert.That(created?.Type, Is.EqualTo(FeedbackRequestTypeEnum.Bug));
            Assert.That(created?.Status, Is.EqualTo(FeedbackRequestStatusEnum.Active));
            Assert.That(created?.Description, Is.EqualTo("The status menu does not update after save."));
            Assert.That(created?.Area, Is.EqualTo("Feedback"));
            Assert.That(created?.CreatedAtUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(created?.UpdatedAtUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(created?.StatusChangedAtUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(history?.Action, Is.EqualTo(FeedbackRequestHistoryActionEnum.Created));
            Assert.That(history?.PreviousTitle, Is.Null);
            Assert.That(history?.NewTitle, Is.EqualTo("Broken status menu"));
            Assert.That(history?.NewDescription, Is.EqualTo("The status menu does not update after save."));
            Assert.That(history?.NewStatus, Is.EqualTo(FeedbackRequestStatusEnum.Active));
            Assert.That(history?.UserId, Is.EqualTo(TestDb.TestUserId));
        });
    }

    [Test]
    public async Task PutFeedbackRequest_UpdatesFieldsAndStatusTimestamp()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PutAsJsonAsync("/api/feedback-requests/1", new FeedbackRequestUpdateDto
        {
            Type = FeedbackRequestTypeEnum.Bug,
            Title = "Updated title",
            Description = "Updated description",
            Area = "Updated area",
            Status = FeedbackRequestStatusEnum.Closed
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await db.FeedbackRequests.FindAsync(1L);
        var history = db.FeedbackRequestHistories.SingleOrDefault(x => x.FeedbackRequestId == 1L);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(updated?.Type, Is.EqualTo(FeedbackRequestTypeEnum.Bug));
            Assert.That(updated?.Title, Is.EqualTo("Updated title"));
            Assert.That(updated?.Description, Is.EqualTo("Updated description"));
            Assert.That(updated?.Area, Is.EqualTo("Updated area"));
            Assert.That(updated?.Status, Is.EqualTo(FeedbackRequestStatusEnum.Closed));
            Assert.That(updated?.UpdatedAtUtc, Is.GreaterThan(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(updated?.StatusChangedAtUtc, Is.GreaterThan(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(history?.Action, Is.EqualTo(FeedbackRequestHistoryActionEnum.Updated));
            Assert.That(history?.PreviousTitle, Is.EqualTo("Improve filters"));
            Assert.That(history?.NewTitle, Is.EqualTo("Updated title"));
            Assert.That(history?.PreviousDescription, Is.EqualTo("Please make the search filters easier to scan."));
            Assert.That(history?.NewDescription, Is.EqualTo("Updated description"));
            Assert.That(history?.PreviousStatus, Is.EqualTo(FeedbackRequestStatusEnum.Active));
            Assert.That(history?.NewStatus, Is.EqualTo(FeedbackRequestStatusEnum.Closed));
        });
    }

    [Test]
    public async Task PatchFeedbackRequestStatus_UpdatesOnlyStatus()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PatchAsJsonAsync("/api/feedback-requests/1/status", new
        {
            status = FeedbackRequestStatusEnum.Processed
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await db.FeedbackRequests.FindAsync(1L);
        var history = db.FeedbackRequestHistories.SingleOrDefault(x => x.FeedbackRequestId == 1L);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(updated?.Title, Is.EqualTo("Improve filters"));
            Assert.That(updated?.Status, Is.EqualTo(FeedbackRequestStatusEnum.Processed));
            Assert.That(updated?.UpdatedAtUtc, Is.GreaterThan(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(updated?.StatusChangedAtUtc, Is.GreaterThan(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(history?.Action, Is.EqualTo(FeedbackRequestHistoryActionEnum.StatusChanged));
            Assert.That(history?.PreviousStatus, Is.EqualTo(FeedbackRequestStatusEnum.Active));
            Assert.That(history?.NewStatus, Is.EqualTo(FeedbackRequestStatusEnum.Processed));
            Assert.That(history?.PreviousTitle, Is.Null);
        });
    }

    [Test]
    public async Task PatchFeedbackRequestStatus_DoesNotCreateHistoryWhenStatusDoesNotChange()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PatchAsJsonAsync("/api/feedback-requests/2/status", new
        {
            status = FeedbackRequestStatusEnum.Processed
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var historyCount = db.FeedbackRequestHistories.Count(x => x.FeedbackRequestId == 2L);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(historyCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task FeedbackRequestHistoryEndpoint_ReturnsOwnedHistoryAndClosedTransition()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var patch = await app.Client.PatchAsJsonAsync("/api/feedback-requests/1/status", new
        {
            status = FeedbackRequestStatusEnum.Closed
        });
        var historyResponse = await app.Client.GetAsync("/api/feedback-requests/1/history");
        var history = await app.ReadJsonAsync<List<FeedbackRequestHistoryDto>>(historyResponse);

        Assert.Multiple(() =>
        {
            Assert.That(patch.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(historyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(history, Has.Count.EqualTo(1));
            Assert.That(history[0].Action, Is.EqualTo(FeedbackRequestHistoryActionEnum.StatusChanged));
            Assert.That(history[0].PreviousStatus, Is.EqualTo(FeedbackRequestStatusEnum.Active));
            Assert.That(history[0].NewStatus, Is.EqualTo(FeedbackRequestStatusEnum.Closed));
        });
    }

    [Test]
    public async Task FeedbackRequestHistoryEndpoint_RequiresAuthAndEnforcesOwnership()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.FeedbackRequests.Add(new FeedbackRequest
            {
                Id = 99,
                Type = FeedbackRequestTypeEnum.Bug,
                Title = "Other user bug",
                Description = "This belongs to a different user.",
                Status = FeedbackRequestStatusEnum.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                StatusChangedAtUtc = DateTime.UtcNow,
                UserId = "other-user"
            });
            db.FeedbackRequestHistories.Add(new FeedbackRequestHistory
            {
                Id = 99,
                FeedbackRequestId = 99,
                Action = FeedbackRequestHistoryActionEnum.Created,
                CreatedAtUtc = DateTime.UtcNow,
                NewTitle = "Other user bug",
                UserId = "other-user"
            });
            db.SaveChanges();
        });

        var noAuthClient = app.CreateClientWithoutAuth();
        var noAuth = await noAuthClient.GetAsync("/api/feedback-requests/1/history");
        var otherUserHistory = await app.Client.GetAsync("/api/feedback-requests/99/history");
        var otherUserReference = await app.Client.GetAsync("/api/feedback-requests/reference/FB-000099");
        var invalidReference = await app.Client.GetAsync("/api/feedback-requests/reference/not-a-ticket");

        Assert.Multiple(() =>
        {
            Assert.That(noAuth.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(otherUserHistory.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(otherUserReference.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(invalidReference.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task FeedbackRequestEndpoints_RejectInvalidEnumsAndMissingOwnership()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.FeedbackRequests.Add(new FeedbackRequest
            {
                Id = 99,
                Type = FeedbackRequestTypeEnum.Bug,
                Title = "Other user bug",
                Description = "This belongs to a different user.",
                Status = FeedbackRequestStatusEnum.Active,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                StatusChangedAtUtc = DateTime.UtcNow,
                UserId = "other-user"
            });
            db.SaveChanges();
        });

        var invalidType = await app.Client.PostAsJsonAsync("/api/feedback-requests/", new
        {
            type = 99,
            title = "Invalid",
            description = "Invalid enum"
        });
        var invalidStatus = await app.Client.PatchAsJsonAsync("/api/feedback-requests/1/status", new
        {
            status = 99
        });
        var otherUserGet = await app.Client.GetAsync("/api/feedback-requests/99");
        var otherUserPatch = await app.Client.PatchAsJsonAsync("/api/feedback-requests/99/status", new
        {
            status = FeedbackRequestStatusEnum.Closed
        });

        Assert.Multiple(() =>
        {
            Assert.That(invalidType.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(invalidStatus.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(otherUserGet.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(otherUserPatch.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task PostGameUrl_RejectsInvalidForeignKeys()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var invalidGame = await app.Client.PostAsJsonAsync("/api/game-urls/", new GameUrlCreateDto
        {
            GameId = 404,
            ScrapingModeId = (long)ScrapingModeEnum.Batch,
            PartialUrl = "https://steam.example/{0}"
        });
        var invalidMode = await app.Client.PostAsJsonAsync("/api/game-urls/", new GameUrlCreateDto
        {
            GameId = 1,
            ScrapingModeId = 404,
            PartialUrl = "https://steam.example/{0}"
        });

        Assert.Multiple(() =>
        {
            Assert.That(invalidGame.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(invalidMode.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task PostPixel_RejectsInvalidGameId()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PostAsJsonAsync("/api/pixels/", new PixelCreateDto
        {
            GameId = 404,
            Name = "Missing Game",
            RedValue = 1,
            GreenValue = 2,
            BlueValue = 3,
            IsActive = true
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostProduct_RejectsDuplicateNameCaseInsensitively()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PostAsJsonAsync("/api/products/", new ProductCreateDto
        {
            GameId = 1,
            Name = "  rocket launcher  ",
            Rating = 5,
            IsActive = true
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PutGame_UpdatesFieldsAndInvalidatesGameCache()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);
        using (var scope = app.App.Services.CreateScope())
        {
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            cache.Set(string.Format(CacheKeys.Game, 1), new Game { Id = 1, Name = "Cached" });
        }

        var response = await app.Client.PutAsJsonAsync("/api/games/1", new GameUpdateDto
        {
            Id = 1,
            Name = "Updated Game",
            BaseUrl = "https://steam.example/updated",
            PageUrl = "https://steam.example/app/updated",
            InternalId = 11,
            IsActive = false
        });

        using var verifyScope = app.App.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await db.Games.FindAsync(1L);
        var cacheAfter = verifyScope.ServiceProvider.GetRequiredService<IMemoryCache>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(updated?.Name, Is.EqualTo("Updated Game"));
            Assert.That(cacheAfter.TryGetValue(string.Format(CacheKeys.Game, 1), out _), Is.False);
        });
    }

    [Test]
    public async Task PutGame_ReturnsNotFoundWhenMissing()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PutAsJsonAsync("/api/games/404", new GameUpdateDto
        {
            Id = 404,
            Name = "Missing"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteGame_ReturnsBadRequestWhenDependenciesExist()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.DeleteAsync("/api/games/1");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteGame_RemovesDependencyFreeGameAndInvalidatesCache()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.Games.Add(new Game { Id = 99, Name = "Disposable", IsActive = true, UserId = TestDb.TestUserId });
            db.SaveChanges();
        });
        using (var scope = app.App.Services.CreateScope())
        {
            scope.ServiceProvider
                .GetRequiredService<IMemoryCache>()
                .Set(string.Format(CacheKeys.Game, 99), new Game { Id = 99 });
        }

        var response = await app.Client.DeleteAsync("/api/games/99");

        using var verifyScope = app.App.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cache = verifyScope.ServiceProvider.GetRequiredService<IMemoryCache>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(db.Games.Find(99L), Is.Null);
            Assert.That(cache.TryGetValue(string.Format(CacheKeys.Game, 99), out _), Is.False);
        });
    }

    [Test]
    public async Task PatchStatus_UsesBodyIdAndTogglesStatus()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PatchAsJsonAsync("/api/games/999", new
        {
            id = 1,
            isActive = false
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var game = await db.Games.FindAsync(1L);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(game?.IsActive, Is.False);
        });
    }

    [Test]
    public async Task PatchStatus_ReturnsNoContentAndDoesNotChangeWhenStatusMatches()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PatchAsJsonAsync("/api/games/1", new
        {
            id = 1,
            isActive = true
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var game = await db.Games.FindAsync(1L);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(game?.IsActive, Is.True);
        });
    }

    [Test]
    public async Task PagedEndpoint_NormalizesAndClampsPaginationWindow()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.GetAsync("/api/games/paged?pageNumber=99&pageSize=1");
        var page = await ReadPageAsync(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(page.PageNumber, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(1));
            Assert.That(page.TotalCount, Is.EqualTo(2));
            Assert.That(page.TotalPages, Is.EqualTo(2));
            Assert.That(page.Items.GetArrayLength(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task PagedEndpoint_FiltersByNameAndSortsDescending()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var filtered = await app.Client.GetAsync("/api/games/paged?name=Beta");
        var sorted = await app.Client.GetAsync("/api/games/paged?pageSize=2&sortBy=name&sortDirection=desc");

        var filteredPage = await ReadPageAsync(filtered);
        var sortedPage = await ReadPageAsync(sorted);

        Assert.Multiple(() =>
        {
            Assert.That(filteredPage.TotalCount, Is.EqualTo(1));
            Assert.That(filteredPage.Items[0].GetProperty("name").GetString(), Is.EqualTo("Beta Game"));
            Assert.That(sortedPage.Items[0].GetProperty("name").GetString(), Is.EqualTo("Beta Game"));
            Assert.That(sortedPage.Items[1].GetProperty("name").GetString(), Is.EqualTo("Alpha Game"));
        });
    }

    [Test]
    public async Task CompositeKeyJoinEndpoints_HandleCreateDuplicateLookupAndDelete()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var duplicateProductTag = await app.Client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId = 1,
            tagId = 1
        });
        var invalidProductTag = await app.Client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId = 404,
            tagId = 1
        });
        var createProductTag = await app.Client.PostAsJsonAsync("/api/product-tags/", new
        {
            productId = 2,
            tagId = 2
        });
        var getProductTag = await app.Client.GetAsync("/api/product-tags/2/2");
        var deleteProductTag = await app.Client.DeleteAsync("/api/product-tags/2/2");
        var missingProductTag = await app.Client.GetAsync("/api/product-tags/2/2");

        var duplicateGameUrlProduct = await app.Client.PostAsJsonAsync("/api/game-url-products/", new
        {
            productId = 1,
            gameUrlId = 1
        });
        var invalidGameUrlProduct = await app.Client.PostAsJsonAsync("/api/game-url-products/", new
        {
            productId = 404,
            gameUrlId = 1
        });

        var duplicateGameUrlPixel = await app.Client.PostAsJsonAsync("/api/game-url-pixels/", new
        {
            pixelId = 1,
            gameUrlId = 2
        });
        var invalidGameUrlPixel = await app.Client.PostAsJsonAsync("/api/game-url-pixels/", new
        {
            pixelId = 404,
            gameUrlId = 2
        });

        Assert.Multiple(() =>
        {
            Assert.That(duplicateProductTag.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(invalidProductTag.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(createProductTag.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(getProductTag.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteProductTag.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(missingProductTag.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(duplicateGameUrlProduct.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(invalidGameUrlProduct.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(duplicateGameUrlPixel.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(invalidGameUrlPixel.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    private static async Task<PageJson> ReadPageAsync(HttpResponseMessage response)
    {
        var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement.Clone();

        return new PageJson(
            root.GetProperty("items"),
            root.GetProperty("pageNumber").GetInt32(),
            root.GetProperty("pageSize").GetInt32(),
            root.GetProperty("totalCount").GetInt32(),
            root.GetProperty("totalPages").GetInt32());
    }

    private sealed record PageJson(
        JsonElement Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages);
}

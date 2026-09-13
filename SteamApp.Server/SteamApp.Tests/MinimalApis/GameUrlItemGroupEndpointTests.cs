using System.Net;
using System.Net.Http.Json;
using SteamApp.Application.DTOs.GameUrl;
using SteamApp.Domain.Entities;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Contracts.Pagination;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class GameUrlItemGroupEndpointTests
{
    [TestCase(1L, "Priority")]
    [TestCase(null, null)]
    public async Task CreateGameUrl_OptionalGroup_ReturnsGroupInEveryProjection(long? groupId, string? groupName)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var create = await app.Client.PostAsJsonAsync("/api/game-urls/", new GameUrlCreateDto
        {
            GameId = 1,
            Name = "Grouped URL",
            ItemGroupId = groupId,
            IsActive = true,
        });
        var created = await app.ReadJsonAsync<GameUrlDto>(create);
        var detailResponse = await app.Client.GetAsync($"/api/game-urls/{created.Id}");
        var detail = await app.ReadJsonAsync<GameUrlDto>(detailResponse);
        var listResponse = await app.Client.GetAsync("/api/game-urls/");
        var list = await app.ReadJsonAsync<GameUrlDto[]>(listResponse);
        var pageResponse = await app.Client.GetAsync("/api/game-urls/paged?gameId=1");
        var page = await app.ReadJsonAsync<PagedResponse<GameUrlDto>>(pageResponse);

        Assert.Multiple(() =>
        {
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(listResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(pageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            foreach (var dto in new[] { created, detail, list.Single(x => x.Id == created.Id), page.Items.Single(x => x.Id == created.Id) })
            {
                Assert.That(dto.ItemGroupId, Is.EqualTo(groupId));
                Assert.That(dto.ItemGroupName, Is.EqualTo(groupName));
            }
        });
    }

    [TestCase(0L)]
    [TestCase(-1L)]
    [TestCase(2L)]
    [TestCase(3L)]
    [TestCase(404L)]
    public async Task CreateAndUpdateGameUrl_InvalidGroup_RejectsAndPreservesExistingUrl(long groupId)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.ItemGroups.Add(new ItemGroup
            {
                Id = 3,
                GameId = 1,
                Name = "Other user's group",
                UserId = "other-user",
            });
            db.GameUrls.Find(1L)!.ItemGroupId = 1;
            db.SaveChanges();
        });

        var create = await app.Client.PostAsJsonAsync("/api/game-urls/", new GameUrlCreateDto
        {
            GameId = 1,
            Name = "Invalid URL",
            ItemGroupId = groupId,
        });
        var update = await app.Client.PutAsJsonAsync("/api/game-urls/1", new GameUrlUpdateDto
        {
            Name = "Changed URL",
            ItemGroupId = groupId,
        });
        var detailResponse = await app.Client.GetAsync("/api/game-urls/1");
        var unchanged = await app.ReadJsonAsync<GameUrlDto>(detailResponse);
        var listResponse = await app.Client.GetAsync("/api/game-urls/");
        var list = await app.ReadJsonAsync<GameUrlDto[]>(listResponse);

        Assert.Multiple(() =>
        {
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(unchanged.Name, Is.EqualTo("Batch URL"));
            Assert.That(unchanged.ItemGroupId, Is.EqualTo(1));
            Assert.That(unchanged.ItemGroupName, Is.EqualTo("Priority"));
            Assert.That(list, Has.Length.EqualTo(2));
        });
    }

    [Test]
    public async Task UpdateGameUrl_AssignThenClearGroup_PersistsBothChanges()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var assign = await app.Client.PutAsJsonAsync("/api/game-urls/1", new GameUrlUpdateDto
        {
            Name = "Batch URL",
            ItemGroupId = 1,
        });
        var assignedResponse = await app.Client.GetAsync("/api/game-urls/1");
        var assigned = await app.ReadJsonAsync<GameUrlDto>(assignedResponse);
        var clear = await app.Client.PutAsJsonAsync("/api/game-urls/1", new GameUrlUpdateDto
        {
            Name = "Batch URL",
            ItemGroupId = null,
        });
        var clearedResponse = await app.Client.GetAsync("/api/game-urls/1");
        var cleared = await app.ReadJsonAsync<GameUrlDto>(clearedResponse);

        Assert.Multiple(() =>
        {
            Assert.That(assign.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(assigned.ItemGroupId, Is.EqualTo(1));
            Assert.That(assigned.ItemGroupName, Is.EqualTo("Priority"));
            Assert.That(clear.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(cleared.ItemGroupId, Is.Null);
            Assert.That(cleared.ItemGroupName, Is.Null);
        });
    }

    [Test]
    public async Task GameUrlGroupEndpoints_UnauthenticatedOrOtherUser_DenyAccess()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);
        using var anonymous = app.CreateClientWithoutAuth();
        using var otherUser = app.CreateClientWithScope("user");
        otherUser.DefaultRequestHeaders.Add(FakeAuthenticationHandler.UserIdHeader, "other-user");

        var input = new GameUrlCreateDto { GameId = 1, ItemGroupId = 1 };
        var anonymousCreate = await anonymous.PostAsJsonAsync("/api/game-urls/", input);
        var anonymousRead = await anonymous.GetAsync("/api/game-urls/1");
        var anonymousUpdate = await anonymous.PutAsJsonAsync("/api/game-urls/1", new GameUrlUpdateDto { ItemGroupId = 1 });
        var otherCreate = await otherUser.PostAsJsonAsync("/api/game-urls/", input);
        var otherRead = await otherUser.GetAsync("/api/game-urls/1");
        var otherUpdate = await otherUser.PutAsJsonAsync("/api/game-urls/1", new GameUrlUpdateDto { ItemGroupId = 1 });
        var otherListResponse = await otherUser.GetAsync("/api/game-urls/");
        var otherList = await app.ReadJsonAsync<GameUrlDto[]>(otherListResponse);
        var otherPageResponse = await otherUser.GetAsync("/api/game-urls/paged");
        var otherPage = await app.ReadJsonAsync<PagedResponse<GameUrlDto>>(otherPageResponse);

        Assert.Multiple(() =>
        {
            Assert.That(anonymousCreate.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(anonymousRead.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(anonymousUpdate.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(otherCreate.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(otherRead.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(otherUpdate.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(otherListResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(otherList, Is.Empty);
            Assert.That(otherPageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(otherPage.Items, Is.Empty);
        });
    }
}

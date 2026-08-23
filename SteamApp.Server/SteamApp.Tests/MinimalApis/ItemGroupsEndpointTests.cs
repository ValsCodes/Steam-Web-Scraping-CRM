using System.Net;
using System.Net.Http.Json;
using SteamApp.Application.DTOs.ItemGroup;
using SteamApp.Application.DTOs.Tag;
using SteamApp.Domain.Entities;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class ItemGroupsEndpointTests
{
    [Test]
    public async Task CreateAndListItemGroups_ValidInput_TrimsSortsAndRejectsDuplicates()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var create = await app.Client.PostAsJsonAsync("/api/item-groups/", new ItemGroupCreateDto
        {
            GameId = 1,
            Name = "  Category  ",
        });
        var duplicate = await app.Client.PostAsJsonAsync("/api/item-groups/", new ItemGroupCreateDto
        {
            GameId = 1,
            Name = "Category",
        });
        var list = await app.Client.GetAsync("/api/item-groups/game/1");

        var created = await app.ReadJsonAsync<ItemGroupDto>(create);
        var items = await app.ReadJsonAsync<ItemGroupDto[]>(list);

        Assert.Multiple(() =>
        {
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.Name, Is.EqualTo("Category"));
            Assert.That(duplicate.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(items.Select(x => x.Name), Is.EqualTo(new[] { "Category", "Priority" }));
        });
    }

    [Test]
    public async Task CreateItemGroup_InvalidNameOrUnownedGame_ReturnsBadRequest()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.Games.Add(new Game
            {
                Id = 3,
                Name = "Other User Game",
                UserId = "other-user",
            });
            db.SaveChanges();
        });

        var blankName = await app.Client.PostAsJsonAsync("/api/item-groups/", new ItemGroupCreateDto
        {
            GameId = 1,
            Name = "   ",
        });
        var missingGame = await app.Client.PostAsJsonAsync("/api/item-groups/", new ItemGroupCreateDto
        {
            GameId = 404,
            Name = "Category",
        });
        var otherUsersGame = await app.Client.PostAsJsonAsync("/api/item-groups/", new ItemGroupCreateDto
        {
            GameId = 3,
            Name = "Category",
        });
        var otherUsersList = await app.Client.GetAsync("/api/item-groups/game/3");

        Assert.Multiple(() =>
        {
            Assert.That(blankName.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(missingGame.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(otherUsersGame.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(otherUsersList.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task CreateAndUpdateTag_ItemGroupAssignment_ValidatesOwnershipAndGame()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.ItemGroups.Add(new ItemGroup
            {
                Id = 3,
                GameId = 1,
                Name = "Other User Group",
                UserId = "other-user",
            });
            db.SaveChanges();
        });

        var valid = await app.Client.PostAsJsonAsync("/api/tags/", new TagCreateDto
        {
            GameId = 1,
            Name = "Grouped",
            IsActive = true,
            ItemGroupId = 1,
        });
        var ungrouped = await app.Client.PostAsJsonAsync("/api/tags/", new TagCreateDto
        {
            GameId = 1,
            Name = "Ungrouped",
            IsActive = true,
        });
        var wrongGame = await app.Client.PostAsJsonAsync("/api/tags/", new TagCreateDto
        {
            GameId = 1,
            Name = "Wrong game",
            IsActive = true,
            ItemGroupId = 2,
        });
        var wrongUser = await app.Client.PostAsJsonAsync("/api/tags/", new TagCreateDto
        {
            GameId = 1,
            Name = "Wrong user",
            IsActive = true,
            ItemGroupId = 3,
        });
        var invalidUpdate = await app.Client.PutAsJsonAsync("/api/tags/1", new TagUpdateDto
        {
            Name = "Changed",
            IsActive = true,
            ItemGroupId = 2,
        });
        var unchanged = await app.Client.GetAsync("/api/tags/1");
        var clearUpdate = await app.Client.PutAsJsonAsync("/api/tags/1", new TagUpdateDto
        {
            Name = "Primary",
            IsActive = true,
            ItemGroupId = null,
        });
        var cleared = await app.Client.GetAsync("/api/tags/1");

        var validTag = await app.ReadJsonAsync<TagDto>(valid);
        var ungroupedTag = await app.ReadJsonAsync<TagDto>(ungrouped);
        var unchangedTag = await app.ReadJsonAsync<TagDto>(unchanged);
        var clearedTag = await app.ReadJsonAsync<TagDto>(cleared);

        Assert.Multiple(() =>
        {
            Assert.That(valid.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(validTag.ItemGroupId, Is.EqualTo(1));
            Assert.That(validTag.ItemGroupName, Is.EqualTo("Priority"));
            Assert.That(ungrouped.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(ungroupedTag.ItemGroupId, Is.Null);
            Assert.That(wrongGame.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(wrongUser.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(invalidUpdate.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(unchangedTag.Name, Is.EqualTo("Primary"));
            Assert.That(unchangedTag.ItemGroupId, Is.EqualTo(1));
            Assert.That(clearUpdate.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(clearedTag.ItemGroupId, Is.Null);
        });
    }

    [Test]
    public async Task GetTagsByGame_MixedGroups_OrdersByGroupThenNameWithUngroupedLast()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.ItemGroups.Add(new ItemGroup
            {
                Id = 3,
                GameId = 1,
                Name = "Category",
                UserId = TestDb.TestUserId,
            });
            db.Tags.AddRange(
                new Tag { Id = 3, GameId = 1, ItemGroupId = 1, Name = "Alpha", UserId = TestDb.TestUserId },
                new Tag { Id = 4, GameId = 1, ItemGroupId = 3, Name = "Beta", UserId = TestDb.TestUserId },
                new Tag { Id = 5, GameId = 1, Name = "No Group", UserId = TestDb.TestUserId });
            db.SaveChanges();
        });

        var response = await app.Client.GetAsync("/api/tags/game/1");
        var tags = await app.ReadJsonAsync<TagDto[]>(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(tags.Select(x => x.Name), Is.EqualTo(new[] { "Beta", "Alpha", "Primary", "No Group" }));
            Assert.That(tags[^1].ItemGroupId, Is.Null);
        });
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.WatchListItem;
using SteamApp.Infrastructure.Context;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class WatchListEndpointTests
{
    [Test]
    public async Task CreateWatchList_ValidInput_AssignsCurrentUtcDate()
    {
        var utcNow = new DateTimeOffset(2026, 10, 2, 23, 30, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(utcNow);
        await using var app = await MinimalApiTestApp.CreateAsync(timeProvider: timeProvider);

        var response = await app.Client.PostAsJsonAsync("/api/watch-list/", new
        {
            Name = "Watch Gamma",
            Url = "https://steam.example/watch/3",
            RegistrationDate = "2000-01-01",
            IsActive = true,
        });
        var created = await app.ReadJsonAsync<WatchListDto>(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.RegistrationDate, Is.EqualTo(new DateOnly(2026, 10, 2)));
        });
    }

    [Test]
    public async Task UpdateWatchList_ValidInput_PreservesRegistrationDate()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PutAsJsonAsync("/api/watch-list/1", new WatchListUpdateDto
        {
            Name = "Updated Watch Alpha",
            Url = "https://steam.example/watch/updated",
            IsActive = false,
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = await db.WatchList.FindAsync(1L);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(updated?.RegistrationDate, Is.EqualTo(new DateOnly(2026, 1, 1)));
            Assert.That(updated?.Name, Is.EqualTo("Updated Watch Alpha"));
        });
    }

    [Test]
    public async Task ReorderWatchList_ValidOwnedIds_PersistsRequestedOrder()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PutAsJsonAsync("/api/watch-list/order", new WatchListReorderDto
        {
            OrderedIds = [2, 1],
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderedIds = await db.WatchList
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Id)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(orderedIds, Is.EqualTo(new long[] { 2, 1 }));
        });
    }

    [Test]
    public async Task ReorderWatchList_MissingOwnedId_ReturnsValidationProblemWithoutChanges()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PutAsJsonAsync("/api/watch-list/order", new WatchListReorderDto
        {
            OrderedIds = [2],
        });

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderedIds = await db.WatchList
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Id)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(orderedIds, Is.EqualTo(new long[] { 1, 2 }));
        });
    }

    [Test]
    public async Task ReorderWatchList_NullIds_ReturnsValidationProblemWithoutChanges()
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);

        var response = await app.Client.PutAsJsonAsync("/api/watch-list/order", new WatchListReorderDto());

        using var scope = app.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orderedIds = await db.WatchList
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Id)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(orderedIds, Is.EqualTo(new long[] { 1, 2 }));
        });
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

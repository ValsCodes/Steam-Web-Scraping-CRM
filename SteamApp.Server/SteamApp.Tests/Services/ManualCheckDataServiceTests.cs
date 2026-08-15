using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class ManualCheckDataServiceTests
{
    [Test]
    public async Task PresetsAreSharedRecordsWithoutUserOwnershipAndNamesAreUniquePerGame()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var input = PresetInput("Mean Green");

        var created = await service.CreatePresetAsync(input, CancellationToken.None);
        var visible = await service.GetPresetsAsync(gameId: null, CancellationToken.None);
        var duplicate = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(input, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(visible.Select(x => x.Id), Does.Contain(created.Id));
            Assert.That(typeof(ManualCheckPreset).GetProperty("UserId"), Is.Null);
            Assert.That(typeof(ManualCheckRun).GetProperty("UserId"), Is.Null);
            Assert.That(duplicate!.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task CreateRunValidatesManualBatchSourceAndSnapshotsProductUrls()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Check"), CancellationToken.None);

        var invalid = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreateRunAsync(source.Id, preset.Id, CancellationToken.None));
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        database.Context.SaveChanges();

        var run = await service.CreateRunAsync(source.Id, preset.Id, CancellationToken.None);
        var detail = await service.GetRunAsync(run.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(invalid!.StatusCode, Is.EqualTo(400));
            Assert.That(run.Status, Is.EqualTo(ManualCheckRunStatusEnum.Queued));
            Assert.That(detail!.Setup.Products, Has.Count.EqualTo(1));
            Assert.That(detail.Setup.Products[0].FullUrl, Is.EqualTo(
                "https://steamcommunity.com/market/listings/440/Rocket%20Launcher"));
        });
    }

    [Test]
    public async Task RerunReusesCriteriaSnapshotAndUsesCurrentlyActiveProducts()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Snapshot"), CancellationToken.None);
        var original = await service.CreateRunAsync(source.Id, preset.Id, CancellationToken.None);

        database.Context.Products.Add(new Product
        {
            Id = 3,
            GameId = 1,
            Name = "New Item",
            IsActive = true,
            UserId = "another-user"
        });
        database.Context.GameUrlsProducts.Add(new GameUrlProducts { GameUrlId = 1, ProductId = 3 });
        database.Context.SaveChanges();

        var rerun = await service.RerunAsync(original.Id, CancellationToken.None);
        var originalDetail = await service.GetRunAsync(original.Id, CancellationToken.None);
        var rerunDetail = await service.GetRunAsync(rerun.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(originalDetail!.Setup.Products, Has.Count.EqualTo(1));
            Assert.That(rerunDetail!.Setup.Products, Has.Count.EqualTo(2));
            Assert.That(rerunDetail.Setup.Criteria[0].ValueContains, Is.EqualTo("Mean Green"));
            Assert.That(rerunDetail.Setup.Products.Select(x => x.ProductName), Does.Contain("New Item"));
        });
    }

    [Test]
    public async Task CancelStopsRunningRunAndKeepsItsLatestProgressTrace()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Cancelable"), CancellationToken.None);
        var run = await service.CreateRunAsync(source.Id, preset.Id, CancellationToken.None);
        await service.MarkRunningAndGetSetupAsync(run.Id, CancellationToken.None);
        var results = new ManualCheckRunResultsDto
        {
            Errors =
            [
                new ManualCheckProductErrorDto
                {
                    ProductId = 1,
                    ProductName = "Rocket Launcher",
                    Error = "Steam returned HTML."
                }
            ]
        };
        await service.UpdateProgressAsync(run.Id, 1, 0, 1, results, CancellationToken.None);

        var canceled = await service.CancelAsync(run.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(canceled.Status, Is.EqualTo(ManualCheckRunStatusEnum.Canceled));
            Assert.That(canceled.CheckedProducts, Is.EqualTo(1));
            Assert.That(canceled.Results.Errors, Has.Count.EqualTo(1));
            Assert.That(canceled.ErrorText, Does.Contain("Canceled by the user after checking 1 of 1 products."));
            Assert.That(canceled.CompletedAtUtc, Is.Not.Null);
        });
    }

    [Test]
    public async Task StartupReconciliationMarksQueuedAndRunningJobsAsInterruptedFailures()
    {
        using var database = TestDb.CreateSeededDatabase();
        database.Context.ManualCheckRuns.AddRange(
            Run(1, ManualCheckRunStatusEnum.Queued),
            Run(2, ManualCheckRunStatusEnum.Running),
            Run(3, ManualCheckRunStatusEnum.Succeeded));
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);

        await service.MarkInterruptedRunsFailedAsync(CancellationToken.None);

        var runs = await service.GetRunsAsync(null, 100, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(runs.Single(x => x.Id == 1).Status, Is.EqualTo(ManualCheckRunStatusEnum.Failed));
            Assert.That(runs.Single(x => x.Id == 2).Status, Is.EqualTo(ManualCheckRunStatusEnum.Failed));
            Assert.That(runs.Single(x => x.Id == 3).Status, Is.EqualTo(ManualCheckRunStatusEnum.Succeeded));
            Assert.That(
                runs.Single(x => x.Id == 1).ErrorText,
                Is.EqualTo("The manual check was interrupted by an API restart."));
        });
    }

    private static ManualCheckPresetWriteDto PresetInput(string name)
    {
        return new ManualCheckPresetWriteDto
        {
            GameId = 1,
            Name = name,
            MatchMode = ManualCheckMatchModeEnum.Any,
            Criteria = [new ManualCheckCriterionDto { ValueContains = " Mean Green " }]
        };
    }

    private static ManualCheckRun Run(long id, ManualCheckRunStatusEnum status)
    {
        return new ManualCheckRun
        {
            Id = id,
            GameId = 1,
            GameUrlId = 1,
            PresetName = "Historical",
            SetupJson = "{}",
            Status = status,
            Date = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString("N")
        };
    }
}

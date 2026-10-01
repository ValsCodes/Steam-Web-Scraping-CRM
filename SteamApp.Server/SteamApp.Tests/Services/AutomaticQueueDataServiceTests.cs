using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class AutomaticQueueDataServiceTests
{
    [Test]
    public async Task CreateDefinition_NormalizesSavedAndPrivateTemplatesAndSelectedProducts()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Saved"), CancellationToken.None);
        var service = CreateService(database, manualChecks);

        var created = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            new AutomaticQueueWriteDto
            {
                Name = "  Mixed checks  ",
                Blocks =
                [
                    ManualBlock(preset.Id, [1]),
                    new AutomaticQueueBlockWriteDto
                    {
                        Key = Guid.NewGuid(),
                        Type = AutomaticQueueBlockTypeEnum.ManualCheck,
                        GameUrlId = 1,
                        TemplateMode = AutomaticQueueTemplateModeEnum.PrivateTemplate,
                        PrivateTemplate = new AutomaticQueuePrivateTemplateDto
                        {
                            Name = "Private",
                            ListingLimit = 20,
                            PriceRange = new ManualCheckPriceRangeDto
                            {
                                Mode = ManualCheckPriceRangeModeEnum.Below,
                                MaximumPriceMinorUnits = 250
                            },
                            Criteria = [new ManualCheckCriterionDto { ValueContains = "Unusual" }]
                        }
                    }
                ]
            },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(created.Name, Is.EqualTo("Mixed checks"));
            Assert.That(created.Blocks, Has.Count.EqualTo(2));
            Assert.That(created.Blocks[0].PresetName, Is.EqualTo("Saved"));
            Assert.That(created.Blocks[0].ProductIds, Is.EqualTo(new long[] { 1 }));
            Assert.That(created.Blocks[1].PrivateTemplate!.Name, Is.EqualTo("Private"));
            Assert.That(created.Blocks[1].PrivateTemplate!.PriceRange!.MaximumPriceMinorUnits, Is.EqualTo(250));
            Assert.That(created.Blocks[1].GameName, Is.EqualTo("Alpha Game"));
        });
    }

    [Test]
    public async Task GetDefinitions_ReturnsNullForIdleQueueAndIdForActiveQueue()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Active state"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var activeDefinition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Active queue", ManualBlock(preset.Id, [1])),
            CancellationToken.None);
        var idleDefinition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Idle queue", ManualBlock(preset.Id, [1])),
            CancellationToken.None);

        var run = await service.StartRunAsync(activeDefinition.Id, TestDb.TestUserId, CancellationToken.None);
        var definitions = await service.GetDefinitionsAsync(TestDb.TestUserId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(definitions.Single(x => x.Id == activeDefinition.Id).ActiveRunId, Is.EqualTo(run.Id));
            Assert.That(definitions.Single(x => x.Id == idleDefinition.Id).ActiveRunId, Is.Null);
        });
    }

    [Test]
    public async Task CreateDefinition_InvalidLaterBlockRejectsTheWholeDefinition()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Atomic"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var input = new AutomaticQueueWriteDto
        {
            Name = "Invalid chain",
            Blocks =
            [
                ManualBlock(preset.Id, [1]),
                new AutomaticQueueBlockWriteDto
                {
                    Key = Guid.NewGuid(),
                    Type = AutomaticQueueBlockTypeEnum.Delay,
                    DelaySeconds = 604801
                }
            ]
        };

        var exception = Assert.ThrowsAsync<AutomaticQueueRequestException>(() =>
            service.CreateDefinitionAsync(TestDb.TestUserId, input, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(database.Context.AutomaticQueueDefinitions, Is.Empty);
            Assert.That(database.Context.AutomaticQueueBlocks, Is.Empty);
        });
    }

    [Test]
    public async Task CreateDefinition_RejectsASelectedProductOwnedByAnotherUser()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        database.Context.Products.Add(new Product
        {
            Id = 30,
            GameId = 1,
            Name = "Other owner's item",
            IsActive = true,
            UserId = "other-user"
        });
        database.Context.GameUrlsProducts.Add(new GameUrlProducts { GameUrlId = 1, ProductId = 30 });
        database.Context.SaveChanges();
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Ownership"), CancellationToken.None);
        var service = CreateService(database, manualChecks);

        var exception = Assert.ThrowsAsync<AutomaticQueueRequestException>(() =>
            service.CreateDefinitionAsync(
                TestDb.TestUserId,
                Definition("Ownership", ManualBlock(preset.Id, [30])),
                CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task StartRun_FreezesSetupAndLocksDefinitionUntilRunCompletes()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Frozen"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var manualBlock = ManualBlock(preset.Id, null);
        manualBlock.BypassCache = true;
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Frozen queue", manualBlock),
            CancellationToken.None);

        var run = await service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None);
        var updatedPreset = Preset("Changed later");
        updatedPreset.ListingLimit = 99;
        await manualChecks.UpdatePresetAsync(preset.Id, updatedPreset, CancellationToken.None);
        var storedBlock = database.Context.AutomaticQueueRunBlocks.Single();
        var snapshot = JsonConvert.DeserializeObject<AutomaticQueueRunBlockSetupDto>(storedBlock.SetupJson)!;
        var secondStart = Assert.ThrowsAsync<AutomaticQueueRequestException>(() =>
            service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None));
        var edit = Assert.ThrowsAsync<AutomaticQueueRequestException>(() =>
            service.UpdateDefinitionAsync(definition.Id, TestDb.TestUserId, Definition("Edited", ManualBlock(preset.Id, null)), CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(run.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.Queued));
            Assert.That(snapshot.ManualCheckSetup!.PresetName, Is.EqualTo("Frozen"));
            Assert.That(snapshot.ManualCheckSetup.ListingLimit, Is.EqualTo(10));
            Assert.That(snapshot.ManualCheckSetup.BypassCache, Is.True);
            Assert.That(snapshot.Configuration.BypassCache, Is.True);
            Assert.That(snapshot.ManualCheckSetup.Products.Select(x => x.ProductId), Is.EqualTo(new long[] { 1 }));
            Assert.That(secondStart!.StatusCode, Is.EqualTo(409));
            Assert.That(edit!.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task PresetCombination_RoundTripsAndResolvesLatestPresetsAtRunStart()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var first = await manualChecks.CreatePresetAsync(Preset("Paints"), CancellationToken.None);
        var second = await manualChecks.CreatePresetAsync(Preset("Effects"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var combinationBlock = new AutomaticQueueBlockWriteDto
        {
            Key = Guid.NewGuid(),
            Type = AutomaticQueueBlockTypeEnum.ManualCheck,
            GameUrlId = 1,
            TemplateMode = AutomaticQueueTemplateModeEnum.PresetCombination,
            PresetCombination = new ManualCheckPresetCombinationWriteDto
            {
                ListingLimit = 20,
                PriceRange = new ManualCheckPriceRangeDto
                {
                    Mode = ManualCheckPriceRangeModeEnum.Between,
                    MinimumPriceMinorUnits = 100,
                    MaximumPriceMinorUnits = 300
                },
                Terms =
                [
                    new ManualCheckPresetCombinationTermWriteDto { PresetId = first.Id },
                    new ManualCheckPresetCombinationTermWriteDto
                    {
                        PresetId = second.Id,
                        Operator = ManualCheckPresetCombinationOperatorEnum.And
                    }
                ]
            }
        };
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Combined", combinationBlock),
            CancellationToken.None);
        var updatedSecond = Preset("Effects");
        updatedSecond.Criteria[0].ValueContains = "Changed before start";
        await manualChecks.UpdatePresetAsync(second.Id, updatedSecond, CancellationToken.None);

        await service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None);
        var storedBlock = database.Context.AutomaticQueueRunBlocks.Single();
        var snapshot = JsonConvert.DeserializeObject<AutomaticQueueRunBlockSetupDto>(storedBlock.SetupJson)!;
        updatedSecond.Criteria[0].ValueContains = "Changed after start";
        await manualChecks.UpdatePresetAsync(second.Id, updatedSecond, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(definition.Blocks[0].TemplateMode, Is.EqualTo(AutomaticQueueTemplateModeEnum.PresetCombination));
            Assert.That(definition.Blocks[0].PresetCombination!.Terms.Select(x => x.PresetId), Is.EqualTo(new[] { first.Id, second.Id }));
            Assert.That(definition.Blocks[0].PresetCombination!.PriceRange!.MinimumPriceMinorUnits, Is.EqualTo(100));
            Assert.That(snapshot.ManualCheckSetup!.ListingLimit, Is.EqualTo(20));
            Assert.That(snapshot.ManualCheckSetup.PriceRange!.MaximumPriceMinorUnits, Is.EqualTo(300));
            Assert.That(snapshot.ManualCheckSetup.Criteria[^1].ValueContains, Is.EqualTo("Changed before start"));
            Assert.That(snapshot.ManualCheckSetup.PresetCombination!.Terms.Select(x => x.PresetName), Is.EqualTo(new[] { "Paints", "Effects" }));
        });
    }

    [Test]
    public async Task PresetCombination_DeletedReferenceRejectsRunWithoutCreatingSnapshot()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var first = await manualChecks.CreatePresetAsync(Preset("First"), CancellationToken.None);
        var second = await manualChecks.CreatePresetAsync(Preset("Second"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var block = new AutomaticQueueBlockWriteDto
        {
            Key = Guid.NewGuid(),
            Type = AutomaticQueueBlockTypeEnum.ManualCheck,
            GameUrlId = 1,
            TemplateMode = AutomaticQueueTemplateModeEnum.PresetCombination,
            PresetCombination = new ManualCheckPresetCombinationWriteDto
            {
                ListingLimit = 10,
                Terms =
                [
                    new ManualCheckPresetCombinationTermWriteDto { PresetId = first.Id },
                    new ManualCheckPresetCombinationTermWriteDto
                    {
                        PresetId = second.Id,
                        Operator = ManualCheckPresetCombinationOperatorEnum.Or
                    }
                ]
            }
        };
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Stale combination", block),
            CancellationToken.None);
        await manualChecks.DeletePresetAsync(second.Id, CancellationToken.None);

        var exception = Assert.ThrowsAsync<AutomaticQueueRequestException>(() =>
            service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(404));
            Assert.That(database.Context.AutomaticQueueRuns, Is.Empty);
            Assert.That(database.Context.AutomaticQueueRunBlocks, Is.Empty);
        });
    }

    [Test]
    public async Task ProcessActiveRuns_ManualDelayManual_UsesTimeProviderAndContinuesAfterFailure()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Sequence"), CancellationToken.None);
        var time = new MutableTimeProvider(new DateTimeOffset(2026, 9, 27, 8, 0, 0, TimeSpan.Zero));
        var service = CreateService(database, manualChecks, time);
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            new AutomaticQueueWriteDto
            {
                Name = "Sequence",
                Blocks =
                [
                    ManualBlock(preset.Id, [1]),
                    new AutomaticQueueBlockWriteDto { Key = Guid.NewGuid(), Type = AutomaticQueueBlockTypeEnum.Delay, DelaySeconds = 30 },
                    ManualBlock(preset.Id, [1])
                ]
            },
            CancellationToken.None);
        var run = await service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None);

        await service.ProcessActiveRunsAsync(CancellationToken.None);
        var first = await service.GetRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        var firstChildRunId = first!.Blocks[0].ManualCheckRunId
            ?? throw new AssertionException("The first manual block did not create a child run.");
        await manualChecks.MarkRunningAndGetRunAsync(firstChildRunId, CancellationToken.None);
        await manualChecks.CompleteAsync(
            firstChildRunId,
            ManualCheckRunStatusEnum.CompletedWithErrors,
            new ManualCheckRunResultsDto
            {
                Errors = [new ManualCheckProductErrorDto { ProductId = 1, ProductName = "Rocket Launcher", Error = "Warning" }]
            },
            CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        time.Advance(TimeSpan.FromSeconds(30));
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        var beforeFailure = await service.GetRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        await manualChecks.FailAsync(
            beforeFailure!.Blocks[2].ManualCheckRunId!.Value,
            "Expected test failure",
            null,
            CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        var completed = await service.GetRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(completed!.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.CompletedWithErrors));
            Assert.That(completed.Blocks.Select(x => x.Status), Is.EqualTo(new[]
            {
                AutomaticQueueBlockRunStatusEnum.CompletedWithWarnings,
                AutomaticQueueBlockRunStatusEnum.Succeeded,
                AutomaticQueueBlockRunStatusEnum.Failed
            }));
            Assert.That(completed.Blocks[0].WarningCount, Is.EqualTo(1));
            Assert.That(completed.CompletedBlocks, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task PauseResumeAndCancel_DelayPreservesRemainderAndSkipsFollowingBlocks()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Pause"), CancellationToken.None);
        var time = new MutableTimeProvider(new DateTimeOffset(2026, 9, 27, 9, 0, 0, TimeSpan.Zero));
        var service = CreateService(database, manualChecks, time);
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            new AutomaticQueueWriteDto
            {
                Name = "Pause and cancel",
                Blocks =
                [
                    new AutomaticQueueBlockWriteDto { Key = Guid.NewGuid(), Type = AutomaticQueueBlockTypeEnum.Delay, DelaySeconds = 60 },
                    ManualBlock(preset.Id, [1])
                ]
            },
            CancellationToken.None);
        var run = await service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        time.Advance(TimeSpan.FromSeconds(20));

        var paused = await service.PauseRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        var resumed = await service.ContinueRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        var canceled = await service.CancelRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(paused.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.Paused));
            Assert.That(paused.Blocks[0].RemainingDelaySeconds, Is.EqualTo(40));
            Assert.That(resumed.Blocks[0].WaitUntilUtc, Is.EqualTo(time.GetUtcNow().UtcDateTime.AddSeconds(40)));
            Assert.That(canceled.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.Canceled));
            Assert.That(canceled.Blocks[0].Status, Is.EqualTo(AutomaticQueueBlockRunStatusEnum.Canceled));
            Assert.That(canceled.Blocks[1].Status, Is.EqualTo(AutomaticQueueBlockRunStatusEnum.Skipped));
        });
    }

    [Test]
    public async Task RecoverInterruptedRuns_PausesQueueAndChildAtTheCurrentBlock()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Recovery"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Recovery", ManualBlock(preset.Id, [1])),
            CancellationToken.None);
        var run = await service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);

        await service.RecoverInterruptedRunsAsync(CancellationToken.None);
        var recovered = await service.GetRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        var child = await manualChecks.GetRunAsync(recovered!.Blocks[0].ManualCheckRunId!.Value, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(recovered.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.Paused));
            Assert.That(recovered.Blocks[0].Status, Is.EqualTo(AutomaticQueueBlockRunStatusEnum.Paused));
            Assert.That(child!.Status, Is.EqualTo(ManualCheckRunStatusEnum.Paused));
        });
    }

    [Test]
    public async Task PauseAndResume_ManualBlockContinuesTheSameChildRun()
    {
        using var database = TestDb.CreateSeededDatabase();
        PrepareManualSource(database);
        var manualChecks = new ManualCheckDataService(database.Factory);
        var preset = await manualChecks.CreatePresetAsync(Preset("Manual pause"), CancellationToken.None);
        var service = CreateService(database, manualChecks);
        var definition = await service.CreateDefinitionAsync(
            TestDb.TestUserId,
            Definition("Manual pause", ManualBlock(preset.Id, [1])),
            CancellationToken.None);
        var run = await service.StartRunAsync(definition.Id, TestDb.TestUserId, CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        var running = await service.GetRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        var childRunId = running!.Blocks[0].ManualCheckRunId!.Value;
        await manualChecks.MarkRunningAndGetRunAsync(childRunId, CancellationToken.None);

        var pauseRequested = await service.PauseRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        await manualChecks.MarkPausedAsync(childRunId, CancellationToken.None);
        await service.ProcessActiveRunsAsync(CancellationToken.None);
        var paused = await service.GetRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);
        var resumed = await service.ContinueRunAsync(run.Id, TestDb.TestUserId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(pauseRequested.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.PauseRequested));
            Assert.That(paused!.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.Paused));
            Assert.That(resumed.Status, Is.EqualTo(AutomaticQueueRunStatusEnum.Running));
            Assert.That(resumed.Blocks[0].ManualCheckRunId, Is.EqualTo(childRunId));
        });
    }

    private static AutomaticQueueDataService CreateService(
        TestDatabase database,
        ManualCheckDataService manualChecks,
        TimeProvider? timeProvider = null)
    {
        return new AutomaticQueueDataService(
            database.Factory,
            manualChecks,
            new ManualCheckQueue(),
            timeProvider ?? new MutableTimeProvider(DateTimeOffset.UtcNow),
            NullLogger<AutomaticQueueDataService>.Instance);
    }

    private static void PrepareManualSource(TestDatabase database)
    {
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
    }

    private static AutomaticQueueWriteDto Definition(string name, AutomaticQueueBlockWriteDto block)
    {
        return new AutomaticQueueWriteDto { Name = name, Blocks = [block] };
    }

    private static AutomaticQueueBlockWriteDto ManualBlock(long presetId, List<long>? productIds)
    {
        return new AutomaticQueueBlockWriteDto
        {
            Key = Guid.NewGuid(),
            Type = AutomaticQueueBlockTypeEnum.ManualCheck,
            GameUrlId = 1,
            TemplateMode = AutomaticQueueTemplateModeEnum.SavedPreset,
            PresetId = presetId,
            ProductIds = productIds
        };
    }

    private static ManualCheckPresetWriteDto Preset(string name)
    {
        return new ManualCheckPresetWriteDto
        {
            GameId = 1,
            Name = name,
            ListingLimit = 10,
            Criteria = [new ManualCheckCriterionDto { ValueContains = "Unusual" }]
        };
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset current = utcNow;

        public override DateTimeOffset GetUtcNow() => current;

        public void Advance(TimeSpan duration) => current = current.Add(duration);
    }
}

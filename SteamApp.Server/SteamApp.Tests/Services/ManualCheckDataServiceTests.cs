using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Services;
using Newtonsoft.Json.Linq;

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
            Assert.That(created.ListingLimit, Is.EqualTo(10));
            Assert.That(created.CooldownMinutes, Is.Null);
            Assert.That(created.Criteria[0].ValueContains, Is.EqualTo("Mean Green"));
            Assert.That(typeof(ManualCheckPreset).GetProperty("UserId"), Is.Null);
            Assert.That(typeof(ManualCheckRun).GetProperty("UserId"), Is.Null);
            Assert.That(duplicate!.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task CreatePresetAsync_OrderedOperatorsAndCooldown_PersistRelationally()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var input = PresetInput("Ordered");
        input.CooldownMinutes = 1;
        input.CooldownSeconds = 9;
        input.Criteria.Add(new ManualCheckCriterionDto
        {
            ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.AndNot,
            NameContains = "cannot trade"
        });

        var created = await service.CreatePresetAsync(input, CancellationToken.None);
        var operators = await service.GetConditionOperatorsAsync(CancellationToken.None);
        var storedCriteria = database.Context.ManualCheckCriteria
            .Where(x => x.ManualCheckPresetId == created.Id)
            .OrderBy(x => x.SortOrder)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(created.CooldownMinutes, Is.EqualTo(1));
            Assert.That(created.CooldownSeconds, Is.EqualTo(9));
            Assert.That(created.Criteria.Select(x => x.ConditionOperatorName), Is.EqualTo(new[] { null, "AND NOT" }));
            Assert.That(storedCriteria.Select(x => x.SortOrder), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(storedCriteria[1].ConditionOperatorId, Is.EqualTo((long)ManualCheckConditionOperatorEnum.AndNot));
            Assert.That(operators.Select(x => x.Name), Is.EqualTo(new[] { "AND", "OR", "AND NOT", "OR NOT", "XOR", "NAND", "NOR" }));
        });
    }

    [Test]
    public async Task PresetItemGroup_CreateListUpdateAndClear_ValidatesGameAndOrdersUngroupedLast()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        database.Context.ItemGroups.Add(new ItemGroup
        {
            Id = 3,
            GameId = 1,
            Name = "Other owner",
            UserId = "other-user"
        });
        database.Context.SaveChanges();
        var groupedInput = PresetInput("Zeta grouped");
        groupedInput.ItemGroupId = 1;
        var ungroupedInput = PresetInput("Alpha ungrouped");

        var grouped = await service.CreatePresetAsync(groupedInput, CancellationToken.None);
        var ungrouped = await service.CreatePresetAsync(ungroupedInput, CancellationToken.None);
        var invalidInput = PresetInput("Wrong game group");
        invalidInput.ItemGroupId = 2;
        var invalid = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(invalidInput, CancellationToken.None));
        var wrongOwnerInput = PresetInput("Wrong owner group");
        wrongOwnerInput.ItemGroupId = 3;
        var wrongOwner = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(wrongOwnerInput, CancellationToken.None));
        var visible = await service.GetPresetsAsync(gameId: 1, CancellationToken.None);
        var clearInput = PresetInput(grouped.Name);
        var cleared = await service.UpdatePresetAsync(grouped.Id, clearInput, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(grouped.ItemGroupId, Is.EqualTo(1));
            Assert.That(grouped.ItemGroupName, Is.EqualTo("Priority"));
            Assert.That(ungrouped.ItemGroupId, Is.Null);
            Assert.That(visible.Select(x => x.Id), Is.EqualTo(new[] { grouped.Id, ungrouped.Id }));
            Assert.That(visible[^1].ItemGroupId, Is.Null);
            Assert.That(invalid!.StatusCode, Is.EqualTo(400));
            Assert.That(wrongOwner!.StatusCode, Is.EqualTo(400));
            Assert.That(cleared.ItemGroupId, Is.Null);
            Assert.That(database.Context.ManualCheckPresets.Single(x => x.Id == grouped.Id).ItemGroupId, Is.Null);
        });
    }

    [Test]
    public async Task GroupedPreset_CreateAndUpdate_PreservesMarkersAndOrder()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var input = PresetInput("Grouped");
        input.Criteria =
        [
            new ManualCheckCriterionDto { ValueContains = "A" },
            new ManualCheckCriterionDto
            {
                ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.And,
                OpenGroupCount = 1,
                ValueContains = "B"
            },
            new ManualCheckCriterionDto
            {
                ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.Or,
                CloseGroupCount = 1,
                ValueContains = "C"
            }
        ];

        var created = await service.CreatePresetAsync(input, CancellationToken.None);
        created.Criteria[1].OpenGroupCount = 2;
        created.Criteria[2].CloseGroupCount = 2;
        created.Criteria[2].ValueContains = "Updated C";
        var updated = await service.UpdatePresetAsync(created.Id, new ManualCheckPresetWriteDto
        {
            GameId = created.GameId,
            Name = created.Name,
            ListingLimit = created.ListingLimit,
            Criteria = created.Criteria
        }, CancellationToken.None);
        var stored = database.Context.ManualCheckCriteria
            .Where(x => x.ManualCheckPresetId == created.Id)
            .OrderBy(x => x.SortOrder)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(updated.Criteria.Select(x => x.ValueContains), Is.EqualTo(new[] { "A", "B", "Updated C" }));
            Assert.That(updated.Criteria.Select(x => x.OpenGroupCount), Is.EqualTo(new[] { 0, 2, 0 }));
            Assert.That(updated.Criteria.Select(x => x.CloseGroupCount), Is.EqualTo(new[] { 0, 0, 2 }));
            Assert.That(stored.Select(x => x.SortOrder), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(stored[1].OpenGroupCount, Is.EqualTo(2));
            Assert.That(stored[2].CloseGroupCount, Is.EqualTo(2));
        });
    }

    [TestCase("negative")]
    [TestCase("premature")]
    [TestCase("excessive")]
    [TestCase("unclosed")]
    public void CreatePresetAsync_MalformedGrouping_RejectsRequest(string scenario)
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var input = PresetInput($"Malformed {scenario}");
        input.Criteria.Add(new ManualCheckCriterionDto
        {
            ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.And,
            ValueContains = "Second"
        });

        switch (scenario)
        {
            case "negative":
                input.Criteria[0].OpenGroupCount = -1;
                break;
            case "premature":
                input.Criteria[0].CloseGroupCount = 1;
                break;
            case "excessive":
                input.Criteria[0].OpenGroupCount = 25;
                input.Criteria[0].CloseGroupCount = 25;
                input.Criteria[1].OpenGroupCount = 1;
                input.Criteria[1].CloseGroupCount = 1;
                break;
            case "unclosed":
                input.Criteria[0].OpenGroupCount = 1;
                break;
        }

        var exception = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(input, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void CreatePresetAsync_MissingOperatorOrPartialCooldown_RejectsRequest()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var missingOperator = PresetInput("Missing operator");
        missingOperator.Criteria.Add(new ManualCheckCriterionDto { ValueContains = "Second" });
        var unsupportedOperator = PresetInput("Unsupported operator");
        unsupportedOperator.Criteria.Add(new ManualCheckCriterionDto
        {
            ConditionOperatorId = 999,
            ValueContains = "Second"
        });
        var partialCooldown = PresetInput("Partial cooldown");
        partialCooldown.CooldownMinutes = 1;

        var operatorException = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(missingOperator, CancellationToken.None));
        var unsupportedOperatorException = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(unsupportedOperator, CancellationToken.None));
        var cooldownException = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(partialCooldown, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(operatorException!.StatusCode, Is.EqualTo(400));
            Assert.That(operatorException.Message, Does.Contain("requires a supported condition operator"));
            Assert.That(unsupportedOperatorException!.StatusCode, Is.EqualTo(400));
            Assert.That(cooldownException!.StatusCode, Is.EqualTo(400));
            Assert.That(cooldownException.Message, Does.Contain("both be provided"));
        });
    }

    [Test]
    public async Task CreatePresetAsync_CooldownBoundaries_AcceptsZeroAndRejectsSixty()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var zeroCooldown = PresetInput("Zero cooldown");
        zeroCooldown.CooldownMinutes = 0;
        zeroCooldown.CooldownSeconds = 0;
        var invalidCooldown = PresetInput("Invalid cooldown");
        invalidCooldown.CooldownMinutes = 0;
        invalidCooldown.CooldownSeconds = 60;

        var created = await service.CreatePresetAsync(zeroCooldown, CancellationToken.None);
        var exception = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(invalidCooldown, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(created.CooldownMinutes, Is.Zero);
            Assert.That(created.CooldownSeconds, Is.Zero);
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Message, Does.Contain("between 0 and 59"));
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
            service.CreateRunAsync(source.Id, preset.Id, false, null, CancellationToken.None));
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        database.Context.SaveChanges();

        var run = await service.CreateRunAsync(source.Id, preset.Id, true, null, CancellationToken.None);
        var detail = await service.GetRunAsync(run.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(invalid!.StatusCode, Is.EqualTo(400));
            Assert.That(run.Status, Is.EqualTo(ManualCheckRunStatusEnum.Queued));
            Assert.That(detail!.Setup.Products, Has.Count.EqualTo(1));
            Assert.That(detail.Setup.ListingLimit, Is.EqualTo(10));
            Assert.That(detail.Setup.CooldownMinutes, Is.Null);
            Assert.That(detail.Setup.BypassCache, Is.True);
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
        var presetInput = PresetInput("Snapshot");
        presetInput.ListingLimit = 37;
        presetInput.Criteria.Add(new ManualCheckCriterionDto
        {
            ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.And,
            OpenGroupCount = 1,
            ValueContains = "Tradable"
        });
        presetInput.Criteria.Add(new ManualCheckCriterionDto
        {
            ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.Or,
            CloseGroupCount = 1,
            ValueContains = "Craftable"
        });
        var preset = await service.CreatePresetAsync(presetInput, CancellationToken.None);
        var original = await service.CreateRunAsync(source.Id, preset.Id, true, null, CancellationToken.None);

        presetInput.ListingLimit = 5;
        await service.UpdatePresetAsync(preset.Id, presetInput, CancellationToken.None);

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
            Assert.That(rerunDetail.Setup.ListingLimit, Is.EqualTo(37));
            Assert.That(rerunDetail.Setup.BypassCache, Is.False);
            Assert.That(rerunDetail.Setup.Criteria[0].ValueContains, Is.EqualTo("Mean Green"));
            Assert.That(rerunDetail.Setup.Criteria[1].OpenGroupCount, Is.EqualTo(1));
            Assert.That(rerunDetail.Setup.Criteria[2].CloseGroupCount, Is.EqualTo(1));
            Assert.That(rerunDetail.Setup.Products.Select(x => x.ProductName), Does.Contain("New Item"));
        });
    }

    [Test]
    public async Task Rerun_OldSnapshotWithoutListingLimit_UsesTopTen()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var presetInput = PresetInput("Legacy snapshot");
        presetInput.ListingLimit = 37;
        var preset = await service.CreatePresetAsync(presetInput, CancellationToken.None);
        var original = await service.CreateRunAsync(source.Id, preset.Id, false, null, CancellationToken.None);

        var originalRow = database.Context.ManualCheckRuns.Single(x => x.Id == original.Id);
        var legacySetup = JObject.Parse(originalRow.SetupJson);
        legacySetup.Remove(nameof(ManualCheckSetupDto.ListingLimit));
        originalRow.SetupJson = legacySetup.ToString();
        database.Context.SaveChanges();

        var rerun = await service.RerunAsync(original.Id, CancellationToken.None);
        var rerunDetail = await service.GetRunAsync(rerun.Id, CancellationToken.None);

        Assert.That(rerunDetail!.Setup.ListingLimit, Is.EqualTo(10));
    }

    [Test]
    public async Task Rerun_LegacyAllSnapshot_TranslatesToAndOperators()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var input = PresetInput("Legacy operators");
        input.Criteria.Add(new ManualCheckCriterionDto
        {
            ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.Or,
            NameContains = "attribute"
        });
        var preset = await service.CreatePresetAsync(input, CancellationToken.None);
        var original = await service.CreateRunAsync(source.Id, preset.Id, false, null, CancellationToken.None);
        var originalRow = database.Context.ManualCheckRuns.Single(x => x.Id == original.Id);
        var legacySetup = JObject.Parse(originalRow.SetupJson);
        legacySetup["MatchMode"] = "All";
        foreach (var criterion in legacySetup[nameof(ManualCheckSetupDto.Criteria)]!.Children<JObject>())
        {
            criterion.Remove(nameof(ManualCheckCriterionDto.ConditionOperatorId));
            criterion.Remove(nameof(ManualCheckCriterionDto.ConditionOperatorName));
            criterion.Remove(nameof(ManualCheckCriterionDto.OpenGroupCount));
            criterion.Remove(nameof(ManualCheckCriterionDto.CloseGroupCount));
        }
        originalRow.SetupJson = legacySetup.ToString();
        database.Context.SaveChanges();

        var rerun = await service.RerunAsync(original.Id, CancellationToken.None);
        var detail = await service.GetRunAsync(rerun.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(detail!.Setup.Criteria[0].ConditionOperatorId, Is.Null);
            Assert.That(detail.Setup.Criteria[1].ConditionOperatorId, Is.EqualTo((long)ManualCheckConditionOperatorEnum.And));
            Assert.That(detail.Setup.Criteria[1].ConditionOperatorName, Is.EqualTo("AND"));
            Assert.That(detail.Setup.Criteria.Select(x => x.OpenGroupCount), Is.All.EqualTo(0));
            Assert.That(detail.Setup.Criteria.Select(x => x.CloseGroupCount), Is.All.EqualTo(0));
        });
    }

    [Test]
    public async Task CreateRunAsync_SelectedProducts_ValidatesScopeAndPreservesOrderForRerun()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.Products.Add(new Product
        {
            Id = 3,
            GameId = 1,
            Name = "Second Item",
            IsActive = true,
            UserId = "another-user"
        });
        database.Context.GameUrlsProducts.Add(new GameUrlProducts { GameUrlId = 1, ProductId = 3 });
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Selected"), CancellationToken.None);

        var run = await service.CreateRunAsync(
            source.Id,
            preset.Id,
            false,
            [3, 1, 3],
            CancellationToken.None);
        var detail = await service.GetRunAsync(run.Id, CancellationToken.None);

        database.Context.Products.Add(new Product
        {
            Id = 4,
            GameId = 1,
            Name = "Later Item",
            IsActive = true,
            UserId = "another-user"
        });
        database.Context.GameUrlsProducts.Add(new GameUrlProducts { GameUrlId = 1, ProductId = 4 });
        database.Context.SaveChanges();
        var rerun = await service.RerunAsync(run.Id, CancellationToken.None);
        var rerunDetail = await service.GetRunAsync(rerun.Id, CancellationToken.None);

        var empty = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreateRunAsync(source.Id, preset.Id, false, [], CancellationToken.None));
        var unrelated = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreateRunAsync(source.Id, preset.Id, false, [999], CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(detail!.Setup.RequestedProductIds, Is.EqualTo(new long[] { 3, 1 }));
            Assert.That(detail.Setup.Products.Select(x => x.ProductId), Is.EqualTo(new long[] { 3, 1 }));
            Assert.That(rerunDetail!.Setup.Products.Select(x => x.ProductId), Is.EqualTo(new long[] { 3, 1 }));
            Assert.That(rerunDetail.Setup.Products.Select(x => x.ProductId), Does.Not.Contain(4));
            Assert.That(empty!.StatusCode, Is.EqualTo(400));
            Assert.That(unrelated!.StatusCode, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task Rerun_MalformedHistoricalGrouping_RejectsRequest()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Malformed snapshot"), CancellationToken.None);
        var original = await service.CreateRunAsync(source.Id, preset.Id, false, null, CancellationToken.None);
        var row = database.Context.ManualCheckRuns.Single(x => x.Id == original.Id);
        var setup = JObject.Parse(row.SetupJson);
        var firstCriterion = setup[nameof(ManualCheckSetupDto.Criteria)]!.Children<JObject>().First();
        firstCriterion[nameof(ManualCheckCriterionDto.CloseGroupCount)] = 1;
        row.SetupJson = setup.ToString();
        database.Context.SaveChanges();

        var exception = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.RerunAsync(original.Id, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void CreatePresetAsync_ListingLimitIsNotPositive_RejectsRequest()
    {
        using var database = TestDb.CreateSeededDatabase();
        var service = new ManualCheckDataService(database.Factory);
        var input = PresetInput("Invalid limit");
        input.ListingLimit = 0;

        var exception = Assert.ThrowsAsync<ManualCheckRequestException>(() =>
            service.CreatePresetAsync(input, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(400));
            Assert.That(exception.Message, Does.Contain("positive whole number"));
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
        var run = await service.CreateRunAsync(source.Id, preset.Id, false, null, CancellationToken.None);
        await service.MarkRunningAndGetRunAsync(run.Id, CancellationToken.None);
        var results = new ManualCheckRunResultsDto
        {
            ProductTraces =
            [
                new ManualCheckProductTraceDto
                {
                    ProductId = 1,
                    ProductName = "Rocket Launcher",
                    SteamApiResultJson = "{\"success\":true}"
                }
            ],
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
            Assert.That(canceled.Results.ProductTraces, Has.Count.EqualTo(1));
            Assert.That(canceled.Results.ProductTraces[0].SteamApiResultJson, Does.Contain("success"));
            Assert.That(canceled.Results.Errors, Has.Count.EqualTo(1));
            Assert.That(canceled.ErrorText, Does.Contain("Canceled by the user after checking 1 of 1 products."));
            Assert.That(canceled.CompletedAtUtc, Is.Not.Null);
        });
    }

    [Test]
    public async Task PauseAndContinue_PersistsProgressAndResumesTheSameRun()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.Products.Add(new Product
        {
            Id = 3,
            GameId = 1,
            Name = "Second Item",
            IsActive = true,
            UserId = "another-user"
        });
        database.Context.GameUrlsProducts.Add(new GameUrlProducts { GameUrlId = 1, ProductId = 3 });
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Pausable"), CancellationToken.None);
        var run = await service.CreateRunAsync(source.Id, preset.Id, false, [1, 3], CancellationToken.None);
        await service.MarkRunningAndGetRunAsync(run.Id, CancellationToken.None);

        var pauseRequested = await service.PauseAsync(run.Id, CancellationToken.None);
        var results = new ManualCheckRunResultsDto
        {
            ProductTraces =
            [
                new ManualCheckProductTraceDto
                {
                    ProductId = 1,
                    ProductName = "Rocket Launcher",
                    MatchEvaluated = true
                }
            ]
        };
        var progressStatus = await service.UpdateProgressAsync(
            run.Id,
            1,
            0,
            0,
            results,
            CancellationToken.None);
        var continued = await service.ContinueAsync(run.Id, CancellationToken.None);
        var resumed = await service.MarkRunningAndGetRunAsync(run.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(pauseRequested.Status, Is.EqualTo(ManualCheckRunStatusEnum.PauseRequested));
            Assert.That(progressStatus, Is.EqualTo(ManualCheckRunStatusEnum.Paused));
            Assert.That(continued.Id, Is.EqualTo(run.Id));
            Assert.That(continued.Status, Is.EqualTo(ManualCheckRunStatusEnum.Queued));
            Assert.That(resumed!.Status, Is.EqualTo(ManualCheckRunStatusEnum.Running));
            Assert.That(resumed.CheckedProducts, Is.EqualTo(1));
            Assert.That(resumed.Results.ProductTraces, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task PauseAsync_QueuedRunCanBeCanceledAndLastProductCompletesInsteadOfPausing()
    {
        using var database = TestDb.CreateSeededDatabase();
        var source = database.Context.GameUrls.Single(x => x.Id == 1);
        source.ScrapingModeId = (long)ScrapingModeEnum.ManualBatch;
        source.PartialUrl = "https://steamcommunity.com/market/listings/440/";
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);
        var preset = await service.CreatePresetAsync(PresetInput("Queued pause"), CancellationToken.None);
        var queuedRun = await service.CreateRunAsync(source.Id, preset.Id, false, [1], CancellationToken.None);

        var paused = await service.PauseAsync(queuedRun.Id, CancellationToken.None);
        var canceled = await service.CancelAsync(queuedRun.Id, CancellationToken.None);

        var finalRun = await service.CreateRunAsync(source.Id, preset.Id, false, [1], CancellationToken.None);
        await service.MarkRunningAndGetRunAsync(finalRun.Id, CancellationToken.None);
        await service.PauseAsync(finalRun.Id, CancellationToken.None);
        var finalStatus = await service.UpdateProgressAsync(
            finalRun.Id,
            1,
            0,
            0,
            new ManualCheckRunResultsDto(),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(paused.Status, Is.EqualTo(ManualCheckRunStatusEnum.Paused));
            Assert.That(canceled.Status, Is.EqualTo(ManualCheckRunStatusEnum.Canceled));
            Assert.That(finalStatus, Is.EqualTo(ManualCheckRunStatusEnum.Running));
        });
    }

    [Test]
    public async Task GetRun_OldResultsWithoutProductTraces_ReturnsAnEmptyTraceList()
    {
        using var database = TestDb.CreateSeededDatabase();
        var row = Run(99, ManualCheckRunStatusEnum.Succeeded);
        row.ResultsJson = "{\"Matches\":[],\"Errors\":[]}";
        database.Context.ManualCheckRuns.Add(row);
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);

        var detail = await service.GetRunAsync(row.Id, CancellationToken.None);

        Assert.That(detail!.Results.ProductTraces, Is.Empty);
    }

    [Test]
    public async Task GetRun_CompletedTimestamps_ReturnsExecutionDuration()
    {
        using var database = TestDb.CreateSeededDatabase();
        var row = Run(100, ManualCheckRunStatusEnum.Succeeded);
        row.StartedAtUtc = new DateTime(2026, 8, 19, 10, 0, 0, DateTimeKind.Utc);
        row.CompletedAtUtc = row.StartedAtUtc.Value.AddMilliseconds(4321);
        database.Context.ManualCheckRuns.Add(row);
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);

        var detail = await service.GetRunAsync(row.Id, CancellationToken.None);

        Assert.That(detail!.DurationMilliseconds, Is.EqualTo(4321));
    }

    [Test]
    public async Task StartupReconciliationMarksQueuedAndRunningJobsAsInterruptedFailures()
    {
        using var database = TestDb.CreateSeededDatabase();
        database.Context.ManualCheckRuns.AddRange(
            Run(1, ManualCheckRunStatusEnum.Queued),
            Run(2, ManualCheckRunStatusEnum.Running),
            Run(3, ManualCheckRunStatusEnum.Succeeded),
            Run(4, ManualCheckRunStatusEnum.PauseRequested),
            Run(5, ManualCheckRunStatusEnum.Paused));
        database.Context.SaveChanges();
        var service = new ManualCheckDataService(database.Factory);

        await service.MarkInterruptedRunsFailedAsync(CancellationToken.None);

        var runs = await service.GetRunsAsync(null, 100, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(runs.Single(x => x.Id == 1).Status, Is.EqualTo(ManualCheckRunStatusEnum.Failed));
            Assert.That(runs.Single(x => x.Id == 2).Status, Is.EqualTo(ManualCheckRunStatusEnum.Failed));
            Assert.That(runs.Single(x => x.Id == 3).Status, Is.EqualTo(ManualCheckRunStatusEnum.Succeeded));
            Assert.That(runs.Single(x => x.Id == 4).Status, Is.EqualTo(ManualCheckRunStatusEnum.Paused));
            Assert.That(runs.Single(x => x.Id == 5).Status, Is.EqualTo(ManualCheckRunStatusEnum.Paused));
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
            ListingLimit = 10,
            Criteria =
            [
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = null,
                    ValueContains = " Mean Green "
                }
            ]
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

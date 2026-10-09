using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Application.Utilities;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.ManualChecks;

namespace SteamApp.WebAPI.Services;

public sealed class ManualCheckDataService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    TimeProvider timeProvider) : IManualCheckDataService
{
    private const int MaxCriteria = 50;
    private const int MaxCombinationPresets = 10;
    private const int MaxPresetGameUrls = 1000;

    public ManualCheckDataService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        : this(dbContextFactory, TimeProvider.System)
    {
    }

    public async Task<IReadOnlyList<ManualCheckConditionOperatorDto>> GetConditionOperatorsAsync(
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await db.ManualCheckConditionOperators
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new ManualCheckConditionOperatorDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(
        long? gameId,
        CancellationToken cancellationToken)
    {
        return await GetPresetsInternalAsync(null, gameId, null, cancellationToken);
    }

    public async Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(
        string userId,
        long? gameId,
        CancellationToken cancellationToken)
    {
        return await GetPresetsInternalAsync(userId, gameId, null, cancellationToken);
    }

    public async Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsAsync(
        string userId,
        long? gameId,
        long? gameUrlId,
        CancellationToken cancellationToken)
    {
        return await GetPresetsInternalAsync(userId, gameId, gameUrlId, cancellationToken);
    }

    private async Task<IReadOnlyList<ManualCheckPresetDto>> GetPresetsInternalAsync(
        string? userId,
        long? gameId,
        long? gameUrlId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        IQueryable<ManualCheckPreset> query = db.ManualCheckPresets
            .AsNoTracking()
            .Include(x => x.Game)
            .Include(x => x.ItemGroup)
            .Include(x => x.GameUrls)
            .Include(x => x.Criteria)
            .ThenInclude(x => x.ConditionOperator);
        if (userId is not null)
        {
            query = query.Where(x => x.UserId == null || x.UserId == userId);
        }
        if (gameId.HasValue)
        {
            query = query.Where(x => x.GameId == gameId.Value);
        }
        if (gameUrlId.HasValue)
        {
            query = query.Where(x => x.GameUrls.Any(y => y.GameUrlId == gameUrlId.Value));
        }

        var presets = await query
            .OrderBy(x => x.Game.Name)
            .ThenBy(x => x.ItemGroupId == null)
            .ThenBy(x => x.ItemGroup!.Name)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return presets.Select(x => ToPresetDto(x, x.Game.Name, x.ItemGroup?.Name)).ToList();
    }

    public async Task<ManualCheckPresetDto> CreatePresetAsync(
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var userId = await db.Games.AsNoTracking()
            .Where(x => x.Id == input.GameId)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw RequestError(StatusCodes.Status400BadRequest, "The game must have an owner when using the legacy preset creation path.");
        return await CreatePresetAsync(userId, input, cancellationToken);
    }

    public async Task<ManualCheckPresetDto> CreatePresetAsync(
        string userId,
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePreset(input);
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => CreatePresetInternalAsync(userId, userId, normalized, cancellationToken));
    }

    public async Task<ManualCheckPresetDto> CreateGlobalPresetAsync(
        string administratorUserId,
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePreset(input);
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() =>
            CreatePresetInternalAsync(null, administratorUserId, normalized, cancellationToken));
    }

    public async Task<ManualCheckPresetDto> UpdatePresetAsync(
        long id,
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var userId = await db.ManualCheckPresets.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.UserId ?? x.Game.UserId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");
        return await UpdatePresetAsync(userId, id, input, cancellationToken);
    }

    public async Task<ManualCheckPresetDto> UpdatePresetAsync(
        string userId,
        long id,
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePreset(input);
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => UpdatePresetInternalAsync(userId, userId, id, normalized, cancellationToken));
    }

    public async Task<ManualCheckPresetDto> UpdateGlobalPresetAsync(
        string administratorUserId,
        long id,
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePreset(input);
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() =>
            UpdatePresetInternalAsync(null, administratorUserId, id, normalized, cancellationToken));
    }

    private async Task<ManualCheckPresetDto> CreatePresetInternalAsync(
        string? ownerUserId,
        string validationUserId,
        ManualCheckPresetWriteDto normalized,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        var gameName = await db.Games.AsNoTracking()
            .Where(x => x.Id == normalized.GameId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw RequestError(StatusCodes.Status400BadRequest, "Game was not found.");
        await ValidatePresetGameUrlsAsync(db, validationUserId, normalized.GameId, normalized.GameUrlIds, cancellationToken);
        var itemGroupName = await GetItemGroupNameAsync(
            db, validationUserId, normalized.GameId, normalized.ItemGroupId, cancellationToken);
        if (await db.ManualCheckPresets.AnyAsync(
                x => x.UserId == ownerUserId && x.GameId == normalized.GameId && x.Name == normalized.Name,
                cancellationToken))
        {
            throw RequestError(StatusCodes.Status409Conflict, "A preset with this name already exists for the game.");
        }

        var now = UtcNow();
        var entity = new ManualCheckPreset
        {
            UserId = ownerUserId,
            GameId = normalized.GameId,
            ItemGroupId = normalized.ItemGroupId,
            Name = normalized.Name,
            ListingLimit = normalized.ListingLimit,
            PriceRangeMode = normalized.PriceRange?.Mode,
            MinimumPriceMinorUnits = normalized.PriceRange?.MinimumPriceMinorUnits,
            MaximumPriceMinorUnits = normalized.PriceRange?.MaximumPriceMinorUnits,
            CooldownMinutes = normalized.CooldownMinutes,
            CooldownSeconds = normalized.CooldownSeconds,
            Criteria = CreateCriterionEntities(normalized.Criteria),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        db.ManualCheckPresets.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var relations = normalized.GameUrlIds.Select(gameUrlId => new ManualCheckPresetGameUrl
        {
            ManualCheckPresetId = entity.Id,
            GameUrlId = gameUrlId
        }).ToList();
        await InsertPresetGameUrlsAsync(db, relations, cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
        return ToPresetDto(entity, gameName, itemGroupName, normalized.GameUrlIds);
    }

    private async Task<ManualCheckPresetDto> UpdatePresetInternalAsync(
        string? ownerUserId,
        string validationUserId,
        long id,
        ManualCheckPresetWriteDto normalized,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var entity = await db.ManualCheckPresets
            .Include(x => x.Criteria)
            .Include(x => x.GameUrls)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == ownerUserId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");
        var gameName = await db.Games.AsNoTracking()
            .Where(x => x.Id == normalized.GameId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw RequestError(StatusCodes.Status400BadRequest, "Game was not found.");
        await ValidatePresetGameUrlsAsync(db, validationUserId, normalized.GameId, normalized.GameUrlIds, cancellationToken);
        var itemGroupName = await GetItemGroupNameAsync(
            db, validationUserId, normalized.GameId, normalized.ItemGroupId, cancellationToken);
        if (await db.ManualCheckPresets.AnyAsync(
                x => x.Id != id && x.UserId == ownerUserId && x.GameId == normalized.GameId && x.Name == normalized.Name,
                cancellationToken))
        {
            throw RequestError(StatusCodes.Status409Conflict, "A preset with this name already exists for the game.");
        }

        entity.GameId = normalized.GameId;
        entity.ItemGroupId = normalized.ItemGroupId;
        entity.Name = normalized.Name;
        entity.ListingLimit = normalized.ListingLimit;
        entity.PriceRangeMode = normalized.PriceRange?.Mode;
        entity.MinimumPriceMinorUnits = normalized.PriceRange?.MinimumPriceMinorUnits;
        entity.MaximumPriceMinorUnits = normalized.PriceRange?.MaximumPriceMinorUnits;
        entity.CooldownMinutes = normalized.CooldownMinutes;
        entity.CooldownSeconds = normalized.CooldownSeconds;
        UpdateCriteria(db, entity, normalized.Criteria);
        entity.UpdatedAtUtc = UtcNow();
        await db.SaveChangesAsync(cancellationToken);
        await ReconcilePresetGameUrlsAsync(db, entity, normalized.GameUrlIds, cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
        return ToPresetDto(entity, gameName, itemGroupName, normalized.GameUrlIds);
    }

    private static void UpdateCriteria(
        ApplicationDbContext db,
        ManualCheckPreset entity,
        IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        var existingCriteria = entity.Criteria.OrderBy(x => x.SortOrder).ToList();
        for (var index = 0; index < criteria.Count; index++)
        {
            var criterion = index < existingCriteria.Count
                ? existingCriteria[index]
                : new ManualCheckCriterion { ManualCheckPreset = entity };
            criterion.ConditionOperatorId = criteria[index].ConditionOperatorId;
            criterion.SortOrder = index;
            criterion.OpenGroupCount = criteria[index].OpenGroupCount;
            criterion.CloseGroupCount = criteria[index].CloseGroupCount;
            criterion.NameContains = criteria[index].NameContains;
            criterion.ValueContains = criteria[index].ValueContains;
            if (index >= existingCriteria.Count)
            {
                entity.Criteria.Add(criterion);
            }
        }

        if (existingCriteria.Count <= criteria.Count)
        {
            return;
        }

        var removedCriteria = existingCriteria.Skip(criteria.Count).ToList();
        db.ManualCheckCriteria.RemoveRange(removedCriteria);
        foreach (var removedCriterion in removedCriteria)
        {
            entity.Criteria.Remove(removedCriterion);
        }
    }

    private static async Task ValidatePresetGameUrlsAsync(
        ApplicationDbContext db,
        string userId,
        long gameId,
        IReadOnlyCollection<long> gameUrlIds,
        CancellationToken cancellationToken)
    {
        var validCount = await db.GameUrls.AsNoTracking().CountAsync(x =>
            gameUrlIds.Contains(x.Id) &&
            x.GameId == gameId &&
            (x.UserId == null || x.UserId == userId) &&
            (x.Game.UserId == null || x.Game.UserId == userId) &&
            x.ScrapingModeId == (long)ScrapingModeEnum.ManualBatch,
            cancellationToken);
        if (validCount != gameUrlIds.Count)
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                "Every assigned Game URL must belong to the preset's game and owner and use Manual Batch mode.");
        }
    }

    private static async Task InsertPresetGameUrlsAsync(
        ApplicationDbContext db,
        List<ManualCheckPresetGameUrl> relations,
        CancellationToken cancellationToken)
    {
        if (db.Database.IsRelational())
        {
            await db.BulkInsertAsync(relations, CreateRelationBulkConfig(), cancellationToken: cancellationToken);
            return;
        }

        db.ManualCheckPresetGameUrls.AddRange(relations);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task ReconcilePresetGameUrlsAsync(
        ApplicationDbContext db,
        ManualCheckPreset entity,
        IReadOnlyCollection<long> desiredGameUrlIds,
        CancellationToken cancellationToken)
    {
        var desired = desiredGameUrlIds.ToHashSet();
        var existing = entity.GameUrls.ToList();
        var existingIds = existing.Select(x => x.GameUrlId).ToHashSet();
        var deletes = existing.Where(x => !desired.Contains(x.GameUrlId)).ToList();
        var inserts = desired.Where(x => !existingIds.Contains(x))
            .Select(gameUrlId => new ManualCheckPresetGameUrl
            {
                ManualCheckPresetId = entity.Id,
                GameUrlId = gameUrlId
            })
            .ToList();

        if (db.Database.IsRelational())
        {
            if (deletes.Count > 0)
            {
                await db.BulkDeleteAsync(deletes, cancellationToken: cancellationToken);
            }
            if (inserts.Count > 0)
            {
                await db.BulkInsertAsync(inserts, CreateRelationBulkConfig(), cancellationToken: cancellationToken);
            }
            return;
        }

        db.ManualCheckPresetGameUrls.RemoveRange(deletes);
        db.ManualCheckPresetGameUrls.AddRange(inserts);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static BulkConfig CreateRelationBulkConfig() => new()
    {
        SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers
    };

    public async Task DeletePresetAsync(string userId, long id, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var entity = await db.ManualCheckPresets
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");

        var definitions = await db.AutomaticQueueDefinitions
            .Include(x => x.Blocks)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        foreach (var definition in definitions)
        {
            var referencedBlocks = definition.Blocks
                .Where(block => AutomaticQueueBlockReferencesPreset(block, id))
                .ToList();
            if (referencedBlocks.Count == 0)
            {
                continue;
            }

            db.AutomaticQueueBlocks.RemoveRange(referencedBlocks);
            definition.UpdatedAtUtc = UtcNow();
        }

        db.ManualCheckPresets.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGlobalPresetAsync(long id, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var entity = await db.ManualCheckPresets
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == null, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Global preset was not found.");
        var definitions = await db.AutomaticQueueDefinitions
            .AsNoTracking()
            .Include(x => x.Blocks)
            .ToListAsync(cancellationToken);
        if (definitions.SelectMany(x => x.Blocks).Any(block => AutomaticQueueBlockReferencesPreset(block, id)))
        {
            throw RequestError(StatusCodes.Status409Conflict, "The global preset is referenced by an automatic queue.");
        }

        db.ManualCheckPresets.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ManualCheckPresetDto> ClonePresetAsync(
        string userId,
        long id,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var source = await db.ManualCheckPresets.AsNoTracking()
            .Include(x => x.Criteria)
            .ThenInclude(x => x.ConditionOperator)
            .Include(x => x.GameUrls)
            .FirstOrDefaultAsync(x => x.Id == id && (x.UserId == null || x.UserId == userId), cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");
        var name = source.Name;
        var suffix = 1;
        while (await db.ManualCheckPresets.AnyAsync(
                   x => x.UserId == userId && x.GameId == source.GameId && x.Name == name,
                   cancellationToken))
        {
            var suffixText = $" (copy {suffix++})";
            var baseLength = ManualCheckPreset.NameMaxLength - suffixText.Length;
            name = $"{source.Name[..Math.Min(source.Name.Length, baseLength)]}{suffixText}";
        }

        return await CreatePresetAsync(userId, new ManualCheckPresetWriteDto
        {
            GameId = source.GameId,
            ItemGroupId = source.ItemGroupId,
            Name = name,
            ListingLimit = source.ListingLimit,
            PriceRange = ToPriceRangeDto(source),
            CooldownMinutes = source.CooldownMinutes,
            CooldownSeconds = source.CooldownSeconds,
            GameUrlIds = source.GameUrls.Select(x => x.GameUrlId).ToList(),
            Criteria = ToCriterionDtos(source.Criteria)
        }, cancellationToken);
    }

    public async Task<ManualCheckRunSummaryDto> CreateRunAsync(
        string userId,
        long gameUrlId,
        long? presetId,
        ManualCheckPresetCombinationWriteDto? presetCombination,
        bool bypassCache,
        IReadOnlyList<long>? productIds,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var setup = await PrepareQueueRunSetupAsync(
            db,
            userId,
            gameUrlId,
            presetId,
            presetCombination,
            null,
            bypassCache,
            productIds,
            cancellationToken);
        return await CreateRunRowAsync(db, setup, cancellationToken);
    }

    public async Task<ManualCheckRunSummaryDto> RerunAsync(
        long runId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var original = await db.ManualCheckRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");

        var setup = DeserializeSetup(original.SetupJson);
        var presetId = original.ManualCheckPresetId;
        if (presetId.HasValue && !await db.ManualCheckPresets.AnyAsync(x => x.Id == presetId.Value, cancellationToken))
        {
            presetId = null;
        }

        return await CreateRunInternalAsync(
            db,
            original.UserId,
            original.GameUrlId,
            presetId,
            original.PresetName,
            original.GameId,
            setup.ListingLimit,
            setup.PriceRange,
            setup.CooldownMinutes,
            setup.CooldownSeconds,
            false,
            setup.RequestedProductIds,
            NormalizeCriteria(setup.Criteria),
            setup.PresetCombination,
            cancellationToken);
    }

    public async Task<ManualCheckSetupDto> PrepareRunSetupAsync(
        string userId,
        long gameUrlId,
        long? presetId,
        ManualCheckPresetCombinationWriteDto? presetCombination,
        AutomaticQueuePrivateTemplateDto? privateTemplate,
        bool bypassCache,
        IReadOnlyList<long>? productIds,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await PrepareQueueRunSetupAsync(
            db,
            userId,
            gameUrlId,
            presetId,
            presetCombination,
            privateTemplate,
            bypassCache,
            productIds,
            cancellationToken);
    }

    internal static async Task<ManualCheckSetupDto> PrepareQueueRunSetupAsync(
        ApplicationDbContext db,
        string userId,
        long gameUrlId,
        long? presetId,
        ManualCheckPresetCombinationWriteDto? presetCombination,
        AutomaticQueuePrivateTemplateDto? privateTemplate,
        bool bypassCache,
        IReadOnlyList<long>? productIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw RequestError(StatusCodes.Status401Unauthorized, "A signed-in user is required.");
        }

        var selectionCount = (presetId.HasValue ? 1 : 0) +
            (presetCombination is not null ? 1 : 0) +
            (privateTemplate is not null ? 1 : 0);
        if (selectionCount != 1)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Choose one saved preset, preset combination, or private template.");
        }

        if (presetId.HasValue)
        {
            var preset = await db.ManualCheckPresets
                .AsNoTracking()
                .Include(x => x.Criteria)
                .ThenInclude(x => x.ConditionOperator)
                .Include(x => x.GameUrls)
                .FirstOrDefaultAsync(
                    x => x.Id == presetId.Value && (x.UserId == null || x.UserId == userId),
                    cancellationToken)
                ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");
            if (preset.GameUrls.All(x => x.GameUrlId != gameUrlId))
            {
                throw RequestError(
                    StatusCodes.Status409Conflict,
                    "The selected preset is not assigned to this Game URL.");
            }

            return await PrepareRunSetupInternalAsync(
                db,
                userId,
                gameUrlId,
                preset.Id,
                preset.Name,
                preset.GameId,
                preset.ListingLimit,
                ToPriceRangeDto(preset),
                preset.CooldownMinutes,
                preset.CooldownSeconds,
                bypassCache,
                productIds,
                ToCriterionDtos(preset.Criteria),
                null,
                cancellationToken);
        }

        if (presetCombination is not null)
        {
            var resolved = await ResolvePresetCombinationAsync(
                db,
                userId,
                gameUrlId,
                presetCombination,
                cancellationToken);
            return await PrepareRunSetupInternalAsync(
                db,
                userId,
                gameUrlId,
                null,
                resolved.DisplayName,
                resolved.GameId,
                resolved.ListingLimit,
                resolved.PriceRange,
                resolved.CooldownMinutes,
                resolved.CooldownSeconds,
                bypassCache,
                productIds,
                resolved.Criteria,
                resolved.Snapshot,
                cancellationToken);
        }

        var gameId = await db.GameUrls
            .AsNoTracking()
            .Where(x => x.Id == gameUrlId)
            .Select(x => (long?)x.GameId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Game URL was not found.");
        var privatePreset = NormalizePreset(new ManualCheckPresetWriteDto
        {
            GameId = gameId,
            Name = privateTemplate!.Name,
            ListingLimit = privateTemplate.ListingLimit,
            PriceRange = privateTemplate.PriceRange,
            CooldownMinutes = privateTemplate.CooldownMinutes,
            CooldownSeconds = privateTemplate.CooldownSeconds,
            Criteria = privateTemplate.Criteria
        }, requireGameUrls: false);

        return await PrepareRunSetupInternalAsync(
            db,
            userId,
            gameUrlId,
            null,
            privatePreset.Name,
            privatePreset.GameId,
            privatePreset.ListingLimit,
            privatePreset.PriceRange,
            privatePreset.CooldownMinutes,
            privatePreset.CooldownSeconds,
            bypassCache,
            productIds,
            privatePreset.Criteria,
            null,
            cancellationToken);
    }

    public async Task<ManualCheckRunSummaryDto> CreateRunFromSetupAsync(
        ManualCheckSetupDto setup,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await CreateRunRowAsync(db, setup, cancellationToken);
    }

    public async Task<ManualCheckRunDetailDto> PauseAsync(
        long runId,
        CancellationToken cancellationToken,
        AutomationPauseReasonEnum reason = AutomationPauseReasonEnum.UserRequested)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var now = UtcNow();
        if (!db.Database.IsRelational())
        {
            var inMemoryRow = await db.ManualCheckRuns
                .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
                ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");
            if (inMemoryRow.Status is not (
                ManualCheckRunStatusEnum.PauseRequested or
                ManualCheckRunStatusEnum.Paused))
            {
                inMemoryRow.Status = inMemoryRow.Status switch
                {
                    ManualCheckRunStatusEnum.Queued => ManualCheckRunStatusEnum.Paused,
                    ManualCheckRunStatusEnum.Running => ManualCheckRunStatusEnum.PauseRequested,
                    _ => throw RequestError(
                        StatusCodes.Status409Conflict,
                        $"Run #{runId} is already {inMemoryRow.Status} and cannot be paused.")
                };
                inMemoryRow.PauseReason = reason;
                if (inMemoryRow.Status == ManualCheckRunStatusEnum.Paused)
                {
                    inMemoryRow.PausedAtUtc = now;
                }
                await db.SaveChangesAsync(cancellationToken);
            }

            return ToRunDetail(inMemoryRow);
        }

        var updated = await db.ManualCheckRuns
            .Where(x => x.Id == runId && x.Status == ManualCheckRunStatusEnum.Queued)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, ManualCheckRunStatusEnum.Paused)
                    .SetProperty(x => x.PausedAtUtc, now)
                    .SetProperty(x => x.PauseReason, reason),
                cancellationToken);
        if (updated == 0)
        {
            updated = await db.ManualCheckRuns
                .Where(x => x.Id == runId && x.Status == ManualCheckRunStatusEnum.Running)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Status, ManualCheckRunStatusEnum.PauseRequested)
                        .SetProperty(x => x.PauseReason, reason),
                    cancellationToken);
        }

        var row = await db.ManualCheckRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");
        if (updated == 0 && row.Status is not (
            ManualCheckRunStatusEnum.PauseRequested or
            ManualCheckRunStatusEnum.Paused))
        {
            throw RequestError(
                StatusCodes.Status409Conflict,
                $"Run #{runId} is already {row.Status} and cannot be paused.");
        }

        return ToRunDetail(row);
    }

    public async Task<ManualCheckRunSummaryDto> ContinueAsync(
        long runId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var now = UtcNow();
        if (!db.Database.IsRelational())
        {
            var inMemoryRow = await db.ManualCheckRuns
                .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
                ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");
            if (inMemoryRow.Status != ManualCheckRunStatusEnum.Paused)
            {
                throw RequestError(
                    StatusCodes.Status409Conflict,
                    $"Run #{runId} is {inMemoryRow.Status} and cannot be continued.");
            }

            inMemoryRow.AccumulatedPausedMilliseconds += GetCurrentPauseMilliseconds(inMemoryRow, now);
            inMemoryRow.PausedAtUtc = null;
            inMemoryRow.Status = ManualCheckRunStatusEnum.Queued;
            inMemoryRow.PauseReason = null;
            await db.SaveChangesAsync(cancellationToken);
            return ToRunSummary(inMemoryRow);
        }

        var pausedRow = await db.ManualCheckRuns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");
        var accumulatedPausedMilliseconds = pausedRow.AccumulatedPausedMilliseconds +
            GetCurrentPauseMilliseconds(pausedRow, now);
        var updated = await db.ManualCheckRuns
            .Where(x =>
                x.Id == runId &&
                x.Status == ManualCheckRunStatusEnum.Paused &&
                x.PausedAtUtc == pausedRow.PausedAtUtc)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, ManualCheckRunStatusEnum.Queued)
                    .SetProperty(x => x.PausedAtUtc, (DateTime?)null)
                    .SetProperty(x => x.PauseReason, (AutomationPauseReasonEnum?)null)
                    .SetProperty(x => x.AccumulatedPausedMilliseconds, accumulatedPausedMilliseconds),
                cancellationToken);
        var row = await db.ManualCheckRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");
        if (updated == 0)
        {
            throw RequestError(
                StatusCodes.Status409Conflict,
                $"Run #{runId} is {row.Status} and cannot be continued.");
        }

        return ToRunSummary(row);
    }

    public async Task<ManualCheckRunDetailDto> UpdateListingLimitAsync(
        long runId,
        int listingLimit,
        CancellationToken cancellationToken)
    {
        var normalizedListingLimit = NormalizeListingLimit(listingLimit);
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");
        if (row.Status != ManualCheckRunStatusEnum.Paused)
        {
            throw RequestError(
                StatusCodes.Status409Conflict,
                $"Run #{runId} is {row.Status} and its listing limit cannot be changed.");
        }

        var setup = DeserializeSetup(row.SetupJson);
        setup.ListingLimit = normalizedListingLimit;
        var setupJson = JsonConvert.SerializeObject(setup);
        if (db.Database.IsRelational())
        {
            var updated = await db.ManualCheckRuns
                .Where(x => x.Id == runId && x.Status == ManualCheckRunStatusEnum.Paused)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.SetupJson, setupJson),
                    cancellationToken);
            if (updated == 0)
            {
                throw RequestError(
                    StatusCodes.Status409Conflict,
                    $"Run #{runId} is no longer paused.");
            }
            row.SetupJson = setupJson;
        }
        else
        {
            var tracked = await db.ManualCheckRuns.FirstAsync(x => x.Id == runId, cancellationToken);
            if (tracked.Status != ManualCheckRunStatusEnum.Paused)
            {
                throw RequestError(StatusCodes.Status409Conflict, $"Run #{runId} is no longer paused.");
            }
            tracked.SetupJson = setupJson;
            await db.SaveChangesAsync(cancellationToken);
            row = tracked;
        }

        return ToRunDetail(row);
    }

    public async Task<ManualCheckRunDetailDto> CancelAsync(
        long runId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");

        if (row.Status is not (
            ManualCheckRunStatusEnum.Queued or
            ManualCheckRunStatusEnum.Running or
            ManualCheckRunStatusEnum.PauseRequested or
            ManualCheckRunStatusEnum.Paused))
        {
            throw RequestError(
                StatusCodes.Status409Conflict,
                $"Run #{runId} is already {row.Status} and cannot be canceled.");
        }

        var now = UtcNow();
        row.AccumulatedPausedMilliseconds += GetCurrentPauseMilliseconds(row, now);
        row.PausedAtUtc = null;
        row.Status = ManualCheckRunStatusEnum.Canceled;
        row.PauseReason = null;
        row.ErrorText = row.CheckedProducts == 0
            ? "Canceled by the user before any products were checked."
            : $"Canceled by the user after checking {row.CheckedProducts} of {row.TotalProducts} products.";
        row.CompletedAtUtc = now;
        await CloseUsageIntervalAsync(db, runId, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return ToRunDetail(row);
    }

    public async Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsAsync(
        long? gameId,
        int take,
        CancellationToken cancellationToken)
    {
        return await GetRunsInternalAsync(null, gameId, take, cancellationToken);
    }

    public async Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsAsync(
        string userId,
        long? gameId,
        int take,
        CancellationToken cancellationToken)
    {
        return await GetRunsInternalAsync(userId, gameId, take, cancellationToken);
    }

    private async Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsInternalAsync(
        string? userId,
        long? gameId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        IQueryable<ManualCheckRun> query = db.ManualCheckRuns
            .AsNoTracking()
            .Include(x => x.AutomaticQueueRunBlock)
            .ThenInclude(x => x!.AutomaticQueueRun);
        if (userId is not null)
        {
            query = query.Where(x => x.UserId == userId);
        }
        if (gameId.HasValue)
        {
            query = query.Where(x => x.GameId == gameId.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);

        return rows.Select(ToRunSummary).ToList();
    }

    public async Task<bool> UserOwnsGameAsync(string userId, long gameId, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await db.Games.AsNoTracking().AnyAsync(x => x.Id == gameId, cancellationToken);
    }

    public async Task<bool> UserOwnsGameUrlAsync(string userId, long gameUrlId, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await db.GameUrls.AsNoTracking().AnyAsync(
            x => x.Id == gameUrlId,
            cancellationToken);
    }

    public async Task<bool> UserOwnsPresetAsync(string userId, long presetId, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await db.ManualCheckPresets.AsNoTracking().AnyAsync(
            x => x.Id == presetId && (x.UserId == null || x.UserId == userId),
            cancellationToken);
    }

    public async Task<bool> UserOwnsRunAsync(string userId, long runId, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await db.ManualCheckRuns.AsNoTracking().AnyAsync(
            x => x.Id == runId && x.UserId == userId,
            cancellationToken);
    }

    public async Task<bool> IsQueueOwnedRunAsync(long runId, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await db.AutomaticQueueRunBlocks.AsNoTracking().AnyAsync(
            x => x.ManualCheckRunId == runId,
            cancellationToken);
    }

    public async Task<ManualCheckRunDetailDto?> GetRunAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns
            .AsNoTracking()
            .Include(x => x.AutomaticQueueRunBlock)
            .ThenInclude(x => x!.AutomaticQueueRun)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null)
        {
            return null;
        }

        return ToRunDetail(row);
    }

    public async Task<ManualCheckRunDetailDto?> MarkRunningAndGetRunAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => MarkRunningAttemptAsync(id, cancellationToken));
    }

    private async Task<ManualCheckRunDetailDto?> MarkRunningAttemptAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var now = UtcNow();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null || row.Status != ManualCheckRunStatusEnum.Queued)
        {
            return null;
        }

        row.Status = ManualCheckRunStatusEnum.Running;
        row.PauseReason = null;
        row.StartedAtUtc ??= now;
        if (!await db.AutomationUsageIntervals.AnyAsync(
                x => x.ManualCheckRunId == id && x.EndedAtUtc == null,
                cancellationToken))
        {
            db.AutomationUsageIntervals.Add(new AutomationUsageInterval
            {
                UserId = row.UserId,
                ManualCheckRunId = id,
                StartedAtUtc = now
            });
        }
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
        return ToRunDetail(row);
    }

    public async Task<ManualCheckRunStatusEnum?> MarkPausedAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => MarkPausedAttemptAsync(id, cancellationToken));
    }

    private async Task<ManualCheckRunStatusEnum?> MarkPausedAttemptAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var now = UtcNow();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null)
        {
            return null;
        }
        if (row.Status == ManualCheckRunStatusEnum.PauseRequested)
        {
            row.Status = ManualCheckRunStatusEnum.Paused;
            row.PausedAtUtc = now;
            await CloseUsageIntervalAsync(db, id, now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }

        return row.Status;
    }

    public async Task<ManualCheckRunStatusEnum?> UpdateProgressAsync(
        long id,
        int checkedProducts,
        int matchedProducts,
        int failedProducts,
        ManualCheckRunResultsDto results,
        CancellationToken cancellationToken)
    {
        var resultsJson = JsonConvert.SerializeObject(results);
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(() => UpdateProgressAttemptAsync(
            id,
            checkedProducts,
            matchedProducts,
            failedProducts,
            resultsJson,
            cancellationToken));
    }

    private async Task<ManualCheckRunStatusEnum?> UpdateProgressAttemptAsync(
        long id,
        int checkedProducts,
        int matchedProducts,
        int failedProducts,
        string resultsJson,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        if (!db.Database.IsRelational())
        {
            var inMemoryRow = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (inMemoryRow is null)
            {
                return null;
            }

            if (inMemoryRow.Status is not (
                ManualCheckRunStatusEnum.Running or
                ManualCheckRunStatusEnum.PauseRequested))
            {
                return inMemoryRow.Status;
            }

            inMemoryRow.CheckedProducts = checkedProducts;
            inMemoryRow.MatchedProducts = matchedProducts;
            inMemoryRow.FailedProducts = failedProducts;
            inMemoryRow.ResultsJson = resultsJson;
            if (inMemoryRow.Status == ManualCheckRunStatusEnum.PauseRequested)
            {
                inMemoryRow.Status = checkedProducts < inMemoryRow.TotalProducts
                    ? ManualCheckRunStatusEnum.Paused
                    : ManualCheckRunStatusEnum.Running;
                if (inMemoryRow.Status == ManualCheckRunStatusEnum.Paused)
                {
                    inMemoryRow.PausedAtUtc = UtcNow();
                    await CloseUsageIntervalAsync(db, id, inMemoryRow.PausedAtUtc.Value, cancellationToken);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return inMemoryRow.Status;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var now = UtcNow();
            var paused = await db.ManualCheckRuns
                .Where(x =>
                    x.Id == id &&
                    x.Status == ManualCheckRunStatusEnum.PauseRequested &&
                    x.TotalProducts > checkedProducts)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.CheckedProducts, checkedProducts)
                        .SetProperty(x => x.MatchedProducts, matchedProducts)
                        .SetProperty(x => x.FailedProducts, failedProducts)
                        .SetProperty(x => x.ResultsJson, resultsJson)
                        .SetProperty(x => x.Status, ManualCheckRunStatusEnum.Paused)
                        .SetProperty(x => x.PausedAtUtc, now),
                    cancellationToken);
            if (paused > 0)
            {
                await CloseUsageIntervalAsync(db, id, now, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ManualCheckRunStatusEnum.Paused;
            }

            var running = await db.ManualCheckRuns
                .Where(x =>
                    x.Id == id &&
                    (x.Status == ManualCheckRunStatusEnum.Running ||
                     (x.Status == ManualCheckRunStatusEnum.PauseRequested &&
                      x.TotalProducts <= checkedProducts)))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.CheckedProducts, checkedProducts)
                        .SetProperty(x => x.MatchedProducts, matchedProducts)
                        .SetProperty(x => x.FailedProducts, failedProducts)
                        .SetProperty(x => x.ResultsJson, resultsJson)
                        .SetProperty(x => x.Status, ManualCheckRunStatusEnum.Running),
                    cancellationToken);
            if (running > 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return ManualCheckRunStatusEnum.Running;
            }
        }

        var currentStatus = await db.ManualCheckRuns
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => (ManualCheckRunStatusEnum?)x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return currentStatus;
    }

    public async Task CompleteAsync(
        long id,
        ManualCheckRunStatusEnum status,
        ManualCheckRunResultsDto results,
        CancellationToken cancellationToken)
    {
        if (status is not (ManualCheckRunStatusEnum.Succeeded or ManualCheckRunStatusEnum.CompletedWithErrors))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(() => CompleteAttemptAsync(id, status, results, cancellationToken));
    }

    private async Task CompleteAttemptAsync(
        long id,
        ManualCheckRunStatusEnum status,
        ManualCheckRunResultsDto results,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var errorText = status == ManualCheckRunStatusEnum.CompletedWithErrors
            ? $"{results.Errors.Count} product check(s) failed."
            : null;
        var resultsJson = JsonConvert.SerializeObject(results);
        var completedAtUtc = UtcNow();
        if (!db.Database.IsRelational())
        {
            var inMemoryRow = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (inMemoryRow is null || inMemoryRow.Status is not (
                ManualCheckRunStatusEnum.Running or
                ManualCheckRunStatusEnum.PauseRequested))
            {
                return;
            }

            inMemoryRow.Status = status;
            inMemoryRow.CheckedProducts = inMemoryRow.TotalProducts;
            inMemoryRow.MatchedProducts = results.Matches.Count;
            inMemoryRow.FailedProducts = results.Errors.Count;
            inMemoryRow.ResultsJson = resultsJson;
            inMemoryRow.ErrorText = errorText;
            inMemoryRow.CompletedAtUtc = completedAtUtc;
            inMemoryRow.PauseReason = null;
            await CloseUsageIntervalAsync(db, id, completedAtUtc, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.ManualCheckRuns
            .Where(x =>
                x.Id == id &&
                (x.Status == ManualCheckRunStatusEnum.Running ||
                 x.Status == ManualCheckRunStatusEnum.PauseRequested))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, status)
                    .SetProperty(x => x.CheckedProducts, x => x.TotalProducts)
                    .SetProperty(x => x.MatchedProducts, results.Matches.Count)
                    .SetProperty(x => x.FailedProducts, results.Errors.Count)
                    .SetProperty(x => x.ResultsJson, resultsJson)
                    .SetProperty(x => x.ErrorText, errorText)
                    .SetProperty(x => x.CompletedAtUtc, completedAtUtc)
                    .SetProperty(x => x.PauseReason, (AutomationPauseReasonEnum?)null),
                cancellationToken);
        await CloseUsageIntervalAsync(db, id, completedAtUtc, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task FailAsync(
        long id,
        string errorText,
        ManualCheckRunResultsDto? results,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null || row.Status is not (
            ManualCheckRunStatusEnum.Queued or
            ManualCheckRunStatusEnum.Running or
            ManualCheckRunStatusEnum.PauseRequested))
        {
            return;
        }

        row.Status = ManualCheckRunStatusEnum.Failed;
        row.ResultsJson = results is null ? row.ResultsJson : JsonConvert.SerializeObject(results);
        row.MatchedProducts = results?.Matches.Count ?? row.MatchedProducts;
        row.FailedProducts = results?.Errors.Count ?? row.FailedProducts;
        row.ErrorText = errorText;
        var now = UtcNow();
        row.StartedAtUtc ??= now;
        row.CompletedAtUtc = now;
        row.PauseReason = null;
        await CloseUsageIntervalAsync(db, id, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkInterruptedRunsFailedAsync(CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var interrupted = await db.ManualCheckRuns
            .Include(x => x.AutomaticQueueRunBlock)
            .Where(x =>
                x.Status == ManualCheckRunStatusEnum.Queued ||
                x.Status == ManualCheckRunStatusEnum.Running ||
                x.Status == ManualCheckRunStatusEnum.PauseRequested)
            .ToListAsync(cancellationToken);

        if (interrupted.Count == 0)
        {
            return;
        }

        var now = UtcNow();
        foreach (var row in interrupted)
        {
            row.Status = ManualCheckRunStatusEnum.Paused;
            row.PauseReason = AutomationPauseReasonEnum.SystemRecovery;
            row.PausedAtUtc = now;
            await CloseUsageIntervalAsync(db, row.Id, now, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ManualCheckRunSummaryDto> CreateRunInternalAsync(
        ApplicationDbContext db,
        string userId,
        long gameUrlId,
        long? presetId,
        string presetName,
        long presetGameId,
        int listingLimit,
        ManualCheckPriceRangeDto? priceRange,
        int? cooldownMinutes,
        int? cooldownSeconds,
        bool bypassCache,
        IReadOnlyList<long>? requestedProductIds,
        List<ManualCheckCriterionDto> criteria,
        ManualCheckPresetCombinationDto? presetCombination,
        CancellationToken cancellationToken)
    {
        var setup = await PrepareRunSetupInternalAsync(
            db,
            userId,
            gameUrlId,
            presetId,
            presetName,
            presetGameId,
            listingLimit,
            priceRange,
            cooldownMinutes,
            cooldownSeconds,
            bypassCache,
            requestedProductIds,
            criteria,
            presetCombination,
            cancellationToken);
        return await CreateRunRowAsync(db, setup, cancellationToken);
    }

    private static async Task<ManualCheckSetupDto> PrepareRunSetupInternalAsync(
        ApplicationDbContext db,
        string? userId,
        long gameUrlId,
        long? presetId,
        string presetName,
        long presetGameId,
        int listingLimit,
        ManualCheckPriceRangeDto? priceRange,
        int? cooldownMinutes,
        int? cooldownSeconds,
        bool bypassCache,
        IReadOnlyList<long>? requestedProductIds,
        List<ManualCheckCriterionDto> criteria,
        ManualCheckPresetCombinationDto? presetCombination,
        CancellationToken cancellationToken)
    {
        var cooldown = NormalizeCooldown(cooldownMinutes, cooldownSeconds);
        var normalizedPriceRange = NormalizePriceRange(priceRange);
        var normalizedProductIds = NormalizeProductIds(requestedProductIds);
        var gameUrl = await db.GameUrls
            .AsNoTracking()
            .Where(x =>
                x.Id == gameUrlId &&
                (x.UserId == null || x.UserId == userId) &&
                (x.Game.UserId == null || x.Game.UserId == userId))
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.GameId,
                GameName = x.Game.Name,
                x.ScrapingModeId,
                x.PartialUrl,
                x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Game URL was not found.");

        if (!gameUrl.IsActive || gameUrl.ScrapingModeId != (long)ScrapingModeEnum.ManualBatch)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "The selected Game URL must be an active Manual Batch source.");
        }

        if (gameUrl.GameId != presetGameId)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "The preset and Game URL must belong to the same game.");
        }

        if (string.IsNullOrWhiteSpace(gameUrl.PartialUrl))
        {
            throw RequestError(StatusCodes.Status400BadRequest, "The selected Game URL has no listing base URL.");
        }

        var productsQuery = db.GameUrlsProducts
            .AsNoTracking()
            .Where(x =>
                x.GameUrlId == gameUrlId &&
                x.Product.IsActive &&
                (x.Product.UserId == null || x.Product.UserId == userId) &&
                (x.Product.Game.UserId == null || x.Product.Game.UserId == userId));

        if (normalizedProductIds is not null)
        {
            productsQuery = productsQuery.Where(x => normalizedProductIds.Contains(x.ProductId));
        }

        var products = await productsQuery
            .Select(x => new
            {
                x.ProductId,
                ProductName = x.Product.Name,
                Tags = x.Product.ProductTags.Select(y => y.Tag.Name).ToList(),
                x.Product.Rating
            })
            .ToListAsync(cancellationToken);

        if (normalizedProductIds is not null)
        {
            if (products.Count != normalizedProductIds.Count)
            {
                throw RequestError(
                    StatusCodes.Status400BadRequest,
                    "Every selected product must be active and belong to the selected Game URL.");
            }

            var productsById = products.ToDictionary(x => x.ProductId);
            products = normalizedProductIds.Select(x => productsById[x]).ToList();
        }

        if (products.Count == 0)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "The selected Game URL has no active products.");
        }

        var inputs = new List<ManualCheckProductInputDto>(products.Count);
        foreach (var product in products)
        {
            var productName = product.ProductName?.Trim();
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw RequestError(StatusCodes.Status400BadRequest, $"Product #{product.ProductId} has no name.");
            }

            var fullUrl = gameUrl.PartialUrl + UrlUtilities.UrlEncode(productName);
            if (!ManualCheckMatcher.TryBuildListingUri(fullUrl, out _))
            {
                throw RequestError(StatusCodes.Status400BadRequest, "The selected source must produce HTTPS Steam Community market listing URLs.");
            }

            inputs.Add(new ManualCheckProductInputDto
            {
                ProductId = product.ProductId,
                ProductName = productName,
                GameUrlId = gameUrl.Id,
                GameUrlName = gameUrl.Name ?? $"Game URL #{gameUrl.Id}",
                FullUrl = fullUrl,
                Tags = product.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()!,
                Rating = product.Rating
            });
        }

        return new ManualCheckSetupDto
        {
            UserId = userId ?? string.Empty,
            PresetId = presetId,
            PresetName = presetName,
            GameId = gameUrl.GameId,
            GameName = gameUrl.GameName,
            GameUrlId = gameUrl.Id,
            GameUrlName = gameUrl.Name,
            ListingLimit = NormalizeListingLimit(listingLimit),
            PriceRange = normalizedPriceRange,
            CooldownMinutes = cooldown.Minutes,
            CooldownSeconds = cooldown.Seconds,
            BypassCache = bypassCache,
            RequestedProductIds = normalizedProductIds,
            PresetCombination = presetCombination,
            Criteria = criteria,
            Products = inputs,
            RequestedAtUtc = DateTime.UtcNow
        };
    }

    private async Task<ManualCheckRunSummaryDto> CreateRunRowAsync(
        ApplicationDbContext db,
        ManualCheckSetupDto setup,
        CancellationToken cancellationToken)
    {
        var now = UtcNow();
        var row = new ManualCheckRun
        {
            UserId = setup.UserId,
            ManualCheckPresetId = setup.PresetId,
            GameId = setup.GameId,
            GameUrlId = setup.GameUrlId,
            PresetName = setup.PresetName,
            SetupJson = JsonConvert.SerializeObject(setup),
            TotalProducts = setup.Products.Count,
            Status = ManualCheckRunStatusEnum.Queued,
            Date = now,
            CorrelationId = Guid.NewGuid().ToString("N")
        };

        db.ManualCheckRuns.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        return ToRunSummary(row);
    }

    private static List<long>? NormalizeProductIds(IReadOnlyList<long>? productIds)
    {
        if (productIds is null)
        {
            return null;
        }

        if (productIds.Count == 0)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "At least one product must be selected.");
        }

        if (productIds.Count > 10000)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "At most 10000 products can be selected.");
        }

        if (productIds.Any(x => x <= 0))
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Selected product identifiers must be positive.");
        }

        return productIds.Distinct().ToList();
    }

    private static ManualCheckPresetWriteDto NormalizePreset(
        ManualCheckPresetWriteDto input,
        bool requireGameUrls = true)
    {
        if (input.GameId <= 0)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Game is required.");
        }

        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length is < 1 or > ManualCheckPreset.NameMaxLength)
        {
            throw RequestError(StatusCodes.Status400BadRequest, $"Preset name must be between 1 and {ManualCheckPreset.NameMaxLength} characters.");
        }

        var cooldown = NormalizeCooldown(input.CooldownMinutes, input.CooldownSeconds);
        var requestedGameUrlIds = input.GameUrlIds ?? [];
        if ((requireGameUrls && requestedGameUrlIds.Count < 1) ||
            requestedGameUrlIds.Count > MaxPresetGameUrls ||
            requestedGameUrlIds.Any(x => x <= 0) ||
            requestedGameUrlIds.Distinct().Count() != requestedGameUrlIds.Count)
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                $"Choose between 1 and {MaxPresetGameUrls} distinct, positive Game URLs for the preset.");
        }
        var gameUrlIds = requestedGameUrlIds.Order().ToList();

        return new ManualCheckPresetWriteDto
        {
            GameId = input.GameId,
            ItemGroupId = input.ItemGroupId,
            Name = name,
            ListingLimit = NormalizeListingLimit(input.ListingLimit),
            PriceRange = NormalizePriceRange(input.PriceRange),
            CooldownMinutes = cooldown.Minutes,
            CooldownSeconds = cooldown.Seconds,
            GameUrlIds = gameUrlIds,
            Criteria = NormalizeCriteria(input.Criteria)
        };
    }

    private static int NormalizeListingLimit(int listingLimit)
    {
        if (listingLimit < 1)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Listing limit must be a positive whole number.");
        }

        return listingLimit;
    }

    private static ManualCheckPriceRangeDto? NormalizePriceRange(ManualCheckPriceRangeDto? priceRange)
    {
        if (priceRange is null)
        {
            return null;
        }

        if (!Enum.IsDefined(priceRange.Mode))
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Choose a supported price range mode.");
        }

        if (priceRange.MinimumPriceMinorUnits < 0 || priceRange.MaximumPriceMinorUnits < 0)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Price range values cannot be negative.");
        }

        switch (priceRange.Mode)
        {
            case ManualCheckPriceRangeModeEnum.Above when
                !priceRange.MinimumPriceMinorUnits.HasValue || priceRange.MaximumPriceMinorUnits.HasValue:
                throw RequestError(StatusCodes.Status400BadRequest, "Above price checks require only a minimum price.");
            case ManualCheckPriceRangeModeEnum.Below when
                priceRange.MinimumPriceMinorUnits.HasValue || !priceRange.MaximumPriceMinorUnits.HasValue:
                throw RequestError(StatusCodes.Status400BadRequest, "Below price checks require only a maximum price.");
            case ManualCheckPriceRangeModeEnum.Between when
                !priceRange.MinimumPriceMinorUnits.HasValue ||
                !priceRange.MaximumPriceMinorUnits.HasValue ||
                priceRange.MinimumPriceMinorUnits > priceRange.MaximumPriceMinorUnits:
                throw RequestError(StatusCodes.Status400BadRequest, "Between price checks require an ordered minimum and maximum price.");
        }

        return new ManualCheckPriceRangeDto
        {
            Mode = priceRange.Mode,
            MinimumPriceMinorUnits = priceRange.MinimumPriceMinorUnits,
            MaximumPriceMinorUnits = priceRange.MaximumPriceMinorUnits
        };
    }

    private static (int? Minutes, int? Seconds) NormalizeCooldown(int? minutes, int? seconds)
    {
        if (!minutes.HasValue && !seconds.HasValue)
        {
            return (null, null);
        }

        if (!minutes.HasValue || !seconds.HasValue)
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                "Cooldown minutes and seconds must both be provided or both be empty.");
        }

        if (minutes.Value is < 0 or > 59 || seconds.Value is < 0 or > 59)
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                "Cooldown minutes and seconds must each be between 0 and 59.");
        }

        return (minutes, seconds);
    }

    private static List<ManualCheckCriterionDto> NormalizeCriteria(IEnumerable<ManualCheckCriterionDto>? criteria)
    {
        var normalized = (criteria ?? [])
            .Select(x => new ManualCheckCriterionDto
            {
                ConditionOperatorId = x.ConditionOperatorId,
                ConditionOperatorName = null,
                OpenGroupCount = x.OpenGroupCount,
                CloseGroupCount = x.CloseGroupCount,
                NameContains = NormalizeTerm(x.NameContains),
                ValueContains = NormalizeTerm(x.ValueContains)
            })
            .ToList();

        if (normalized.Count is < 1 or > MaxCriteria)
        {
            throw RequestError(StatusCodes.Status400BadRequest, $"A preset must contain between 1 and {MaxCriteria} criteria.");
        }

        if (normalized.Any(x => x.NameContains is null && x.ValueContains is null))
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Each criterion requires a name or value search term.");
        }

        var expressionError = ManualCheckExpression.Validate(normalized);
        if (expressionError is not null)
        {
            throw RequestError(StatusCodes.Status400BadRequest, expressionError);
        }

        for (var index = 1; index < normalized.Count; index++)
        {
            var conditionOperatorId = normalized[index].ConditionOperatorId!.Value;
            normalized[index].ConditionOperatorName = GetOperatorName(conditionOperatorId);
        }

        return normalized;
    }

    private static async Task<ResolvedPresetCombination> ResolvePresetCombinationAsync(
        ApplicationDbContext db,
        string userId,
        long gameUrlId,
        ManualCheckPresetCombinationWriteDto combination,
        CancellationToken cancellationToken)
    {
        var terms = combination.Terms ?? [];
        if (terms.Count is < 2 or > MaxCombinationPresets)
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                $"A preset combination must contain between 2 and {MaxCombinationPresets} presets.");
        }

        if (terms.Any(x => x.PresetId <= 0) || terms.Select(x => x.PresetId).Distinct().Count() != terms.Count)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Preset combinations require unique, valid preset IDs.");
        }

        if (terms[0].Operator.HasValue || terms.Skip(1).Any(x =>
                !x.Operator.HasValue ||
                !Enum.IsDefined(x.Operator.Value)))
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                "The first combined preset cannot have an operator and every later preset requires AND or OR.");
        }

        var listingLimit = NormalizeListingLimit(combination.ListingLimit);
        var priceRange = NormalizePriceRange(combination.PriceRange);
        var cooldown = NormalizeCooldown(combination.CooldownMinutes, combination.CooldownSeconds);
        var presetIds = terms.Select(x => x.PresetId).ToList();
        var presets = await db.ManualCheckPresets
            .AsNoTracking()
            .Include(x => x.Criteria)
            .ThenInclude(x => x.ConditionOperator)
            .Include(x => x.GameUrls)
            .Where(x => presetIds.Contains(x.Id) && (x.UserId == null || x.UserId == userId))
            .ToListAsync(cancellationToken);

        if (presets.Count != presetIds.Count)
        {
            throw RequestError(StatusCodes.Status404NotFound, "One or more presets were not found.");
        }
        if (presets.Any(x => x.GameUrls.All(y => y.GameUrlId != gameUrlId)))
        {
            throw RequestError(
                StatusCodes.Status409Conflict,
                "Every combined preset must be assigned to the selected Game URL.");
        }

        var presetsById = presets.ToDictionary(x => x.Id);
        var orderedPresets = presetIds.Select(x => presetsById[x]).ToList();
        var gameId = orderedPresets[0].GameId;
        if (orderedPresets.Any(x => x.GameId != gameId))
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Combined presets must belong to the same game.");
        }

        var criteria = new List<ManualCheckCriterionDto>();
        var snapshotTerms = new List<ManualCheckPresetCombinationTermDto>(terms.Count);
        for (var index = 0; index < terms.Count; index++)
        {
            var term = terms[index];
            var preset = orderedPresets[index];
            var presetCriteria = ToCriterionDtos(preset.Criteria);
            if (presetCriteria.Count == 0)
            {
                throw RequestError(StatusCodes.Status400BadRequest, $"Preset #{preset.Id} has no criteria.");
            }

            presetCriteria[0].OpenGroupCount++;
            presetCriteria[^1].CloseGroupCount++;
            if (index == 0)
            {
                presetCriteria[0].ConditionOperatorId = null;
                presetCriteria[0].ConditionOperatorName = null;
            }
            else
            {
                var conditionOperator = term.Operator == ManualCheckPresetCombinationOperatorEnum.And
                    ? ManualCheckConditionOperatorEnum.And
                    : ManualCheckConditionOperatorEnum.Or;
                presetCriteria[0].ConditionOperatorId = (long)conditionOperator;
                presetCriteria[0].ConditionOperatorName = GetOperatorName((long)conditionOperator);
            }

            criteria.AddRange(presetCriteria);
            snapshotTerms.Add(new ManualCheckPresetCombinationTermDto
            {
                PresetId = preset.Id,
                PresetName = preset.Name,
                Operator = term.Operator
            });
        }

        var normalizedCriteria = NormalizeCriteria(criteria);
        var displayName = string.Join(
            " ",
            snapshotTerms.Select((term, index) => index == 0
                ? $"({term.PresetName})"
                : $"{term.Operator!.Value.ToString().ToUpperInvariant()} ({term.PresetName})"));
        if (displayName.Length > ManualCheckPreset.NameMaxLength)
        {
            displayName = displayName[..(ManualCheckPreset.NameMaxLength - 1)] + "…";
        }

        return new ResolvedPresetCombination
        {
            GameId = gameId,
            DisplayName = displayName,
            ListingLimit = listingLimit,
            PriceRange = priceRange,
            CooldownMinutes = cooldown.Minutes,
            CooldownSeconds = cooldown.Seconds,
            Criteria = normalizedCriteria,
            Snapshot = new ManualCheckPresetCombinationDto
            {
                ListingLimit = listingLimit,
                PriceRange = priceRange,
                CooldownMinutes = cooldown.Minutes,
                CooldownSeconds = cooldown.Seconds,
                Terms = snapshotTerms
            }
        };
    }

    private static string? NormalizeTerm(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (normalized.Length > ManualCheckCriterion.TermMaxLength)
        {
            throw RequestError(
                StatusCodes.Status400BadRequest,
                $"Criterion terms cannot exceed {ManualCheckCriterion.TermMaxLength} characters.");
        }

        return normalized;
    }

    private static async Task<string?> GetItemGroupNameAsync(
        ApplicationDbContext db,
        string userId,
        long gameId,
        long? itemGroupId,
        CancellationToken cancellationToken)
    {
        if (!itemGroupId.HasValue)
        {
            return null;
        }

        var itemGroupName = await db.ItemGroups
            .AsNoTracking()
            .Where(x =>
                x.Id == itemGroupId.Value &&
                x.GameId == gameId &&
                (x.UserId == null || x.UserId == userId) &&
                (x.Game.UserId == null || x.Game.UserId == userId))
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return itemGroupName
            ?? throw RequestError(StatusCodes.Status400BadRequest, "Item group was not found for the selected game.");
    }

    private static ManualCheckPresetDto ToPresetDto(
        ManualCheckPreset entity,
        string? gameName,
        string? itemGroupName,
        IReadOnlyCollection<long>? gameUrlIds = null)
    {
        return new ManualCheckPresetDto
        {
            Id = entity.Id,
            GameId = entity.GameId,
            GameName = gameName,
            ItemGroupId = entity.ItemGroupId,
            ItemGroupName = itemGroupName,
            Name = entity.Name,
            ListingLimit = entity.ListingLimit,
            PriceRange = ToPriceRangeDto(entity),
            CooldownMinutes = entity.CooldownMinutes,
            CooldownSeconds = entity.CooldownSeconds,
            GameUrlIds = (gameUrlIds ?? entity.GameUrls.Select(x => x.GameUrlId)).Order().ToList(),
            Criteria = ToCriterionDtos(entity.Criteria),
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Scope = entity.UserId is null ? "global" : "personal",
            CanEdit = entity.UserId is not null,
            CanClone = entity.UserId is null,
            CanRun = true
        };
    }

    private static ManualCheckPriceRangeDto? ToPriceRangeDto(ManualCheckPreset entity)
    {
        if (!entity.PriceRangeMode.HasValue)
        {
            return null;
        }

        return new ManualCheckPriceRangeDto
        {
            Mode = entity.PriceRangeMode.Value,
            MinimumPriceMinorUnits = entity.MinimumPriceMinorUnits,
            MaximumPriceMinorUnits = entity.MaximumPriceMinorUnits
        };
    }

    private ManualCheckRunSummaryDto ToRunSummary(ManualCheckRun row)
    {
        var setup = DeserializeSetup(row.SetupJson);
        return new ManualCheckRunSummaryDto
        {
            Id = row.Id,
            PresetId = row.ManualCheckPresetId,
            PresetName = row.PresetName,
            GameId = row.GameId,
            GameName = setup.GameName,
            GameUrlId = row.GameUrlId,
            GameUrlName = setup.GameUrlName,
            TotalProducts = row.TotalProducts,
            CheckedProducts = row.CheckedProducts,
            MatchedProducts = row.MatchedProducts,
            FailedProducts = row.FailedProducts,
            Progress = new ManualCheckProgressDto
            {
                TotalProducts = row.TotalProducts,
                CheckedProducts = row.CheckedProducts,
                MatchedProducts = row.MatchedProducts,
                FailedProducts = row.FailedProducts
            },
            Status = row.Status,
            PauseReason = row.PauseReason,
            Date = row.Date,
            StartedAtUtc = row.StartedAtUtc,
            CompletedAtUtc = row.CompletedAtUtc,
            DurationMilliseconds = GetRunDurationMilliseconds(row),
            CorrelationId = row.CorrelationId,
            ErrorText = row.ErrorText,
            AutomaticQueueRunId = row.AutomaticQueueRunBlock?.AutomaticQueueRunId,
            AutomaticQueueName = row.AutomaticQueueRunBlock?.AutomaticQueueRun.QueueName,
            AutomaticQueueBlockIndex = row.AutomaticQueueRunBlock?.SortOrder
        };
    }

    private ManualCheckRunDetailDto ToRunDetail(ManualCheckRun row)
    {
        var summary = ToRunSummary(row);
        return new ManualCheckRunDetailDto
        {
            Id = summary.Id,
            PresetId = summary.PresetId,
            PresetName = summary.PresetName,
            GameId = summary.GameId,
            GameName = summary.GameName,
            GameUrlId = summary.GameUrlId,
            GameUrlName = summary.GameUrlName,
            TotalProducts = summary.TotalProducts,
            CheckedProducts = summary.CheckedProducts,
            MatchedProducts = summary.MatchedProducts,
            FailedProducts = summary.FailedProducts,
            Progress = summary.Progress,
            Status = summary.Status,
            PauseReason = summary.PauseReason,
            Date = summary.Date,
            StartedAtUtc = summary.StartedAtUtc,
            CompletedAtUtc = summary.CompletedAtUtc,
            DurationMilliseconds = summary.DurationMilliseconds,
            CorrelationId = summary.CorrelationId,
            Setup = DeserializeSetup(row.SetupJson),
            Results = DeserializeResults(row.ResultsJson),
            ErrorText = summary.ErrorText
        };
    }

    private static ICollection<ManualCheckCriterion> CreateCriterionEntities(
        IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        return criteria.Select((criterion, index) => new ManualCheckCriterion
        {
            ConditionOperatorId = criterion.ConditionOperatorId,
            SortOrder = index,
            OpenGroupCount = criterion.OpenGroupCount,
            CloseGroupCount = criterion.CloseGroupCount,
            NameContains = criterion.NameContains,
            ValueContains = criterion.ValueContains
        }).ToList();
    }

    private static List<ManualCheckCriterionDto> ToCriterionDtos(
        IEnumerable<ManualCheckCriterion> criteria)
    {
        return criteria
            .OrderBy(x => x.SortOrder)
            .Select(x => new ManualCheckCriterionDto
            {
                ConditionOperatorId = x.ConditionOperatorId,
                ConditionOperatorName = x.ConditionOperator?.Name ??
                    (x.ConditionOperatorId.HasValue ? GetOperatorName(x.ConditionOperatorId.Value) : null),
                OpenGroupCount = x.OpenGroupCount,
                CloseGroupCount = x.CloseGroupCount,
                NameContains = x.NameContains,
                ValueContains = x.ValueContains
            })
            .ToList();
    }

    private static string GetOperatorName(long conditionOperatorId)
    {
        return (ManualCheckConditionOperatorEnum)conditionOperatorId switch
        {
            ManualCheckConditionOperatorEnum.And => "AND",
            ManualCheckConditionOperatorEnum.Or => "OR",
            ManualCheckConditionOperatorEnum.AndNot => "AND NOT",
            ManualCheckConditionOperatorEnum.OrNot => "OR NOT",
            ManualCheckConditionOperatorEnum.Xor => "XOR",
            ManualCheckConditionOperatorEnum.Nand => "NAND",
            ManualCheckConditionOperatorEnum.Nor => "NOR",
            _ => throw new InvalidOperationException($"Unknown manual-check condition operator #{conditionOperatorId}.")
        };
    }

    private static bool IsSupportedConditionOperator(long conditionOperatorId)
    {
        return conditionOperatorId is >= (long)ManualCheckConditionOperatorEnum.And and <= (long)ManualCheckConditionOperatorEnum.Nor &&
               Enum.IsDefined(typeof(ManualCheckConditionOperatorEnum), (int)conditionOperatorId);
    }

    private long? GetRunDurationMilliseconds(ManualCheckRun row)
    {
        if (!row.StartedAtUtc.HasValue)
        {
            return null;
        }

        var end = row.CompletedAtUtc ?? UtcNow();
        var pausedMilliseconds = row.AccumulatedPausedMilliseconds + GetCurrentPauseMilliseconds(row, end);
        return Math.Max(
            0,
            (long)Math.Round((end - row.StartedAtUtc.Value).TotalMilliseconds) - pausedMilliseconds);
    }

    private static long GetCurrentPauseMilliseconds(ManualCheckRun row, DateTime end)
    {
        if (!row.StartedAtUtc.HasValue || !row.PausedAtUtc.HasValue || end <= row.PausedAtUtc.Value)
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round((end - row.PausedAtUtc.Value).TotalMilliseconds));
    }

    private DateTime UtcNow()
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }

    private static async Task CloseUsageIntervalAsync(
        ApplicationDbContext db,
        long runId,
        DateTime endedAtUtc,
        CancellationToken cancellationToken)
    {
        var interval = await db.AutomationUsageIntervals
            .FirstOrDefaultAsync(
                x => x.ManualCheckRunId == runId && x.EndedAtUtc == null,
                cancellationToken);
        if (interval is not null)
        {
            interval.EndedAtUtc = endedAtUtc;
        }
    }

    private static bool AutomaticQueueBlockReferencesPreset(AutomaticQueueBlock block, long presetId)
    {
        var configuration = JsonConvert.DeserializeObject<AutomaticQueueBlockDto>(block.ConfigurationJson)
            ?? throw new InvalidOperationException("Automatic queue block configuration is invalid.");
        return configuration.TemplateMode switch
        {
            AutomaticQueueTemplateModeEnum.SavedPreset => configuration.PresetId == presetId,
            AutomaticQueueTemplateModeEnum.PresetCombination =>
                configuration.PresetCombination?.Terms.Any(term => term.PresetId == presetId) == true,
            _ => false
        };
    }

    private static ManualCheckSetupDto DeserializeSetup(string json)
    {
        var jsonObject = JObject.Parse(json);
        var setup = jsonObject.ToObject<ManualCheckSetupDto>() ?? new ManualCheckSetupDto();
        if (setup.Criteria.Count == 0)
        {
            return setup;
        }

        setup.Criteria[0].ConditionOperatorId = null;
        setup.Criteria[0].ConditionOperatorName = null;

        var legacyMatchModeText = jsonObject.GetValue(
            "MatchMode",
            StringComparison.OrdinalIgnoreCase)?.ToString();
        var hasLegacyMatchMode = Enum.TryParse<ManualCheckMatchModeEnum>(
            legacyMatchModeText,
            ignoreCase: true,
            out var legacyMatchMode);

        for (var index = 1; index < setup.Criteria.Count; index++)
        {
            if (!setup.Criteria[index].ConditionOperatorId.HasValue && hasLegacyMatchMode)
            {
                setup.Criteria[index].ConditionOperatorId = legacyMatchMode == ManualCheckMatchModeEnum.All
                    ? (long)ManualCheckConditionOperatorEnum.And
                    : (long)ManualCheckConditionOperatorEnum.Or;
            }

            var conditionOperatorId = setup.Criteria[index].ConditionOperatorId;
            if (conditionOperatorId.HasValue &&
                IsSupportedConditionOperator(conditionOperatorId.Value))
            {
                setup.Criteria[index].ConditionOperatorName = GetOperatorName(conditionOperatorId.Value);
            }
        }

        return setup;
    }

    private static ManualCheckRunResultsDto DeserializeResults(string? json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? new ManualCheckRunResultsDto()
            : JsonConvert.DeserializeObject<ManualCheckRunResultsDto>(json) ?? new ManualCheckRunResultsDto();
    }

    private static ManualCheckRequestException RequestError(int statusCode, string message)
    {
        return new ManualCheckRequestException(statusCode, message);
    }

    private sealed class ResolvedPresetCombination
    {
        public long GameId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public int ListingLimit { get; init; }
        public ManualCheckPriceRangeDto? PriceRange { get; init; }
        public int? CooldownMinutes { get; init; }
        public int? CooldownSeconds { get; init; }
        public List<ManualCheckCriterionDto> Criteria { get; init; } = [];
        public ManualCheckPresetCombinationDto Snapshot { get; init; } = new();
    }
}

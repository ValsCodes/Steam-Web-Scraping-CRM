using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.Utilities;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;

namespace SteamApp.WebAPI.Services;

public sealed class ManualCheckDataService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory) : IManualCheckDataService
{
    private const int MaxCriteria = 25;

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
        await using var db = dbContextFactory.CreateDbContext();

        IQueryable<ManualCheckPreset> query = db.ManualCheckPresets
            .AsNoTracking()
            .Include(x => x.Game)
            .Include(x => x.Criteria)
            .ThenInclude(x => x.ConditionOperator);
        if (gameId.HasValue)
        {
            query = query.Where(x => x.GameId == gameId.Value);
        }

        var presets = await query
            .OrderBy(x => x.Game.Name)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return presets.Select(x => ToPresetDto(x, x.Game.Name)).ToList();
    }

    public async Task<ManualCheckPresetDto> CreatePresetAsync(
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePreset(input);
        await using var db = dbContextFactory.CreateDbContext();

        var gameName = await db.Games
            .AsNoTracking()
            .Where(x => x.Id == normalized.GameId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (gameName is null)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Game was not found.");
        }

        var duplicate = await db.ManualCheckPresets.AnyAsync(
            x => x.GameId == normalized.GameId && x.Name == normalized.Name,
            cancellationToken);
        if (duplicate)
        {
            throw RequestError(StatusCodes.Status409Conflict, "A preset with this name already exists for the game.");
        }

        var now = DateTime.UtcNow;
        var entity = new ManualCheckPreset
        {
            GameId = normalized.GameId,
            Name = normalized.Name,
            ListingLimit = normalized.ListingLimit,
            CooldownMinutes = normalized.CooldownMinutes,
            CooldownSeconds = normalized.CooldownSeconds,
            Criteria = CreateCriterionEntities(normalized.Criteria),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.ManualCheckPresets.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return ToPresetDto(entity, gameName);
    }

    public async Task<ManualCheckPresetDto> UpdatePresetAsync(
        long id,
        ManualCheckPresetWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePreset(input);
        await using var db = dbContextFactory.CreateDbContext();

        var entity = await db.ManualCheckPresets
            .Include(x => x.Criteria)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");

        var gameName = await db.Games
            .AsNoTracking()
            .Where(x => x.Id == normalized.GameId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (gameName is null)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Game was not found.");
        }

        var duplicate = await db.ManualCheckPresets.AnyAsync(
            x => x.Id != id && x.GameId == normalized.GameId && x.Name == normalized.Name,
            cancellationToken);
        if (duplicate)
        {
            throw RequestError(StatusCodes.Status409Conflict, "A preset with this name already exists for the game.");
        }

        entity.GameId = normalized.GameId;
        entity.Name = normalized.Name;
        entity.ListingLimit = normalized.ListingLimit;
        entity.CooldownMinutes = normalized.CooldownMinutes;
        entity.CooldownSeconds = normalized.CooldownSeconds;
        var existingCriteria = entity.Criteria.OrderBy(x => x.SortOrder).ToList();
        for (var index = 0; index < normalized.Criteria.Count; index++)
        {
            var criterion = index < existingCriteria.Count
                ? existingCriteria[index]
                : new ManualCheckCriterion { ManualCheckPreset = entity };
            criterion.ConditionOperatorId = normalized.Criteria[index].ConditionOperatorId;
            criterion.SortOrder = index;
            criterion.NameContains = normalized.Criteria[index].NameContains;
            criterion.ValueContains = normalized.Criteria[index].ValueContains;
            if (index >= existingCriteria.Count)
            {
                entity.Criteria.Add(criterion);
            }
        }
        if (existingCriteria.Count > normalized.Criteria.Count)
        {
            var removedCriteria = existingCriteria.Skip(normalized.Criteria.Count).ToList();
            db.ManualCheckCriteria.RemoveRange(removedCriteria);
            foreach (var removedCriterion in removedCriteria)
            {
                entity.Criteria.Remove(removedCriterion);
            }
        }
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return ToPresetDto(entity, gameName);
    }

    public async Task DeletePresetAsync(long id, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var entity = await db.ManualCheckPresets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");

        db.ManualCheckPresets.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ManualCheckRunSummaryDto> CreateRunAsync(
        long gameUrlId,
        long presetId,
        bool bypassCache,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var preset = await db.ManualCheckPresets
            .AsNoTracking()
            .Include(x => x.Criteria)
            .ThenInclude(x => x.ConditionOperator)
            .FirstOrDefaultAsync(x => x.Id == presetId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Preset was not found.");

        return await CreateRunInternalAsync(
            db,
            gameUrlId,
            preset.Id,
            preset.Name,
            preset.GameId,
            preset.ListingLimit,
            preset.CooldownMinutes,
            preset.CooldownSeconds,
            bypassCache,
            ToCriterionDtos(preset.Criteria),
            cancellationToken);
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
            original.GameUrlId,
            presetId,
            original.PresetName,
            original.GameId,
            setup.ListingLimit,
            setup.CooldownMinutes,
            setup.CooldownSeconds,
            false,
            NormalizeCriteria(setup.Criteria),
            cancellationToken);
    }

    public async Task<ManualCheckRunDetailDto> CancelAsync(
        long runId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns
            .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Run was not found.");

        if (row.Status is not (ManualCheckRunStatusEnum.Queued or ManualCheckRunStatusEnum.Running))
        {
            throw RequestError(
                StatusCodes.Status409Conflict,
                $"Run #{runId} is already {row.Status} and cannot be canceled.");
        }

        row.Status = ManualCheckRunStatusEnum.Canceled;
        row.ErrorText = row.CheckedProducts == 0
            ? "Canceled by the user before any products were checked."
            : $"Canceled by the user after checking {row.CheckedProducts} of {row.TotalProducts} products.";
        row.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return ToRunDetail(row);
    }

    public async Task<IReadOnlyList<ManualCheckRunSummaryDto>> GetRunsAsync(
        long? gameId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var query = db.ManualCheckRuns.AsNoTracking();
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

    public async Task<ManualCheckRunDetailDto?> GetRunAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null)
        {
            return null;
        }

        return ToRunDetail(row);
    }

    public async Task<ManualCheckSetupDto?> MarkRunningAndGetSetupAsync(
        long id,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null || row.Status != ManualCheckRunStatusEnum.Queued)
        {
            return null;
        }

        row.Status = ManualCheckRunStatusEnum.Running;
        row.StartedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return DeserializeSetup(row.SetupJson);
    }

    public async Task UpdateProgressAsync(
        long id,
        int checkedProducts,
        int matchedProducts,
        int failedProducts,
        ManualCheckRunResultsDto results,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null || row.Status != ManualCheckRunStatusEnum.Running)
        {
            return;
        }

        row.CheckedProducts = checkedProducts;
        row.MatchedProducts = matchedProducts;
        row.FailedProducts = failedProducts;
        row.ResultsJson = JsonConvert.SerializeObject(results);
        await db.SaveChangesAsync(cancellationToken);
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

        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null || row.Status != ManualCheckRunStatusEnum.Running)
        {
            return;
        }

        row.Status = status;
        row.CheckedProducts = row.TotalProducts;
        row.MatchedProducts = results.Matches.Count;
        row.FailedProducts = results.Errors.Count;
        row.ResultsJson = JsonConvert.SerializeObject(results);
        row.ErrorText = status == ManualCheckRunStatusEnum.CompletedWithErrors
            ? $"{results.Errors.Count} product check(s) failed."
            : null;
        row.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task FailAsync(
        long id,
        string errorText,
        ManualCheckRunResultsDto? results,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.ManualCheckRuns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (row is null || row.Status is not (ManualCheckRunStatusEnum.Queued or ManualCheckRunStatusEnum.Running))
        {
            return;
        }

        row.Status = ManualCheckRunStatusEnum.Failed;
        row.ResultsJson = results is null ? row.ResultsJson : JsonConvert.SerializeObject(results);
        row.MatchedProducts = results?.Matches.Count ?? row.MatchedProducts;
        row.FailedProducts = results?.Errors.Count ?? row.FailedProducts;
        row.ErrorText = errorText;
        row.StartedAtUtc ??= DateTime.UtcNow;
        row.CompletedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkInterruptedRunsFailedAsync(CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var interrupted = await db.ManualCheckRuns
            .Where(x => x.Status == ManualCheckRunStatusEnum.Queued || x.Status == ManualCheckRunStatusEnum.Running)
            .ToListAsync(cancellationToken);

        if (interrupted.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var row in interrupted)
        {
            row.Status = ManualCheckRunStatusEnum.Failed;
            row.ErrorText = "The manual check was interrupted by an API restart.";
            row.StartedAtUtc ??= now;
            row.CompletedAtUtc = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<ManualCheckRunSummaryDto> CreateRunInternalAsync(
        ApplicationDbContext db,
        long gameUrlId,
        long? presetId,
        string presetName,
        long presetGameId,
        int listingLimit,
        int? cooldownMinutes,
        int? cooldownSeconds,
        bool bypassCache,
        List<ManualCheckCriterionDto> criteria,
        CancellationToken cancellationToken)
    {
        var cooldown = NormalizeCooldown(cooldownMinutes, cooldownSeconds);
        var gameUrl = await db.GameUrls
            .AsNoTracking()
            .Where(x => x.Id == gameUrlId)
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

        var products = await db.GameUrlsProducts
            .AsNoTracking()
            .Where(x => x.GameUrlId == gameUrlId && x.Product.IsActive)
            .Select(x => new
            {
                x.ProductId,
                ProductName = x.Product.Name,
                Tags = x.Product.ProductTags.Select(y => y.Tag.Name).ToList(),
                x.Product.Rating
            })
            .ToListAsync(cancellationToken);

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

        var now = DateTime.UtcNow;
        var setup = new ManualCheckSetupDto
        {
            PresetId = presetId,
            PresetName = presetName,
            GameId = gameUrl.GameId,
            GameName = gameUrl.GameName,
            GameUrlId = gameUrl.Id,
            GameUrlName = gameUrl.Name,
            ListingLimit = NormalizeListingLimit(listingLimit),
            CooldownMinutes = cooldown.Minutes,
            CooldownSeconds = cooldown.Seconds,
            BypassCache = bypassCache,
            Criteria = criteria,
            Products = inputs,
            RequestedAtUtc = now
        };

        var row = new ManualCheckRun
        {
            ManualCheckPresetId = presetId,
            GameId = gameUrl.GameId,
            GameUrlId = gameUrl.Id,
            PresetName = presetName,
            SetupJson = JsonConvert.SerializeObject(setup),
            TotalProducts = inputs.Count,
            Status = ManualCheckRunStatusEnum.Queued,
            Date = now,
            CorrelationId = Guid.NewGuid().ToString("N")
        };

        db.ManualCheckRuns.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        return ToRunSummary(row);
    }

    private static ManualCheckPresetWriteDto NormalizePreset(ManualCheckPresetWriteDto input)
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

        return new ManualCheckPresetWriteDto
        {
            GameId = input.GameId,
            Name = name,
            ListingLimit = NormalizeListingLimit(input.ListingLimit),
            CooldownMinutes = cooldown.Minutes,
            CooldownSeconds = cooldown.Seconds,
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

        if (normalized[0].ConditionOperatorId.HasValue)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "The first criterion cannot have a condition operator.");
        }

        for (var index = 1; index < normalized.Count; index++)
        {
            var conditionOperatorId = normalized[index].ConditionOperatorId;
            if (!conditionOperatorId.HasValue ||
                !IsSupportedConditionOperator(conditionOperatorId.Value))
            {
                throw RequestError(
                    StatusCodes.Status400BadRequest,
                    $"Criterion #{index + 1} requires a supported condition operator.");
            }

            normalized[index].ConditionOperatorName = GetOperatorName(conditionOperatorId.Value);
        }

        return normalized;
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

    private static ManualCheckPresetDto ToPresetDto(
        ManualCheckPreset entity,
        string? gameName)
    {
        return new ManualCheckPresetDto
        {
            Id = entity.Id,
            GameId = entity.GameId,
            GameName = gameName,
            Name = entity.Name,
            ListingLimit = entity.ListingLimit,
            CooldownMinutes = entity.CooldownMinutes,
            CooldownSeconds = entity.CooldownSeconds,
            Criteria = ToCriterionDtos(entity.Criteria),
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static ManualCheckRunSummaryDto ToRunSummary(ManualCheckRun row)
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
            Date = row.Date,
            StartedAtUtc = row.StartedAtUtc,
            CompletedAtUtc = row.CompletedAtUtc,
            DurationMilliseconds = GetRunDurationMilliseconds(row),
            CorrelationId = row.CorrelationId,
            ErrorText = row.ErrorText
        };
    }

    private static ManualCheckRunDetailDto ToRunDetail(ManualCheckRun row)
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

    private static long? GetRunDurationMilliseconds(ManualCheckRun row)
    {
        if (!row.StartedAtUtc.HasValue)
        {
            return null;
        }

        var end = row.CompletedAtUtc ?? DateTime.UtcNow;
        return Math.Max(0, (long)Math.Round((end - row.StartedAtUtc.Value).TotalMilliseconds));
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
}

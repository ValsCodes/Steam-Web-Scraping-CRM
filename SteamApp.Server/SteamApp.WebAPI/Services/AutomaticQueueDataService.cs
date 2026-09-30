using System.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SteamApp.Application.DTOs.AutomaticQueue;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;

namespace SteamApp.WebAPI.Services;

public sealed class AutomaticQueueDataService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IManualCheckDataService manualCheckDataService,
    IManualCheckQueue manualCheckQueue,
    TimeProvider timeProvider,
    ILogger<AutomaticQueueDataService> logger) : IAutomaticQueueDataService
{
    private const int MaxBlocks = 100;
    private const int MaxDelaySeconds = 7 * 24 * 60 * 60;
    private const int MaxSelectedProducts = 10000;

    public async Task<IReadOnlyList<AutomaticQueueDto>> GetDefinitionsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var rows = await db.AutomaticQueueDefinitions
            .AsNoTracking()
            .Include(x => x.Blocks)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        var activeRuns = await db.AutomaticQueueRuns
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.AutomaticQueueDefinitionId != null &&
                (x.Status == AutomaticQueueRunStatusEnum.Queued ||
                 x.Status == AutomaticQueueRunStatusEnum.Running ||
                 x.Status == AutomaticQueueRunStatusEnum.PauseRequested ||
                 x.Status == AutomaticQueueRunStatusEnum.Paused))
            .ToDictionaryAsync(x => x.AutomaticQueueDefinitionId!.Value, x => x.Id, cancellationToken);

        return rows.Select(x => ToDefinitionDto(
            x,
            activeRuns.TryGetValue(x.Id, out var activeRunId) ? activeRunId : null)).ToList();
    }

    public async Task<AutomaticQueueDto?> GetDefinitionAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.AutomaticQueueDefinitions
            .AsNoTracking()
            .Include(x => x.Blocks)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (row is null)
        {
            return null;
        }

        var activeRunId = await db.AutomaticQueueRuns
            .AsNoTracking()
            .Where(x =>
                x.AutomaticQueueDefinitionId == id &&
                x.UserId == userId &&
                (x.Status == AutomaticQueueRunStatusEnum.Queued ||
                 x.Status == AutomaticQueueRunStatusEnum.Running ||
                 x.Status == AutomaticQueueRunStatusEnum.PauseRequested ||
                 x.Status == AutomaticQueueRunStatusEnum.Paused))
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return ToDefinitionDto(row, activeRunId);
    }

    public async Task<AutomaticQueueDto> CreateDefinitionAsync(
        string userId,
        AutomaticQueueWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = await NormalizeDefinitionAsync(userId, input, cancellationToken);
        await using var db = dbContextFactory.CreateDbContext();
        if (await db.AutomaticQueueDefinitions.AnyAsync(
                x => x.UserId == userId && x.Name == normalized.Name,
                cancellationToken))
        {
            throw RequestError(StatusCodes.Status409Conflict, "A queue with this name already exists.");
        }

        var now = UtcNow();
        var row = new AutomaticQueueDefinition
        {
            UserId = userId,
            Name = normalized.Name,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Blocks = CreateBlockEntities(normalized.Blocks)
        };
        db.AutomaticQueueDefinitions.Add(row);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw RequestError(StatusCodes.Status409Conflict, "A queue with this name already exists.");
        }
        return ToDefinitionDto(row, null);
    }

    public async Task<AutomaticQueueDto> UpdateDefinitionAsync(
        long id,
        string userId,
        AutomaticQueueWriteDto input,
        CancellationToken cancellationToken)
    {
        var normalized = await NormalizeDefinitionAsync(userId, input, cancellationToken);
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.AutomaticQueueDefinitions
            .Include(x => x.Blocks)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Queue was not found.");
        await EnsureDefinitionIsIdleAsync(db, id, userId, cancellationToken);
        if (await db.AutomaticQueueDefinitions.AnyAsync(
                x => x.Id != id && x.UserId == userId && x.Name == normalized.Name,
                cancellationToken))
        {
            throw RequestError(StatusCodes.Status409Conflict, "A queue with this name already exists.");
        }

        db.AutomaticQueueBlocks.RemoveRange(row.Blocks);
        row.Blocks = CreateBlockEntities(normalized.Blocks);
        row.Name = normalized.Name;
        row.UpdatedAtUtc = UtcNow();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw RequestError(StatusCodes.Status409Conflict, "A queue with this name already exists.");
        }
        return ToDefinitionDto(row, null);
    }

    public async Task DeleteDefinitionAsync(long id, string userId, CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await db.AutomaticQueueDefinitions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Queue was not found.");
        await EnsureDefinitionIsIdleAsync(db, id, userId, cancellationToken);
        db.AutomaticQueueDefinitions.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AutomaticQueueRunDto> StartRunAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var strategyDb = dbContextFactory.CreateDbContext();
        var strategy = strategyDb.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(
                () => StartRunAttemptAsync(id, userId, cancellationToken));
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "Concurrent start was rejected for automatic queue {QueueId}.", id);
            throw RequestError(StatusCodes.Status409Conflict, "This queue already has an active run.");
        }
    }

    private async Task<AutomaticQueueRunDto> StartRunAttemptAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
        var definition = await db.AutomaticQueueDefinitions
            .AsNoTracking()
            .Include(x => x.Blocks)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Queue was not found.");
        await EnsureDefinitionIsIdleAsync(db, id, userId, cancellationToken);

        var snapshots = new List<AutomaticQueueRunBlockSetupDto>(definition.Blocks.Count);
        foreach (var block in definition.Blocks.OrderBy(x => x.SortOrder))
        {
            var configuration = DeserializeConfiguration(block.ConfigurationJson);
            snapshots.Add(await CreateRunSnapshotAsync(db, userId, configuration, cancellationToken));
        }

        var now = UtcNow();
        var run = new AutomaticQueueRun
        {
            AutomaticQueueDefinitionId = definition.Id,
            UserId = userId,
            QueueName = definition.Name,
            Status = AutomaticQueueRunStatusEnum.Queued,
            CurrentBlockIndex = 0,
            Date = now,
            CorrelationId = Guid.NewGuid().ToString("N"),
            Blocks = snapshots.Select((snapshot, index) => new AutomaticQueueRunBlock
            {
                BlockKey = snapshot.Configuration.Key,
                SortOrder = index,
                BlockType = snapshot.Configuration.Type,
                Status = AutomaticQueueBlockRunStatusEnum.Pending,
                SetupJson = JsonConvert.SerializeObject(snapshot),
                RemainingDelaySeconds = snapshot.DelaySeconds
            }).ToList()
        };
        db.AutomaticQueueRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return ToRunDto(run);
    }

    public async Task<IReadOnlyList<AutomaticQueueRunDto>> GetRunsAsync(
        string userId,
        long? queueId,
        int take,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var query = db.AutomaticQueueRuns
            .AsNoTracking()
            .Include(x => x.Blocks)
            .ThenInclude(x => x.ManualCheckRun)
            .Where(x => x.UserId == userId);
        if (queueId.HasValue)
        {
            query = query.Where(x => x.AutomaticQueueDefinitionId == queueId.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
        return rows.Select(ToRunDto).ToList();
    }

    public async Task<AutomaticQueueRunDto?> GetRunAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var row = await LoadRunAsync(db, id, userId, tracking: false, cancellationToken);
        return row is null ? null : ToRunDto(row);
    }

    public async Task<AutomaticQueueRunDto> PauseRunAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var run = await LoadRunAsync(db, id, userId, tracking: true, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Queue run was not found.");
        if (run.Status is not (AutomaticQueueRunStatusEnum.Queued or AutomaticQueueRunStatusEnum.Running or AutomaticQueueRunStatusEnum.PauseRequested))
        {
            throw RequestError(StatusCodes.Status409Conflict, $"Queue run #{id} is {run.Status} and cannot be paused.");
        }

        var block = CurrentBlock(run);
        if (block is null || block.Status == AutomaticQueueBlockRunStatusEnum.Pending)
        {
            if (block is not null)
            {
                block.Status = AutomaticQueueBlockRunStatusEnum.Paused;
            }
            run.Status = AutomaticQueueRunStatusEnum.Paused;
        }
        else if (block.BlockType == AutomaticQueueBlockTypeEnum.Delay)
        {
            block.RemainingDelaySeconds = block.WaitUntilUtc.HasValue
                ? Math.Max(1, (int)Math.Ceiling((block.WaitUntilUtc.Value - UtcNow()).TotalSeconds))
                : Math.Clamp(block.RemainingDelaySeconds ?? 1, 1, MaxDelaySeconds);
            block.WaitUntilUtc = null;
            block.Status = AutomaticQueueBlockRunStatusEnum.Paused;
            run.Status = AutomaticQueueRunStatusEnum.Paused;
        }
        else if (block.ManualCheckRunId.HasValue)
        {
            var child = await manualCheckDataService.PauseAsync(block.ManualCheckRunId.Value, cancellationToken);
            manualCheckQueue.TryPause(block.ManualCheckRunId.Value);
            run.Status = child.Status == ManualCheckRunStatusEnum.Paused
                ? AutomaticQueueRunStatusEnum.Paused
                : AutomaticQueueRunStatusEnum.PauseRequested;
            block.Status = child.Status == ManualCheckRunStatusEnum.Paused
                ? AutomaticQueueBlockRunStatusEnum.Paused
                : AutomaticQueueBlockRunStatusEnum.Running;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw RequestError(StatusCodes.Status409Conflict, "The queue changed while the pause was requested. Refresh and try again.");
        }
        return ToRunDto(run);
    }

    public async Task<AutomaticQueueRunDto> ContinueRunAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var run = await LoadRunAsync(db, id, userId, tracking: true, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Queue run was not found.");
        if (run.Status != AutomaticQueueRunStatusEnum.Paused)
        {
            throw RequestError(StatusCodes.Status409Conflict, $"Queue run #{id} is {run.Status} and cannot be continued.");
        }

        var block = CurrentBlock(run);
        long? childToEnqueue = null;
        if (block is not null && block.Status == AutomaticQueueBlockRunStatusEnum.Paused)
        {
            if (block.BlockType == AutomaticQueueBlockTypeEnum.Delay)
            {
                var seconds = Math.Clamp(block.RemainingDelaySeconds ?? 1, 1, MaxDelaySeconds);
                block.WaitUntilUtc = UtcNow().AddSeconds(seconds);
                block.Status = AutomaticQueueBlockRunStatusEnum.Running;
            }
            else if (block.ManualCheckRunId.HasValue)
            {
                await manualCheckDataService.ContinueAsync(block.ManualCheckRunId.Value, cancellationToken);
                block.Status = AutomaticQueueBlockRunStatusEnum.Running;
                childToEnqueue = block.ManualCheckRunId.Value;
            }
            else
            {
                block.Status = AutomaticQueueBlockRunStatusEnum.Pending;
            }
        }

        run.Status = AutomaticQueueRunStatusEnum.Running;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (childToEnqueue.HasValue)
            {
                await manualCheckDataService.PauseAsync(childToEnqueue.Value, CancellationToken.None);
            }
            throw RequestError(StatusCodes.Status409Conflict, "The queue changed while it was being resumed. Refresh and try again.");
        }
        if (childToEnqueue.HasValue)
        {
            try
            {
                await manualCheckQueue.EnqueueAsync(childToEnqueue.Value, cancellationToken);
            }
            catch (Exception exception)
            {
                await manualCheckDataService.PauseAsync(childToEnqueue.Value, CancellationToken.None);
                block!.Status = AutomaticQueueBlockRunStatusEnum.Paused;
                run.Status = AutomaticQueueRunStatusEnum.Paused;
                await db.SaveChangesAsync(CancellationToken.None);
                logger.LogWarning(
                    exception,
                    "Manual child run {ManualCheckRunId} could not be requeued for automatic queue run {QueueRunId}.",
                    childToEnqueue.Value,
                    run.Id);
                throw RequestError(StatusCodes.Status503ServiceUnavailable, "The manual check could not be requeued. The queue remains paused.");
            }
        }
        return ToRunDto(run);
    }

    public async Task<AutomaticQueueRunDto> CancelRunAsync(
        long id,
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var run = await LoadRunAsync(db, id, userId, tracking: true, cancellationToken)
            ?? throw RequestError(StatusCodes.Status404NotFound, "Queue run was not found.");
        if (!IsActiveStatus(run.Status))
        {
            throw RequestError(StatusCodes.Status409Conflict, $"Queue run #{id} is {run.Status} and cannot be canceled.");
        }

        var current = CurrentBlock(run);
        if (current?.ManualCheckRunId is long childRunId &&
            current.ManualCheckRun?.Status is ManualCheckRunStatusEnum.Queued or ManualCheckRunStatusEnum.Running or ManualCheckRunStatusEnum.PauseRequested or ManualCheckRunStatusEnum.Paused)
        {
            await manualCheckDataService.CancelAsync(childRunId, cancellationToken);
            manualCheckQueue.TryCancel(childRunId);
        }
        if (current is not null && !IsTerminalBlockStatus(current.Status))
        {
            current.Status = AutomaticQueueBlockRunStatusEnum.Canceled;
            current.CompletedAtUtc = UtcNow();
        }
        foreach (var remaining in run.Blocks.Where(x => x.SortOrder > run.CurrentBlockIndex && !IsTerminalBlockStatus(x.Status)))
        {
            remaining.Status = AutomaticQueueBlockRunStatusEnum.Skipped;
        }
        run.Status = AutomaticQueueRunStatusEnum.Canceled;
        run.CompletedAtUtc = UtcNow();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw RequestError(StatusCodes.Status409Conflict, "The queue changed while it was being canceled. Refresh and try again.");
        }
        return ToRunDto(run);
    }

    public async Task RecoverInterruptedRunsAsync(CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var runs = await db.AutomaticQueueRuns
            .Include(x => x.Blocks)
            .ThenInclude(x => x.ManualCheckRun)
            .Where(x =>
                x.Status == AutomaticQueueRunStatusEnum.Queued ||
                x.Status == AutomaticQueueRunStatusEnum.Running ||
                x.Status == AutomaticQueueRunStatusEnum.PauseRequested)
            .ToListAsync(cancellationToken);
        var now = UtcNow();
        foreach (var run in runs)
        {
            var block = CurrentBlock(run);
            if (block is not null && !IsTerminalBlockStatus(block.Status))
            {
                if (block.BlockType == AutomaticQueueBlockTypeEnum.Delay && block.WaitUntilUtc.HasValue)
                {
                    block.RemainingDelaySeconds = Math.Max(1, (int)Math.Ceiling((block.WaitUntilUtc.Value - now).TotalSeconds));
                    block.WaitUntilUtc = null;
                }
                block.Status = AutomaticQueueBlockRunStatusEnum.Paused;
                if (block.ManualCheckRun is not null &&
                    block.ManualCheckRun.Status is ManualCheckRunStatusEnum.Queued or ManualCheckRunStatusEnum.Running or ManualCheckRunStatusEnum.PauseRequested)
                {
                    block.ManualCheckRun.Status = ManualCheckRunStatusEnum.Paused;
                }
            }
            run.Status = AutomaticQueueRunStatusEnum.Paused;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessActiveRunsAsync(CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var runs = await db.AutomaticQueueRuns
            .Include(x => x.Blocks)
            .ThenInclude(x => x.ManualCheckRun)
            .Where(x =>
                x.Status == AutomaticQueueRunStatusEnum.Queued ||
                x.Status == AutomaticQueueRunStatusEnum.Running ||
                x.Status == AutomaticQueueRunStatusEnum.PauseRequested)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        foreach (var run in runs)
        {
            try
            {
                await ProcessRunAsync(db, run, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (DbUpdateConcurrencyException)
            {
                db.ChangeTracker.Clear();
                logger.LogDebug(
                    "Automatic queue run {QueueRunId} changed while block {BlockIndex} was being advanced.",
                    run.Id,
                    run.CurrentBlockIndex);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Automatic queue run {QueueRunId} failed at block {BlockIndex} with correlation {CorrelationId}.",
                    run.Id,
                    run.CurrentBlockIndex,
                    run.CorrelationId);
                run.Status = AutomaticQueueRunStatusEnum.Failed;
                run.ErrorText = "The queue orchestrator could not continue this run.";
                run.CompletedAtUtc = UtcNow();
                var failedBlock = CurrentBlock(run);
                if (failedBlock is not null && !IsTerminalBlockStatus(failedBlock.Status))
                {
                    failedBlock.Status = AutomaticQueueBlockRunStatusEnum.Failed;
                    failedBlock.ErrorText = run.ErrorText;
                    failedBlock.CompletedAtUtc = run.CompletedAtUtc;
                }
                foreach (var remaining in run.Blocks.Where(x => x.SortOrder > run.CurrentBlockIndex && !IsTerminalBlockStatus(x.Status)))
                {
                    remaining.Status = AutomaticQueueBlockRunStatusEnum.Skipped;
                }
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ProcessRunAsync(
        ApplicationDbContext db,
        AutomaticQueueRun run,
        CancellationToken cancellationToken)
    {
        if (run.Status == AutomaticQueueRunStatusEnum.PauseRequested)
        {
            await ReconcilePauseRequestedAsync(db, run, cancellationToken);
            return;
        }

        if (!await TryMarkRunRunningAsync(db, run, cancellationToken))
        {
            return;
        }
        var block = CurrentBlock(run);
        if (block is null)
        {
            CompleteQueue(run);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (block.Status == AutomaticQueueBlockRunStatusEnum.Pending)
        {
            if (!await TryClaimPendingBlockAsync(db, block, cancellationToken))
            {
                return;
            }

            if (block.BlockType == AutomaticQueueBlockTypeEnum.Delay)
            {
                var snapshot = DeserializeSnapshot(block.SetupJson);
                block.RemainingDelaySeconds = snapshot.DelaySeconds;
                block.WaitUntilUtc = UtcNow().AddSeconds(snapshot.DelaySeconds!.Value);
                await db.SaveChangesAsync(cancellationToken);
                return;
            }

            var manualSnapshot = DeserializeSnapshot(block.SetupJson).ManualCheckSetup
                ?? throw new InvalidOperationException("Manual-check queue block has no setup snapshot.");
            var child = await manualCheckDataService.CreateRunFromSetupAsync(manualSnapshot, cancellationToken);
            block.ManualCheckRunId = child.Id;
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await manualCheckDataService.FailAsync(
                    child.Id,
                    "The automatic queue could not link this manual check.",
                    null,
                    CancellationToken.None);
                throw;
            }
            try
            {
                await manualCheckQueue.EnqueueAsync(child.Id, cancellationToken);
            }
            catch (Exception exception)
            {
                await manualCheckDataService.FailAsync(child.Id, "The queue could not enqueue this manual check.", null, cancellationToken);
                block.Status = AutomaticQueueBlockRunStatusEnum.Failed;
                block.ErrorText = "The manual check could not be queued.";
                block.CompletedAtUtc = UtcNow();
                Advance(run);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogWarning(exception, "Manual child run {ManualCheckRunId} could not be queued.", child.Id);
            }
            return;
        }

        if (block.BlockType == AutomaticQueueBlockTypeEnum.Delay)
        {
            if (block.WaitUntilUtc <= UtcNow())
            {
                block.Status = AutomaticQueueBlockRunStatusEnum.Succeeded;
                block.RemainingDelaySeconds = 0;
                block.CompletedAtUtc = UtcNow();
                Advance(run);
                await db.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        if (block.ManualCheckRun is null)
        {
            return;
        }
        switch (block.ManualCheckRun.Status)
        {
            case ManualCheckRunStatusEnum.Succeeded:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.Succeeded, null);
                break;
            case ManualCheckRunStatusEnum.CompletedWithErrors:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.CompletedWithWarnings, block.ManualCheckRun.ErrorText);
                break;
            case ManualCheckRunStatusEnum.Failed:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.Failed, block.ManualCheckRun.ErrorText);
                break;
            case ManualCheckRunStatusEnum.Canceled:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.Failed, "The child manual check was canceled.");
                break;
            case ManualCheckRunStatusEnum.Paused:
                block.Status = AutomaticQueueBlockRunStatusEnum.Paused;
                run.Status = AutomaticQueueRunStatusEnum.Paused;
                break;
            default:
                return;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ReconcilePauseRequestedAsync(
        ApplicationDbContext db,
        AutomaticQueueRun run,
        CancellationToken cancellationToken)
    {
        var block = CurrentBlock(run);
        if (block?.ManualCheckRun is null)
        {
            run.Status = AutomaticQueueRunStatusEnum.Paused;
            if (block is not null)
            {
                block.Status = AutomaticQueueBlockRunStatusEnum.Paused;
            }
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        if (block.ManualCheckRun.Status == ManualCheckRunStatusEnum.Paused)
        {
            block.Status = AutomaticQueueBlockRunStatusEnum.Paused;
            run.Status = AutomaticQueueRunStatusEnum.Paused;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        switch (block.ManualCheckRun.Status)
        {
            case ManualCheckRunStatusEnum.Succeeded:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.Succeeded, null);
                break;
            case ManualCheckRunStatusEnum.CompletedWithErrors:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.CompletedWithWarnings, block.ManualCheckRun.ErrorText);
                break;
            case ManualCheckRunStatusEnum.Failed:
            case ManualCheckRunStatusEnum.Canceled:
                CompleteManualBlock(run, block, AutomaticQueueBlockRunStatusEnum.Failed, block.ManualCheckRun.ErrorText);
                break;
            default:
                return;
        }

        if (IsActiveStatus(run.Status))
        {
            var next = CurrentBlock(run);
            if (next is not null)
            {
                next.Status = AutomaticQueueBlockRunStatusEnum.Paused;
            }
            run.Status = AutomaticQueueRunStatusEnum.Paused;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> TryClaimPendingBlockAsync(
        ApplicationDbContext db,
        AutomaticQueueRunBlock block,
        CancellationToken cancellationToken)
    {
        var now = UtcNow();
        if (db.Database.IsRelational())
        {
            var updated = await db.AutomaticQueueRunBlocks
                .Where(x =>
                    x.Id == block.Id &&
                    x.Status == AutomaticQueueBlockRunStatusEnum.Pending &&
                    x.AutomaticQueueRun.Status == AutomaticQueueRunStatusEnum.Running)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Status, AutomaticQueueBlockRunStatusEnum.Running)
                        .SetProperty(x => x.StartedAtUtc, now),
                    cancellationToken);
            if (updated == 0)
            {
                return false;
            }

            var entry = db.Entry(block);
            entry.Property(x => x.Status).OriginalValue = AutomaticQueueBlockRunStatusEnum.Running;
            entry.Property(x => x.StartedAtUtc).OriginalValue = now;
        }

        block.Status = AutomaticQueueBlockRunStatusEnum.Running;
        block.StartedAtUtc = now;
        return true;
    }

    private async Task<bool> TryMarkRunRunningAsync(
        ApplicationDbContext db,
        AutomaticQueueRun run,
        CancellationToken cancellationToken)
    {
        var now = UtcNow();
        if (run.Status == AutomaticQueueRunStatusEnum.Running)
        {
            run.StartedAtUtc ??= now;
            return true;
        }
        if (run.Status != AutomaticQueueRunStatusEnum.Queued)
        {
            return false;
        }
        if (!db.Database.IsRelational())
        {
            run.Status = AutomaticQueueRunStatusEnum.Running;
            run.StartedAtUtc ??= now;
            return true;
        }

        var updated = await db.AutomaticQueueRuns
            .Where(x => x.Id == run.Id && x.Status == AutomaticQueueRunStatusEnum.Queued)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, AutomaticQueueRunStatusEnum.Running)
                    .SetProperty(x => x.StartedAtUtc, x => x.StartedAtUtc ?? now),
                cancellationToken);
        if (updated == 0)
        {
            return false;
        }

        run.Status = AutomaticQueueRunStatusEnum.Running;
        run.StartedAtUtc ??= now;
        var entry = db.Entry(run);
        entry.Property(x => x.Status).OriginalValue = AutomaticQueueRunStatusEnum.Running;
        entry.Property(x => x.StartedAtUtc).OriginalValue = run.StartedAtUtc;
        return true;
    }

    private async Task<AutomaticQueueWriteDto> NormalizeDefinitionAsync(
        string userId,
        AutomaticQueueWriteDto input,
        CancellationToken cancellationToken)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length is < 1 or > AutomaticQueueDefinition.NameMaxLength)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Queue name must be between 1 and 100 characters.");
        }
        if (input.Blocks.Count is < 1 or > MaxBlocks)
        {
            throw RequestError(StatusCodes.Status400BadRequest, $"A queue must contain between 1 and {MaxBlocks} blocks.");
        }
        if (!input.Blocks.Any(x => x.Type == AutomaticQueueBlockTypeEnum.ManualCheck))
        {
            throw RequestError(StatusCodes.Status400BadRequest, "A queue must contain at least one manual-check block.");
        }
        if (input.Blocks.Any(x => x.Key == Guid.Empty) || input.Blocks.Select(x => x.Key).Distinct().Count() != input.Blocks.Count)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Every queue block requires a unique key.");
        }

        var normalized = new List<AutomaticQueueBlockWriteDto>(input.Blocks.Count);
        foreach (var block in input.Blocks)
        {
            normalized.Add(await NormalizeBlockAsync(userId, block, cancellationToken));
        }
        return new AutomaticQueueWriteDto { Name = name, Blocks = normalized };
    }

    private async Task<AutomaticQueueBlockWriteDto> NormalizeBlockAsync(
        string userId,
        AutomaticQueueBlockWriteDto block,
        CancellationToken cancellationToken)
    {
        if (block.Type == AutomaticQueueBlockTypeEnum.Delay)
        {
            if (block.DelaySeconds is < 1 or > MaxDelaySeconds)
            {
                throw RequestError(StatusCodes.Status400BadRequest, "Delay blocks must be between 1 second and 7 days.");
            }
            return new AutomaticQueueBlockDto
            {
                Key = block.Key,
                Type = block.Type,
                DelaySeconds = block.DelaySeconds
            };
        }
        if (block.Type != AutomaticQueueBlockTypeEnum.ManualCheck || !block.GameUrlId.HasValue || !block.TemplateMode.HasValue)
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Manual-check blocks require a Game URL and template mode.");
        }
        if (block.ProductIds is { Count: > MaxSelectedProducts } || block.ProductIds?.Any(x => x <= 0) == true)
        {
            throw RequestError(StatusCodes.Status400BadRequest, $"Select at most {MaxSelectedProducts} valid products.");
        }
        var productIds = block.ProductIds?.Distinct().ToList();
        if (productIds is { Count: 0 })
        {
            throw RequestError(StatusCodes.Status400BadRequest, "Choose all products or at least one product.");
        }

        long? presetId = block.TemplateMode == AutomaticQueueTemplateModeEnum.SavedPreset
            ? block.PresetId ?? throw RequestError(StatusCodes.Status400BadRequest, "A saved preset is required.")
            : null;
        var privateTemplate = block.TemplateMode == AutomaticQueueTemplateModeEnum.PrivateTemplate
            ? block.PrivateTemplate ?? throw RequestError(StatusCodes.Status400BadRequest, "A private template is required.")
            : null;
        var presetCombination = block.TemplateMode == AutomaticQueueTemplateModeEnum.PresetCombination
            ? block.PresetCombination ?? throw RequestError(StatusCodes.Status400BadRequest, "A preset combination is required.")
            : null;
        var setup = await PrepareManualSetupAsync(
            null,
            userId,
            block.GameUrlId.Value,
            presetId,
            presetCombination,
            privateTemplate,
            block.BypassCache,
            productIds,
            cancellationToken);
        return CreateConfiguration(block, setup, productIds);
    }

    private async Task<AutomaticQueueRunBlockSetupDto> CreateRunSnapshotAsync(
        ApplicationDbContext db,
        string userId,
        AutomaticQueueBlockDto configuration,
        CancellationToken cancellationToken)
    {
        if (configuration.Type == AutomaticQueueBlockTypeEnum.Delay)
        {
            return new AutomaticQueueRunBlockSetupDto
            {
                Configuration = configuration,
                DelaySeconds = configuration.DelaySeconds
            };
        }
        var setup = await PrepareManualSetupAsync(
            db,
            userId,
            configuration.GameUrlId!.Value,
            configuration.TemplateMode == AutomaticQueueTemplateModeEnum.SavedPreset ? configuration.PresetId : null,
            configuration.TemplateMode == AutomaticQueueTemplateModeEnum.PresetCombination ? configuration.PresetCombination : null,
            configuration.TemplateMode == AutomaticQueueTemplateModeEnum.PrivateTemplate ? configuration.PrivateTemplate : null,
            configuration.BypassCache,
            configuration.ProductIds,
            cancellationToken);
        return new AutomaticQueueRunBlockSetupDto
        {
            Configuration = CreateConfiguration(configuration, setup, configuration.ProductIds),
            ManualCheckSetup = setup
        };
    }

    private async Task<ManualCheckSetupDto> PrepareManualSetupAsync(
        ApplicationDbContext? db,
        string userId,
        long gameUrlId,
        long? presetId,
        ManualCheckPresetCombinationWriteDto? presetCombination,
        AutomaticQueuePrivateTemplateDto? privateTemplate,
        bool bypassCache,
        IReadOnlyList<long>? productIds,
        CancellationToken cancellationToken)
    {
        try
        {
            return db is null
                ? await manualCheckDataService.PrepareRunSetupAsync(
                    userId,
                    gameUrlId,
                    presetId,
                    presetCombination,
                    privateTemplate,
                    bypassCache,
                    productIds,
                    cancellationToken)
                : await ManualCheckDataService.PrepareQueueRunSetupAsync(
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
        catch (ManualCheckRequestException exception)
        {
            throw RequestError(exception.StatusCode, exception.Message);
        }
    }

    private static AutomaticQueueBlockDto CreateConfiguration(
        AutomaticQueueBlockWriteDto source,
        ManualCheckSetupDto setup,
        List<long>? productIds)
    {
        return new AutomaticQueueBlockDto
        {
            Key = source.Key,
            Type = source.Type,
            GameUrlId = source.GameUrlId,
            TemplateMode = source.TemplateMode,
            PresetId = source.TemplateMode == AutomaticQueueTemplateModeEnum.SavedPreset ? source.PresetId : null,
            PresetCombination = source.TemplateMode == AutomaticQueueTemplateModeEnum.PresetCombination && setup.PresetCombination is not null
                ? new ManualCheckPresetCombinationWriteDto
                {
                    ListingLimit = setup.PresetCombination.ListingLimit,
                    CooldownMinutes = setup.PresetCombination.CooldownMinutes,
                    CooldownSeconds = setup.PresetCombination.CooldownSeconds,
                    Terms = setup.PresetCombination.Terms.Select(x => new ManualCheckPresetCombinationTermWriteDto
                    {
                        PresetId = x.PresetId,
                        Operator = x.Operator
                    }).ToList()
                }
                : null,
            PrivateTemplate = source.TemplateMode == AutomaticQueueTemplateModeEnum.PrivateTemplate
                ? new AutomaticQueuePrivateTemplateDto
                {
                    Name = setup.PresetName,
                    ListingLimit = setup.ListingLimit,
                    CooldownMinutes = setup.CooldownMinutes,
                    CooldownSeconds = setup.CooldownSeconds,
                    Criteria = setup.Criteria
                }
                : null,
            BypassCache = source.BypassCache,
            ProductIds = productIds,
            GameId = setup.GameId,
            GameName = setup.GameName,
            GameUrlName = setup.GameUrlName,
            PresetName = setup.PresetName
        };
    }

    private static ICollection<AutomaticQueueBlock> CreateBlockEntities(
        IReadOnlyList<AutomaticQueueBlockWriteDto> blocks)
    {
        return blocks.Select((block, index) => new AutomaticQueueBlock
        {
            BlockKey = block.Key,
            SortOrder = index,
            BlockType = block.Type,
            ConfigurationJson = JsonConvert.SerializeObject(block)
        }).ToList();
    }

    private static AutomaticQueueDto ToDefinitionDto(AutomaticQueueDefinition row, long? activeRunId)
    {
        return new AutomaticQueueDto
        {
            Id = row.Id,
            Name = row.Name,
            CreatedAtUtc = row.CreatedAtUtc,
            UpdatedAtUtc = row.UpdatedAtUtc,
            ActiveRunId = activeRunId,
            Blocks = row.Blocks.OrderBy(x => x.SortOrder).Select(x =>
            {
                var block = DeserializeConfiguration(x.ConfigurationJson);
                block.Id = x.Id;
                block.SortOrder = x.SortOrder;
                return block;
            }).ToList()
        };
    }

    private static AutomaticQueueRunDto ToRunDto(AutomaticQueueRun row)
    {
        var blocks = row.Blocks.OrderBy(x => x.SortOrder).Select(ToRunBlockDto).ToList();
        return new AutomaticQueueRunDto
        {
            Id = row.Id,
            QueueId = row.AutomaticQueueDefinitionId,
            QueueName = row.QueueName,
            Status = row.Status,
            CurrentBlockIndex = row.CurrentBlockIndex,
            TotalBlocks = blocks.Count,
            CompletedBlocks = blocks.Count(x => IsTerminalBlockStatus(x.Status)),
            Date = row.Date,
            StartedAtUtc = row.StartedAtUtc,
            CompletedAtUtc = row.CompletedAtUtc,
            ErrorText = row.ErrorText,
            CorrelationId = row.CorrelationId,
            Blocks = blocks
        };
    }

    private static AutomaticQueueRunBlockDto ToRunBlockDto(AutomaticQueueRunBlock row)
    {
        var snapshot = DeserializeSnapshot(row.SetupJson);
        return new AutomaticQueueRunBlockDto
        {
            Id = row.Id,
            Key = row.BlockKey,
            SortOrder = row.SortOrder,
            Type = row.BlockType,
            Status = row.Status,
            Configuration = snapshot.Configuration,
            ManualCheckRunId = row.ManualCheckRunId,
            WarningCount = row.ManualCheckRun?.FailedProducts ?? 0,
            WaitUntilUtc = row.WaitUntilUtc,
            RemainingDelaySeconds = row.RemainingDelaySeconds,
            StartedAtUtc = row.StartedAtUtc,
            CompletedAtUtc = row.CompletedAtUtc,
            ErrorText = row.ErrorText
        };
    }

    private static AutomaticQueueBlockDto DeserializeConfiguration(string json)
    {
        return JsonConvert.DeserializeObject<AutomaticQueueBlockDto>(json)
            ?? throw new InvalidOperationException("Automatic queue block configuration is invalid.");
    }

    private static AutomaticQueueRunBlockSetupDto DeserializeSnapshot(string json)
    {
        return JsonConvert.DeserializeObject<AutomaticQueueRunBlockSetupDto>(json)
            ?? throw new InvalidOperationException("Automatic queue block snapshot is invalid.");
    }

    private static async Task<AutomaticQueueRun?> LoadRunAsync(
        ApplicationDbContext db,
        long id,
        string userId,
        bool tracking,
        CancellationToken cancellationToken)
    {
        var query = db.AutomaticQueueRuns
            .Include(x => x.Blocks)
            .ThenInclude(x => x.ManualCheckRun)
            .Where(x => x.Id == id && x.UserId == userId);
        if (!tracking)
        {
            query = query.AsNoTracking();
        }
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task EnsureDefinitionIsIdleAsync(
        ApplicationDbContext db,
        long definitionId,
        string userId,
        CancellationToken cancellationToken)
    {
        if (await db.AutomaticQueueRuns.AnyAsync(
                x =>
                    x.AutomaticQueueDefinitionId == definitionId &&
                    x.UserId == userId &&
                    (x.Status == AutomaticQueueRunStatusEnum.Queued ||
                     x.Status == AutomaticQueueRunStatusEnum.Running ||
                     x.Status == AutomaticQueueRunStatusEnum.PauseRequested ||
                     x.Status == AutomaticQueueRunStatusEnum.Paused),
                cancellationToken))
        {
            throw RequestError(StatusCodes.Status409Conflict, "The queue cannot be changed while it has an active or paused run.");
        }
    }

    private static AutomaticQueueRunBlock? CurrentBlock(AutomaticQueueRun run)
    {
        return run.Blocks.FirstOrDefault(x => x.SortOrder == run.CurrentBlockIndex);
    }

    private void CompleteManualBlock(
        AutomaticQueueRun run,
        AutomaticQueueRunBlock block,
        AutomaticQueueBlockRunStatusEnum status,
        string? error)
    {
        block.Status = status;
        block.ErrorText = error;
        block.CompletedAtUtc = UtcNow();
        Advance(run);
    }

    private void Advance(AutomaticQueueRun run)
    {
        run.CurrentBlockIndex++;
        if (run.CurrentBlockIndex >= run.Blocks.Count)
        {
            CompleteQueue(run);
        }
    }

    private void CompleteQueue(AutomaticQueueRun run)
    {
        run.Status = run.Blocks.Any(x => x.Status is AutomaticQueueBlockRunStatusEnum.Failed or AutomaticQueueBlockRunStatusEnum.CompletedWithWarnings)
            ? AutomaticQueueRunStatusEnum.CompletedWithErrors
            : AutomaticQueueRunStatusEnum.Succeeded;
        run.CompletedAtUtc = UtcNow();
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private static bool IsActiveStatus(AutomaticQueueRunStatusEnum status)
    {
        return status is AutomaticQueueRunStatusEnum.Queued or
            AutomaticQueueRunStatusEnum.Running or
            AutomaticQueueRunStatusEnum.PauseRequested or
            AutomaticQueueRunStatusEnum.Paused;
    }

    private static bool IsTerminalBlockStatus(AutomaticQueueBlockRunStatusEnum status)
    {
        return status is AutomaticQueueBlockRunStatusEnum.Succeeded or
            AutomaticQueueBlockRunStatusEnum.CompletedWithWarnings or
            AutomaticQueueBlockRunStatusEnum.Failed or
            AutomaticQueueBlockRunStatusEnum.Canceled or
            AutomaticQueueBlockRunStatusEnum.Skipped;
    }

    private static AutomaticQueueRequestException RequestError(int statusCode, string message)
    {
        return new AutomaticQueueRequestException(statusCode, message);
    }
}

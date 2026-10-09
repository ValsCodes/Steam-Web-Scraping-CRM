using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.Automation;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Security;

namespace SteamApp.WebAPI.Services;

public sealed class AutomationAccessService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    TimeProvider timeProvider,
    ILogger<AutomationAccessService> logger) : IAutomationAccessService
{
    public static readonly TimeSpan RollingWindow = TimeSpan.FromHours(24);
    public static readonly TimeSpan PresenceLeaseDuration = TimeSpan.FromSeconds(30);

    public async Task<AutomationUsageDto> GetUsageAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        return await BuildUsageAsync(db, userId, cancellationToken);
    }

    public async Task EnsureCanExecuteAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var usage = await GetUsageAsync(userId, cancellationToken);
        if (usage.Unlimited)
        {
            return;
        }

        if (!usage.PresenceActive)
        {
            throw new AutomationAccessException(
                StatusCodes.Status409Conflict,
                "An active visible SteamApp tab is required to run automated checks.");
        }

        if (usage.RemainingSeconds <= 0)
        {
            throw new AutomationAccessException(
                StatusCodes.Status429TooManyRequests,
                "The automated-check allowance is exhausted.",
                usage.NextAllowanceAtUtc);
        }
    }

    public async Task UpsertPresenceAsync(
        string userId,
        Guid tabId,
        CancellationToken cancellationToken)
    {
        if (tabId == Guid.Empty)
        {
            throw new AutomationAccessException(StatusCodes.Status400BadRequest, "A valid tab identifier is required.");
        }

        await using var db = dbContextFactory.CreateDbContext();
        var now = UtcNow();
        var lease = await db.SessionPresenceLeases
            .FirstOrDefaultAsync(x => x.TabId == tabId, cancellationToken);
        if (lease is null)
        {
            db.SessionPresenceLeases.Add(new SessionPresenceLease
            {
                UserId = userId,
                TabId = tabId,
                UpdatedAtUtc = now,
                ExpiresAtUtc = now.Add(PresenceLeaseDuration)
            });
        }
        else if (lease.UserId != userId)
        {
            throw new AutomationAccessException(
                StatusCodes.Status409Conflict,
                "The tab identifier is already associated with another session.");
        }
        else
        {
            lease.UpdatedAtUtc = now;
            lease.ExpiresAtUtc = now.Add(PresenceLeaseDuration);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleasePresenceAsync(
        string userId,
        Guid tabId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var lease = await db.SessionPresenceLeases
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TabId == tabId, cancellationToken);
        if (lease is null)
        {
            return;
        }

        db.SessionPresenceLeases.Remove(lease);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AutomationPolicyDto> GetPolicyAsync(CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var policy = await GetOrCreatePolicyAsync(db, cancellationToken);
        return ToPolicyDto(policy);
    }

    public async Task<AutomationPolicyDto> UpdatePolicyAsync(
        string administratorUserId,
        AutomationPolicyUpdateDto input,
        CancellationToken cancellationToken)
    {
        if (input.NonAdminLimitMinutes is < 0 or > 1440)
        {
            throw new AutomationAccessException(
                StatusCodes.Status400BadRequest,
                "The non-admin limit must be a whole number of minutes from 0 to 1440.");
        }

        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(input.RowVersion);
        }
        catch (FormatException)
        {
            throw new AutomationAccessException(StatusCodes.Status400BadRequest, "The policy version is invalid.");
        }

        await using var db = dbContextFactory.CreateDbContext();
        var policy = await GetOrCreatePolicyAsync(db, cancellationToken);
        db.Entry(policy).Property(x => x.RowVersion).OriginalValue = rowVersion;
        policy.NonAdminLimitSeconds = checked(input.NonAdminLimitMinutes * 60);
        policy.LastModifiedByUserId = administratorUserId;
        policy.LastModifiedAtUtc = UtcNow();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AutomationAccessException(
                StatusCodes.Status409Conflict,
                "The automation policy changed. Refresh it and try again.");
        }

        logger.LogInformation(
            "Automation limit changed to {LimitSeconds} seconds by administrator {AdministratorUserId} at {ChangedAtUtc}.",
            policy.NonAdminLimitSeconds,
            administratorUserId,
            policy.LastModifiedAtUtc);
        return ToPolicyDto(policy);
    }

    public async Task<AutomationPolicyDto> ResetUsageAsync(
        string administratorUserId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var policy = await GetOrCreatePolicyAsync(db, cancellationToken);
        var now = UtcNow();
        policy.UsageResetAtUtc = now;
        policy.LastResetAtUtc = now;
        policy.LastResetByUserId = administratorUserId;
        policy.LastModifiedAtUtc = now;
        policy.LastModifiedByUserId = administratorUserId;
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Automation usage was globally reset by administrator {AdministratorUserId} at {ResetAtUtc}.",
            administratorUserId,
            now);
        return ToPolicyDto(policy);
    }

    public async Task<IReadOnlyList<AdminAutomationUsageDto>> GetAllUsageAsync(
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();
        var users = await db.Users.AsNoTracking()
            .OrderBy(x => x.Email)
            .Select(x => new { x.Id, x.Email })
            .ToListAsync(cancellationToken);
        var result = new List<AdminAutomationUsageDto>(users.Count);
        foreach (var user in users)
        {
            var usage = await BuildUsageAsync(db, user.Id, cancellationToken);
            result.Add(new AdminAutomationUsageDto
            {
                UserId = user.Id,
                Email = user.Email,
                IsAdmin = usage.Unlimited,
                Usage = usage
            });
        }

        return result;
    }

    internal async Task<AutomationUsageDto> BuildUsageAsync(
        ApplicationDbContext db,
        string userId,
        CancellationToken cancellationToken)
    {
        var now = UtcNow();
        var isAdmin = await IsAdminAsync(db, userId, cancellationToken);
        var policy = await GetOrCreatePolicyAsync(db, cancellationToken);
        var presenceExpiry = await db.SessionPresenceLeases.AsNoTracking()
            .Where(x => x.UserId == userId && x.ExpiresAtUtc > now)
            .MaxAsync(x => (DateTime?)x.ExpiresAtUtc, cancellationToken);

        if (isAdmin)
        {
            return new AutomationUsageDto
            {
                Unlimited = true,
                UsedSeconds = 0,
                PresenceRequired = false,
                PresenceActive = true,
                PresenceExpiresAtUtc = presenceExpiry,
                UsageResetAtUtc = policy.UsageResetAtUtc
            };
        }

        var cutoff = now.Subtract(RollingWindow);
        if (policy.UsageResetAtUtc > cutoff)
        {
            cutoff = policy.UsageResetAtUtc.Value;
        }

        var intervals = await db.AutomationUsageIntervals.AsNoTracking()
            .Where(x => x.UserId == userId && (x.EndedAtUtc == null || x.EndedAtUtc > cutoff))
            .OrderBy(x => x.StartedAtUtc)
            .ToListAsync(cancellationToken);
        var usedSeconds = intervals.Sum(x => OverlapSeconds(x.StartedAtUtc, x.EndedAtUtc ?? now, cutoff, now));
        var roundedUsedSeconds = Math.Max(0, (int)Math.Ceiling(usedSeconds));
        var remainingSeconds = Math.Max(0, policy.NonAdminLimitSeconds - roundedUsedSeconds);

        return new AutomationUsageDto
        {
            Unlimited = false,
            LimitSeconds = policy.NonAdminLimitSeconds,
            UsedSeconds = roundedUsedSeconds,
            RemainingSeconds = remainingSeconds,
            UsageResetAtUtc = policy.UsageResetAtUtc,
            PresenceRequired = true,
            PresenceActive = presenceExpiry.HasValue,
            PresenceExpiresAtUtc = presenceExpiry,
            NextAllowanceAtUtc = remainingSeconds > 0
                ? null
                : CalculateNextAllowance(intervals, cutoff, now, policy.NonAdminLimitSeconds)
        };
    }

    private async Task<AutomationPolicy> GetOrCreatePolicyAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var policy = await db.AutomationPolicies
            .FirstOrDefaultAsync(x => x.Id == AutomationPolicy.SingletonId, cancellationToken);
        if (policy is not null)
        {
            return policy;
        }

        policy = new AutomationPolicy
        {
            Id = AutomationPolicy.SingletonId,
            NonAdminLimitSeconds = AutomationPolicy.DefaultNonAdminLimitSeconds,
            LastModifiedAtUtc = UtcNow()
        };
        db.AutomationPolicies.Add(policy);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return policy;
        }
        catch (DbUpdateException)
        {
            db.Entry(policy).State = EntityState.Detached;
            return await db.AutomationPolicies.FirstAsync(
                x => x.Id == AutomationPolicy.SingletonId,
                cancellationToken);
        }
    }

    private static async Task<bool> IsAdminAsync(
        ApplicationDbContext db,
        string userId,
        CancellationToken cancellationToken)
    {
        return await (
            from userRole in db.UserRoles.AsNoTracking()
            join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.Name == SecurityPolicies.AdminRole
            select userRole.UserId)
            .AnyAsync(cancellationToken);
    }

    private static double OverlapSeconds(DateTime start, DateTime end, DateTime cutoff, DateTime now)
    {
        var effectiveStart = start > cutoff ? start : cutoff;
        var effectiveEnd = end < now ? end : now;
        return effectiveEnd > effectiveStart ? (effectiveEnd - effectiveStart).TotalSeconds : 0;
    }

    private static DateTime? CalculateNextAllowance(
        IReadOnlyList<AutomationUsageInterval> intervals,
        DateTime cutoff,
        DateTime now,
        int limitSeconds)
    {
        if (limitSeconds <= 0)
        {
            return null;
        }

        var used = intervals.Sum(x => OverlapSeconds(x.StartedAtUtc, x.EndedAtUtc ?? now, cutoff, now));
        var secondsToExpire = Math.Max(1, used - limitSeconds + 1);
        foreach (var interval in intervals)
        {
            var start = interval.StartedAtUtc > cutoff ? interval.StartedAtUtc : cutoff;
            var end = (interval.EndedAtUtc ?? now) < now ? interval.EndedAtUtc!.Value : now;
            var length = Math.Max(0, (end - start).TotalSeconds);
            if (secondsToExpire <= length)
            {
                return start.AddSeconds(secondsToExpire).Add(RollingWindow);
            }

            secondsToExpire -= length;
        }

        return intervals.Count == 0 ? null : now.Add(RollingWindow);
    }

    private static AutomationPolicyDto ToPolicyDto(AutomationPolicy policy)
    {
        return new AutomationPolicyDto
        {
            NonAdminLimitMinutes = policy.NonAdminLimitSeconds / 60,
            UsageResetAtUtc = policy.UsageResetAtUtc,
            LastModifiedByUserId = policy.LastModifiedByUserId,
            LastModifiedAtUtc = policy.LastModifiedAtUtc,
            LastResetByUserId = policy.LastResetByUserId,
            LastResetAtUtc = policy.LastResetAtUtc,
            RowVersion = Convert.ToBase64String(policy.RowVersion)
        };
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}

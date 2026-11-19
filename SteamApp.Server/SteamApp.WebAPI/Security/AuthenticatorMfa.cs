using SteamApp.Contracts.Requests;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OtpNet;
using SteamApp.Application.Common.Results;
using SteamApp.Infrastructure.Persistence;
using SteamApp.Infrastructure.Identity;

namespace SteamApp.WebAPI.Security;

public sealed class AuthenticatorMfa(
    IDbContextFactory<ApplicationDbContext> contexts,
    IDataProtectionProvider protection,
    IOptions<IdentityOptions> identityOptions,
    TimeProvider timeProvider)
{
    private const string LoginProvider = "SteamApp";
    private const string TokenName = "AuthenticatorMfa";
    private static readonly VerificationWindow CodeWindow = new(previous: 1, future: 1);

    internal async Task<int> CountRecoveryCodesAsync(string userId, CancellationToken ct)
    {
        await using var db = await contexts.CreateDbContextAsync(ct);
        return ReadState(await FindTokenAsync(db, userId, ct))?.RecoveryHashes.Length ?? 0;
    }

    internal async Task<Result<MfaChange>> EnableAsync(MfaFlow setup, string code, CancellationToken ct)
    {
        var key = setup.SetupKey!;
        if (!VerifyCode(key, code, out var step))
        {
            return Result<MfaChange>.Failure(MfaErrors.InvalidSetupCode);
        }

        await using var db = await contexts.CreateDbContextAsync(ct);
        var user = await db.Users.FindAsync([setup.UserId], ct);
        if (!CanChange(user, setup.SecurityStamp, enabled: false))
        {
            return Result<MfaChange>.Failure(MfaErrors.Conflict);
        }

        var codes = GenerateRecoveryCodes();
        var state = new MfaState(Protector(setup.UserId).Protect(key), codes.Select(Hash).ToArray(), step);
        db.Set<IdentityUserToken<string>>().Add(new()
        {
            UserId = setup.UserId,
            LoginProvider = LoginProvider,
            Name = TokenName,
            Value = JsonSerializer.Serialize(state)
        });
        user!.TwoFactorEnabled = true;
        user.LockoutEnabled = true;
        RotateStamp(user);
        try
        {
            return await SaveAsync(db, user, ct)
                ? Result<MfaChange>.Success(new(user, codes))
                : Result<MfaChange>.Failure(MfaErrors.Conflict);
        }
        catch (DbUpdateException exception) when (ExternalAccountRegistration.IsUniqueConflict(exception))
        {
            return Result<MfaChange>.Failure(MfaErrors.Conflict);
        }
    }

    internal async Task<Result<MfaChange>> UpdateAsync(ApplicationUser current, bool disable, CancellationToken ct)
    {
        await using var db = await contexts.CreateDbContextAsync(ct);
        var user = await db.Users.FindAsync([current.Id], ct);
        if (!CanChange(user, current.SecurityStamp, enabled: true))
        {
            return Result<MfaChange>.Failure(MfaErrors.Conflict);
        }

        var token = await FindTokenAsync(db, current.Id, ct);
        if (token == null)
        {
            return Result<MfaChange>.Failure(MfaErrors.Conflict);
        }

        string[]? codes = null;
        if (disable)
        {
            db.Remove(token);
            user!.TwoFactorEnabled = false;
            RotateStamp(user);
        }
        else
        {
            codes = GenerateRecoveryCodes();
            var state = ReadState(token)!;
            token.Value = JsonSerializer.Serialize(state with { RecoveryHashes = codes.Select(Hash).ToArray() });
        }

        return await SaveAsync(db, user!, ct)
            ? Result<MfaChange>.Success(new(user!, codes))
            : Result<MfaChange>.Failure(MfaErrors.Conflict);
    }

    internal async Task<Result<MfaChange>> RedeemAsync(MfaFlow challenge, MfaVerificationRequest request, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            await using var db = await contexts.CreateDbContextAsync(ct);
            var user = await db.Users.FindAsync([challenge.UserId], ct);
            if (!CanChange(user, challenge.SecurityStamp, enabled: true) ||
                challenge.ExpiresAtUtc <= timeProvider.GetUtcNow())
            {
                return Result<MfaChange>.Failure(MfaErrors.Expired);
            }

            var token = await FindTokenAsync(db, challenge.UserId, ct);
            var state = ReadState(token);
            if (state == null)
            {
                return Result<MfaChange>.Failure(MfaErrors.Expired);
            }

            var updated = RedeemCode(challenge.UserId, state, request);
            if (updated != null)
            {
                token!.Value = JsonSerializer.Serialize(updated);
                user!.AccessFailedCount = 0;
            }
            else
            {
                RecordFailure(user!);
            }

            if (await SaveAsync(db, user!, ct))
            {
                if (updated != null)
                {
                    return Result<MfaChange>.Success(new(user!));
                }

                return Result<MfaChange>.Failure(IsLockedOut(user!) ? MfaErrors.Expired : MfaErrors.InvalidCode);
            }
        }

        return Result<MfaChange>.Failure(MfaErrors.Conflict);
    }

    private MfaState? RedeemCode(string userId, MfaState state, MfaVerificationRequest request)
    {
        if (request.Method == "authenticator")
        {
            return VerifyCode(Protector(userId).Unprotect(state.ProtectedKey), request.Code.Trim(), out var step) &&
                step > state.LastUsedStep
                ? state with { LastUsedStep = step }
                : null;
        }

        var hash = Hash(request.Code.Trim().Replace("-", "").ToUpperInvariant());
        return state.RecoveryHashes.Contains(hash, StringComparer.Ordinal)
            ? state with { RecoveryHashes = state.RecoveryHashes.Where(value => value != hash).ToArray() }
            : null;
    }

    private bool VerifyCode(string key, string code, out long step)
    {
        step = 0;
        return code.Length == 6 && code.All(char.IsAsciiDigit) &&
            new Totp(Base32Encoding.ToBytes(key)).VerifyTotp(timeProvider.GetUtcNow().UtcDateTime, code, out step, CodeWindow);
    }

    private void RecordFailure(ApplicationUser user)
    {
        user.AccessFailedCount++;
        var lockout = identityOptions.Value.Lockout;
        if (user.AccessFailedCount >= lockout.MaxFailedAccessAttempts)
        {
            user.LockoutEnd = timeProvider.GetUtcNow().Add(lockout.DefaultLockoutTimeSpan);
            user.AccessFailedCount = 0;
        }
    }

    private bool IsLockedOut(ApplicationUser user) => user.LockoutEnd > timeProvider.GetUtcNow();

    private bool CanChange(ApplicationUser? user, string? stamp, bool enabled) =>
        user != null && stamp != null && user.SecurityStamp == stamp &&
        user.TwoFactorEnabled == enabled && !IsLockedOut(user);

    private IDataProtector Protector(string userId) =>
        protection.CreateProtector("SteamApp.AuthenticatorMfa.v1", userId);

    private static ValueTask<IdentityUserToken<string>?> FindTokenAsync(
        ApplicationDbContext db, string id, CancellationToken ct) =>
        db.Set<IdentityUserToken<string>>().FindAsync([id, LoginProvider, TokenName], ct);

    private static MfaState? ReadState(IdentityUserToken<string>? token) =>
        token?.Value is { } value ? JsonSerializer.Deserialize<MfaState>(value) : null;

    private static void RotateStamp(ApplicationUser user) =>
        user.SecurityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string[] GenerateRecoveryCodes() =>
        Enumerable.Range(0, 10).Select(_ => Convert.ToHexString(RandomNumberGenerator.GetBytes(16))).ToArray();

    private static string Hash(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    private static async Task<bool> SaveAsync(ApplicationDbContext db, ApplicationUser user, CancellationToken ct)
    {
        // Updating the Identity concurrency stamp makes the user and MFA token one atomic optimistic write.
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}

using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;

namespace SteamApp.WebAPI.Security;

public sealed class MfaFlows(TimeProvider timeProvider) : IDisposable
{
    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = 1024 });
    private readonly object setupGate = new();
    internal static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    internal string? Add(MfaFlow flow)
    {
        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        cache.Set(id, flow, new MemoryCacheEntryOptions
        {
            Size = 1,
            AbsoluteExpirationRelativeToNow = Lifetime
        });
        return cache.TryGetValue(id, out _) ? id : null;
    }

    internal MfaFlow? Get(string id) =>
        cache.TryGetValue<MfaFlow>(id, out var flow) && flow != null &&
        !flow.Consumed && flow.ExpiresAtUtc > timeProvider.GetUtcNow()
            ? flow
            : null;

    internal (string Id, MfaFlow Flow)? GetOrCreateSetup(
        string userId, string securityStamp, DateTimeOffset authenticationTime)
    {
        var sessionKey = (userId, securityStamp, authenticationTime);
        lock (setupGate)
        {
            if (cache.TryGetValue<string>(sessionKey, out var existingId) &&
                existingId != null && Get(existingId) is { } existing)
            {
                return (existingId, existing);
            }

            var flow = new MfaFlow(userId, securityStamp, timeProvider.GetUtcNow().Add(Lifetime))
            {
                SetupKey = Base32Encoding.ToString(RandomNumberGenerator.GetBytes(20))
            };
            if (Add(flow) is not { } id)
            {
                return null;
            }

            cache.Set(sessionKey, id, new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = Lifetime
            });
            if (!cache.TryGetValue(sessionKey, out _))
            {
                cache.Remove(id);
                return null;
            }

            return (id, flow);
        }
    }

    internal void Consume(string id, MfaFlow flow)
    {
        flow.Consumed = true;
        cache.Remove(id);
    }

    public void Dispose() => cache.Dispose();
}

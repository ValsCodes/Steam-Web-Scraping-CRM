using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SteamApp.WebAPI.Caching;

public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<bool> ExistsAsync(
        this IDistributedCache cache,
        string key,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cache);

        return await cache.GetAsync(key, cancellationToken) is not null;
    }

    public static Task SetMarkerAsync(
        this IDistributedCache cache,
        string key,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cache);

        return cache.SetStringAsync(
            key,
            "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }

    public static async Task<T?> GetJsonAsync<T>(
        this IDistributedCache cache,
        string key,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cache);

        var json = await cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public static Task SetJsonAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cache);

        return cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value, JsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }
}

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteamApp.Application.Abstractions.Services;
using SteamApp.Application.Caching;
using SteamApp.Application.JsonObjects;
using SteamApp.Infrastructure.Caching;

namespace SteamApp.Infrastructure.ExternalServices.ManualChecks;

public sealed class SteamManualCheckListingSource(
    IHttpClientFactory httpClientFactory,
    IOptions<ManualCheckOptions> options,
    IDistributedCache cache,
    ILogger<SteamManualCheckListingSource> logger) : IManualCheckListingSource
{
    private static readonly TimeSpan SteamListingCacheDuration = TimeSpan.FromMinutes(20);

    public TimeSpan DefaultCheckDelay => options.Value.DelayBetweenRequests;

    public async Task<Listing> FetchListingAsync(
        Uri uri,
        bool bypassCache,
        string userAgent,
        CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(uri);
        if (!bypassCache)
        {
            try
            {
                var cachedListing = await cache.GetJsonAsync<Listing>(cacheKey, cancellationToken);
                if (cachedListing?.Success == true)
                {
                    return cachedListing;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Unable to read cached Steam listing response for key {CacheKey}; fetching a fresh response.",
                    cacheKey);
            }
        }

        var listing = await FetchListingFromSteamAsync(uri, userAgent, cancellationToken);
        try
        {
            await cache.SetJsonAsync(
                cacheKey,
                listing,
                SteamListingCacheDuration,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Unable to cache Steam listing response for key {CacheKey}.",
                cacheKey);
        }

        return listing;
    }

    private async Task<Listing> FetchListingFromSteamAsync(
        Uri uri,
        string userAgent,
        CancellationToken cancellationToken)
    {
        await ThrowIfSteamRateLimitedAsync(cancellationToken);

        var client = httpClientFactory.CreateClient(ManualCheckOptions.HttpClientName);
        var maxAttempts = Math.Clamp(options.Value.MaxAttempts, 1, 10);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(NormalizedTimeout(options.Value.RequestTimeout));

                using var request = CreateRequest(uri, userAgent);
                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);

                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadAsStringAsync(timeout.Token);
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (IsHtmlResponse(contentType, payload))
                    {
                        if (IsSteamRateLimitPage(payload))
                        {
                            var retryAtUtc = await ActivateSteamRateLimitAsync(
                                response.Headers.RetryAfter,
                                cancellationToken);

                            throw new HttpRequestException(
                                "Steam rate-limited the automated market request. Browser access may still work " +
                                $"because it uses a separate Steam session. Requests are paused until {retryAtUtc:O}.",
                                null,
                                HttpStatusCode.TooManyRequests);
                        }

                        if (SteamMarketPageParser.TryParseListing(payload, out var pageListing))
                        {
                            return pageListing;
                        }

                        throw new HttpRequestException(
                            "Steam returned an HTML market page (HTTP 200), but its embedded listing data could not be read. " +
                            "Steam may have changed the market page format or returned an interstitial page.",
                            null,
                            response.StatusCode);
                    }

                    Listing listing;
                    try
                    {
                        listing = JsonConvert.DeserializeObject<Listing>(payload)
                            ?? throw new JsonSerializationException("Steam returned an empty JSON object.");
                    }
                    catch (JsonException exception)
                    {
                        throw new HttpRequestException(
                            $"Steam returned malformed listing JSON (HTTP 200, content type {contentType ?? "unknown"}).",
                            exception,
                            response.StatusCode);
                    }

                    if (!listing.Success)
                    {
                        throw new InvalidOperationException("Steam reported an unsuccessful listing response.");
                    }

                    return listing;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAtUtc = await ActivateSteamRateLimitAsync(
                        response.Headers.RetryAfter,
                        cancellationToken);
                    throw new HttpRequestException(
                        $"Steam returned HTTP 429. Requests are paused until {retryAtUtc:O}.",
                        null,
                        HttpStatusCode.TooManyRequests);
                }

                if (!IsTransient(response.StatusCode) || attempt == maxAttempts)
                {
                    throw new HttpRequestException(
                        $"Steam returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).",
                        null,
                        response.StatusCode);
                }

                await Task.Delay(GetRetryDelay(response.Headers.RetryAfter, attempt), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException) when (attempt < maxAttempts)
            {
                await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
            }
            catch (HttpRequestException exception) when (
                exception.StatusCode != HttpStatusCode.TooManyRequests &&
                IsTransient(exception.StatusCode) &&
                attempt < maxAttempts)
            {
                await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
            }
        }

        throw new TimeoutException("Steam listing request timed out after all retry attempts.");
    }

    private static HttpRequestMessage CreateRequest(Uri uri, string userAgent)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.UserAgent.ParseAdd(userAgent);
        request.Headers.Accept.ParseAdd("text/html, application/xhtml+xml, application/json;q=0.9, */*;q=0.8");
        request.Headers.AcceptLanguage.ParseAdd("en-US, en;q=0.9");
        return request;
    }

    public string SelectUserAgent(long runId)
    {
        var userAgents = options.Value.UserAgents
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        if (userAgents.Count == 0)
        {
            return "SteamApp-ManualCheck/1.0";
        }

        var remainder = runId % userAgents.Count;
        var index = (int)(remainder < 0 ? -remainder : remainder);
        return userAgents[index];
    }

    private async Task ThrowIfSteamRateLimitedAsync(CancellationToken cancellationToken)
    {
        string? cachedRetryAt;
        try
        {
            cachedRetryAt = await cache.GetStringAsync(
                CacheKeys.ManualCheckSteamRateLimit,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to read the shared Steam rate-limit cooldown.");
            return;
        }

        if (DateTimeOffset.TryParseExact(
                cachedRetryAt,
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var retryAtUtc))
        {
            if (retryAtUtc > DateTimeOffset.UtcNow)
            {
                throw new HttpRequestException(
                    $"Steam requests are paused until {retryAtUtc:O} after a previous rate-limit response.",
                    null,
                    HttpStatusCode.TooManyRequests);
            }
        }
    }

    private async Task<DateTimeOffset> ActivateSteamRateLimitAsync(
        RetryConditionHeaderValue? retryAfter,
        CancellationToken cancellationToken)
    {
        var cooldown = GetRateLimitCooldown(retryAfter);
        var retryAtUtc = DateTimeOffset.UtcNow.Add(cooldown);
        try
        {
            await cache.SetStringAsync(
                CacheKeys.ManualCheckSteamRateLimit,
                retryAtUtc.ToString("O", CultureInfo.InvariantCulture),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cooldown
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to store the shared Steam rate-limit cooldown.");
        }

        return retryAtUtc;
    }

    private static string BuildCacheKey(Uri uri)
    {
        var urlHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(uri.AbsoluteUri)));
        return string.Format(CacheKeys.ManualCheckSteamListing, urlHash);
    }

    private static bool IsHtmlResponse(string? contentType, string payload)
    {
        if (contentType?.Contains("html", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        var firstContent = payload.AsSpan().TrimStart();
        return firstContent.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
               firstContent.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSteamRateLimitPage(string payload)
    {
        return payload.Contains("too many requests", StringComparison.OrdinalIgnoreCase) &&
               payload.Contains("wait and try", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTransient(HttpStatusCode? statusCode)
    {
        if (statusCode is null)
        {
            return true;
        }

        return statusCode == HttpStatusCode.RequestTimeout ||
               statusCode == HttpStatusCode.TooManyRequests ||
               (int)statusCode >= 500;
    }

    private TimeSpan GetRetryDelay(RetryConditionHeaderValue? retryAfter, int attempt)
    {
        if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta;
        }

        if (retryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return delay;
            }
        }

        return GetBackoffDelay(attempt);
    }

    private TimeSpan GetRateLimitCooldown(RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta;
        }

        if (retryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return delay;
            }
        }

        var baseCooldown = options.Value.RateLimitCooldown <= TimeSpan.Zero
            ? TimeSpan.FromMinutes(15)
            : options.Value.RateLimitCooldown;
        var maximumJitter = options.Value.RateLimitJitter <= TimeSpan.Zero
            ? TimeSpan.Zero
            : options.Value.RateLimitJitter;
        var maximumJitterMilliseconds = (int)Math.Min(
            maximumJitter.TotalMilliseconds,
            int.MaxValue - 1d);
        var jitterMilliseconds = maximumJitterMilliseconds == 0
            ? 0
            : RandomNumberGenerator.GetInt32(maximumJitterMilliseconds + 1);
        return baseCooldown + TimeSpan.FromMilliseconds(jitterMilliseconds);
    }

    private TimeSpan GetBackoffDelay(int attempt)
    {
        var baseDelay = options.Value.RetryBaseDelay <= TimeSpan.Zero
            ? TimeSpan.FromSeconds(1)
            : options.Value.RetryBaseDelay;
        var milliseconds = baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        return TimeSpan.FromMilliseconds(Math.Min(milliseconds, TimeSpan.FromMinutes(2).TotalMilliseconds));
    }

    private static TimeSpan NormalizedTimeout(TimeSpan timeout)
    {
        return timeout <= TimeSpan.Zero ? TimeSpan.FromSeconds(30) : timeout;
    }
}

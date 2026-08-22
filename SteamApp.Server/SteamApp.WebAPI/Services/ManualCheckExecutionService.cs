using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.JsonObjects;
using SteamApp.Domain.Enums;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.ManualChecks;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace SteamApp.WebAPI.Services;

public sealed class ManualCheckExecutionService(
    IHttpClientFactory httpClientFactory,
    IOptions<ManualCheckOptions> options,
    IDistributedCache cache,
    IManualCheckDataService dataService,
    IManualCheckDelay delay,
    ILogger<ManualCheckExecutionService> logger) : IManualCheckExecutionService
{
    private static readonly TimeSpan SteamListingCacheDuration = TimeSpan.FromMinutes(20);

    public async Task ExecuteAsync(long runId, CancellationToken cancellationToken)
    {
        var setup = await dataService.MarkRunningAndGetSetupAsync(runId, cancellationToken);
        if (setup is null)
        {
            return;
        }

        var results = new ManualCheckRunResultsDto();
        var successfulProducts = 0;
        var delayBetweenChecks = GetDelayBetweenChecks(setup);

        try
        {
            for (var index = 0; index < setup.Products.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var product = setup.Products[index];

                if (index > 0)
                {
                    await delay.DelayAsync(delayBetweenChecks, cancellationToken);
                }

                var productTrace = CreateProductTrace(product);
                results.ProductTraces.Add(productTrace);
                var productStartedTimestamp = Stopwatch.GetTimestamp();
                try
                {
                    if (!ManualCheckMatcher.TryBuildListingUri(product.FullUrl, out var listingUri) || listingUri is null)
                    {
                        throw new InvalidOperationException("The product URL is not a supported Steam Community listing URL.");
                    }

                    var listing = await FetchListingAsync(
                        listingUri,
                        setup.BypassCache,
                        cancellationToken);
                    productTrace.SteamApiResultJson = JsonConvert.SerializeObject(listing);
                    successfulProducts++;

                    var match = ManualCheckMatcher.MatchProduct(
                        product,
                        listing,
                        setup.Criteria,
                        setup.ListingLimit);
                    productTrace.MatchEvaluated = true;
                    productTrace.Matched = match is not null;
                    productTrace.MatchedAssetCount = match?.MatchedAssets.Count ?? 0;
                    if (match is not null)
                    {
                        results.Matches.Add(match);
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
                        "Manual check run {RunId} failed for product {ProductId}.",
                        runId,
                        product.ProductId);

                    results.Errors.Add(new ManualCheckProductErrorDto
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        FullUrl = product.FullUrl,
                        Error = exception.Message,
                        ErrorType = exception.GetType().Name,
                        HttpStatusCode = exception is HttpRequestException requestException
                            ? (int?)requestException.StatusCode
                            : null,
                        OccurredAtUtc = DateTime.UtcNow
                    });
                }
                finally
                {
                    productTrace.DurationMilliseconds = Math.Max(
                        0,
                        (long)Math.Round(Stopwatch.GetElapsedTime(productStartedTimestamp).TotalMilliseconds));
                }

                await dataService.UpdateProgressAsync(
                    runId,
                    index + 1,
                    results.Matches.Count,
                    results.Errors.Count,
                    results,
                    cancellationToken);
            }

            if (successfulProducts == 0)
            {
                await dataService.FailAsync(
                    runId,
                    BuildAllProductsFailedMessage(results.Errors),
                    results,
                    cancellationToken);
                return;
            }

            var status = results.Errors.Count == 0
                ? ManualCheckRunStatusEnum.Succeeded
                : ManualCheckRunStatusEnum.CompletedWithErrors;

            await dataService.CompleteAsync(runId, status, results, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Manual check run {RunId} failed.", runId);
            await dataService.FailAsync(runId, exception.Message, results, cancellationToken);
        }
    }

    private static ManualCheckProductTraceDto CreateProductTrace(ManualCheckProductInputDto product)
    {
        return new ManualCheckProductTraceDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            FullUrl = product.FullUrl
        };
    }

    private TimeSpan GetDelayBetweenChecks(ManualCheckSetupDto setup)
    {
        if (setup.CooldownMinutes is >= 0 and <= 59 && setup.CooldownSeconds is >= 0 and <= 59)
        {
            return TimeSpan.FromMinutes(setup.CooldownMinutes.Value) +
                   TimeSpan.FromSeconds(setup.CooldownSeconds.Value);
        }

        if (setup.CooldownMinutes.HasValue || setup.CooldownSeconds.HasValue)
        {
            logger.LogWarning(
                "Manual-check setup contains an invalid custom cooldown; the configured server delay will be used.");
        }

        return NormalizedDelay(options.Value.DelayBetweenRequests);
    }

    private async Task<Listing> FetchListingAsync(
        Uri uri,
        bool bypassCache,
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

        var listing = await FetchListingFromSteamAsync(uri, cancellationToken);
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

    private async Task<Listing> FetchListingFromSteamAsync(Uri uri, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(ManualCheckOptions.HttpClientName);
        var maxAttempts = Math.Clamp(options.Value.MaxAttempts, 1, 10);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(NormalizedTimeout(options.Value.RequestTimeout));

                using var response = await client.GetAsync(
                    uri,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);

                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadAsStringAsync(timeout.Token);
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (IsHtmlResponse(contentType, payload))
                    {
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
            catch (HttpRequestException exception) when (IsTransient(exception.StatusCode) && attempt < maxAttempts)
            {
                await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
            }
        }

        throw new TimeoutException("Steam listing request timed out after all retry attempts.");
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

    private static string BuildAllProductsFailedMessage(
        IReadOnlyCollection<ManualCheckProductErrorDto> errors)
    {
        if (errors.Count == 0)
        {
            return "Every product check failed without a recorded reason.";
        }

        var mostCommon = errors
            .Where(x => !string.IsNullOrWhiteSpace(x.Error))
            .GroupBy(x => x.Error.Trim(), StringComparer.Ordinal)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .FirstOrDefault();

        if (mostCommon is null)
        {
            return $"All {errors.Count} product checks failed without a recorded reason.";
        }

        return $"All {errors.Count} product checks failed. " +
               $"Most common error ({mostCommon.Count()}/{errors.Count}): {mostCommon.Key}";
    }

    private TimeSpan GetRetryDelay(RetryConditionHeaderValue? retryAfter, int attempt)
    {
        if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta > TimeSpan.FromMinutes(2) ? TimeSpan.FromMinutes(2) : delta;
        }

        if (retryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return delay > TimeSpan.FromMinutes(2) ? TimeSpan.FromMinutes(2) : delay;
            }
        }

        return GetBackoffDelay(attempt);
    }

    private TimeSpan GetBackoffDelay(int attempt)
    {
        var baseDelay = options.Value.RetryBaseDelay <= TimeSpan.Zero
            ? TimeSpan.FromSeconds(1)
            : options.Value.RetryBaseDelay;
        var milliseconds = baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        return TimeSpan.FromMilliseconds(Math.Min(milliseconds, TimeSpan.FromMinutes(2).TotalMilliseconds));
    }

    private static TimeSpan NormalizedDelay(TimeSpan delay)
    {
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }

    private static TimeSpan NormalizedTimeout(TimeSpan timeout)
    {
        return timeout <= TimeSpan.Zero ? TimeSpan.FromSeconds(30) : timeout;
    }
}

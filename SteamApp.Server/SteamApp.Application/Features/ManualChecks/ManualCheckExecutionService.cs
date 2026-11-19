using SteamApp.Application.Abstractions.Persistence;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamApp.Application.Abstractions.Services;
using SteamApp.Contracts.ManualCheck;
using SteamApp.Domain.Enums;

namespace SteamApp.Application.Features.ManualChecks;

public sealed class ManualCheckExecutionService(
    IManualCheckListingSource listingSource,
    IManualCheckDataService dataService,
    IManualCheckDelay delay,
    ILogger<ManualCheckExecutionService> logger) : IManualCheckExecutionService
{
    public async Task ExecuteAsync(
        long runId,
        CancellationToken cancellationToken,
        CancellationToken pauseToken)
    {
        var run = await dataService.MarkRunningAndGetRunAsync(runId, cancellationToken);
        if (run is null)
        {
            return;
        }

        var setup = run.Setup;
        var results = run.Results;
        var successfulProducts = results.ProductTraces.Count(x => x.MatchEvaluated);
        var startIndex = Math.Clamp(run.CheckedProducts, 0, setup.Products.Count);
        var delayBetweenChecks = GetDelayBetweenChecks(setup);
        var userAgent = listingSource.SelectUserAgent(runId);

        try
        {
            for (var index = startIndex; index < setup.Products.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (pauseToken.IsCancellationRequested)
                {
                    await dataService.MarkPausedAsync(runId, cancellationToken);
                    return;
                }

                var product = setup.Products[index];

                if (index > 0)
                {
                    using var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationToken,
                        pauseToken);
                    try
                    {
                        await delay.DelayAsync(delayBetweenChecks, delayCancellation.Token);
                    }
                    catch (OperationCanceledException) when (
                        pauseToken.IsCancellationRequested &&
                        !cancellationToken.IsCancellationRequested)
                    {
                        await dataService.MarkPausedAsync(runId, cancellationToken);
                        return;
                    }
                }

                var productTrace = CreateProductTrace(product);
                results.ProductTraces.Add(productTrace);
                var productStartedTimestamp = Stopwatch.GetTimestamp();
                using var productCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    pauseToken);
                try
                {
                    if (!ManualCheckMatcher.TryBuildListingUri(product.FullUrl, out var listingUri) || listingUri is null)
                    {
                        throw new InvalidOperationException("The product URL is not a supported Steam Community listing URL.");
                    }

                    var listing = await listingSource.FetchListingAsync(
                        listingUri,
                        setup.BypassCache,
                        userAgent,
                        productCancellation.Token);
                    productTrace.SteamApiResultJson = JsonConvert.SerializeObject(listing);
                    successfulProducts++;

                    var match = ManualCheckMatcher.MatchProduct(
                        product,
                        listing,
                        setup.Criteria,
                        setup.ListingLimit,
                        out var lowestCheckedPriceMinorUnits,
                        out var checkedAssets);
                    productTrace.LowestCheckedPriceMinorUnits = lowestCheckedPriceMinorUnits;
                    productTrace.PriceCurrencyCode = lowestCheckedPriceMinorUnits.HasValue ? "EUR" : null;
                    productTrace.PriceRangeMatched = setup.PriceRange is not null && lowestCheckedPriceMinorUnits.HasValue
                        ? ManualCheckMatcher.MatchesPriceRange(lowestCheckedPriceMinorUnits.Value, setup.PriceRange)
                        : null;
                    productTrace.CheckedAssets = checkedAssets;
                    foreach (var checkedAsset in checkedAssets)
                    {
                        checkedAsset.PriceRangeMatched = setup.PriceRange is not null && checkedAsset.PriceMinorUnits.HasValue
                            ? ManualCheckMatcher.MatchesPriceRange(checkedAsset.PriceMinorUnits.Value, setup.PriceRange)
                            : null;
                    }
                    productTrace.MatchEvaluated = true;
                    productTrace.Matched = match is not null;
                    productTrace.MatchedAssetCount = match?.MatchedAssets.Count ?? 0;
                    if (match is not null)
                    {
                        foreach (var matchedAsset in match.MatchedAssets)
                        {
                            matchedAsset.PriceRangeMatched = setup.PriceRange is not null && matchedAsset.PriceMinorUnits.HasValue
                                ? ManualCheckMatcher.MatchesPriceRange(matchedAsset.PriceMinorUnits.Value, setup.PriceRange)
                                : null;
                        }
                        results.Matches.Add(match);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException) when (pauseToken.IsCancellationRequested)
                {
                    results.ProductTraces.Remove(productTrace);
                    await dataService.MarkPausedAsync(runId, cancellationToken);
                    return;
                }
                catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    logger.LogWarning(
                        exception,
                        "Manual check run {RunId} stopped after Steam rate-limited product {ProductId}.",
                        runId,
                        product.ProductId);
                    results.Errors.Add(CreateProductError(product, exception));
                    await dataService.FailAsync(
                        runId,
                        BuildAllProductsFailedMessage(results.Errors),
                        results,
                        cancellationToken);
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(
                        exception,
                        "Manual check run {RunId} failed for product {ProductId}.",
                        runId,
                        product.ProductId);

                    results.Errors.Add(CreateProductError(product, exception));
                }
                finally
                {
                    productTrace.DurationMilliseconds = Math.Max(
                        0,
                        (long)Math.Round(Stopwatch.GetElapsedTime(productStartedTimestamp).TotalMilliseconds));
                }

                var progressStatus = await dataService.UpdateProgressAsync(
                    runId,
                    index + 1,
                    results.Matches.Count,
                    results.Errors.Count,
                    results,
                    cancellationToken);

                if (progressStatus is ManualCheckRunStatusEnum.Paused or ManualCheckRunStatusEnum.Canceled)
                {
                    return;
                }
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

        return NormalizedDelay(listingSource.DefaultCheckDelay);
    }

    private static ManualCheckProductErrorDto CreateProductError(
        ManualCheckProductInputDto product,
        Exception exception)
    {
        return new ManualCheckProductErrorDto
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
        };
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

    private static TimeSpan NormalizedDelay(TimeSpan delay)
    {
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }
}

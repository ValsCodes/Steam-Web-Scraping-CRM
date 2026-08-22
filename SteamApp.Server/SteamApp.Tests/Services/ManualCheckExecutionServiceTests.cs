using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.JsonObjects;
using SteamApp.Domain.Enums;
using SteamApp.Interfaces.Services;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.ManualChecks;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class ManualCheckExecutionServiceTests
{
    [Test]
    public async Task ExecuteAsync_RetriesThreeTransientFailuresThenSucceeds()
    {
        var attempts = 0;
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            if (attempts < 4)
            {
                var transient = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                transient.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                return transient;
            }
            return JsonResponse(new Listing { Success = true });
        });
        var data = DataServiceMock(Setup(Product(1, "First")));
        var service = CreateService(handler, data.Object, maxAttempts: 4);

        await service.ExecuteAsync(10, CancellationToken.None, CancellationToken.None);

        Assert.That(attempts, Is.EqualTo(4));
        data.Verify(x => x.CompleteAsync(
            10,
            ManualCheckRunStatusEnum.Succeeded,
            It.Is<ManualCheckRunResultsDto>(r =>
                r.Errors.Count == 0 &&
                r.ProductTraces.Count == 1 &&
                r.ProductTraces[0].ProductId == 1 &&
                r.ProductTraces[0].MatchEvaluated &&
                !r.ProductTraces[0].Matched &&
                r.ProductTraces[0].DurationMilliseconds.HasValue &&
                r.ProductTraces[0].SteamApiResultJson!.Contains("\"success\":true")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ContinuesAfterProductFailureAndCompletesWithPartialResults()
    {
        var matchingListing = new Listing { Success = true };
        matchingListing.Assets["440"] = new Dictionary<string, Dictionary<string, AssetDetail>>
        {
            ["2"] = new()
            {
                ["asset"] = new AssetDetail
                {
                    Descriptions = [new Description { Name = "attribute", Value = "Sheen: Mean Green" }]
                }
            }
        };
        matchingListing.TotalCount = 1;
        matchingListing.ListingInfo = new Dictionary<string, ListingInfo>
        {
            ["listing"] = new ListingInfo
            {
                ListingId = "listing",
                Price = 100,
                Fee = 15,
                Asset = new Asset { AppId = 440, ContextId = "2", Id = "asset" }
            }
        };
        var handler = new HttpMessageHandlerStub((request, _) =>
            request.RequestUri!.AbsoluteUri.Contains("First", StringComparison.Ordinal)
                ? JsonResponse(matchingListing)
                : new HttpResponseMessage(HttpStatusCode.BadRequest));
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(11, CancellationToken.None, CancellationToken.None);

        data.Verify(x => x.UpdateProgressAsync(
            11,
            2,
            1,
            1,
            It.Is<ManualCheckRunResultsDto>(results => results.Matches.Count == 1 && results.Errors.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        data.Verify(x => x.CompleteAsync(
            11,
            ManualCheckRunStatusEnum.CompletedWithErrors,
            It.Is<ManualCheckRunResultsDto>(r =>
                r.Matches.Count == 1 &&
                r.Errors.Count == 1 &&
                r.ProductTraces.Count == 2 &&
                r.ProductTraces.All(trace => trace.DurationMilliseconds.HasValue)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_FailsRunWhenNoProductsCanBeChecked()
    {
        var handler = new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(HttpStatusCode.BadRequest));
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(12, CancellationToken.None, CancellationToken.None);

        data.Verify(x => x.FailAsync(
            12,
            It.Is<string>(message =>
                message.Contains("All 2 product checks failed.") &&
                message.Contains("HTTP 400")),
            It.Is<ManualCheckRunResultsDto>(r =>
                r.Errors.Count == 2 &&
                r.ProductTraces.Count == 2 &&
                r.ProductTraces.All(trace =>
                    trace.DurationMilliseconds.HasValue &&
                    trace.SteamApiResultJson == null) &&
                r.Errors.All(error =>
                    error.HttpStatusCode == 400 &&
                    error.ErrorType == nameof(HttpRequestException) &&
                    error.OccurredAtUtc.HasValue)),
            It.IsAny<CancellationToken>()), Times.Once);
        data.Verify(x => x.CompleteAsync(
            It.IsAny<long>(),
            It.IsAny<ManualCheckRunStatusEnum>(),
            It.IsAny<ManualCheckRunResultsDto>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_RecordsReadableTraceWhenSteamReturnsHtmlInsteadOfJson()
    {
        var handler = new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<!DOCTYPE html><html><body>Steam Market</body></html>",
                    Encoding.UTF8,
                    "text/html")
            });
        var data = DataServiceMock(Setup(Product(1, "First")));
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(13, CancellationToken.None, CancellationToken.None);

        data.Verify(x => x.FailAsync(
            13,
            It.Is<string>(message =>
                message.Contains("embedded listing data could not be read") &&
                !message.Contains("Unexpected character")),
            It.Is<ManualCheckRunResultsDto>(results =>
                results.Errors.Count == 1 &&
                results.Errors[0].HttpStatusCode == 200 &&
                results.Errors[0].ErrorType == nameof(HttpRequestException)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ParsesListingsFromSteamsServerRenderedMarketPage()
    {
        var handler = new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    SteamMarketPage("Sheen: Mean Green"),
                    Encoding.UTF8,
                    "text/html")
            });
        var data = DataServiceMock(Setup(Product(1, "First")));
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(14, CancellationToken.None, CancellationToken.None);

        data.Verify(x => x.CompleteAsync(
            14,
            ManualCheckRunStatusEnum.Succeeded,
            It.Is<ManualCheckRunResultsDto>(results =>
                results.Errors.Count == 0 &&
                results.Matches.Count == 1 &&
                results.ProductTraces.Count == 1 &&
                results.ProductTraces[0].MatchEvaluated &&
                results.ProductTraces[0].Matched &&
                results.ProductTraces[0].MatchedAssetCount == 1 &&
                results.Matches[0].MatchedAssets.Count == 1 &&
                results.Matches[0].MatchedAssets[0].Descriptions[0].Value == "Sheen: Mean Green"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_RecordsActionableErrorWhenListingsHaveNoUsablePrice()
    {
        var handler = new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    SteamMarketPage("Sheen: Mean Green", price: 0, fee: 0),
                    Encoding.UTF8,
                    "text/html")
            });
        var data = DataServiceMock(Setup(Product(1, "First")));
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(15, CancellationToken.None, CancellationToken.None);

        data.Verify(x => x.CompleteAsync(
            15,
            ManualCheckRunStatusEnum.CompletedWithErrors,
            It.Is<ManualCheckRunResultsDto>(results =>
                results.Errors.Count == 1 &&
                results.ProductTraces.Count == 1 &&
                !results.ProductTraces[0].MatchEvaluated &&
                !results.ProductTraces[0].Matched &&
                results.Errors[0].ErrorType == nameof(InvalidOperationException) &&
                results.Errors[0].Error.Contains("usable price and asset information")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_SuccessfulResponseIsCachedForTwentyMinutes()
    {
        var attempts = 0;
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            return JsonResponse(new Listing { Success = true });
        });
        DistributedCacheEntryOptions? cacheOptions = null;
        var cache = StatefulCache(options => cacheOptions = options);
        var data = DataServiceMock(Setup(Product(1, "First")));
        var service = CreateService(handler, data.Object, maxAttempts: 1, cache.Object);

        await service.ExecuteAsync(16, CancellationToken.None, CancellationToken.None);
        await service.ExecuteAsync(17, CancellationToken.None, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(attempts, Is.EqualTo(1));
            Assert.That(cacheOptions?.AbsoluteExpirationRelativeToNow, Is.EqualTo(TimeSpan.FromMinutes(20)));
        });
    }

    [Test]
    public async Task ExecuteAsync_BypassCacheFetchesAndReplacesSteamResponse()
    {
        var attempts = 0;
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            return JsonResponse(new Listing { Success = true });
        });
        var cache = StatefulCache();
        var cachedRun = CreateService(
            handler,
            DataServiceMock(Setup(Product(1, "First"))).Object,
            maxAttempts: 1,
            cache.Object);
        var refreshRun = CreateService(
            handler,
            DataServiceMock(Setup(true, Product(1, "First"))).Object,
            maxAttempts: 1,
            cache.Object);

        await cachedRun.ExecuteAsync(18, CancellationToken.None, CancellationToken.None);
        await refreshRun.ExecuteAsync(19, CancellationToken.None, CancellationToken.None);

        Assert.That(attempts, Is.EqualTo(2));
        cache.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task ExecuteAsync_CacheReadFailureFallsBackToSteam()
    {
        var attempts = 0;
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            return JsonResponse(new Listing { Success = true });
        });
        var cache = new Mock<IDistributedCache>();
        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));
        cache.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var data = DataServiceMock(Setup(Product(1, "First")));
        var service = CreateService(handler, data.Object, maxAttempts: 1, cache.Object);

        await service.ExecuteAsync(20, CancellationToken.None, CancellationToken.None);

        Assert.That(attempts, Is.EqualTo(1));
        data.Verify(x => x.CompleteAsync(
            20,
            ManualCheckRunStatusEnum.Succeeded,
            It.IsAny<ManualCheckRunResultsDto>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_CustomCooldown_AppliesOnceBetweenTwoChecks()
    {
        var handler = new HttpMessageHandlerStub((_, _) => JsonResponse(new Listing { Success = true }));
        var setup = Setup(Product(1, "First"), Product(2, "Second"));
        setup.CooldownMinutes = 1;
        setup.CooldownSeconds = 9;
        var data = DataServiceMock(setup);
        var delay = new Mock<IManualCheckDelay>();
        delay.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(handler, data.Object, maxAttempts: 1, delay: delay.Object);

        await service.ExecuteAsync(21, CancellationToken.None, CancellationToken.None);

        delay.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(69), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_NullCooldown_UsesConfiguredDefault()
    {
        var handler = new HttpMessageHandlerStub((_, _) => JsonResponse(new Listing { Success = true }));
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        var delay = new Mock<IManualCheckDelay>();
        delay.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(
            handler,
            data.Object,
            maxAttempts: 1,
            delay: delay.Object,
            defaultDelay: TimeSpan.FromSeconds(3));

        await service.ExecuteAsync(22, CancellationToken.None, CancellationToken.None);

        delay.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(3), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_PauseDuringProduct_FinishesAndPersistsTheCurrentProduct()
    {
        var attempts = 0;
        using var pause = new CancellationTokenSource();
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            pause.Cancel();
            return JsonResponse(new Listing { Success = true });
        });
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        data.Setup(x => x.UpdateProgressAsync(
                23,
                1,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ManualCheckRunResultsDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ManualCheckRunStatusEnum.Paused);
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(23, CancellationToken.None, pause.Token);

        Assert.That(attempts, Is.EqualTo(1));
        data.Verify(x => x.UpdateProgressAsync(
            23,
            1,
            0,
            0,
            It.Is<ManualCheckRunResultsDto>(results =>
                results.ProductTraces.Count == 1 &&
                results.ProductTraces[0].MatchEvaluated),
            It.IsAny<CancellationToken>()), Times.Once);
        data.Verify(x => x.CompleteAsync(
            It.IsAny<long>(),
            It.IsAny<ManualCheckRunStatusEnum>(),
            It.IsAny<ManualCheckRunResultsDto>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_PauseDuringCooldown_ReleasesBeforeTheNextProduct()
    {
        var attempts = 0;
        using var pause = new CancellationTokenSource();
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            return JsonResponse(new Listing { Success = true });
        });
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        var delay = new Mock<IManualCheckDelay>();
        delay.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns((TimeSpan _, CancellationToken token) =>
            {
                pause.Cancel();
                return Task.FromCanceled(token);
            });
        var service = CreateService(handler, data.Object, maxAttempts: 1, delay: delay.Object);

        await service.ExecuteAsync(24, CancellationToken.None, pause.Token);

        Assert.That(attempts, Is.EqualTo(1));
        data.Verify(x => x.MarkPausedAsync(24, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ContinuedRun_ResumesAtTheFirstUncheckedProduct()
    {
        var requestedProducts = new List<string>();
        var handler = new HttpMessageHandlerStub((request, _) =>
        {
            requestedProducts.Add(request.RequestUri!.AbsoluteUri);
            return JsonResponse(new Listing { Success = true });
        });
        var setup = Setup(Product(1, "First"), Product(2, "Second"));
        var data = new Mock<IManualCheckDataService>();
        data.Setup(x => x.MarkRunningAndGetRunAsync(25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckRunDetailDto
            {
                Id = 25,
                Status = ManualCheckRunStatusEnum.Running,
                CheckedProducts = 1,
                Setup = setup,
                Results = new ManualCheckRunResultsDto
                {
                    ProductTraces =
                    [
                        new ManualCheckProductTraceDto
                        {
                            ProductId = 1,
                            ProductName = "First",
                            MatchEvaluated = true
                        }
                    ]
                }
            });
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(25, CancellationToken.None, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(requestedProducts, Has.Count.EqualTo(1));
            Assert.That(requestedProducts[0], Does.Contain("Second"));
        });
        data.Verify(x => x.UpdateProgressAsync(
            25,
            2,
            0,
            0,
            It.Is<ManualCheckRunResultsDto>(results => results.ProductTraces.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void ExecuteAsync_CancellationDuringCooldown_StopsBeforeNextProduct()
    {
        var attempts = 0;
        var handler = new HttpMessageHandlerStub((_, _) =>
        {
            attempts++;
            return JsonResponse(new Listing { Success = true });
        });
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        using var cancellation = new CancellationTokenSource();
        var delay = new Mock<IManualCheckDelay>();
        delay.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns((TimeSpan _, CancellationToken token) =>
            {
                cancellation.Cancel();
                return Task.FromCanceled(token);
            });
        var service = CreateService(handler, data.Object, maxAttempts: 1, delay: delay.Object);

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await service.ExecuteAsync(23, cancellation.Token, CancellationToken.None));

        Assert.That(attempts, Is.EqualTo(1));
        data.Verify(x => x.CompleteAsync(
            It.IsAny<long>(),
            It.IsAny<ManualCheckRunStatusEnum>(),
            It.IsAny<ManualCheckRunResultsDto>(),
            It.IsAny<CancellationToken>()), Times.Never);
        data.Verify(x => x.FailAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<ManualCheckRunResultsDto>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ManualCheckExecutionService CreateService(
        HttpMessageHandler handler,
        IManualCheckDataService dataService,
        int maxAttempts,
        IDistributedCache? cache = null,
        IManualCheckDelay? delay = null,
        TimeSpan? defaultDelay = null)
    {
        var client = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(ManualCheckOptions.HttpClientName)).Returns(client);
        var options = Options.Create(new ManualCheckOptions
        {
            DelayBetweenRequests = defaultDelay ?? TimeSpan.Zero,
            RetryBaseDelay = TimeSpan.FromMilliseconds(1),
            RequestTimeout = TimeSpan.FromSeconds(2),
            MaxAttempts = maxAttempts
        });
        return new ManualCheckExecutionService(
            factory.Object,
            options,
            cache ?? EmptyCache().Object,
            dataService,
            delay ?? new ImmediateManualCheckDelay(),
            NullLogger<ManualCheckExecutionService>.Instance);
    }

    private static Mock<IManualCheckDataService> DataServiceMock(ManualCheckSetupDto setup)
    {
        var data = new Mock<IManualCheckDataService>();
        data.Setup(x => x.MarkRunningAndGetRunAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManualCheckRunDetailDto
            {
                Status = ManualCheckRunStatusEnum.Running,
                Setup = setup,
                Results = new ManualCheckRunResultsDto()
            });
        return data;
    }

    private static ManualCheckSetupDto Setup(params ManualCheckProductInputDto[] products)
    {
        return Setup(false, products);
    }

    private static ManualCheckSetupDto Setup(
        bool bypassCache,
        params ManualCheckProductInputDto[] products)
    {
        return new ManualCheckSetupDto
        {
            ListingLimit = 10,
            BypassCache = bypassCache,
            Criteria =
            [
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = null,
                    ValueContains = "Mean Green"
                }
            ],
            Products = products.ToList()
        };
    }

    private static Mock<IDistributedCache> EmptyCache()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        cache.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return cache;
    }

    private static Mock<IDistributedCache> StatefulCache(
        Action<DistributedCacheEntryOptions>? onSet = null)
    {
        byte[]? value = null;
        var cache = new Mock<IDistributedCache>();
        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => value);
        cache.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback((string _, byte[] bytes, DistributedCacheEntryOptions options, CancellationToken _) =>
            {
                value = bytes;
                onSet?.Invoke(options);
            })
            .Returns(Task.CompletedTask);
        return cache;
    }

    private static ManualCheckProductInputDto Product(long id, string name)
    {
        return new ManualCheckProductInputDto
        {
            ProductId = id,
            ProductName = name,
            GameUrlId = 1,
            GameUrlName = "Market",
            FullUrl = $"https://steamcommunity.com/market/listings/440/{name}"
        };
    }

    private static HttpResponseMessage JsonResponse(Listing listing)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(listing),
                Encoding.UTF8,
                "application/json")
        };
    }

    private static string SteamMarketPage(string descriptionValue, int price = 100, int fee = 15)
    {
        var queryData = JsonConvert.SerializeObject(new
        {
            mutations = Array.Empty<object>(),
            queries = new object[]
            {
                new
                {
                    state = new
                    {
                        data = new
                        {
                            pages = new object[]
                            {
                                new
                                {
                                    start = 0,
                                    total_count = 1,
                                    listings = new object[]
                                    {
                                        new
                                        {
                                            listingid = "listing-1",
                                            unPrice = price,
                                            unFee = fee,
                                            eCurrency = 3,
                                            description = new
                                            {
                                                appid = 440,
                                                classid = "class-1",
                                                instanceid = "instance-1",
                                                market_name = "First",
                                                icon_url = "icon",
                                                descriptions = new object[]
                                                {
                                                    new
                                                    {
                                                        name = "attribute",
                                                        value = descriptionValue,
                                                        color = "7ea9d1"
                                                    }
                                                }
                                            },
                                            asset = new
                                            {
                                                appid = 440,
                                                contextid = "2",
                                                assetid = "asset-1",
                                                classid = "class-1",
                                                instanceid = "instance-1"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    queryKey = new object[] { "market_item_search", new { appid = 440 } }
                }
            }
        });
        var renderContext = JsonConvert.SerializeObject(new { queryData });
        return $"<!DOCTYPE html><html><script>window.SSR.renderContext=JSON.parse({JsonConvert.SerializeObject(renderContext)});</script></html>";
    }

    private sealed class ImmediateManualCheckDelay : IManualCheckDelay
    {
        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

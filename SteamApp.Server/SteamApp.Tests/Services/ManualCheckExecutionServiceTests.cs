using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.JsonObjects;
using SteamApp.Domain.Enums;
using SteamApp.Tests.TestSupport;
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

        await service.ExecuteAsync(10, CancellationToken.None);

        Assert.That(attempts, Is.EqualTo(4));
        data.Verify(x => x.CompleteAsync(
            10,
            ManualCheckRunStatusEnum.Succeeded,
            It.Is<ManualCheckRunResultsDto>(r => r.Errors.Count == 0),
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

        await service.ExecuteAsync(11, CancellationToken.None);

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
            It.Is<ManualCheckRunResultsDto>(r => r.Matches.Count == 1 && r.Errors.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_FailsRunWhenNoProductsCanBeChecked()
    {
        var handler = new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(HttpStatusCode.BadRequest));
        var data = DataServiceMock(Setup(Product(1, "First"), Product(2, "Second")));
        var service = CreateService(handler, data.Object, maxAttempts: 1);

        await service.ExecuteAsync(12, CancellationToken.None);

        data.Verify(x => x.FailAsync(
            12,
            It.Is<string>(message =>
                message.Contains("All 2 product checks failed.") &&
                message.Contains("HTTP 400")),
            It.Is<ManualCheckRunResultsDto>(r =>
                r.Errors.Count == 2 &&
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

        await service.ExecuteAsync(13, CancellationToken.None);

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

        await service.ExecuteAsync(14, CancellationToken.None);

        data.Verify(x => x.CompleteAsync(
            14,
            ManualCheckRunStatusEnum.Succeeded,
            It.Is<ManualCheckRunResultsDto>(results =>
                results.Errors.Count == 0 &&
                results.Matches.Count == 1 &&
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

        await service.ExecuteAsync(15, CancellationToken.None);

        data.Verify(x => x.CompleteAsync(
            15,
            ManualCheckRunStatusEnum.CompletedWithErrors,
            It.Is<ManualCheckRunResultsDto>(results =>
                results.Errors.Count == 1 &&
                results.Errors[0].ErrorType == nameof(InvalidOperationException) &&
                results.Errors[0].Error.Contains("usable price and asset information")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ManualCheckExecutionService CreateService(
        HttpMessageHandler handler,
        IManualCheckDataService dataService,
        int maxAttempts)
    {
        var client = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(ManualCheckOptions.HttpClientName)).Returns(client);
        var options = Options.Create(new ManualCheckOptions
        {
            DelayBetweenRequests = TimeSpan.Zero,
            RetryBaseDelay = TimeSpan.FromMilliseconds(1),
            RequestTimeout = TimeSpan.FromSeconds(2),
            MaxAttempts = maxAttempts
        });
        return new ManualCheckExecutionService(
            factory.Object,
            options,
            dataService,
            NullLogger<ManualCheckExecutionService>.Instance);
    }

    private static Mock<IManualCheckDataService> DataServiceMock(ManualCheckSetupDto setup)
    {
        var data = new Mock<IManualCheckDataService>();
        data.Setup(x => x.MarkRunningAndGetSetupAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(setup);
        return data;
    }

    private static ManualCheckSetupDto Setup(params ManualCheckProductInputDto[] products)
    {
        return new ManualCheckSetupDto
        {
            MatchMode = ManualCheckMatchModeEnum.Any,
            ListingLimit = 10,
            Criteria = [new ManualCheckCriterionDto { ValueContains = "Mean Green" }],
            Products = products.ToList()
        };
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
}

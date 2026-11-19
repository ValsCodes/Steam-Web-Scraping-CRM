using Moq;
using OpenQA.Selenium;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.ExternalServices;
using SteamApp.Application.Abstractions.Persistence;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class SteamServiceTests
{
    [TestCase("1234", "$99.99", true, 12.34)]
    [TestCase(null, "$12.34", true, 12.34)]
    [TestCase(null, "12,34 EUR", false, 12.34)]
    [TestCase(null, "$1,234.56", false, 1234.56)]
    [TestCase(null, "Free To Play", false, 0.0)]
    public void ParseSteamPrice_ParsesSupportedFormats(
        string? cents,
        string text,
        bool preferCents,
        double expected)
    {
        var element = PriceElement(cents, text);

        var result = SteamService.ParseSteamPrice(element.Object, preferCents);

        Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    [Test]
    public void ParseSteamPrice_ThrowsForInvalidText()
    {
        var element = PriceElement(null, "price missing");

        Assert.That(
            () => SteamService.ParseSteamPrice(element.Object, preferCentsAttribute: false),
            Throws.Exception.With.Message.Contains("Could not parse price text"));
    }

    [Test]
    public void ScrapeFromPublicApi_RejectsPageOutsideAllowedRange()
    {
        var service = new SteamService(Mock.Of<ISteamRepository>(), Mock.Of<IHttpClientFactory>());

        Assert.That(
            async () => await service.ScrapeFromPublicApi(1, -1),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ScrapePage_RejectsMissingOrWrongScrapingMode()
    {
        var repository = new Mock<ISteamRepository>();
        repository
            .Setup(x => x.GetGameUrl(1))
            .ReturnsAsync(new GameUrl
            {
                Id = 1,
                ScrapingModeId = (long)ScrapingModeEnum.PublicApi,
                PartialUrl = "https://steam.example/{0}"
            });

        var service = new SteamService(repository.Object, Mock.Of<IHttpClientFactory>());

        Assert.That(
            async () => await service.ScrapePage(1, 1),
            Throws.Exception.With.Message.Contains("Batch Scraping"));
    }

    [Test]
    public void ScrapePage_RejectsPartialUrlWithoutPagePlaceholder()
    {
        var repository = new Mock<ISteamRepository>();
        repository
            .Setup(x => x.GetGameUrl(1))
            .ReturnsAsync(new GameUrl
            {
                Id = 1,
                ScrapingModeId = (long)ScrapingModeEnum.Batch,
                PartialUrl = "https://steam.example/no-page-placeholder"
            });

        var service = new SteamService(repository.Object, Mock.Of<IHttpClientFactory>());

        Assert.That(
            async () => await service.ScrapePage(1, 1),
            Throws.Exception.With.Message.Contains("placeholder"));
    }

    [Test]
    public void ScrapeFromPublicApi_RejectsWrongScrapingMode()
    {
        var repository = new Mock<ISteamRepository>();
        repository
            .Setup(x => x.GetGameUrl(1))
            .ReturnsAsync(new GameUrl
            {
                Id = 1,
                ScrapingModeId = (long)ScrapingModeEnum.Batch,
                PartialUrl = "https://steam.example/{0}"
            });

        var service = new SteamService(repository.Object, Mock.Of<IHttpClientFactory>());

        Assert.That(
            async () => await service.ScrapeFromPublicApi(1, 1),
            Throws.Exception.With.Message.Contains("public API"));
    }

    [TestCase("http://steamcommunity.com/market/search/render/?start={0}")]
    [TestCase("https://example.test/market/search/render/?start={0}")]
    [TestCase("https://steamcommunity.com:8443/market/search/render/?start={0}")]
    [TestCase("https://steamcommunity.com/profiles/123")]
    public async Task ScrapeFromPublicApi_RejectsDestinationsOutsideSteamMarket(string url)
    {
        var repository = new Mock<ISteamRepository>();
        repository.Setup(x => x.GetGameUrl(1)).ReturnsAsync(new GameUrl
        {
            ScrapingModeId = (long)ScrapingModeEnum.PublicApi,
            PartialUrl = url
        });
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var service = new SteamService(repository.Object, factory.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ScrapeFromPublicApi(1, 2));
        factory.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ScrapeFromPublicApi_MapsSteamResultsToSortedWatchItems()
    {
        const string json = """
            {
              "success": true,
              "results": [
                { "name": "Expensive", "sell_price": 250, "sell_listings": 3 },
                { "name": "Cheap", "sell_price": 100, "sell_listings": 2 }
              ]
            }
            """;

        using var client = new HttpClient(new HttpMessageHandlerStub((_, _) =>
            new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(json) }));
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(SteamService.HttpClientName)).Returns(client);
        var repository = new Mock<ISteamRepository>();
        repository
            .Setup(x => x.GetGameUrl(7))
            .ReturnsAsync(new GameUrl
            {
                Id = 7,
                ScrapingModeId = (long)ScrapingModeEnum.PublicApi,
                PartialUrl = "https://steamcommunity.com/market/search/render/?start={0}"
            });

        var service = new SteamService(repository.Object, factory.Object);

        var result = (await service.ScrapeFromPublicApi(7, 1)).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result.Select(x => x.Name), Is.EqualTo(new[] { "Cheap", "Expensive" }));
            Assert.That(result[0].Price, Is.EqualTo(1.00).Within(0.001));
            Assert.That(result[0].Quantity, Is.EqualTo(2));
            Assert.That(result[1].Price, Is.EqualTo(2.50).Within(0.001));
        });
    }

    [Test]
    public void ScrapeWithPixels_RejectsPageOutsideAllowedRange()
    {
        var service = new SteamService(Mock.Of<ISteamRepository>(), Mock.Of<IHttpClientFactory>());

        Assert.That(
            async () => await service.ScrapeWithPixels(1, -1),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ScrapeWithPixels_RejectsWrongScrapingMode()
    {
        var repository = new Mock<ISteamRepository>();
        repository
            .Setup(x => x.GetGameUrl(1))
            .ReturnsAsync(new GameUrl
            {
                Id = 1,
                ScrapingModeId = (long)ScrapingModeEnum.PublicApi,
                PartialUrl = "https://steam.example/{0}"
            });

        var service = new SteamService(repository.Object, Mock.Of<IHttpClientFactory>());

        Assert.That(
            async () => await service.ScrapeWithPixels(1, 1),
            Throws.Exception.With.Message.Contains("configured"));
    }

    [Test]
    [CancelAfter(10000)]
    public async Task ScrapeWithPixels_DefaultsMissingPixelCoordinatesBeforeDriverWork()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            Assert.Ignore("The production method explicitly supports pixel scraping only on Windows.");
        }

        var gameUrl = new GameUrl
        {
            Id = 1,
            ScrapingModeId = (long)ScrapingModeEnum.PixelBatch,
            PartialUrl = "not-a-url-{0}"
        };

        var repository = new Mock<ISteamRepository>();
        repository.Setup(x => x.GetGameUrl(1)).ReturnsAsync(gameUrl);
        var service = new SteamService(repository.Object, Mock.Of<IHttpClientFactory>());

        try
        {
            await service.ScrapeWithPixels(1, 1);
        }
        catch
        {
            // The test only needs to reach the code that applies defaults before Selenium takes over.
        }

        Assert.Multiple(() =>
        {
            Assert.That(gameUrl.PixelX, Is.EqualTo(450));
            Assert.That(gameUrl.PixelY, Is.EqualTo(50));
            Assert.That(gameUrl.PixelImageWidth, Is.EqualTo(62));
            Assert.That(gameUrl.PixelImageHeight, Is.EqualTo(62));
        });
    }

    private static Mock<IWebElement> PriceElement(string? cents, string text)
    {
        var element = new Mock<IWebElement>();
        element.Setup(x => x.GetAttribute("data-price-final")).Returns(cents);
        element.SetupGet(x => x.Text).Returns(text);
        return element;
    }
}

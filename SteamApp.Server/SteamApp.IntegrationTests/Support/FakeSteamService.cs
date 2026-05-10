using Newtonsoft.Json;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Interfaces.Services;

namespace SteamApp.IntegrationTests.Support;

public sealed class FakeSteamService : ISteamService
{
    public int ScrapePageCalls { get; private set; }
    public int ScrapeFromPublicApiCalls { get; private set; }
    public int ScrapeWithPixelsCalls { get; private set; }

    public IReadOnlyCollection<WatchItemDto> PageResults { get; set; } =
    [
        new WatchItemDto { Name = "Batch Item", Price = 2.50, Quantity = 3 }
    ];

    public IReadOnlyCollection<WatchItemDto> PublicApiResults { get; set; } =
    [
        new WatchItemDto { Name = "Public Item", Price = 1.25, Quantity = 2 }
    ];

    public IReadOnlyCollection<WatchItemDto> PixelResults { get; set; } =
    [
        new WatchItemDto { Name = "Pixel Item", Price = 3.75, Quantity = 1 }
    ];

    public bool ThrowInvalidPixelListing { get; set; }

    public Task<IEnumerable<WatchItemDto>> ScrapePage(long gamerUrlId, short page)
    {
        ScrapePageCalls++;
        return Task.FromResult(PageResults.AsEnumerable());
    }

    public Task<IEnumerable<WatchItemDto>> ScrapeFromPublicApi(long gameUrlId, short page)
    {
        ScrapeFromPublicApiCalls++;
        return Task.FromResult(PublicApiResults.AsEnumerable());
    }

    public Task<IEnumerable<WatchItemDto>> ScrapeWithPixels(long gameUrlId, short page)
    {
        ScrapeWithPixelsCalls++;

        if (ThrowInvalidPixelListing)
        {
            throw new JsonSerializationException("Invalid listing.");
        }

        return Task.FromResult(PixelResults.AsEnumerable());
    }

    public void Reset()
    {
        ScrapePageCalls = 0;
        ScrapeFromPublicApiCalls = 0;
        ScrapeWithPixelsCalls = 0;
        ThrowInvalidPixelListing = false;
    }
}

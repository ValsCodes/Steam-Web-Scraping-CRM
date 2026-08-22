using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamApp.Application.JsonObjects;

namespace SteamApp.WebAPI.ManualChecks;

public static class SteamMarketPageParser
{
    private const string RenderContextName = "window.SSR.renderContext";
    private const string JsonParseCall = "JSON.parse(";

    public static bool TryParseListing(string html, out Listing listing)
    {
        listing = new Listing();

        try
        {
            var contextIndex = html.IndexOf(RenderContextName, StringComparison.Ordinal);
            if (contextIndex < 0)
            {
                return false;
            }

            var parseIndex = html.IndexOf(JsonParseCall, contextIndex, StringComparison.Ordinal);
            if (parseIndex < 0)
            {
                return false;
            }

            using var textReader = new StringReader(html[(parseIndex + JsonParseCall.Length)..]);
            using var jsonReader = new JsonTextReader(textReader);
            if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.String || jsonReader.Value is not string contextJson)
            {
                return false;
            }

            var renderContext = JObject.Parse(contextJson);
            var queryDataJson = renderContext.Value<string>("queryData");
            if (string.IsNullOrWhiteSpace(queryDataJson))
            {
                return false;
            }

            var marketQuery = JObject.Parse(queryDataJson)["queries"]?
                .Children<JObject>()
                .FirstOrDefault(query =>
                    string.Equals(
                        query["queryKey"]?.First?.Value<string>(),
                        "market_item_search",
                        StringComparison.Ordinal));
            var pages = marketQuery?["state"]?["data"]?["pages"]?.Children<JObject>().ToList();
            if (pages is null)
            {
                return false;
            }

            listing = new Listing
            {
                Success = true,
                Start = pages.FirstOrDefault()?.Value<int?>("start") ?? 0,
                TotalCount = pages.FirstOrDefault()?.Value<int?>("total_count") ?? 0,
                ListingInfo = new Dictionary<string, ListingInfo>()
            };

            foreach (var page in pages)
            {
                foreach (var sellListing in page["listings"]?.Children<JObject>() ?? [])
                {
                    AddAsset(listing, sellListing);
                }
            }

            listing.PageSize = listing.ListingInfo?.Count ?? 0;
            return true;
        }
        catch (JsonException)
        {
            listing = new Listing();
            return false;
        }
    }

    private static void AddAsset(Listing listing, JObject sellListing)
    {
        var description = sellListing["description"] as JObject;
        var asset = sellListing["asset"] as JObject;
        if (description is null || asset is null)
        {
            return;
        }

        var appId = (asset.Value<int?>("appid") ?? description.Value<int?>("appid"))?.ToString();
        var contextId = asset.Value<string>("contextid");
        var listingId = sellListing.Value<string>("listingid");
        var assetId = asset.Value<string>("assetid")
            ?? asset.Value<string>("id")
            ?? listingId;
        if (string.IsNullOrWhiteSpace(appId) ||
            string.IsNullOrWhiteSpace(contextId) ||
            string.IsNullOrWhiteSpace(assetId) ||
            string.IsNullOrWhiteSpace(listingId))
        {
            return;
        }

        listing.ListingInfo![listingId] = new ListingInfo
        {
            ListingId = listingId,
            Price = sellListing.Value<int?>("unPrice") ?? 0,
            Fee = sellListing.Value<int?>("unFee") ?? 0,
            PublisherFeeApp = sellListing.Value<int?>("publisherFeeApp") ?? 0,
            CurrencyId = sellListing.Value<int?>("eCurrency") ?? 0,
            SteamFee = sellListing.Value<int?>("unSteamFee") ?? 0,
            PublisherFee = sellListing.Value<int?>("unPublisherFee") ?? 0,
            Asset = new Asset
            {
                AppId = int.TryParse(appId, out var assetAppId) ? assetAppId : 0,
                ContextId = contextId,
                Id = assetId,
                Amount = asset["amount"]?.ToString()
            }
        };

        if (!listing.Assets.TryGetValue(appId, out var contexts))
        {
            contexts = [];
            listing.Assets[appId] = contexts;
        }

        if (!contexts.TryGetValue(contextId, out var assets))
        {
            assets = [];
            contexts[contextId] = assets;
        }

        assets[assetId] = new AssetDetail
        {
            AppId = int.TryParse(appId, out var numericAppId) ? numericAppId : 0,
            ContextId = contextId,
            Id = assetId,
            ClassId = asset.Value<string>("classid") ?? description.Value<string>("classid") ?? string.Empty,
            InstanceId = asset.Value<string>("instanceid") ?? description.Value<string>("instanceid") ?? string.Empty,
            IconUrl = description.Value<string>("icon_url") ?? string.Empty,
            IconUrlLarge = description.Value<string>("icon_url_large") ?? string.Empty,
            Name = description.Value<string>("name") ?? string.Empty,
            Type = description.Value<string>("type") ?? string.Empty,
            MarketName = description.Value<string>("market_name") ?? string.Empty,
            MarketHashName = description.Value<string>("market_hash_name") ?? string.Empty,
            Descriptions = description["descriptions"]?.ToObject<List<Description>>() ?? []
        };
    }
}

using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.JsonObjects;
using SteamApp.Domain.Enums;

namespace SteamApp.WebAPI.Services;

public static class ManualCheckMatcher
{
    private const string ListingQuery = "?country=US&language=english&currency=3";

    public static bool TryBuildListingUri(string listingUrl, out Uri? listingUri)
    {
        listingUri = null;
        if (!Uri.TryCreate(listingUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !IsSteamCommunityHost(uri.Host))
        {
            return false;
        }

        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 4 ||
            !string.Equals(segments[0], "market", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(segments[1], "listings", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var listingPath = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        listingUri = new Uri(listingPath + ListingQuery, UriKind.Absolute);
        return true;
    }

    public static ManualCheckProductResultDto? MatchProduct(
        ManualCheckProductInputDto product,
        Listing listing,
        ManualCheckMatchModeEnum matchMode,
        IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        var matchedAssets = new List<ManualCheckAssetMatchDto>();

        foreach (var appEntry in listing.Assets ?? [])
        {
            foreach (var contextEntry in appEntry.Value)
            {
                foreach (var assetEntry in contextEntry.Value)
                {
                    var asset = assetEntry.Value;
                    var criterionMatches = new bool[criteria.Count];
                    var descriptions = new List<ManualCheckDescriptionMatchDto>();

                    foreach (var description in asset.Descriptions ?? [])
                    {
                        var matchedIndexes = new List<int>();
                        for (var index = 0; index < criteria.Count; index++)
                        {
                            if (!Matches(description, criteria[index]))
                            {
                                continue;
                            }

                            criterionMatches[index] = true;
                            matchedIndexes.Add(index);
                        }

                        if (matchedIndexes.Count > 0)
                        {
                            descriptions.Add(new ManualCheckDescriptionMatchDto
                            {
                                Name = description.Name ?? string.Empty,
                                Value = description.Value ?? string.Empty,
                                Color = description.Color ?? string.Empty,
                                MatchedCriterionIndexes = matchedIndexes
                            });
                        }
                    }

                    var qualifies = matchMode == ManualCheckMatchModeEnum.All
                        ? criterionMatches.All(x => x)
                        : criterionMatches.Any(x => x);

                    if (!qualifies)
                    {
                        continue;
                    }

                    matchedAssets.Add(new ManualCheckAssetMatchDto
                    {
                        AppId = appEntry.Key,
                        ContextId = contextEntry.Key,
                        AssetId = assetEntry.Key,
                        ClassId = asset.ClassId ?? string.Empty,
                        InstanceId = asset.InstanceId ?? string.Empty,
                        MarketName = asset.MarketName ?? string.Empty,
                        IconUrl = !string.IsNullOrWhiteSpace(asset.IconUrlLarge)
                            ? asset.IconUrlLarge
                            : asset.IconUrl ?? string.Empty,
                        Descriptions = descriptions
                    });
                }
            }
        }

        if (matchedAssets.Count == 0)
        {
            return null;
        }

        return new ManualCheckProductResultDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            GameUrlId = product.GameUrlId,
            GameUrlName = product.GameUrlName,
            FullUrl = product.FullUrl,
            Tags = product.Tags,
            Rating = product.Rating,
            MatchedAssets = matchedAssets
        };
    }

    private static bool Matches(Description description, ManualCheckCriterionDto criterion)
    {
        var name = criterion.NameContains;
        var value = criterion.ValueContains;

        return (string.IsNullOrEmpty(name) || (description.Name ?? string.Empty).Contains(name, StringComparison.OrdinalIgnoreCase)) &&
               (string.IsNullOrEmpty(value) || (description.Value ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSteamCommunityHost(string host)
    {
        return string.Equals(host, "steamcommunity.com", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(host, "www.steamcommunity.com", StringComparison.OrdinalIgnoreCase);
    }
}

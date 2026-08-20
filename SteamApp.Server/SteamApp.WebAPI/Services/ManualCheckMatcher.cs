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
        IReadOnlyList<ManualCheckCriterionDto> criteria,
        int listingLimit)
    {
        if (listingLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(listingLimit), "Listing limit must be positive.");
        }

        var matchedAssets = new List<ManualCheckAssetMatchDto>();
        var listingInfo = listing.ListingInfo ?? new Dictionary<string, ListingInfo>();
        var pricedListings = listingInfo
            .Select(entry => new
            {
                ListingId = string.IsNullOrWhiteSpace(entry.Value.ListingId)
                    ? entry.Key
                    : entry.Value.ListingId,
                Info = entry.Value,
                BuyerTotal = GetBuyerTotal(entry.Value)
            })
            .Where(entry =>
                entry.BuyerTotal > 0 &&
                entry.Info.Asset is { AppId: > 0 } asset &&
                !string.IsNullOrWhiteSpace(asset.ContextId) &&
                !string.IsNullOrWhiteSpace(asset.Id))
            .OrderBy(entry => entry.BuyerTotal)
            .ThenBy(entry => entry.ListingId, StringComparer.Ordinal)
            .Take(listingLimit)
            .ToList();

        if (pricedListings.Count == 0)
        {
            if (listing.TotalCount > 0 || listingInfo.Count > 0)
            {
                throw new InvalidOperationException(
                    "Steam returned listings, but none contained usable price and asset information.");
            }

            return null;
        }

        var checkedAssets = 0;
        foreach (var pricedListing in pricedListings)
        {
            var assetReference = pricedListing.Info.Asset!;
            var appId = assetReference.AppId.ToString();
            var contextId = assetReference.ContextId!;
            var assetId = assetReference.Id!;
            if (!TryGetAsset(listing, appId, contextId, assetId, out var asset))
            {
                continue;
            }

            checkedAssets++;

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

            var qualifies = EvaluateCriteria(criterionMatches, criteria);

            if (!qualifies)
            {
                continue;
            }

            matchedAssets.Add(new ManualCheckAssetMatchDto
            {
                AppId = appId,
                ContextId = contextId,
                AssetId = assetId,
                ClassId = asset.ClassId ?? string.Empty,
                InstanceId = asset.InstanceId ?? string.Empty,
                MarketName = asset.MarketName ?? string.Empty,
                IconUrl = !string.IsNullOrWhiteSpace(asset.IconUrlLarge)
                    ? asset.IconUrlLarge
                    : asset.IconUrl ?? string.Empty,
                Descriptions = descriptions
            });
        }

        if (checkedAssets == 0)
        {
            throw new InvalidOperationException(
                "Steam returned listings, but none contained usable price and asset information.");
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

    private static long GetBuyerTotal(ListingInfo listingInfo)
    {
        if (listingInfo.ConvertedPrice > 0)
        {
            return (long)listingInfo.ConvertedPrice + listingInfo.ConvertedFee;
        }

        return (long)listingInfo.Price + listingInfo.Fee;
    }

    private static bool TryGetAsset(
        Listing listing,
        string appId,
        string contextId,
        string assetId,
        out AssetDetail asset)
    {
        if (listing.Assets is not null &&
            listing.Assets.TryGetValue(appId, out var contexts) &&
            contexts.TryGetValue(contextId, out var assets) &&
            assets.TryGetValue(assetId, out var foundAsset) &&
            foundAsset is not null)
        {
            asset = foundAsset;
            return true;
        }

        asset = null!;
        return false;
    }

    private static bool Matches(Description description, ManualCheckCriterionDto criterion)
    {
        var name = criterion.NameContains;
        var value = criterion.ValueContains;

        return (string.IsNullOrEmpty(name) || (description.Name ?? string.Empty).Contains(name, StringComparison.OrdinalIgnoreCase)) &&
               (string.IsNullOrEmpty(value) || (description.Value ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static bool EvaluateCriteria(
        IReadOnlyList<bool> criterionMatches,
        IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        if (criterionMatches.Count == 0)
        {
            return false;
        }

        var result = criterionMatches[0];
        for (var index = 1; index < criterionMatches.Count; index++)
        {
            var conditionOperator = (ManualCheckConditionOperatorEnum?)criteria[index].ConditionOperatorId
                ?? throw new InvalidOperationException($"Criterion #{index + 1} has no condition operator.");
            var criterionMatch = criterionMatches[index];
            result = conditionOperator switch
            {
                ManualCheckConditionOperatorEnum.And => result && criterionMatch,
                ManualCheckConditionOperatorEnum.Or => result || criterionMatch,
                ManualCheckConditionOperatorEnum.AndNot => result && !criterionMatch,
                ManualCheckConditionOperatorEnum.OrNot => result || !criterionMatch,
                ManualCheckConditionOperatorEnum.Xor => result != criterionMatch,
                ManualCheckConditionOperatorEnum.Nand => !(result && criterionMatch),
                ManualCheckConditionOperatorEnum.Nor => !(result || criterionMatch),
                _ => throw new InvalidOperationException($"Criterion #{index + 1} has an unsupported condition operator.")
            };
        }

        return result;
    }

    private static bool IsSteamCommunityHost(string host)
    {
        return string.Equals(host, "steamcommunity.com", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(host, "www.steamcommunity.com", StringComparison.OrdinalIgnoreCase);
    }
}

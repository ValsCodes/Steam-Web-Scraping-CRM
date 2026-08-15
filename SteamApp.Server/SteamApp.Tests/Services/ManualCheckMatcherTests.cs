using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Application.JsonObjects;
using SteamApp.Domain.Enums;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class ManualCheckMatcherTests
{
    [Test]
    public void TryBuildListingUri_NormalizesTrailingSlashAndUsesPageQuery()
    {
        var result = ManualCheckMatcher.TryBuildListingUri(
            "https://steamcommunity.com/market/listings/440/Scattergun/?ignored=true",
            out var uri);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(uri?.AbsoluteUri, Is.EqualTo(
                "https://steamcommunity.com/market/listings/440/Scattergun?country=US&language=english&currency=3"));
        });
    }

    [TestCase("http://steamcommunity.com/market/listings/440/Item")]
    [TestCase("https://example.com/market/listings/440/Item")]
    [TestCase("https://steamcommunity.com/market/search/440/Item")]
    [TestCase("https://steamcommunity.com/market/listings/440")]
    public void TryBuildListingUri_RejectsUnsupportedUrls(string url)
    {
        Assert.That(ManualCheckMatcher.TryBuildListingUri(url, out _), Is.False);
    }

    [Test]
    public void MatchProduct_FlattensNestedAssetsAndIgnoresMissingDescriptions()
    {
        var listing = ListingWithAssets(
            ("440", "2", "a1", AssetWith(("attribute", "Sheen: Mean Green"))),
            ("440", "2", "a2", new AssetDetail { Descriptions = null! }),
            ("730", "5", "a3", AssetWith(("attribute", "Killstreaks Active"))));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            ManualCheckMatchModeEnum.Any,
            [new ManualCheckCriterionDto { ValueContains = "mean green" }]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.MatchedAssets, Has.Count.EqualTo(1));
            Assert.That(result.MatchedAssets[0].AppId, Is.EqualTo("440"));
            Assert.That(result.MatchedAssets[0].ContextId, Is.EqualTo("2"));
            Assert.That(result.MatchedAssets[0].AssetId, Is.EqualTo("a1"));
        });
    }

    [Test]
    public void MatchProduct_RequiresNameAndValueOnTheSameDescription()
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(
            ("attribute", "Other value"),
            ("other", "Sheen: Mean Green"))));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            ManualCheckMatchModeEnum.Any,
            [new ManualCheckCriterionDto
            {
                NameContains = "attribute",
                ValueContains = "mean green"
            }]);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MatchProduct_AllRequiresEveryCriterionInTheSameAsset()
    {
        var listing = ListingWithAssets(
            ("440", "2", "a1", AssetWith(("attribute", "Sheen: Mean Green"))),
            ("440", "2", "a2", AssetWith(("attribute", "Killstreaks Active"))));
        var criteria = new[]
        {
            new ManualCheckCriterionDto { ValueContains = "mean green" },
            new ManualCheckCriterionDto { ValueContains = "killstreaks" }
        };

        var any = ManualCheckMatcher.MatchProduct(Product(), listing, ManualCheckMatchModeEnum.Any, criteria);
        var all = ManualCheckMatcher.MatchProduct(Product(), listing, ManualCheckMatchModeEnum.All, criteria);

        Assert.Multiple(() =>
        {
            Assert.That(any?.MatchedAssets, Has.Count.EqualTo(2));
            Assert.That(all, Is.Null);
        });
    }

    [Test]
    public void MatchProduct_AllAllowsCriteriaAcrossDescriptionsWithinOneAsset()
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(
            ("attribute", "Sheen: Mean Green"),
            ("attribute", "Killstreaks Active"))));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            ManualCheckMatchModeEnum.All,
            [
                new ManualCheckCriterionDto { ValueContains = "MEAN GREEN" },
                new ManualCheckCriterionDto { NameContains = "ATTRIBUTE", ValueContains = "active" }
            ]);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.MatchedAssets, Has.Count.EqualTo(1));
            Assert.That(result.MatchedAssets[0].Descriptions, Has.Count.EqualTo(2));
        });
    }

    private static ManualCheckProductInputDto Product()
    {
        return new ManualCheckProductInputDto
        {
            ProductId = 10,
            ProductName = "Scattergun",
            GameUrlId = 5,
            GameUrlName = "Steam Market",
            FullUrl = "https://steamcommunity.com/market/listings/440/Scattergun"
        };
    }

    private static AssetDetail AssetWith(params (string Name, string Value)[] descriptions)
    {
        return new AssetDetail
        {
            ClassId = "class",
            InstanceId = "instance",
            MarketName = "Item",
            Descriptions = descriptions
                .Select(x => new Description { Name = x.Name, Value = x.Value })
                .ToList()
        };
    }

    private static Listing ListingWithAssets(
        params (string AppId, string ContextId, string AssetId, AssetDetail Asset)[] assets)
    {
        var listing = new Listing { Success = true };
        foreach (var item in assets)
        {
            if (!listing.Assets.TryGetValue(item.AppId, out var contexts))
            {
                contexts = [];
                listing.Assets[item.AppId] = contexts;
            }
            if (!contexts.TryGetValue(item.ContextId, out var contextAssets))
            {
                contextAssets = [];
                contexts[item.ContextId] = contextAssets;
            }
            contextAssets[item.AssetId] = item.Asset;
        }
        return listing;
    }
}

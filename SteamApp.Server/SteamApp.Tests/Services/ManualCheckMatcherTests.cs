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
            [new ManualCheckCriterionDto { ValueContains = "mean green" }],
            10);

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
            [new ManualCheckCriterionDto
            {
                NameContains = "attribute",
                ValueContains = "mean green"
            }],
            10);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MatchProduct_AllRequiresEveryCriterionInTheSameAsset()
    {
        var listing = ListingWithAssets(
            ("440", "2", "a1", AssetWith(("attribute", "Sheen: Mean Green"))),
            ("440", "2", "a2", AssetWith(("attribute", "Killstreaks Active"))));
        var orCriteria = new[]
        {
            new ManualCheckCriterionDto { ValueContains = "mean green" },
            new ManualCheckCriterionDto
            {
                ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.Or,
                ValueContains = "killstreaks"
            }
        };
        var andCriteria = orCriteria.Select(x => new ManualCheckCriterionDto
        {
            ConditionOperatorId = x.ConditionOperatorId,
            ValueContains = x.ValueContains
        }).ToArray();
        andCriteria[1].ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.And;

        var any = ManualCheckMatcher.MatchProduct(Product(), listing, orCriteria, 10);
        var all = ManualCheckMatcher.MatchProduct(Product(), listing, andCriteria, 10);

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
            [
                new ManualCheckCriterionDto { ValueContains = "MEAN GREEN" },
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.And,
                    NameContains = "ATTRIBUTE",
                    ValueContains = "active"
                }
            ],
            10);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.MatchedAssets, Has.Count.EqualTo(1));
            Assert.That(result.MatchedAssets[0].Descriptions, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void MatchProduct_OrdersByBuyerTotalBeforeApplyingListingLimit()
    {
        var listing = ListingWithPricedAssets(
            ("listing-expensive", "expensive-match", 200, 30, "Mean Green"),
            ("listing-cheapest", "cheapest-no-match", 80, 10, "Other"),
            ("listing-second", "second-match", 70, 30, "Mean Green"));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            [new ManualCheckCriterionDto { ValueContains = "Mean Green" }],
            1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void MatchProduct_UsesConvertedBuyerTotalAndListingIdTieBreak()
    {
        var listing = ListingWithPricedAssets(
            ("listing-b", "asset-b", 1, 0, "Mean Green"),
            ("listing-a", "asset-a", 40, 10, "Mean Green"),
            ("listing-c", "asset-c", 20, 5, "Mean Green"));
        listing.ListingInfo!["listing-b"].ConvertedPrice = 40;
        listing.ListingInfo["listing-b"].ConvertedFee = 10;
        listing.ListingInfo["listing-a"].ConvertedPrice = 45;
        listing.ListingInfo["listing-a"].ConvertedFee = 5;

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            [new ManualCheckCriterionDto { ValueContains = "Mean Green" }],
            3);

        Assert.That(
            result!.MatchedAssets.Select(x => x.AssetId),
            Is.EqualTo(new[] { "asset-c", "asset-a", "asset-b" }));
    }

    [Test]
    public void MatchProduct_LimitExceedsAvailableListings_ChecksEveryAvailableListing()
    {
        var listing = ListingWithPricedAssets(
            ("listing-2", "asset-2", 200, 20, "Mean Green"),
            ("listing-1", "asset-1", 100, 10, "Mean Green"));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            [new ManualCheckCriterionDto { ValueContains = "Mean Green" }],
            50);

        Assert.That(
            result!.MatchedAssets.Select(x => x.AssetId),
            Is.EqualTo(new[] { "asset-1", "asset-2" }));
    }

    [Test]
    public void MatchProduct_ListingsHaveNoUsablePrice_ThrowsActionableError()
    {
        var listing = ListingWithPricedAssets(
            ("listing-1", "asset-1", 0, 0, "Mean Green"));

        var exception = Assert.Throws<InvalidOperationException>(() => ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            [new ManualCheckCriterionDto { ValueContains = "Mean Green" }],
            10));

        Assert.That(exception!.Message, Does.Contain("usable price and asset information"));
    }

    [TestCase(ManualCheckConditionOperatorEnum.And, "Present", "Present", true)]
    [TestCase(ManualCheckConditionOperatorEnum.Or, "Missing", "Present", true)]
    [TestCase(ManualCheckConditionOperatorEnum.AndNot, "Present", "Missing", true)]
    [TestCase(ManualCheckConditionOperatorEnum.OrNot, "Missing", "Present", false)]
    [TestCase(ManualCheckConditionOperatorEnum.Xor, "Present", "Missing", true)]
    [TestCase(ManualCheckConditionOperatorEnum.Nand, "Present", "Present", false)]
    [TestCase(ManualCheckConditionOperatorEnum.Nor, "Missing", "Missing", true)]
    public void MatchProduct_ConditionOperator_EvaluatesLeftToRight(
        ManualCheckConditionOperatorEnum conditionOperator,
        string firstTerm,
        string secondTerm,
        bool expectedMatch)
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(("attribute", "Present"))));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            [
                new ManualCheckCriterionDto { ValueContains = firstTerm },
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = (long)conditionOperator,
                    ValueContains = secondTerm
                }
            ],
            10);

        Assert.That(result is not null, Is.EqualTo(expectedMatch));
    }

    [Test]
    public void MatchProduct_MixedOperators_UsesStrictDisplayedOrder()
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(("attribute", "Present"))));

        var result = ManualCheckMatcher.MatchProduct(
            Product(),
            listing,
            [
                new ManualCheckCriterionDto { ValueContains = "Present" },
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.Or,
                    ValueContains = "Missing"
                },
                new ManualCheckCriterionDto
                {
                    ConditionOperatorId = (long)ManualCheckConditionOperatorEnum.And,
                    ValueContains = "Missing"
                }
            ],
            10);

        Assert.That(result, Is.Null);
    }

    [TestCase("cond-a cond-c cond-e cond-y", true)]
    [TestCase("cond-a cond-c cond-e", false)]
    [TestCase("cond-a cond-b cond-d", true)]
    [TestCase("cond-b cond-d", false)]
    public void MatchProduct_NestedRequestedExpression_EvaluatesGroups(
        string descriptionValue,
        bool expectedMatch)
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(("attribute", descriptionValue))));
        var criteria = new[]
        {
            Criterion("cond-a"),
            Criterion("cond-b", ManualCheckConditionOperatorEnum.And, open: 1),
            Criterion("cond-c", ManualCheckConditionOperatorEnum.Or, close: 1),
            Criterion("cond-d", ManualCheckConditionOperatorEnum.And, open: 1),
            Criterion("cond-e", ManualCheckConditionOperatorEnum.Or, open: 1),
            Criterion("cond-y", ManualCheckConditionOperatorEnum.And, close: 2)
        };

        var result = ManualCheckMatcher.MatchProduct(Product(), listing, criteria, 10);

        Assert.That(result is not null, Is.EqualTo(expectedMatch));
    }

    [TestCase(ManualCheckConditionOperatorEnum.And, "Present", "Present", true)]
    [TestCase(ManualCheckConditionOperatorEnum.Or, "Missing", "Present", true)]
    [TestCase(ManualCheckConditionOperatorEnum.AndNot, "Present", "Missing", true)]
    [TestCase(ManualCheckConditionOperatorEnum.OrNot, "Missing", "Present", false)]
    [TestCase(ManualCheckConditionOperatorEnum.Xor, "Present", "Missing", true)]
    [TestCase(ManualCheckConditionOperatorEnum.Nand, "Present", "Present", false)]
    [TestCase(ManualCheckConditionOperatorEnum.Nor, "Missing", "Missing", true)]
    public void MatchProduct_ConditionOperator_CanCombineCompletedGroup(
        ManualCheckConditionOperatorEnum conditionOperator,
        string firstTerm,
        string groupedTerm,
        bool expectedMatch)
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(("attribute", "Present"))));
        var criteria = new[]
        {
            Criterion(firstTerm),
            Criterion(groupedTerm, conditionOperator, open: 1, close: 1)
        };

        var result = ManualCheckMatcher.MatchProduct(Product(), listing, criteria, 10);

        Assert.That(result is not null, Is.EqualTo(expectedMatch));
    }

    [Test]
    public void MatchProduct_NestedGroup_RemainsWithinOneAssetAndPreservesCriterionIndexes()
    {
        var splitAcrossAssets = ListingWithAssets(
            ("440", "2", "a1", AssetWith(("attribute", "cond-a"))),
            ("440", "2", "a2", AssetWith(("attribute", "cond-c"))));
        var sameAsset = ListingWithAssets(("440", "2", "a3", AssetWith(
            ("attribute", "cond-a"),
            ("attribute", "cond-c"))));
        var criteria = new[]
        {
            Criterion("cond-a"),
            Criterion("cond-b", ManualCheckConditionOperatorEnum.And, open: 1),
            Criterion("cond-c", ManualCheckConditionOperatorEnum.Or, close: 1)
        };

        var splitResult = ManualCheckMatcher.MatchProduct(Product(), splitAcrossAssets, criteria, 10);
        var sameAssetResult = ManualCheckMatcher.MatchProduct(Product(), sameAsset, criteria, 10);

        Assert.Multiple(() =>
        {
            Assert.That(splitResult, Is.Null);
            Assert.That(sameAssetResult, Is.Not.Null);
            Assert.That(
                sameAssetResult!.MatchedAssets[0].Descriptions.SelectMany(x => x.MatchedCriterionIndexes),
                Is.EqualTo(new[] { 0, 2 }));
        });
    }

    [Test]
    public void MatchProduct_NestedGroup_UsesStrictLeftFoldWithinGroup()
    {
        var listing = ListingWithAssets(("440", "2", "a1", AssetWith(("attribute", "cond-a"))));
        var criteria = new[]
        {
            Criterion("cond-a", open: 1),
            Criterion("cond-b", ManualCheckConditionOperatorEnum.Or),
            Criterion("cond-c", ManualCheckConditionOperatorEnum.And, close: 1)
        };

        var result = ManualCheckMatcher.MatchProduct(Product(), listing, criteria, 10);

        Assert.That(result, Is.Null);
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

    private static ManualCheckCriterionDto Criterion(
        string value,
        ManualCheckConditionOperatorEnum? conditionOperator = null,
        int open = 0,
        int close = 0)
    {
        return new ManualCheckCriterionDto
        {
            ConditionOperatorId = conditionOperator.HasValue ? (long)conditionOperator.Value : null,
            OpenGroupCount = open,
            CloseGroupCount = close,
            ValueContains = value
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
        var listing = new Listing
        {
            Success = true,
            TotalCount = assets.Length,
            ListingInfo = new Dictionary<string, ListingInfo>()
        };
        for (var index = 0; index < assets.Length; index++)
        {
            var item = assets[index];
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
            var listingId = $"listing-{index:D4}";
            listing.ListingInfo[listingId] = new ListingInfo
            {
                ListingId = listingId,
                Price = (index + 1) * 100,
                Fee = 10,
                Asset = new Asset
                {
                    AppId = int.Parse(item.AppId),
                    ContextId = item.ContextId,
                    Id = item.AssetId
                }
            };
        }
        return listing;
    }

    private static Listing ListingWithPricedAssets(
        params (string ListingId, string AssetId, int Price, int Fee, string Description)[] items)
    {
        var assets = items
            .Select(item => ("440", "2", item.AssetId, AssetWith(("attribute", item.Description))))
            .ToArray();
        var listing = ListingWithAssets(assets);
        listing.ListingInfo = items.ToDictionary(
            item => item.ListingId,
            item => new ListingInfo
            {
                ListingId = item.ListingId,
                Price = item.Price,
                Fee = item.Fee,
                Asset = new Asset
                {
                    AppId = 440,
                    ContextId = "2",
                    Id = item.AssetId
                }
            });
        return listing;
    }
}

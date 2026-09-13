using System.Net;
using SteamApp.Application.DTOs.Product;
using SteamApp.Domain.Entities;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Contracts.Pagination;

namespace SteamApp.Tests.MinimalApis;

[TestFixture]
public sealed class ProductEndpointsTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task GetProducts_LinkedTags_ReturnsOrderedDetailsAndOnlyOwnedProducts(bool paged)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(db =>
        {
            TestDb.SeedBaseline(db);
            db.Tags.Add(new Tag { Id = 3, GameId = 1, Name = "Archived", IsActive = false, UserId = TestDb.TestUserId });
            db.ProductTags.Add(new ProductTags { ProductId = 1, TagId = 3 });
            db.Products.Add(new Product { Id = 3, GameId = 1, Name = "Private product", UserId = "other-user" });
            db.SaveChanges();
        });

        var response = await app.Client.GetAsync(paged ? "/api/products/paged" : "/api/products/");
        var products = paged
            ? (await app.ReadJsonAsync<PagedResponse<ProductDto>>(response)).Items.ToArray()
            : await app.ReadJsonAsync<ProductDto[]>(response);
        var product = products.Single(x => x.Id == 1);
        var untagged = products.Single(x => x.Id == 2);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(products.Select(x => x.Id), Is.EquivalentTo(new long[] { 1, 2 }));
            Assert.That(product.Tags, Is.EqualTo(new[] { "Primary", "Archived" }));
            Assert.That(product.TagDetails.Select(x => x.Id), Is.EqualTo(new long[] { 1, 3 }));
            Assert.That(product.TagDetails.Select(x => x.Name), Is.EqualTo(product.Tags));
            Assert.That(product.TagDetails[0].IsActive, Is.True);
            Assert.That(product.TagDetails[0].ItemGroupId, Is.EqualTo(1));
            Assert.That(product.TagDetails[0].ItemGroupName, Is.EqualTo("Priority"));
            Assert.That(product.TagDetails[1].IsActive, Is.False);
            Assert.That(product.TagDetails[1].ItemGroupId, Is.Null);
            Assert.That(product.TagDetails[1].ItemGroupName, Is.Null);
            Assert.That(untagged.Tags, Is.Empty);
            Assert.That(untagged.TagDetails, Is.Empty);
        });
    }

    [TestCase("/api/products/")]
    [TestCase("/api/products/paged")]
    public async Task GetProducts_Unauthenticated_ReturnsUnauthorized(string path)
    {
        await using var app = await MinimalApiTestApp.CreateAsync(TestDb.SeedBaseline);
        using var client = app.CreateClientWithoutAuth();

        var response = await client.GetAsync(path);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}

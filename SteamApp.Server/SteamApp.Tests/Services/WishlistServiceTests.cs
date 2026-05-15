using AutoMapper;
using Moq;
using OpenQA.Selenium;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Domain.Entities;
using SteamApp.Interfaces.Repositories;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class WishlistServiceTests
{
    [Test]
    public void SelectFinalPrice_PrefersDiscountedPriceWhenPresent()
    {
        var basePrice = PriceElement("1000", "$10.00");
        var discountPrice = PriceElement("500", "$5.00");

        var result = WishlistService.SelectFinalPrice(basePrice.Object, discountPrice.Object);

        Assert.That(result, Is.EqualTo(5.00).Within(0.001));
    }

    [Test]
    public void SelectFinalPrice_UsesBasePriceWhenDiscountIsMissing()
    {
        var basePrice = PriceElement("1000", "$10.00");

        var result = WishlistService.SelectFinalPrice(basePrice.Object, null);

        Assert.That(result, Is.EqualTo(10.00).Within(0.001));
    }

    [Test]
    public void SelectFinalPrice_ThrowsWhenNoPriceElementExists()
    {
        Assert.That(
            () => WishlistService.SelectFinalPrice(null, null),
            Throws.Exception.With.Message.Contains("No Price element"));
    }

    [Test]
    public void CheckWishlistItem_ThrowsWhenWishlistIsMissing()
    {
        var repository = new Mock<IWishlistRepository>();
        repository
            .Setup(x => x.GetAsync(42, CancellationToken.None))
            .ReturnsAsync((WishList?)null);

        var service = new WishlistService(repository.Object, Mock.Of<IMapper>());

        Assert.That(
            async () => await service.CheckWishlistItem(42),
            Throws.Exception.With.Message.Contains("Wishlist not found"));
    }

    [Test]
    public void CheckWishlistItem_ThrowsWhenGameUrlIsMissing()
    {
        var repository = new Mock<IWishlistRepository>();
        repository
            .Setup(x => x.GetAsync(42, CancellationToken.None))
            .ReturnsAsync(new WishList
            {
                Id = 42,
                Game = new Game { Id = 1, PageUrl = "" },
                Price = 10
            });

        var service = new WishlistService(repository.Object, Mock.Of<IMapper>());

        Assert.That(
            async () => await service.CheckWishlistItem(42),
            Throws.Exception.With.Message.Contains("Game URL is null or empty"));
    }

    [Test]
    public async Task GetAllAsync_MapsRepositoryEntitiesToDtos()
    {
        var entities = new[]
        {
            new WishList { Id = 1, Name = "One" },
            new WishList { Id = 2, Name = "Two" }
        };
        var dtos = new[]
        {
            new WishListDto { Id = 1, Name = "One" },
            new WishListDto { Id = 2, Name = "Two" }
        };

        var repository = new Mock<IWishlistRepository>();
        repository.Setup(x => x.GetAllAsync(CancellationToken.None)).ReturnsAsync(entities);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<IEnumerable<WishListDto>>(entities)).Returns(dtos);

        var service = new WishlistService(repository.Object, mapper.Object);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.That(result, Is.SameAs(dtos));
    }

    [Test]
    public async Task GetAsync_MapsRepositoryEntityToDto()
    {
        var entity = new WishList { Id = 1, Name = "One" };
        var dto = new WishListDto { Id = 1, Name = "One" };

        var repository = new Mock<IWishlistRepository>();
        repository.Setup(x => x.GetAsync(1, CancellationToken.None)).ReturnsAsync(entity);

        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<WishListDto>(entity)).Returns(dto);

        var service = new WishlistService(repository.Object, mapper.Object);

        var result = await service.GetAsync(1, CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    private static Mock<IWebElement> PriceElement(string? cents, string text)
    {
        var element = new Mock<IWebElement>();
        element.Setup(x => x.GetAttribute("data-price-final")).Returns(cents);
        element.SetupGet(x => x.Text).Returns(text);
        return element;
    }
}

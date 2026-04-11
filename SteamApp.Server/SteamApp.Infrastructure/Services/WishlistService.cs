using AutoMapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.Repositories;
using SteamApp.Application.Services;
using SteamApp.Infrastructure.Services;

namespace SteamApp.WebAPI.Services;

public class WishlistService(IWishlistRepository repository, IMapper mapper) : IWishlistService
{
    public async Task<WhishListResponse?> CheckWishlistItem(long wishListId)
    {
        var wishList = await repository.GetAsync(wishListId, CancellationToken.None);
        if (wishList == null)
        {
            throw new Exception("Wishlist not found.");
        }

        var url = wishList.Game.PageUrl;
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("Game URL is null or empty.");
        }

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        //options.PlatformName = "Linux";
        options.AcceptInsecureCertificates = true;
        options.UnhandledPromptBehavior = UnhandledPromptBehavior.AcceptAndNotify;

        using IWebDriver driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl(url);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div[id^='appHubAppName']")));

        // Price elements are not guaranteed; use FindElements to avoid exceptions.
        IWebElement? gamePriceEl = driver.FindElements(By.CssSelector(".game_purchase_price.price")).FirstOrDefault();
        IWebElement? discountPriceEl = driver.FindElements(By.CssSelector(".discount_final_price")).FirstOrDefault();

        if (gamePriceEl == null && discountPriceEl == null)
        {
            throw new Exception("No Price element was found.");
        }

        // Prefer discounted price if present; otherwise base price.
        double finalPrice = SteamService.ParseSteamPrice(gamePriceEl!, preferCentsAttribute: true);

        return new WhishListResponse
        {
            IsPriceReached = finalPrice <= wishList.Price,
            CurrentPrice = finalPrice,
            GameName = wishList.Game.Name ?? "Missing Game Name",
        };
    }

    public async Task<IEnumerable<WishListDto>> GetAllAsync(CancellationToken ct)
    {
        var wishLists = await repository.GetAllAsync(ct);

        return mapper.Map<IEnumerable<WishListDto>>(wishLists);
    }

    public async Task<WishListDto> GetAsync(long id, CancellationToken ct)
    {
        var wishList = await repository.GetAsync(id, ct);

        return mapper.Map<WishListDto>(wishList);
    }
}

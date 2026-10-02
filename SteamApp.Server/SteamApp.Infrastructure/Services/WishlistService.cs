using AutoMapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Application.OperationResults;
using SteamApp.Interfaces.Repositories;
using SteamApp.Interfaces.Services;

namespace SteamApp.Infrastructure.Services;

public class WishlistService(IWishlistRepository repository, IMapper mapper) : IWishlistService
{
    public async Task<Result<WhishListResponse>> CheckWishlistItem(
        long wishListId,
        CancellationToken cancellationToken)
    {
        var wishList = await repository.GetAsync(wishListId, cancellationToken);
        if (wishList is null)
        {
            return Result<WhishListResponse>.Failure(new Error(
                "WishlistCheck.NotFound",
                "The price alert was not found.",
                ErrorType.NotFound));
        }

        var url = wishList.Game.PageUrl;
        if (string.IsNullOrWhiteSpace(url))
        {
            return Result<WhishListResponse>.Failure(new Error(
                "WishlistCheck.MissingGameUrl",
                "The selected game does not have a page URL.",
                ErrorType.Validation));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AcceptInsecureCertificates = true;
            options.UnhandledPromptBehavior = UnhandledPromptBehavior.AcceptAndNotify;

            using IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div[id^='appHubAppName']")));

            cancellationToken.ThrowIfCancellationRequested();

            IWebElement? gamePriceEl = driver
                .FindElements(By.CssSelector(".game_purchase_price.price"))
                .FirstOrDefault();
            IWebElement? discountPriceEl = driver
                .FindElements(By.CssSelector(".discount_final_price"))
                .FirstOrDefault();

            double finalPrice = SelectFinalPrice(gamePriceEl, discountPriceEl);

            return Result<WhishListResponse>.Success(new WhishListResponse
            {
                IsPriceReached = finalPrice <= wishList.Price,
                CurrentPrice = finalPrice,
                GameName = wishList.Game.Name ?? "Missing Game Name",
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (WebDriverException)
        {
            return UnavailableResult();
        }
        catch (FormatException)
        {
            return UnavailableResult();
        }
        catch (InvalidOperationException)
        {
            return UnavailableResult();
        }
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

    internal static double SelectFinalPrice(
        IWebElement? gamePriceEl,
        IWebElement? discountPriceEl)
    {
        if (gamePriceEl == null && discountPriceEl == null)
        {
            throw new InvalidOperationException("No price element was found.");
        }

        return SteamService.ParseSteamPrice(
            discountPriceEl ?? gamePriceEl!,
            preferCentsAttribute: true);
    }

    private static Result<WhishListResponse> UnavailableResult()
    {
        return Result<WhishListResponse>.Failure(new Error(
            "WishlistCheck.PriceUnavailable",
            "Steam did not provide a readable price for this game.",
            ErrorType.Unavailable));
    }
}

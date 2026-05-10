using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SteamApp.Application.DTOs.WatchItem;
using SteamApp.Application.JsonObjects;
using SteamApp.Domain.Common;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Utilities;
using SteamApp.Interfaces.Repositories;
using SteamApp.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;

namespace SteamApp.Infrastructure.Services;

public class SteamService(ISteamRepository steamRepository) : ISteamService
{
    public async Task<IEnumerable<WatchItemDto>> ScrapePage(long gamerUrlId, short page)
    {
        var gameUrl = await steamRepository.GetGameUrl(gamerUrlId);

        if (gameUrl == null || !IsBatchScrapingMode(gameUrl.ScrapingModeId))
        {
            throw new Exception("Game URL is missing or not configured for Batch Scraping.");
        }

        if (gameUrl.PartialUrl == null || !gameUrl.PartialUrl.Contains("{0}"))
        {
            throw new Exception("Game URL Partial URL is missing or does not contain a placeholder for page number.");
        }

        var url = string.Format(gameUrl.PartialUrl, page);

        var result = await ScrapePageFromUrl(url);

        return result.OrderBy(x => x.Price);
    }


    public async Task<IEnumerable<WatchItemDto>> ScrapeFromPublicApi(long gameUrlId, short page)
    {
        if (page < 0 || page > 500)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var gameUrl = await steamRepository.GetGameUrl(gameUrlId);

        if (gameUrl == null || !IsPublicApiScrapingMode(gameUrl.ScrapingModeId))
        {
            throw new Exception("Game URL is not configured for public API Scrape.");
        }

        //var url = $"{Constants.JSON_100_LISTINGS_URL_PART_1}{page}{Constants.JSON_100_LISTINGS_URL_PART_2}";

        var url = string.Format($"{gameUrl.PartialUrl}", page);
        var filteredResult = await GetResultsFromUrl(url);

        return [.. filteredResult.OrderBy(x => x.SellPrice).Select(x => new WatchItemDto { Name = x.Name, Price = x.SellPrice / 100, Quantity = x.SellListings })];
    }

    public async Task<IEnumerable<WatchItemDto>> ScrapeWithPixels(long gameUrlId, short page)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            throw new PlatformNotSupportedException("Pixel scraping uses System.Drawing and is only supported on Windows.");
        }

        if (page < 0 || page > 500)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var gameUrl = await steamRepository.GetGameUrl(gameUrlId);

        if (gameUrl == null || !IsPixelScrapingMode(gameUrl.ScrapingModeId))
        {
            throw new Exception("Game URL is not configured for public API Scrape.");
        }

        if (gameUrl.PixelImageWidth == null || gameUrl.PixelX == null)
        {
            gameUrl.PixelX = 450;
            gameUrl.PixelImageWidth = 62; // default values, can be updated later if needed
        }

        if (gameUrl.PixelImageHeight == null || gameUrl.PixelY == null)
        {
            gameUrl.PixelY = 50;
            gameUrl.PixelImageHeight = 62; // default values, can be updated later if needed
        }

        var url = string.Format($"{gameUrl.PartialUrl}", page);

        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        options.AcceptInsecureCertificates = true;
        options.UnhandledPromptBehavior = UnhandledPromptBehavior.AcceptAndNotify;


        var results = new List<WatchItemDto>();

        using var driver = new ChromeDriver(options);

        driver.Navigate().GoToUrl(url);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        wait.Until(ExpectedConditions.ElementExists(By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]")));

        ReadOnlyCollection<IWebElement> listingAnchors =
            driver.FindElements(By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]"));

        foreach (var anchor in listingAnchors)
        {
            var listing = ParseListingFromAnchor(anchor);
            listing.PageUrl = url;

            string srcImage = listing.ImageUrl!.Replace($"/{gameUrl.PixelImageWidth}fx{gameUrl.PixelImageHeight}f", string.Empty);

            using HttpClient client = new();
            byte[] bytes = await client.GetByteArrayAsync(srcImage);

            using var ms = new MemoryStream(bytes);
            using Bitmap bmp = new(ms);

            // TODO add pixel location
            //Color? pixelValue = bmp.GetPixel(gameUrl.PixelX., gameUrl.PixelY!.Value);
            Color pixelValue = bmp.GetPixel(gameUrl.PixelX!.Value, gameUrl.PixelY!.Value);

            var colorMatch = gameUrl.GameUrlsPixels.FirstOrDefault(x => x.Pixel.RedValue == pixelValue.R && 
                x.Pixel.BlueValue == pixelValue.B &&
                x.Pixel.GreenValue == pixelValue.G);

            if (colorMatch != null)
            {
                listing.IsPainted = true;
                listing.PixelName = colorMatch.Pixel.Name;
                listing.RedValue = colorMatch.Pixel.RedValue;
                listing.BlueValue = colorMatch.Pixel.BlueValue;
                listing.GreenValue = colorMatch.Pixel.GreenValue;
            }

            results.Add(listing);
        }

        return results;
    }

    #region Not Done
    //public async Task<WatchItemDto> ScrapeProductPixels(long gameId, string productName)
    //{
    //    productName = productName.Trim();
    //    if (string.IsNullOrEmpty(productName))
    //    {
    //        throw new ArgumentNullException("Product Name is null or empty.");
    //    }

    //    if (productName.Length > 200)
    //    {
    //        throw new ArgumentOutOfRangeException("Name is too long.");
    //    }

    //    var game = await steamRepository.GetGame(gameId);

    //    if (game == null)
    //    {
    //        throw new Exception("Game not found.");
    //    }

    //    var encodedProductName = UrlUtilities.UrlEncode(productName);

    //    //var url = $"{Constants.FIRST_PAGE_URL_PART_1}{encodedProductName}{Constants.FIRST_PAGE_URL_PART_2}";

    //    var url = string.Format(Constants.STEAM_GAME_LISTING_URL, game.InternalId, encodedProductName);

    //    var jsonResponse = await HttpUtilities.GetHttpResposeAsync(url);

    //    var jsonString = JsonUtilities.FormatJsonStringForDeserialization(jsonResponse);

    //    var result = JsonUtilities.DeserializeFormattedJsonString<Listing>(jsonString);

    //    var resultAssets = result?.Assets?.FirstOrDefault().Value?.FirstOrDefault().Value?.FirstOrDefault().Value; // assets/440/2/ first

    //    var descriptions = resultAssets!.Descriptions?.Where(x => x.Value != null);

    //    var listingResult = new WatchItemDto
    //    {
    //        Name = resultAssets.Name
    //    };

    //    if (resultAssets != null)
    //    {
    //        var pixel = descriptions!.FirstOrDefault(x => x.Value.StartsWith("Paint Color:"))?.Value;
    //        if (pixel != null)
    //        {
    //            listingResult.IsPainted = true;
    //            listingResult.PaintText = pixel.Replace("Paint Color:", string.Empty).Trim();
    //            listingResult.IsGoodPaint = game.Pixels.Any(x => string.Equals(x.Name, listingResult.PaintText, StringComparison.OrdinalIgnoreCase));
    //        }
    //    }

    //    return listingResult;
    //}

    // Get Pixel info from image source
    //public async Task<string> GetPixelInfoFromSource(long gamerUrlId, string srcUrl)
    //{
    //    var gameUrl = await steamRepository.GetGameUrl(gamerUrlId);

    //    if (gameUrl == null || !IsPixelScrapingMode(gameUrl.ScrapingModeId) || gameUrl.PixelX == null || gameUrl.PixelY == null)
    //    {
    //        throw new Exception("Game URL is not configured for pixel scrape or pixel coordinates are missing.");
    //    }

    //    if (gameUrl.PixelImageWidth == null || gameUrl.PixelImageHeight == null)
    //    {
    //        gameUrl.PixelImageWidth = 62; // default values, can be updated later if needed
    //        gameUrl.PixelImageHeight = 62; // default values, can be updated later if needed
    //    }

    //    string srcImage = srcUrl.Replace($"/{gameUrl.PixelImageWidth}fx{gameUrl.PixelImageHeight}f", string.Empty);

    //    var result = new StringBuilder();

    //    using (HttpClient client = new())
    //    {
    //        byte[] bytes = await client.GetByteArrayAsync(srcImage);

    //        using var ms = new MemoryStream(bytes);
    //        using Bitmap bmp = new(ms);


    //        Color c = bmp.GetPixel(gameUrl.PixelX.Value, gameUrl.PixelY.Value);


    //        result.AppendLine(c.Name);
    //        result.AppendLine($"R - {c.R}, G - {c.G}, B - {c.B}");
    //    }

    //    return result.ToString();
    //}
    #endregion

    #region Private Methods
    private static bool IsBatchScrapingMode(long? scrapingModeId)
    {
        return scrapingModeId == (long)ScrapingModeEnum.Batch ||
               scrapingModeId == (long)ScrapingModeEnum.PixelBatch;
    }

    private static bool IsPixelScrapingMode(long? scrapingModeId)
    {
        return scrapingModeId == (long)ScrapingModeEnum.PixelBatch;
    }

    private static bool IsPublicApiScrapingMode(long? scrapingModeId)
    {
        return scrapingModeId == (long)ScrapingModeEnum.PublicApi;
    }

    public static double ParseSteamPrice(IWebElement el, bool preferCentsAttribute)
    {
        if (preferCentsAttribute)
        {
            string? centsRaw = el.GetAttribute("data-price-final");
            if (!string.IsNullOrWhiteSpace(centsRaw) &&
                long.TryParse(centsRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long cents))
            {
                return cents / 100.0;
            }
        }

        string text = (el.Text ?? string.Empty).Trim();

        if (text.Contains("Free To Play", StringComparison.OrdinalIgnoreCase))
        {
            return 0.0;
        }

        // Keep only digits and decimal separators
        string cleaned = new([.. text.Where(c => char.IsDigit(c) || c == ',' || c == '.')]);

        // Normalize to invariant decimal format
        if (cleaned.Contains(',') && !cleaned.Contains('.'))
        {
            cleaned = cleaned.Replace(',', '.');
        }
        else
        {
            cleaned = cleaned.Replace(",", "");
        }

        if (double.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        throw new Exception($"Could not parse price text: '{text}'");
    }

    private static async Task<IEnumerable<WatchItemDto>> ScrapePageFromUrl(string url)
    {
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        options.AcceptInsecureCertificates = true;
        options.UnhandledPromptBehavior = UnhandledPromptBehavior.AcceptAndNotify;

        options.AddArgument("--disable-gpu");
        options.PageLoadStrategy = PageLoadStrategy.Eager;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(30));

        return await Task.Factory.StartNew(() =>
        {
            linkedCts.Token.ThrowIfCancellationRequested();

            using var driver = new ChromeDriver(options);
            try
            {
                driver.Navigate().GoToUrl(url);
                var resultBy = By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]");

                var wait = new DefaultWait<IWebDriver>(driver)
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    PollingInterval = TimeSpan.FromMilliseconds(200)
                };
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

                try
                {
                    wait.Until(drv => linkedCts.Token.IsCancellationRequested || drv.FindElements(resultBy).Count != 0);
                    linkedCts.Token.ThrowIfCancellationRequested();
                }
                catch (WebDriverTimeoutException)
                {
                    linkedCts.Cancel();
                    linkedCts.Token.ThrowIfCancellationRequested();
                }

                var anchors = driver.FindElements(resultBy);
                var results = new List<WatchItemDto>(anchors.Count);
                foreach (var anchor in anchors)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
                    results.Add(ParseListingFromAnchor(anchor));
                }
                return results.ToArray();
            }
            finally
            {
                driver.Quit();
            }
        }, linkedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private static WatchItemDto ParseListingFromAnchor(IWebElement anchor)
    {
        var listing = new WatchItemDto
        {
            ListingUrl = anchor.GetAttribute(Constants.HREF) ?? string.Empty
        };

        // Find the main container div for the listing
        var listingDiv = anchor.FindElement(
            By.XPath($".//div[contains(@class,'{Constants.MARKET_LISTING_SEARCHRESULT}')]")
        );
        listing.Name = listingDiv.GetAttribute(Constants.DATA_HASH_NAME);

        // Extract the image URL
        var imageElem = listingDiv.FindElement(
            By.XPath($".//img[contains(@id, '{Constants.RESULT}') and contains(@id, '{Constants._IMAGE}')]")
        );
        listing.ImageUrl = imageElem.GetAttribute(Constants.SRC);

        // Extract the quantity and parse it
        var quantitySpan = listingDiv.FindElement(
            By.XPath($".//span[contains(@class,'{Constants.MARKET_LISTING_NUM_LISTING_QTY}') and @{Constants.DATA_QTY}]")
        );
        var quantityRaw = quantitySpan.GetAttribute(Constants.DATA_QTY);
        if (!short.TryParse(quantityRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity))
        {
            throw new FormatException($"Could not parse listing quantity: '{quantityRaw}'");
        }

        listing.Quantity = quantity;

        // Extract the price, convert it, and assign it
        var priceSpan = listingDiv.FindElement(
            By.XPath($".//span[contains(@class,'{Constants.NORMAL_PRICE}') and @{Constants.DATA_PRICE}]")
        );
        var nonConvertedPrice = priceSpan.GetAttribute(Constants.DATA_PRICE);
        if (!double.TryParse(nonConvertedPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var priceInCents))
        {
            throw new FormatException($"Could not parse listing price: '{nonConvertedPrice}'");
        }

        listing.Price = priceInCents / 100;

        return listing;
    }

    private static async Task<IEnumerable<Result>> GetResultsFromUrl(string url)
    {
        var jsonResponse = await HttpUtilities.GetHttpResposeAsync(url);
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new Exception("No JSON received or request failed.");
        }

        var jsonString = JsonUtilities.FormatJsonStringForDeserialization(jsonResponse);
        var result = JsonUtilities.DeserializeFormattedJsonString<ListingDetails>(jsonString);

        return result == null ? throw new Exception("Failed to deserialize JSON.") : (IEnumerable<Result>)result.Results!;
    }

    #endregion
}

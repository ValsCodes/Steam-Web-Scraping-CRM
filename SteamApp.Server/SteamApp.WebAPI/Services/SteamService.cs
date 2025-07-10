using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.DTOs;
using SteamApp.Models.JsonObjects;
using SteamApp.WebAPI.Common;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace SteamApp.WebAPI.Services;

public class SteamService : ISteamService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromPage"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IEnumerable<string> GetHatBatchUrls(short fromPage, short batchSize)
    {
        if (fromPage <= 0 || fromPage > 500)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var url = Constants.MANUAL_HATS_URL;
        var result = new List<string>();
        var currentPage = fromPage;

        for (; currentPage < fromPage + batchSize; currentPage++)
        {
            result.Add($"{url}p{currentPage}_price_asc`");
        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromIndex"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IEnumerable<string> GetWeaponBatchUrls(short fromIndex, short batchSize)
    {
        if (fromIndex < 0 || fromIndex > 500)
        {
            throw new ArgumentOutOfRangeException("Index is either negative or greater than 500.");
        }

        var url = Constants.MANUAL_WEAPONS_URL;
        var result = new List<string>();
        var currentIndex = fromIndex;

        for (; currentIndex < fromIndex + batchSize; currentIndex++)
        {
            result.Add($"{url}{StaticCollections.WEAPON_NAMES[currentIndex - 1]}`");
        }

        return result;
    }

    /// <summary>
    /// Web Scrapes the market by loading the DOM
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ListingDto>> ScrapePage(short page)
    {
        if (page < 0 || page > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var url = Constants.MANUAL_HATS_URL + $"p{page}_price_asc";

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");

        return await Task.Run(() =>
        {
            var results = new List<ListingDto>();

            using (IWebDriver driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl(url);

                // Wait until at least one listing anchor is present.
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(ExpectedConditions.ElementExists(By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]")));

                // Get all listing anchor elements.
                ReadOnlyCollection<IWebElement> listingAnchors =
                    driver.FindElements(By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]"));

                foreach (var anchor in listingAnchors)
                {
                    var listing = ParseListingFromAnchor(anchor);
                    results.Add(listing);
                }
            }

            return results;
        });
    }

    public async Task<string> GetPaintInfoFromSource(string src)
    {
        //no color
        //string srcImage = "https://community.fastly.steamstatic.com/economy/image/fWFc82js0fmoRAP-qOIPu5THSWqfSmTELLqcUywGkijVjZULUrsm1j-9xgEDaQYVVQ7srD1Rm_fxH_OcN-wBid0wq8AE2mc7xlV4MOazZjIyJgLHWfRcBPBsoli-CnZjvZEyAtPv8ewFfl3x9NyREldD0mM";

        string srcImage = src.Replace("/62fx62f", string.Empty);

        var result = new StringBuilder();

        using (HttpClient client = new())
        {
            byte[] bytes = await client.GetByteArrayAsync(srcImage);

            using var ms = new MemoryStream(bytes);
            using Bitmap bmp = new(ms);

            Color c = bmp.GetPixel(450, 50);

            result.AppendLine(c.Name);
            result.AppendLine($"R - {c.R}, G - {c.G}, B - {c.B}");
        }

        return result.ToString();
    }

    public async Task<IEnumerable<ListingDto>> ScrapePageByPixel(short page, bool isGoodColorOnly = true)
    {
        if (page < 0 || page > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var url = Constants.MANUAL_HATS_URL + $"p{page}_price_asc";

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");


        var results = new List<ListingDto>();

        using (IWebDriver driver = new ChromeDriver(options))
        {
            driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementExists(By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]")));

            ReadOnlyCollection<IWebElement> listingAnchors =
                driver.FindElements(By.XPath($"//a[starts-with(@id, '{Constants.RESULTLINK}')]"));

            foreach (var anchor in listingAnchors)
            {
                var listing = ParseListingFromAnchor(anchor);

                string srcImage = listing.ImageUrl!.Replace("/62fx62f", string.Empty);
                using HttpClient client = new HttpClient();
                byte[] bytes = await client.GetByteArrayAsync(srcImage);

                using var ms = new MemoryStream(bytes);
                using Bitmap bmp = new Bitmap(ms);
                Color c = bmp.GetPixel(450, 50);

                if (isGoodColorOnly)
                {
                    var colorMatch = StaticCollections.GoodPaintsColorCollection.FirstOrDefault(x => x.Color.Name.Equals(c.Name) && x.IsGoodPaint);
                    if (colorMatch != null)
                    {
                        listing.Color = colorMatch.Name;
                        results.Add(listing);
                    }
                }
                else
                {
                    if (!c.Name.Equals("0"))
                    {
                        var colorMatch = StaticCollections.GoodPaintsColorCollection.FirstOrDefault(x => x.Color.Name.Equals(c.Name));
                        listing.Color = colorMatch!.Name;
                        results.Add(listing);
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Accepts a page value from 0 to 99 and returns only listings in a good price range
    /// </summary>
    /// <param name="page"></param>
    /// <returns> A Collection of ListingDTOs</returns>
    public async Task<IEnumerable<ListingDto>> GetFilteredBulkListings(short page)
    {
        if (page < 0 || page > 500)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var url = $"{Constants.JSON_100_LISTINGS_URL_PART_1}{page}{Constants.JSON_100_LISTINGS_URL_PART_2}";
        var filteredResult = await GetResultsFromUrl(url);

        return filteredResult.Where(x => x.SellPrice >= 150 && x.SellPrice <= 450)
            .OrderBy(x => x.SellPrice)
            .Select(x => new ListingDto { Name = x.Name, Price = x.SellPrice / 100, Quantity = x.SellListings });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<PaintedListingDto> CheckIsListingPainted(string name)
    {
        // TODO check if name is in DB of item names
        name = name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("Name is null or empty.");
        }

        if (name.Length > 200)
        {
            throw new ArgumentNullException("Name is too long.");
        }

        var formattedName = EncodeStringForUrl(name);
        var url = $"{Constants.FIRST_PAGE_URL_PART_1}{formattedName}{Constants.FIRST_PAGE_URL_PART_2}";

        var jsonResponse = await GetHttpResposeAsync(url);

        var jsonString = FormatJsonStringForDeserialization(jsonResponse);

        var result = DeserializeFormattedJsonString<Listing>(jsonString);

        var resultAssets = result?.Assets?.FirstOrDefault().Value?.FirstOrDefault().Value?.FirstOrDefault().Value; // assets/440/2/ first

        var descriptions = resultAssets!.Descriptions?.Where(x => x.Value != null);

        var lisitngResult = new PaintedListingDto();
        lisitngResult.Name = resultAssets.Name;

        if (resultAssets != null)
        {
            var paint = descriptions!.FirstOrDefault(x => x.Value.StartsWith("Paint Color:"))?.Value;
            if (paint != null)
            {
                lisitngResult.IsPainted = true;
                lisitngResult.PaintText = paint.Replace("Paint Color:", string.Empty).Trim();
                lisitngResult.IsGoodPaint = StaticCollections.GoodPaints.Contains(lisitngResult.PaintText);
            }
        }

        return lisitngResult;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IEnumerable<PaintedListingsDto>> ScrapePageForPaintedListingsOnly(short page)
    {
        if (page < 0 || page > 500)
        {
            throw new ArgumentOutOfRangeException("Page is either negative or greater than 500.");
        }

        var results = new List<PaintedListingsDto>();
        var listings = await ScrapePage(page);

        if (listings.Any())
        {
            foreach (var listing in listings)
            {
                var isListingPaintedResult = await CheckIsListingPainted(listing.Name!);
                if (isListingPaintedResult.IsPainted)
                {
                    var paintedListing = new PaintedListingsDto
                    {
                        Name = listing.Name,
                        Quantity = listing.Quantity,
                        Price = listing.Price,
                        ImageUrl = listing.ImageUrl,
                        ListingUrl = listing.ListingUrl,
                        IsPainted = isListingPaintedResult.IsPainted,
                        PaintText = isListingPaintedResult.PaintText,
                        IsGoodPaint = isListingPaintedResult.IsGoodPaint
                    };

                    results.Add(paintedListing);
                }
            }
        }

        return results.OrderBy(x => x.Name);
    }


    #region Private Methods
    private ListingDto ParseListingFromAnchor(IWebElement anchor)
    {
        var listing = new ListingDto
        {
            ListingUrl = anchor.GetAttribute(Constants.HREF)
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
        listing.Quantity = short.Parse(quantitySpan.GetAttribute(Constants.DATA_QTY));

        // Extract the price, convert it, and assign it
        var priceSpan = listingDiv.FindElement(
            By.XPath($".//span[contains(@class,'{Constants.NORMAL_PRICE}') and @{Constants.DATA_PRICE}]")
        );
        var nonConvertedPrice = priceSpan.GetAttribute(Constants.DATA_PRICE);
        listing.Price = double.Parse(nonConvertedPrice) / 100;

        return listing;
    }

    private async Task<IEnumerable<Result>> GetResultsFromUrl(string url)
    {
        var jsonResponse = await GetHttpResposeAsync(url);
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new Exception("No JSON received or request failed.");
        }

        var jsonString = FormatJsonStringForDeserialization(jsonResponse);
        var result = DeserializeFormattedJsonString<ListingDetails>(jsonString);
        if (result == null)
        {
            throw new Exception("Failed to deserialize JSON.");
        }

        return result.Results!;
    }


    private async Task<string> GetHttpResposeAsync(string url)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent == null)
            {
                return null!;
            }

            return responseContent;
        }
    }

    private T? DeserializeFormattedJsonString<T>(string formattedJsonObject) where T : class
    {
        if (string.IsNullOrEmpty(formattedJsonObject))
        {
            throw new JsonSerializationException("Object for Deserialization was null.");
        }

        return JsonConvert.DeserializeObject<T>(formattedJsonObject);
    }

    private string FormatJsonStringForDeserialization(string jsonString)
    {
        string formattedJson = jsonString.Replace("\\r\\n", "\r\n");
        return formattedJson;
    }

    private string EncodeStringForUrl(string name)
    {
        return Uri.EscapeDataString(name);
    }
    #endregion
}

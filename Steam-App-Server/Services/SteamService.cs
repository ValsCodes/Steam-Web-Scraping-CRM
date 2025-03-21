using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SteamApp.Infrastructure.Services;
using SteamApp.Models;
using SteamApp.Models.Dto;
using SteamApp.Models.Models.Json;
using System.Collections.ObjectModel;
using System.Web;

namespace SteamApp.Services
{
    public class SteamService : ISteamService
    {
        private readonly HttpClient _httpClient;

        public SteamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IEnumerable<string> GetHatListingsUrls(short fromPage, short batchSize)
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

        public IEnumerable<string> GetWeaponListingsUrls(short fromIndex, short batchSize)
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
        public async Task<IEnumerable<ListingDto>> ScrapePageAsync(short page)
        {
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
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.ElementExists(By.XPath("//a[starts-with(@id, 'resultlink_')]")));

                    // Get all listing anchor elements.
                    ReadOnlyCollection<IWebElement> listingAnchors =
                        driver.FindElements(By.XPath("//a[starts-with(@id, 'resultlink_')]"));

                    foreach (var anchor in listingAnchors)
                    {
                        var listing = new ListingDto();

                        listing.ListingUrl = anchor.GetAttribute("href");

                        IWebElement listingDiv = anchor.FindElement(
                            By.XPath(".//div[contains(@class,'market_listing_searchresult')]")
                        );
                        listing.Name = listingDiv.GetAttribute("data-hash-name");

                        IWebElement imageElem = listingDiv.FindElement(
                            By.XPath(".//img[contains(@id, 'result_') and contains(@id, '_image')]")
                        );
                        listing.ImageUrl = imageElem.GetAttribute("src");

                        IWebElement quantitySpan = listingDiv.FindElement(
                            By.XPath(".//span[contains(@class,'market_listing_num_listings_qty') and @data-qty]")
                        );
                        listing.Quantity = short.Parse(quantitySpan.GetAttribute("data-qty"));

                        IWebElement priceSpan = listingDiv.FindElement(
                            By.XPath(".//span[contains(@class,'normal_price') and @data-price]")
                        );
                        var nonConvertedPrice = priceSpan.GetAttribute("data-price");
                        listing.Price = double.Parse(nonConvertedPrice) / 100;

                        results.Add(listing);
                    }
                }

                return results;
            });
        }

        /// <summary>
        /// Accepts a page value from 0 to 99 and returns only listings in a good price range
        /// </summary>
        /// <param name="page"></param>
        /// <returns> A Collection of ListingDTOs</returns>
        public async Task<IEnumerable<ListingDto>> GetFilterredListingsAsync(short page)
        {
            var url = $"{Constants.JSON_100_LISTINGS_URL_PART_1}{page}{Constants.JSON_100_LISTINGS_URL_PART_2}";
            var filteredResult = await GetFilterredResults(url);

            return filteredResult.Select(x => new ListingDto { Name = x.Name, Price = x.SellPrice / 100, Quantity = x.SellListings });
        }

        public async Task<(bool, string)> IsListingPaintedAsync(string name)
        {
            var formattedName = EncodeStringForUrl(name);
            var url = $"{Constants.FIRST_PAGE_URL_PART_1}{formattedName}{Constants.FIRST_PAGE_URL_PART_2}";
            var jsonResponse = await GetHttpResposeAsync(url);
            var jsonString = FormatJsonStringForDeserialization(jsonResponse);
            var result = DeserializeFormattedJsonString<Listing>(jsonString);

            //When Deserializing assets:440:2 doesn't deserialize the data correctly and leaves everything as null
            //Model binding problem probably

            var resultAssets = result?.Assets?.FirstOrDefault().Value?.FirstOrDefault().Value?.FirstOrDefault().Value?.Descriptions?.Where(x => x.Value != null);

            if (resultAssets != null && resultAssets.Any(x => StaticCollections.GoodPaintsStringValues.Contains(x.Value)))
            {
                var paint = resultAssets.FirstOrDefault(x => x.Value.StartsWith("Paint Color:"))?.Value;
                if (paint != null)
                {
                    return (true, paint);
                }
            }

            return (false, string.Empty);
        }

        public async Task<IEnumerable<ListingDto>> GetPaintedListingsOnlyAsync(short page)
        {
            var url = $"{Constants.JSON_100_LISTINGS_URL_PART_1}{page}{Constants.JSON_100_LISTINGS_URL_PART_2}";
            var results = await GetFilterredResults(url);

            if (!results.Any())
            {
                throw new Exception("No results found.");
            }

            var onlyPaintedResutls = await GetOnlyPaintedResults(results);

            return onlyPaintedResutls;
        }


        #region Private Methods
        private async Task<Result[]> GetFilterredResults(string url)
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

            return result.Results!.Where(x => x.SellPrice >= 150 && x.SellPrice <= 450).OrderBy(x => x.SellPrice).ToArray();
        }

        private async Task<IEnumerable<ListingDto>> GetOnlyPaintedResults(Result[] results)
        {
            var itemCollection = new List<ListingDto>();
            foreach (var item in results)
            {
                var formattedName = EncodeStringForUrl(item.Name!);
                var url = $"{Constants.FIRST_PAGE_URL_PART_1}{formattedName}{Constants.FIRST_PAGE_URL_PART_2}";
                var jsonResponse = await GetHttpResposeAsync(url);
                var jsonString = FormatJsonStringForDeserialization(jsonResponse);
                var deserializedResult = DeserializeFormattedJsonString<Listing>(jsonString);

                //When Deserializing assets:440:2 doesn't deserialize the data correctly and leaves everything as null
                //Model binding problem *probably*
                if (deserializedResult == null)
                {
                    continue;
                }

                var firstAppAssets = deserializedResult.Assets?.FirstOrDefault().Value;
                var firstAssetDetail = firstAppAssets?.FirstOrDefault().Value?.FirstOrDefault().Value;
                var resultAssets = firstAssetDetail?.Descriptions?.Where(x => x.Value != null);
                var isContained = resultAssets?.Any(x => StaticCollections.GoodPaintsStringValues.Contains(x.Value)) ?? false;

                if (isContained)
                {
                    var color = resultAssets?.FirstOrDefault(x => x.Value!.StartsWith("Paint Color"))?.Value;

                    if (color != null)
                    {
                        var listing = new ListingDto
                        {
                            Name = item.Name,
                            Price = item.SellPrice,
                            ImageUrl = item.AssetDescription?.IconUrl,  // Use null-conditional here as well
                            Quantity = item.SellListings,
                            Color = color
                        };

                        itemCollection.Add(listing);
                    }
                }
            }

            return itemCollection;
        }

        private async Task<string> GetHttpResposeAsync(string url)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent == null)
            {
                return null!;
            }

            return responseContent;
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
            return HttpUtility.UrlEncode(name);
        }
        #endregion
    }
}

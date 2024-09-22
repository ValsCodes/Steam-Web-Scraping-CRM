using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SteamAppServer.Common;
using SteamAppServer.Models;
using SteamAppServer.Models.Proxies;
using SteamAppServer.Repositories.Interfaces;
using SteamAppServer.Services.Interfaces;
using System.Web;

namespace SteamAppServer.Services
{
    public class SteamService : ISteamService
    {
        private HttpClient _httpClient;
        private ISalesRepository _steamRepository;

        public SteamService(ISalesRepository steamRepository, HttpClient httpClient)
        {
            _steamRepository = steamRepository;
            _httpClient = httpClient;
        }

        #region Repository Requests

        public async Task<IEnumerable<SellListing>> GetListingsAsync()
        {
            var result = await _steamRepository.GetListingsAsync();
            return result;
        }

        public async Task<SellListing?> CreateListingAsync(SellListing sellListing)
        {
            var result = await _steamRepository.CreateListingAsync(sellListing);
            return result;
        }

        public async Task<SellListing?> UpdateListingAsync(long id, SellListing sellListing)
        {
            var result = await _steamRepository.UpdateListingAsync(id, sellListing);
            return result;
        }

        public async Task<SellListing?> DeleteListingAsync(long id)
        {
            var result = await _steamRepository.DeleteListingAsync(id);
            return result;
        }

        #endregion

        public async Task<IEnumerable<ListingProxy>> GetFilterredListingsAsync(short page)
        {
            var url = $"{Constants.JSON_100_LISTINGS_URL}{page}";
            var filteredResult = await GetFilterredResults(url);
            if (!filteredResult.Any())
            {
                return Enumerable.Empty<ListingProxy>();
            }

            return filteredResult.Select(x => new ListingProxy { Name = x.Name, Price = x.SellPrice, Quantity = x.SellListings });
        }

        public async Task<(bool, string)> IsListingPaintedAsync(string name)
        {
            var formattedName = FormatListingNameForUrl(name);
            var url = $"{Constants.FIRST_PAGE_URL_PART_1}{formattedName}{Constants.FIRST_PAGE_URL_PART_2}";
            var jsonResponse = await GetHttpResposeAsync(url);
            var jsonString = FormatJsonStringForDeserialization(jsonResponse);
            var result = DeserializeFormattedJsonString<Listing_Json>(jsonString);

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
        public Task<IEnumerable<ListingProxy>> GetPaintedListingsOnlyAsync(short page)
        {
            throw new NotImplementedException();
        }

        #region Private Methods
        private async Task<IEnumerable<Result>> GetFilterredResults(string url)
        {
            var jsonResponse = await GetHttpResposeAsync(url);
            if (string.IsNullOrEmpty(jsonResponse))
            {
                Console.WriteLine("No JSON received or request failed.");
                return null!;
            }

            var jsonString = FormatJsonStringForDeserialization(jsonResponse);
            var result = DeserializeFormattedJsonString<ListingDetails_Json>(jsonString);
            if (result == null)
            {
                Console.WriteLine("Failed to deserialize JSON.");
                return null!;
            }

            if (result.Results!.Any())
            {
                return result.Results!.Where(x => x.SellPrice >= 150 && x.SellPrice <= 450).OrderBy(x => x.SellPrice).ToList();
            }

            return Enumerable.Empty<Result>();
        }

        private async Task<IEnumerable<ListingProxy>> GetOnlyPaintedListingsFromResults(Result[] results)
        {
            var itemCollection = new List<ListingProxy>();

            foreach (var item in results)
            {
                var formattedName = FormatListingNameForUrl(item.Name!);

                var url = $"{Constants.FIRST_PAGE_URL_PART_1}{formattedName}{Constants.FIRST_PAGE_URL_PART_2}";

                var jsonResponse = await GetHttpResposeAsync(url);

                var jsonString = FormatJsonStringForDeserialization(jsonResponse);

                var deserializedResult = DeserializeFormattedJsonString<Listing_Json>(jsonString);

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
                        var listing = new ListingProxy
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

        private async Task<string?> GetHttpResponseAsync(string url)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        private async Task<string> GetHttpResposeAsync(string url)
        {
            string? httpResponse = await GetHttpResponseAsync(url);
            if (httpResponse == null)
            {
                return null!;
            }

            return httpResponse;
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

        private string FormatListingNameForUrl(string name)
        {
            return HttpUtility.UrlEncode(name);
        }
        #endregion
    }
}

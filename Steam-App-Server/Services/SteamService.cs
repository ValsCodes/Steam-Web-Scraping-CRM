using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SteamAppServer.Common;
using SteamAppServer.Exceptions;
using SteamAppServer.Models;
using SteamAppServer.Models.DTO;
using SteamAppServer.Models.Proxies;
using SteamAppServer.Repositories.Interfaces;
using SteamAppServer.Services.Interfaces;
using System.Web;

namespace SteamAppServer.Services
{
    public class SteamService : ISteamService
    {
        private readonly HttpClient _httpClient;
        private readonly ISalesRepository _steamRepository;
        private readonly IMapper _mapper;

        public SteamService(ISalesRepository steamRepository, HttpClient httpClient, IMapper mapper)
        {
            _steamRepository = steamRepository;
            _httpClient = httpClient;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetProductAsync(long? id)
        {
            if (id == null || id <= 0 || id > long.MaxValue)
            {
                throw new ItemNotFoundException($"Product with ID {id} doesn't exist.");
            }

            var product = await _steamRepository.GetProductAsync(id.Value);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            var products = await _steamRepository.GetProductsAsync();
            var result = new List<ProductDto>();

            if (products.Any())
            {
                foreach (var product in products)
                {
                    result.Add(_mapper.Map<ProductDto>(product));
                }
            }

            return result;
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto? productDto)
        {
            if (productDto == null)
            {
                throw new NullReferenceException($"Product cannot be null.");
            }

            if (productDto.QualityId == 0)
            {
                productDto.QualityId = null;
            }

            var product = await _steamRepository.CreateProductAsync(productDto);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> CreateProductsAsync(ProductDto[] productDtos)
        {
            if (!productDtos.Any())
            {
                throw new Exception("Empty collection.");
            }

            if (productDtos.Where(x => x.QualityId == 0).Any())
            {
                foreach (var productDto in productDtos.Where(x => x.QualityId == 0))
                {
                    productDto.QualityId = null;
                }
            }

            var products = await _steamRepository.CreateProductsAsync(productDtos);
            var result = new List<ProductDto>();
            if (products.Any())
            {
                foreach (var product in products)
                {
                    result.Add(_mapper.Map<ProductDto>(product));
                }
            }

            return result;
        }

        public async Task<bool> UpdateProductAsync(ProductDto? product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "ProductDto cannot be null.");
            }

            if (product?.Id == null || product.Id <= 0 || product.Id > long.MaxValue)
            {
                throw new ItemNotFoundException($"Product with ID {product!.Id} doesn't exist.");
            }

            return await _steamRepository.UpdateProductAsync(product);
        }

        public async Task<bool[]> UpdateProductsAsync(ProductDto[] productDtos)
        {
            if (productDtos == null || productDtos.Length == 0)
            {
                throw new ArgumentNullException(nameof(productDtos), "ProductDtos cannot be null or empty.");
            }

            return await _steamRepository.UpdateProductsAsync(productDtos);
        }

        public async Task<bool> DeleteProductAsync(long? id)
        {
            if (id == null || id <= 0 || id > long.MaxValue)
            {
                throw new ItemNotFoundException($"Product with ID {id} doesn't exist.");
            }

            return await _steamRepository.DeleteProductAsync(id.Value);
        }

        public async Task<bool[]> DeleteProductsAsync(long[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentNullException(nameof(ids), "IDs cannot be null or empty.");
            }

            return await _steamRepository.DeleteProductsAsync(ids);
        }

        #region Test Teritorry

        public async Task<IEnumerable<ProductProxy>> GetFilterredListingsAsync(short page)
        {
            var url = $"{Constants.JSON_100_LISTINGS_URL}{page}";
            var filteredResult = await GetFilterredResults(url);
            if (!filteredResult.Any())
            {
                return Enumerable.Empty<ProductProxy>();
            }

            return filteredResult.Select(x => new ProductProxy { Name = x.Name, Price = x.SellPrice, Quantity = x.SellListings });
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

        public Task<IEnumerable<ProductProxy>> GetPaintedListingsOnlyAsync(short page)
        {
            throw new NotImplementedException("This endpoint is not implemented yet.");
        }

        #endregion

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

        private async Task<IEnumerable<ProductProxy>> GetOnlyPaintedListingsFromResults(Result[] results)
        {
            var itemCollection = new List<ProductProxy>();
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
                        var listing = new ProductProxy
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

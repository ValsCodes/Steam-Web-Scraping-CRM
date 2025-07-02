using Newtonsoft.Json;

namespace SteamApp.Models.JsonObjects
{
    public class Result
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("hash_name")]
        public string? HashName { get; set; }

        [JsonProperty("sell_listings")]
        public short SellListings { get; set; }

        [JsonProperty("sell_price")]
        public double SellPrice { get; set; }

        [JsonProperty("sell_price_text")]
        public string? SellPriceText { get; set; }

        [JsonProperty("app_icon")]
        public string? AppIcon { get; set; }

        [JsonProperty("app_name")]
        public string? AppName { get; set; }

        [JsonProperty("asset_description")]
        public AssetDescription? AssetDescription { get; set; }

        [JsonProperty("sale_price_text")]
        public string? SalePriceText { get; set; }
    }
}

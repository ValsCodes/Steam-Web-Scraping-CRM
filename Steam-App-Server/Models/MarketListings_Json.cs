using Newtonsoft.Json;

namespace SteamAppServer.Models
{
    public class MarketListings_Json
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("pagesize")]
        public int PageSize { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("searchdata")]
        public SearchData? SearchData { get; set; }

        [JsonProperty("results")]
        public List<Result>? Results { get; set; }
    }

    public class SearchData
    {
        [JsonProperty("query")]
        public string? Query { get; set; }

        [JsonProperty("search_descriptions")]
        public bool SearchDescriptions { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("pagesize")]
        public int PageSize { get; set; }

        [JsonProperty("prefix")]
        public string? Prefix { get; set; }

        [JsonProperty("class_prefix")]
        public string? ClassPrefix { get; set; }
    }

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

    public class AssetDescription
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("classid")]
        public string? ClassId { get; set; }

        [JsonProperty("instanceid")]
        public string? InstanceId { get; set; }

        [JsonProperty("background_color")]
        public string? BackgroundColor { get; set; }

        [JsonProperty("icon_url")]
        public string? IconUrl { get; set; }

        [JsonProperty("tradable")]
        public int Tradable { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("name_color")]
        public string? NameColor { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("market_name")]
        public string? MarketName { get; set; }

        [JsonProperty("market_hash_name")]
        public string? MarketHashName { get; set; }

        [JsonProperty("commodity")]
        public int Commodity { get; set; }
    }
}

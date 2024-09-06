using Newtonsoft.Json;

namespace SteamAppServer.Models
{
    public class Listing_Json
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("pagesize")]
        public int PageSize { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("results_html")]
        public string? ResultsHtml { get; set; }

        [JsonProperty("listinginfo")]
        public IDictionary<string, ListingInfo>? ListingInfo { get; set; }

        [JsonProperty("assets")]
        public IDictionary<string, IDictionary<string, IDictionary<string, AssetDetail>>> Assets { get; set; }

        [JsonProperty("currency")]
        public IList<object>? Currency { get; set; }

        [JsonProperty("hovers")]
        public string? Hovers { get; set; }

        [JsonProperty("app_data")]
        public IDictionary<string, AppData>? AppData { get; set; }
    }

    public class ListingInfo
    {
        [JsonProperty("listingid")]
        public string? ListingId { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("fee")]
        public int Fee { get; set; }

        [JsonProperty("publisher_fee_app")]
        public int PublisherFeeApp { get; set; }

        [JsonProperty("publisher_fee_percent")]
        public string? PublisherFeePercent { get; set; }

        [JsonProperty("currencyid")]
        public int CurrencyId { get; set; }

        [JsonProperty("steam_fee")]
        public int SteamFee { get; set; }

        [JsonProperty("publisher_fee")]
        public int PublisherFee { get; set; }

        [JsonProperty("converted_price")]
        public int ConvertedPrice { get; set; }

        [JsonProperty("converted_fee")]
        public int ConvertedFee { get; set; }

        [JsonProperty("converted_currencyid")]
        public int ConvertedCurrencyId { get; set; }

        [JsonProperty("converted_steam_fee")]
        public int ConvertedSteamFee { get; set; }

        [JsonProperty("converted_publisher_fee")]
        public int ConvertedPublisherFee { get; set; }

        [JsonProperty("converted_price_per_unit")]
        public int ConvertedPricePerUnit { get; set; }

        [JsonProperty("converted_fee_per_unit")]
        public int ConvertedFeePerUnit { get; set; }

        [JsonProperty("converted_steam_fee_per_unit")]
        public int ConvertedSteamFeePerUnit { get; set; }

        [JsonProperty("converted_publisher_fee_per_unit")]
        public int ConvertedPublisherFeePerUnit { get; set; }

        [JsonProperty("asset")]
        public Asset? Asset { get; set; }
    }

    public class Asset
    {
        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("contextid")]
        public string? ContextId { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("amount")]
        public string? Amount { get; set; }

        [JsonProperty("market_actions")]
        public IList<MarketAction>? MarketActions { get; set; }
    }

    public class MarketAction
    {
        [JsonProperty("link")]
        public string? Link { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class AssetDetail
    {
        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("contextid")]
        public string ContextId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("classid")]
        public string ClassId { get; set; }

        [JsonProperty("instanceid")]
        public string InstanceId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("icon_url_large")]
        public string IconUrlLarge { get; set; }

        [JsonProperty("descriptions")]
        public IList<Description> Descriptions { get; set; }

        [JsonProperty("tradable")]
        public int Tradable { get; set; }

        [JsonProperty("actions")]
        public IList<Action> Actions { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_color")]
        public string NameColor { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("market_name")]
        public string MarketName { get; set; }

        [JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; }

        [JsonProperty("commodity")]
        public int Commodity { get; set; }

        [JsonProperty("market_tradable_restriction")]
        public int MarketTradableRestriction { get; set; }

        [JsonProperty("market_marketable_restriction")]
        public int MarketMarketableRestriction { get; set; }

        [JsonProperty("marketable")]
        public int Marketable { get; set; }

        [JsonProperty("app_icon")]
        public string AppIcon { get; set; }

        [JsonProperty("owner")]
        public int Owner { get; set; }
    }

    public class Description
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }

    public class Action
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class AppData
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("icon")]
        public string? Icon { get; set; }

        [JsonProperty("link")]
        public string? Link { get; set; }
    }
}
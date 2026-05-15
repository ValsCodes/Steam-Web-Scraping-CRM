using Newtonsoft.Json;

namespace SteamApp.Application.JsonObjects
{
    public class AssetDetail
    {
        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("contextid")]
        public string ContextId { get; set; } = string.Empty;

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("classid")]
        public string ClassId { get; set; } = string.Empty;

        [JsonProperty("instanceid")]
        public string InstanceId { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; } = string.Empty;

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; } = string.Empty;

        [JsonProperty("icon_url_large")]
        public string IconUrlLarge { get; set; } = string.Empty;

        [JsonProperty("descriptions")]
        public IList<Description> Descriptions { get; set; } = [];

        [JsonProperty("tradable")]
        public int Tradable { get; set; }

        [JsonProperty("actions")]
        public IList<Action> Actions { get; set; } = [];

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("name_color")]
        public string NameColor { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("market_name")]
        public string MarketName { get; set; } = string.Empty;

        [JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; } = string.Empty;

        [JsonProperty("commodity")]
        public int Commodity { get; set; }

        [JsonProperty("market_tradable_restriction")]
        public int MarketTradableRestriction { get; set; }

        [JsonProperty("market_marketable_restriction")]
        public int MarketMarketableRestriction { get; set; }

        [JsonProperty("marketable")]
        public int Marketable { get; set; }

        [JsonProperty("app_icon")]
        public string AppIcon { get; set; } = string.Empty;

        [JsonProperty("owner")]
        public int Owner { get; set; }
    }
}

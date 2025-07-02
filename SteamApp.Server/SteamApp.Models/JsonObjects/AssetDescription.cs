using Newtonsoft.Json;

namespace SteamApp.Models.JsonObjects
{
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

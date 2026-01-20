using Newtonsoft.Json;

namespace SteamApp.Application.JsonObjects
{
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
}

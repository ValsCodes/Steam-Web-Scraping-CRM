using Newtonsoft.Json;

namespace SteamApp.Models.Models.Json
{
    public class MarketAction
    {
        [JsonProperty("link")]
        public string? Link { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}

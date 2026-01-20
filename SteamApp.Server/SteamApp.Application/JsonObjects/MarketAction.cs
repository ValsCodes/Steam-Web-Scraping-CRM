using Newtonsoft.Json;

namespace SteamApp.Application.JsonObjects
{
    public class MarketAction
    {
        [JsonProperty("link")]
        public string? Link { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}

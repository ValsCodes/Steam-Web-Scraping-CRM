using Newtonsoft.Json;

namespace SteamApp.Models.Models.Json
{
    public class Description
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}

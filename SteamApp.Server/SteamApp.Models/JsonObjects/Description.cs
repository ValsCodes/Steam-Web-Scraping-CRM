using Newtonsoft.Json;

namespace SteamApp.Models.JsonObjects
{
    public class Description
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}

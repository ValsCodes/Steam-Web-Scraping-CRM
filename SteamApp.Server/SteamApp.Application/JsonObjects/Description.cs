using Newtonsoft.Json;

namespace SteamApp.Application.JsonObjects
{
    public class Description
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}

using Newtonsoft.Json;

namespace SteamApp.Application.JsonObjects
{
    public class Description
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("color")]
        public string Color { get; set; } = string.Empty;
    }
}

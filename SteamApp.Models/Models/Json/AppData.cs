using Newtonsoft.Json;

namespace SteamApp.Models.Models.Json
{
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

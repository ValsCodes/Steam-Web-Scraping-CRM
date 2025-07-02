using Newtonsoft.Json;

namespace SteamApp.Models.JsonObjects
{
    public class Action
    {
        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

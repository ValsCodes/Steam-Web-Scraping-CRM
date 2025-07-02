using Newtonsoft.Json;

namespace SteamApp.Models.JsonObjects
{
    public class SearchData
    {
        [JsonProperty("query")]
        public string? Query { get; set; }

        [JsonProperty("search_descriptions")]
        public bool SearchDescriptions { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("pagesize")]
        public int PageSize { get; set; }

        [JsonProperty("prefix")]
        public string? Prefix { get; set; }

        [JsonProperty("class_prefix")]
        public string? ClassPrefix { get; set; }
    }
}

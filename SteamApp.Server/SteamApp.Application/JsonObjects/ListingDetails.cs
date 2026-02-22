using Newtonsoft.Json;

namespace SteamApp.Application.JsonObjects
{
    public class ListingDetails
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("pagesize")]
        public int PageSize { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("searchdata")]
        public SearchData? SearchData { get; set; }

        [JsonProperty("results")]
        public List<Result>? Results { get; set; }
    }
}

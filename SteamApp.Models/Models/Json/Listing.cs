using Newtonsoft.Json;

namespace SteamApp.Models.Models.Json
{
    public class Listing
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("pagesize")]
        public int PageSize { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("results_html")]
        public string? ResultsHtml { get; set; }

        [JsonProperty("listinginfo")]
        public IDictionary<string, ListingInfo>? ListingInfo { get; set; }

        [JsonProperty("assets")]
        public IDictionary<string, IDictionary<string, IDictionary<string, AssetDetail>>> Assets { get; set; }

        [JsonProperty("currency")]
        public IList<object>? Currency { get; set; }

        [JsonProperty("hovers")]
        public string? Hovers { get; set; }

        [JsonProperty("app_data")]
        public IDictionary<string, AppData>? AppData { get; set; }
    }
}
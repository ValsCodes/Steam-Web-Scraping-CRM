using Newtonsoft.Json;

namespace SteamApp.Models.Models.Json
{
    public class ListingInfo
    {
        [JsonProperty("listingid")]
        public string? ListingId { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("fee")]
        public int Fee { get; set; }

        [JsonProperty("publisher_fee_app")]
        public int PublisherFeeApp { get; set; }

        [JsonProperty("publisher_fee_percent")]
        public string? PublisherFeePercent { get; set; }

        [JsonProperty("currencyid")]
        public int CurrencyId { get; set; }

        [JsonProperty("steam_fee")]
        public int SteamFee { get; set; }

        [JsonProperty("publisher_fee")]
        public int PublisherFee { get; set; }

        [JsonProperty("converted_price")]
        public int ConvertedPrice { get; set; }

        [JsonProperty("converted_fee")]
        public int ConvertedFee { get; set; }

        [JsonProperty("converted_currencyid")]
        public int ConvertedCurrencyId { get; set; }

        [JsonProperty("converted_steam_fee")]
        public int ConvertedSteamFee { get; set; }

        [JsonProperty("converted_publisher_fee")]
        public int ConvertedPublisherFee { get; set; }

        [JsonProperty("converted_price_per_unit")]
        public int ConvertedPricePerUnit { get; set; }

        [JsonProperty("converted_fee_per_unit")]
        public int ConvertedFeePerUnit { get; set; }

        [JsonProperty("converted_steam_fee_per_unit")]
        public int ConvertedSteamFeePerUnit { get; set; }

        [JsonProperty("converted_publisher_fee_per_unit")]
        public int ConvertedPublisherFeePerUnit { get; set; }

        [JsonProperty("asset")]
        public Asset? Asset { get; set; }
    }
}

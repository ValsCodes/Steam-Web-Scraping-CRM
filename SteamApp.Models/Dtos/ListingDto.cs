using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Dto
{
    public class ListingDto
    {
        [Required]
        public string? Name { get; set; }

        public double Price { get; set; }

        [JsonIgnore]
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        public string ListingUrl { get; set; }

        public short Quantity { get; set; }

        [JsonIgnore]
        public string? Color { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SteamApp.Models.Dto
{
    public class ListingDto
    {
        [Required]
        public string? Name { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public string ListingUrl { get; set; }

        public short Quantity { get; set; }

        public string? Color { get; set; }
    }
}

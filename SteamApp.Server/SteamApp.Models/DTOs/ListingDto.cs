namespace SteamApp.Models.DTOs
{
    public class ListingDto
    {
        public string? Name { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public string ListingUrl { get; set; }

        public short Quantity { get; set; }
    }
}

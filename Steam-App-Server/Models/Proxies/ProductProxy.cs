using System.Drawing;

namespace SteamAppServer.Models.Proxies
{
    public class ProductProxy
    {
        public string? Name { get; set; }

        public double Price { get; set; }

        public string? ImageUrl { get; set; }

        public string ListingUrl { get; set; }

        public short Quantity { get; set; }

        public string? Color { get; set; }
  
    }
}

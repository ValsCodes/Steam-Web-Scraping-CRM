using System.Drawing;

namespace SteamAppServer.Common.Proxies
{
    public class ListingProxy
    {
        public string? ImageUrl { get; set; }
        public double Price { get; set; }
        public string? Name { get; set; }
        public short Quantity { get; set; }
        public string? Color { get; set; }
    }
}

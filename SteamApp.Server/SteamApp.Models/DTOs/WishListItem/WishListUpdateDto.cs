namespace SteamApp.Models.DTOs.WishList
{
    public sealed class WishListUpdateDto
    {
        public long Id { get; set; }
        public double? Price { get; set; }
        public bool? IsActive { get; set; }
    }
}

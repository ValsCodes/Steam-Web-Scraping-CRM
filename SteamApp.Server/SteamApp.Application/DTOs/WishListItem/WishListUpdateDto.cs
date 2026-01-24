namespace SteamApp.Application.DTOs.WishListItem
{
    public sealed class WishListUpdateDto
    {
        public long Id { get; set; }
        public double? Price { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}

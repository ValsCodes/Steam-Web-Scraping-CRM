namespace SteamApp.Application.DTOs.WishListItem
{
    public sealed class WishListCreateDto
    {
        public long GameId { get; set; }
        public double? Price { get; set; }
        public bool IsActive { get; set; }
    }
}

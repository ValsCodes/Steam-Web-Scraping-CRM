namespace SteamApp.Application.DTOs.WishListItem
{
    public sealed class WishListDto
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public double? Price { get; set; }
        public bool IsActive { get; set; }
    }
}

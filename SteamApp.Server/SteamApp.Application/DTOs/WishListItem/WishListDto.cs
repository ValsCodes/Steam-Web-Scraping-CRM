namespace SteamApp.Application.DTOs.WishListItem
{
    public class WishListDto
    {
        public long Id { get; set; }
        public long? GameId { get; set; }
        public string? Name { get; set; }
        public double? Price { get; set; }
        public bool IsActive { get; set; }
    }
}

namespace SteamApp.Contracts.WishListItem
{
    public sealed class WishListCreateDto
    {
        public long GameId { get; set; }
        public string? Name { get; set; }
        public double? Price { get; set; }
        public bool IsActive { get; set; }
    }
}

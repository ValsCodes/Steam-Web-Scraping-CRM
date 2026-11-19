namespace SteamApp.Contracts.WishListItem
{
    public sealed class WishListUpdateStatusDto
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
    }
}

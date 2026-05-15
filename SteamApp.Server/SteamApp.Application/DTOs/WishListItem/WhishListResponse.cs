namespace SteamApp.Application.DTOs.WishListItem
{
    public class WhishListResponse
    {
        public string GameName { get; set; } = string.Empty;

        public bool IsPriceReached { get; set; }

        public double CurrentPrice { get; set; }
    }
}

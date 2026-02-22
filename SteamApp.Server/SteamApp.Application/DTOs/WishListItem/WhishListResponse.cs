namespace SteamApp.Application.DTOs.WishListItem
{
    public class WhishListResponse
    {
        public string GameName { get; set; }

        public bool IsPriceReached { get; set; }

        public double CurrentPrice { get; set; }
    }
}

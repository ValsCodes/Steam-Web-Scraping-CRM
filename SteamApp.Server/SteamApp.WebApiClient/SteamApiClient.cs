using SteamApp.WebApiClient.Managers;

namespace SteamApp.WebApiClient
{
    public class SteamApiClient(BaseApiClient core)
    {
        public GameManager Games { get; } = new GameManager(core);
        public ProductManager Products { get; } = new ProductManager(core);
        public GameUrlManager GameUrls { get; } = new GameUrlManager(core);
        public ExtraPixelManager ExtraPixels { get; } = new ExtraPixelManager(core);
        public WatchListManager WatchList { get; } = new WatchListManager(core);
        public WishListManager WishList { get; } = new WishListManager(core);
    }
}

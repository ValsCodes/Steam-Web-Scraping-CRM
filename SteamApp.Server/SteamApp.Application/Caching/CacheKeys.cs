namespace SteamApp.Application.Caching
{
    public static class CacheKeys
    {
        // Steam Controller
        public const string ScrapePage = "v1:scrape-page:{0}:{1}";

        public const string ScrapePublic = "v1:scrape-public:{0}:{1}";

        public const string PixelInfo = "v1:pixel-info:{0}:{1}";

        public const string ScrapePixels = "v1:scrape-pixels:{0}:{1}";

        public const string ProductPixels = "v1:product-pixels:{0}:{1}";

        // Steam Repository
        public const string GameUrl = "game-url:{0}";

        public const string Game = "game:{0}";

        public const string WishListItem = "wish-list-item:{0}";
    }
}

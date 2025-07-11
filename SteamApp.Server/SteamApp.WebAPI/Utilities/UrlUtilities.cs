namespace SteamApp.WebAPI.Helpers
{
    public static class UrlUtilities
    {
        public static string UrlEncode(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Name for Encoding was null.");
            }

            return Uri.EscapeDataString(name);
        }
    }
}

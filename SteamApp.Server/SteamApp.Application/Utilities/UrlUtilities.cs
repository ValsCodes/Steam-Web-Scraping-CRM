namespace SteamApp.Application.Utilities
{
    public static class UrlUtilities
    {
        public static string UrlEncode(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return Uri.EscapeDataString(name);
        }
    }
}

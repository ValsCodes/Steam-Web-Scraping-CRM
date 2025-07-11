using Newtonsoft.Json;

namespace SteamApp.WebAPI.Helpers
{
    public static class JsonUtilities
    {
        public static T? DeserializeFormattedJsonString<T>(string formattedJsonObject) where T : class
        {
            if (string.IsNullOrEmpty(formattedJsonObject))
            {
                throw new JsonSerializationException("Object for Deserialization was null.");
            }

            return JsonConvert.DeserializeObject<T>(formattedJsonObject);
        }

        public static string FormatJsonStringForDeserialization(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                throw new ArgumentNullException("Name for Encoding was null.");
            }

            return jsonString.Replace("\\r\\n", "\r\n");
        }
    }
}

namespace SteamApp.Common
{
    public static class Validator
    {
        public static ErrorResponce ValidateId(string id)
        {
            var responce = new ErrorResponce();
            var numId = long.TryParse(id, out var idValue);

            if (!numId || idValue <= 0)
            {
                responce.Errors.Add("Id must be a positive number");
            }

            return responce;
        }
    }

    public class ErrorResponce
    {
        public bool Valid { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}

namespace SteamApp.Domain.Common;

public class ErrorResponce
{
    public bool Valid { get; set; }
    public List<string> Errors { get; set; } = [];
}

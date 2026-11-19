namespace SteamApp.Application.Common.Exceptions;

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message) { }
}

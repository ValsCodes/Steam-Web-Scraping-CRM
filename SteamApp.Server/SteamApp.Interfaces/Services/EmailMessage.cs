namespace SteamApp.Interfaces.Services;

public sealed record EmailMessage(string To, string Subject, string Body);

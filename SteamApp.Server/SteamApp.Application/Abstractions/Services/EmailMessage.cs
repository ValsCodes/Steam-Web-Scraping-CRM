namespace SteamApp.Application.Abstractions.Services;

public sealed record EmailMessage(string To, string Subject, string Body);

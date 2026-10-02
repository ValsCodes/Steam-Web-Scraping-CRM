namespace SteamApp.Application.OperationResults;

public sealed record Error(string Code, string Description, ErrorType Type);

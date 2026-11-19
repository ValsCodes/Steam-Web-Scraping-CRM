using System.ComponentModel.DataAnnotations;

namespace SteamApp.WebAPI.Contracts;

public sealed record ExternalSignInCompletion(
    [EmailAddress, StringLength(256)] string? Email,
    [RegularExpression("[0-9]{6}")] string? Code);

﻿namespace SteamApp.Models.ValueObjects;

public class ClientDefinition
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string AllowedScope { get; set; } = null!;
}

namespace SteamApp.Infrastructure.Services;

public sealed class EncryptionHashingOptions
{
    public const string SectionName = "Cryptography";

    public string? EncryptionKeyBase64 { get; set; }
    public int Argon2MemorySizeKb { get; set; } = 65_536;
    public int Argon2Iterations { get; set; } = 3;
    public int Argon2DegreeOfParallelism { get; set; } = 1;
    public int Argon2SaltSizeBytes { get; set; } = 16;
    public int Argon2HashSizeBytes { get; set; } = 32;
}

namespace SteamApp.Interfaces.Services;

public interface IEncryptionHashingService
{
    string EncryptString(string plaintext, string? associatedData = null);

    string DecryptString(string encryptedValue, string? associatedData = null);

    string HashSecret(string secret);

    bool VerifySecret(string secret, string hash);

    bool HashNeedsUpgrade(string hash);
}

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

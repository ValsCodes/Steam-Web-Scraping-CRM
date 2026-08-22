namespace SteamApp.Interfaces.Services;

public interface IEncryptionHashingService
{
    string EncryptString(string plaintext, string? associatedData = null);

    string DecryptString(string encryptedValue, string? associatedData = null);

    string HashSecret(string secret);

    bool VerifySecret(string secret, string hash);

    bool HashNeedsUpgrade(string hash);
}

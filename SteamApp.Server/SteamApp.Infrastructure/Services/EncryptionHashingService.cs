using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using SteamApp.Interfaces.Services;

namespace SteamApp.Infrastructure.Services;

public sealed class EncryptionHashingService(
    IOptions<EncryptionHashingOptions> options) : IEncryptionHashingService
{
    private const string EncryptionVersion = "saesgcm1";
    private const string HashAlgorithm = "argon2id";
    private const int AesKeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int AuthenticationTagSizeBytes = 16;
    private const int Argon2Version = 19;

    public string EncryptString(string plaintext, string? associatedData = null)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var key = GetEncryptionKey();
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AuthenticationTagSizeBytes];
        var associatedDataBytes = GetAssociatedDataBytes(associatedData);

        using var aes = new AesGcm(key, AuthenticationTagSizeBytes);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, associatedDataBytes);

        return string.Join(
            '.',
            EncryptionVersion,
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag),
            Convert.ToBase64String(ciphertext));
    }

    public string DecryptString(string encryptedValue, string? associatedData = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedValue);

        var parts = encryptedValue.Split('.');

        if (parts.Length != 4 || parts[0] != EncryptionVersion)
        {
            throw new FormatException("Encrypted value is not a supported AES-GCM payload.");
        }

        var key = GetEncryptionKey();
        var nonce = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var ciphertext = Convert.FromBase64String(parts[3]);

        if (nonce.Length != NonceSizeBytes || tag.Length != AuthenticationTagSizeBytes)
        {
            throw new FormatException("Encrypted value has an invalid nonce or authentication tag.");
        }

        var plaintext = new byte[ciphertext.Length];
        var associatedDataBytes = GetAssociatedDataBytes(associatedData);

        using var aes = new AesGcm(key, AuthenticationTagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedDataBytes);

        return Encoding.UTF8.GetString(plaintext);
    }

    public string HashSecret(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        var hashingOptions = Normalize(options.Value);
        var salt = RandomNumberGenerator.GetBytes(hashingOptions.Argon2SaltSizeBytes);
        var hash = DeriveHash(secret, salt, hashingOptions);

        return string.Join(
            '$',
            string.Empty,
            HashAlgorithm,
            $"v={Argon2Version}",
            $"m={hashingOptions.Argon2MemorySizeKb},t={hashingOptions.Argon2Iterations},p={hashingOptions.Argon2DegreeOfParallelism}",
            ToUnpaddedBase64(salt),
            ToUnpaddedBase64(hash));
    }

    public bool VerifySecret(string secret, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        if (!TryReadHash(hash, out var parsedHash))
        {
            return false;
        }

        var candidateHash = DeriveHash(secret, parsedHash.Salt, parsedHash.Options);

        return CryptographicOperations.FixedTimeEquals(candidateHash, parsedHash.Hash);
    }

    public bool HashNeedsUpgrade(string hash)
    {
        if (!TryReadHash(hash, out var parsedHash))
        {
            return true;
        }

        var currentOptions = Normalize(options.Value);

        return parsedHash.Options.Argon2MemorySizeKb < currentOptions.Argon2MemorySizeKb ||
               parsedHash.Options.Argon2Iterations < currentOptions.Argon2Iterations ||
               parsedHash.Options.Argon2DegreeOfParallelism < currentOptions.Argon2DegreeOfParallelism ||
               parsedHash.Hash.Length < currentOptions.Argon2HashSizeBytes ||
               parsedHash.Salt.Length < currentOptions.Argon2SaltSizeBytes;
    }

    private byte[] GetEncryptionKey()
    {
        var configuredKey = options.Value.EncryptionKeyBase64;

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw new InvalidOperationException(
                "Cryptography:EncryptionKeyBase64 must be configured with a 32-byte base64 key before encryption can be used.");
        }

        byte[] key;

        try
        {
            key = Convert.FromBase64String(configuredKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Cryptography:EncryptionKeyBase64 must be a valid base64 value.",
                ex);
        }

        if (key.Length != AesKeySizeBytes)
        {
            throw new InvalidOperationException(
                "Cryptography:EncryptionKeyBase64 must decode to exactly 32 bytes for AES-256-GCM.");
        }

        return key;
    }

    private static byte[] DeriveHash(
        string secret,
        byte[] salt,
        EncryptionHashingOptions hashingOptions)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);

        using var argon2 = new Argon2id(secretBytes)
        {
            Salt = salt,
            MemorySize = hashingOptions.Argon2MemorySizeKb,
            Iterations = hashingOptions.Argon2Iterations,
            DegreeOfParallelism = hashingOptions.Argon2DegreeOfParallelism
        };

        return argon2.GetBytes(hashingOptions.Argon2HashSizeBytes);
    }

    private static bool TryReadHash(string hash, out ParsedHash parsedHash)
    {
        parsedHash = default;

        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        var parts = hash.Split('$');

        if (parts.Length != 6 ||
            parts[0] != string.Empty ||
            parts[1] != HashAlgorithm ||
            parts[2] != $"v={Argon2Version}")
        {
            return false;
        }

        if (!TryReadParameters(parts[3], out var hashingOptions))
        {
            return false;
        }

        try
        {
            var salt = FromUnpaddedBase64(parts[4]);
            var storedHash = FromUnpaddedBase64(parts[5]);

            hashingOptions.Argon2SaltSizeBytes = salt.Length;
            hashingOptions.Argon2HashSizeBytes = storedHash.Length;

            parsedHash = new ParsedHash(
                hashingOptions,
                salt,
                storedHash);

            return parsedHash.Salt.Length > 0 && parsedHash.Hash.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryReadParameters(
        string parameterValue,
        out EncryptionHashingOptions hashingOptions)
    {
        hashingOptions = new EncryptionHashingOptions();
        var parameters = parameterValue.Split(',');

        if (parameters.Length != 3)
        {
            return false;
        }

        foreach (var parameter in parameters)
        {
            var parts = parameter.Split('=');

            if (parts.Length != 2 || !int.TryParse(parts[1], out var value) || value <= 0)
            {
                return false;
            }

            switch (parts[0])
            {
                case "m":
                    hashingOptions.Argon2MemorySizeKb = value;
                    break;
                case "t":
                    hashingOptions.Argon2Iterations = value;
                    break;
                case "p":
                    hashingOptions.Argon2DegreeOfParallelism = value;
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private static EncryptionHashingOptions Normalize(EncryptionHashingOptions value)
    {
        return new EncryptionHashingOptions
        {
            EncryptionKeyBase64 = value.EncryptionKeyBase64,
            Argon2MemorySizeKb = Math.Clamp(value.Argon2MemorySizeKb, 8_192, 1_048_576),
            Argon2Iterations = Math.Clamp(value.Argon2Iterations, 1, 20),
            Argon2DegreeOfParallelism = Math.Clamp(
                value.Argon2DegreeOfParallelism,
                1,
                Math.Max(Environment.ProcessorCount, 1)),
            Argon2SaltSizeBytes = Math.Clamp(value.Argon2SaltSizeBytes, 16, 64),
            Argon2HashSizeBytes = Math.Clamp(value.Argon2HashSizeBytes, 32, 64)
        };
    }

    private static byte[]? GetAssociatedDataBytes(string? associatedData)
    {
        return associatedData is null ? null : Encoding.UTF8.GetBytes(associatedData);
    }

    private static string ToUnpaddedBase64(byte[] value)
    {
        return Convert.ToBase64String(value).TrimEnd('=');
    }

    private static byte[] FromUnpaddedBase64(string value)
    {
        var padding = (4 - value.Length % 4) % 4;
        return Convert.FromBase64String(value + new string('=', padding));
    }

    private readonly record struct ParsedHash(
        EncryptionHashingOptions Options,
        byte[] Salt,
        byte[] Hash);
}

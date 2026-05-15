using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SteamApp.Infrastructure.Services;
using SteamApp.Interfaces.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class EncryptionHashingServiceTests
{
    [Test]
    public void EncryptString_ProducesVersionedPayloadAndDecryptsWithMatchingAssociatedData()
    {
        var service = CreateService();

        var encrypted = service.EncryptString("market-secret", "user-profile:42");
        var decrypted = service.DecryptString(encrypted, "user-profile:42");

        Assert.Multiple(() =>
        {
            Assert.That(encrypted, Does.StartWith("saesgcm1."));
            Assert.That(encrypted, Does.Not.Contain("market-secret"));
            Assert.That(decrypted, Is.EqualTo("market-secret"));
        });
    }

    [Test]
    public void DecryptString_RejectsTamperedCiphertext()
    {
        var service = CreateService();
        var encrypted = service.EncryptString("market-secret");
        var parts = encrypted.Split('.');
        var ciphertext = Convert.FromBase64String(parts[3]);
        ciphertext[0] ^= 0x01;
        parts[3] = Convert.ToBase64String(ciphertext);
        var tampered = string.Join('.', parts);

        Assert.That(
            () => service.DecryptString(tampered),
            Throws.InstanceOf<CryptographicException>());
    }

    [Test]
    public void DecryptString_RejectsMismatchedAssociatedData()
    {
        var service = CreateService();
        var encrypted = service.EncryptString("market-secret", "profile:1");

        Assert.That(
            () => service.DecryptString(encrypted, "profile:2"),
            Throws.InstanceOf<CryptographicException>());
    }

    [Test]
    public void EncryptString_RequiresConfiguredAes256Key()
    {
        var service = CreateService(configureEncryptionKey: false);

        Assert.That(
            () => service.EncryptString("market-secret"),
            Throws.InvalidOperationException.With.Message.Contains("EncryptionKeyBase64"));
    }

    [Test]
    public void HashSecret_UsesArgon2idFormatAndVerifiesOnlyMatchingSecret()
    {
        var service = CreateService();

        var hash = service.HashSecret("correct horse battery staple");

        Assert.Multiple(() =>
        {
            Assert.That(hash, Does.StartWith("$argon2id$v=19$m=8192,t=1,p=1$"));
            Assert.That(service.VerifySecret("correct horse battery staple", hash), Is.True);
            Assert.That(service.VerifySecret("wrong secret", hash), Is.False);
            Assert.That(service.VerifySecret("correct horse battery staple", "not-a-hash"), Is.False);
        });
    }

    [Test]
    public void HashNeedsUpgrade_DetectsWeakerStoredParameters()
    {
        var weakerService = CreateService(memorySizeKb: 8192, iterations: 1);
        var strongerService = CreateService(memorySizeKb: 8192, iterations: 2);
        var hash = weakerService.HashSecret("market-secret");

        Assert.Multiple(() =>
        {
            Assert.That(weakerService.HashNeedsUpgrade(hash), Is.False);
            Assert.That(strongerService.HashNeedsUpgrade(hash), Is.True);
        });
    }

    private static EncryptionHashingService CreateService(
        string? encryptionKeyBase64 = null,
        bool configureEncryptionKey = true,
        int memorySizeKb = 8192,
        int iterations = 1)
    {
        return new EncryptionHashingService(
            Options.Create(new EncryptionHashingOptions
            {
                EncryptionKeyBase64 = configureEncryptionKey
                    ? encryptionKeyBase64 ?? CreateTestEncryptionKey()
                    : null,
                Argon2MemorySizeKb = memorySizeKb,
                Argon2Iterations = iterations,
                Argon2DegreeOfParallelism = 1,
                Argon2SaltSizeBytes = 16,
                Argon2HashSizeBytes = 32
            }));
    }

    private static string CreateTestEncryptionKey()
    {
        var key = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();

        return Convert.ToBase64String(key);
    }
}

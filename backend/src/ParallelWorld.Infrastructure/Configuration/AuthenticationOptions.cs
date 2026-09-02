using System.Security.Cryptography;

namespace ParallelWorld.Infrastructure.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Issuer { get; set; } = "parallel-world-api";

    public string Audience { get; set; } = "parallel-world-mobile";

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public int RefreshTokenLifetimeDays { get; set; } = 30;

    public int BootstrapRecoveryMinutes { get; set; } = 10;

    public int ClockSkewSeconds { get; set; } = 30;

    public string CurrentKeyId { get; set; } = string.Empty;

    public string CurrentPrivateKeyPem { get; set; } = string.Empty;

    public string CurrentPublicKeyPem { get; set; } = string.Empty;

    public string? PreviousKeyId { get; set; }

    public string? PreviousPublicKeyPem { get; set; }

    public static bool IsValid(AuthenticationOptions options)
    {
        var hasValidShape = options.Issuer == "parallel-world-api"
        && options.Audience == "parallel-world-mobile"
        && options.AccessTokenLifetimeMinutes == 15
        && options.RefreshTokenLifetimeDays == 30
        && options.BootstrapRecoveryMinutes == 10
        && options.ClockSkewSeconds == 30
        && !string.IsNullOrWhiteSpace(options.CurrentKeyId)
        && !string.IsNullOrWhiteSpace(options.CurrentPrivateKeyPem)
        && !string.IsNullOrWhiteSpace(options.CurrentPublicKeyPem)
        && (string.IsNullOrWhiteSpace(options.PreviousKeyId)
            == string.IsNullOrWhiteSpace(options.PreviousPublicKeyPem));

        if (!hasValidShape
            || !options.CurrentPrivateKeyPem.Contains("PRIVATE KEY", StringComparison.Ordinal)
            || options.CurrentPublicKeyPem.Contains("PRIVATE KEY", StringComparison.Ordinal)
            || options.PreviousPublicKeyPem?.Contains("PRIVATE KEY", StringComparison.Ordinal) == true
            || string.Equals(options.CurrentKeyId, options.PreviousKeyId, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var privateKey = RSA.Create();
            using var currentPublicKey = RSA.Create();
            privateKey.ImportFromPem(options.CurrentPrivateKeyPem);
            currentPublicKey.ImportFromPem(options.CurrentPublicKeyPem);
            if (privateKey.KeySize < 2048 || currentPublicKey.KeySize < 2048)
            {
                return false;
            }

            var challenge = RandomNumberGenerator.GetBytes(32);
            var signature = privateKey.SignData(
                challenge,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            if (!currentPublicKey.VerifyData(
                challenge,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(options.PreviousPublicKeyPem))
            {
                using var previousPublicKey = RSA.Create();
                previousPublicKey.ImportFromPem(options.PreviousPublicKeyPem);
                if (previousPublicKey.KeySize < 2048)
                {
                    return false;
                }
            }

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}

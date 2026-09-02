using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParallelWorld.Infrastructure.Configuration;

namespace ParallelWorld.Infrastructure.Authentication;

public sealed class JwtKeyMaterial : IDisposable
{
    private readonly RSA _privateRsa;
    private readonly List<RSA> _validationRsa = [];

    public JwtKeyMaterial(IOptions<AuthenticationOptions> optionsAccessor)
    {
        var options = optionsAccessor.Value;
        _privateRsa = RSA.Create();
        _privateRsa.ImportFromPem(options.CurrentPrivateKeyPem);
        SigningCredentials = new SigningCredentials(
            CreateKey(_privateRsa, options.CurrentKeyId),
            SecurityAlgorithms.RsaSha256);

        ValidationKeys = CreateValidationKeys(options);
    }

    public SigningCredentials SigningCredentials { get; }

    public IReadOnlyList<SecurityKey> ValidationKeys { get; }

    public void Dispose()
    {
        _privateRsa.Dispose();
        foreach (var rsa in _validationRsa)
        {
            rsa.Dispose();
        }
    }

    private IReadOnlyList<SecurityKey> CreateValidationKeys(AuthenticationOptions options)
    {
        var keys = new List<SecurityKey>
        {
            CreatePublicKey(options.CurrentPublicKeyPem, options.CurrentKeyId),
        };

        if (!string.IsNullOrWhiteSpace(options.PreviousKeyId)
            && !string.IsNullOrWhiteSpace(options.PreviousPublicKeyPem))
        {
            keys.Add(CreatePublicKey(options.PreviousPublicKeyPem, options.PreviousKeyId));
        }

        return keys;
    }

    private SecurityKey CreatePublicKey(string pem, string keyId)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        _validationRsa.Add(rsa);
        return CreateKey(rsa, keyId);
    }

    private static RsaSecurityKey CreateKey(RSA rsa, string keyId) => new(rsa)
    {
        KeyId = keyId,
        CryptoProviderFactory = new CryptoProviderFactory
        {
            CacheSignatureProviders = false,
        },
    };
}

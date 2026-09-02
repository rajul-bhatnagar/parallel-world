using System.Security.Cryptography;

namespace ParallelWorld.IntegrationTests;

internal static class TestAuthenticationConfiguration
{
    private static readonly RSA CurrentKey = RSA.Create(2048);
    private static readonly RSA PreviousKey = RSA.Create(2048);

    public const string CurrentKeyId = "integration-current";
    public const string PreviousKeyId = "integration-previous";

    public static string CurrentPrivateKeyPem { get; } = CurrentKey.ExportPkcs8PrivateKeyPem();

    public static string CurrentPublicKeyPem { get; } = CurrentKey.ExportSubjectPublicKeyInfoPem();

    public static string PreviousPrivateKeyPem { get; } = PreviousKey.ExportPkcs8PrivateKeyPem();

    public static string PreviousPublicKeyPem { get; } = PreviousKey.ExportSubjectPublicKeyInfoPem();

    public static Dictionary<string, string?> Create(string connectionString) => new()
    {
        ["ConnectionStrings:Default"] = connectionString,
        ["Authentication:CurrentKeyId"] = CurrentKeyId,
        ["Authentication:CurrentPrivateKeyPem"] = CurrentPrivateKeyPem,
        ["Authentication:CurrentPublicKeyPem"] = CurrentPublicKeyPem,
        ["Authentication:PreviousKeyId"] = PreviousKeyId,
        ["Authentication:PreviousPublicKeyPem"] = PreviousPublicKeyPem,
    };
}

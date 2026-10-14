using System.Security.Cryptography;
using Npgsql;

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

    private static string PrivateKeyFile { get; } = CreatePrivateKeyFile();

    public static Dictionary<string, string?> Create(string connectionString) => new()
    {
        ["ConnectionStrings:Default"] = connectionString,
        ["Authentication:CurrentKeyId"] = CurrentKeyId,
        ["Authentication:CurrentPrivateKeyPem"] = CurrentPrivateKeyPem,
        ["Authentication:CurrentPublicKeyPem"] = CurrentPublicKeyPem,
        ["Authentication:PreviousKeyId"] = PreviousKeyId,
        ["Authentication:PreviousPublicKeyPem"] = PreviousPublicKeyPem,
    };

    public static Dictionary<string, string?> CreateProduction(string connectionString)
    {
        var values = Create(WithRequiredTls(connectionString));
        values["Authentication:CurrentPrivateKeyPem"] = string.Empty;
        values["Authentication:CurrentPrivateKeyFile"] = PrivateKeyFile;
        values["Deployment:AllowedHosts:0"] = "localhost";
        values["Deployment:CorsAllowedOrigins:0"] = "https://mobile.example.test";
        values["Deployment:ForwardedHeadersEnabled"] = "true";
        return values;
    }

    private static string WithRequiredTls(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString) { SslMode = SslMode.Require };
        return builder.ConnectionString;
    }

    private static string CreatePrivateKeyFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"parallel-world-test-key-{Guid.NewGuid():N}.pem");
        File.WriteAllText(path, CurrentPrivateKeyPem);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => File.Delete(path);
        return path;
    }
}

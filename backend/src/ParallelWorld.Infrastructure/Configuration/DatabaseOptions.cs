using Npgsql;

namespace ParallelWorld.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string ConnectionStringName = "Default";

    public string ConnectionString { get; set; } = string.Empty;

    public int CommandTimeoutSeconds { get; set; } = 30;

    public static bool IsValid(DatabaseOptions options, bool requireTls)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return false;
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(options.ConnectionString);

            return !string.IsNullOrWhiteSpace(builder.Host)
                && !string.IsNullOrWhiteSpace(builder.Database)
                && !string.IsNullOrWhiteSpace(builder.Username)
                && builder.Pooling
                && builder.MaxPoolSize is > 0 and <= 200
                && options.CommandTimeoutSeconds is > 0 and <= 120
                && (!requireTls || builder.SslMode is SslMode.Require or SslMode.VerifyCA or SslMode.VerifyFull);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}

using Npgsql;

namespace ParallelWorld.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string ConnectionStringName = "Default";

    public string ConnectionString { get; set; } = string.Empty;

    public static bool IsValid(DatabaseOptions options)
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
                && !string.IsNullOrWhiteSpace(builder.Username);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}

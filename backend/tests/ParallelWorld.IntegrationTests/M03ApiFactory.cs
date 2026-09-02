using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ParallelWorld.Api.Observability;
using ParallelWorld.Infrastructure.Persistence;
using Serilog;
using Serilog.Core;

namespace ParallelWorld.IntegrationTests;

internal sealed partial class M03ApiFactory : ApiFactory
{
    private readonly string _administrativeConnectionString;
    private readonly string _testConnectionString;
    private readonly TimeProvider? _timeProvider;
    private readonly ILogEventSink? _logSink;
    private bool _databaseCreated;

    private M03ApiFactory(
        string administrativeConnectionString,
        string testConnectionString,
        string databaseName,
        TimeProvider? timeProvider,
        ILogEventSink? logSink)
    {
        _administrativeConnectionString = administrativeConnectionString;
        _testConnectionString = testConnectionString;
        DatabaseName = databaseName;
        _timeProvider = timeProvider;
        _logSink = logSink;
    }

    public string DatabaseName { get; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (_logSink is not null)
        {
            builder.UseSerilog((context, services, loggerConfiguration) =>
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.With<SensitiveDataEnricher>()
                    .WriteTo.Console()
                    .WriteTo.Sink(_logSink));
        }

        return base.CreateHost(builder);
    }

    public static async Task<M03ApiFactory> CreateAsync(
        string baseConnectionString,
        TimeProvider? timeProvider = null,
        ILogEventSink? logSink = null)
    {
        var configuredBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        var configuredDatabaseName = configuredBuilder.Database;
        if (string.IsNullOrWhiteSpace(configuredDatabaseName))
        {
            throw new InvalidOperationException(
                "The M03 integration-test administrative connection must specify a database name.");
        }

        TestDatabaseGuard.EnsureAdministrativeBase(configuredDatabaseName);
        var databaseName = $"parallel_world_test_{Guid.NewGuid():N}";
        TestDatabaseGuard.EnsureSafe(databaseName);
        var administrativeBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres",
            Pooling = false,
        };
        var testBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = databaseName,
        };
        var factory = new M03ApiFactory(
            administrativeBuilder.ConnectionString,
            testBuilder.ConnectionString,
            databaseName,
            timeProvider,
            logSink);

        await factory.CreateDatabaseAsync();
        try
        {
            await factory.MigrateAsync();
            return factory;
        }
        catch
        {
            await factory.DisposeAsync();
            throw;
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await DropDatabaseAsync();
        await CreateDatabaseAsync();
        await MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (!_databaseCreated)
        {
            return;
        }

        await DropDatabaseAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(
                TestAuthenticationConfiguration.Create(_testConnectionString)));
        if (_timeProvider is not null)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton(_timeProvider);
            });
        }
    }

    private async Task CreateDatabaseAsync()
    {
        TestDatabaseGuard.EnsureSafe(DatabaseName);
        await using var connection = new NpgsqlConnection(_administrativeConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{DatabaseName}\"";
        await command.ExecuteNonQueryAsync();
        _databaseCreated = true;
    }

    private async Task MigrateAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private async Task DropDatabaseAsync()
    {
        TestDatabaseGuard.EnsureSafe(DatabaseName);
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_administrativeConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{DatabaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
        _databaseCreated = false;
    }
}

internal static partial class TestDatabaseGuard
{
    public static void EnsureAdministrativeBase(string databaseName)
    {
        if (!string.Equals(databaseName, "parallel_world_tests", StringComparison.Ordinal)
            && !SafeDatabaseName().IsMatch(databaseName))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__Default must name parallel_world_tests or a generated test database.");
        }
    }

    public static void EnsureSafe(string databaseName)
    {
        if (!SafeDatabaseName().IsMatch(databaseName))
        {
            throw new InvalidOperationException(
                "M03 integration tests may only manage generated parallel_world_test_<32 hex> databases.");
        }
    }

    [GeneratedRegex("^parallel_world_test_[a-f0-9]{32}$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeDatabaseName();
}

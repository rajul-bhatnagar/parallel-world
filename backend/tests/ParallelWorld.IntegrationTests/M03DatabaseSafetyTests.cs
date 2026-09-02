using Npgsql;

namespace ParallelWorld.IntegrationTests;

public sealed class M03DatabaseSafetyTests
{
    [Theory]
    [InlineData("parallel_world")]
    [InlineData("parallel_world_tests")]
    [InlineData("postgres")]
    [InlineData("parallel_world_test_")]
    [InlineData("parallel_world_test_0123456789abcdef0123456789ABCDE")]
    [InlineData("parallel_world_test_0123456789abcdef0123456789abcdef_extra")]
    public void LifecycleGuard_RejectsEveryNonGeneratedDatabaseName(string databaseName) =>
        Assert.Throws<InvalidOperationException>(() => TestDatabaseGuard.EnsureSafe(databaseName));

    [Fact]
    public void LifecycleGuard_AcceptsExactGeneratedDatabaseName() =>
        TestDatabaseGuard.EnsureSafe("parallel_world_test_0123456789abcdef0123456789abcdef");

    [Theory]
    [InlineData("parallel_world")]
    [InlineData("postgres")]
    [InlineData("developer_database")]
    public void AdministrativeBaseGuard_RejectsUnsafeConfiguredDatabase(string databaseName) =>
        Assert.Throws<InvalidOperationException>(() =>
            TestDatabaseGuard.EnsureAdministrativeBase(databaseName));

    [Theory]
    [InlineData("parallel_world_tests")]
    [InlineData("parallel_world_test_0123456789abcdef0123456789abcdef")]
    public void AdministrativeBaseGuard_AcceptsOnlyTestSpecificDatabaseNames(string databaseName) =>
        TestDatabaseGuard.EnsureAdministrativeBase(databaseName);

    [Fact]
    [Trait("Category", "PostgreSql")]
    public async Task FixtureCleanup_DropsOnlyItsGeneratedDatabase()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        var factory = await M03ApiFactory.CreateAsync(connectionString);
        var generatedDatabase = factory.DatabaseName;
        TestDatabaseGuard.EnsureSafe(generatedDatabase);

        await factory.DisposeAsync();

        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres",
            Pooling = false,
        };
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = $1)";
        command.Parameters.AddWithValue(generatedDatabase);
        Assert.False((bool)(await command.ExecuteScalarAsync())!);
    }
}

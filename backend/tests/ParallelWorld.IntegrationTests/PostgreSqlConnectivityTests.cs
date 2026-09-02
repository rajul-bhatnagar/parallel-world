using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class PostgreSqlConnectivityTests
{
    [Fact]
    public async Task GeneratedTestPostgreSql_IsReachableThroughRegisteredDbContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "ConnectionStrings__Default must identify the PostgreSQL test administrative base.");

        await using var factory = await M03ApiFactory.CreateAsync(connectionString);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Assert.Same(dbContext, unitOfWork);
        Assert.True(await dbContext.Database.CanConnectAsync());
        Assert.NotEmpty(dbContext.Model.GetEntityTypes());
        TestDatabaseGuard.EnsureSafe(factory.DatabaseName);
    }
}

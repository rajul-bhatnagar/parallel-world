using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

public sealed class PostgreSqlConnectivityTests
{
    [Fact]
    public async Task ConfiguredPostgreSql_IsReachableThroughRegisteredDbContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "ConnectionStrings__Default must identify the isolated M02 PostgreSQL test database.");

        await using var factory = new ApiFactoryWithConnection(connectionString);
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Assert.Same(dbContext, unitOfWork);
        Assert.True(await dbContext.Database.CanConnectAsync());
        Assert.Empty(dbContext.Model.GetEntityTypes());
    }

    private sealed class ApiFactoryWithConnection(string connectionString) : ApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connectionString,
                }));
        }
    }
}

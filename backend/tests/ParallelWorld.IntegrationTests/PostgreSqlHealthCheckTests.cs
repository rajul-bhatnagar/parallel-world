using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ParallelWorld.Api.Health;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

public sealed class PostgreSqlHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenCancellationIsRequested_PropagatesCancellation()
    {
        var options = new DbContextOptionsBuilder<ParallelWorldDbContext>()
            .UseNpgsql(ApiFactory.UnavailableDatabaseConnectionString)
            .Options;
        await using var dbContext = new ParallelWorldDbContext(options);
        var healthCheck = new PostgreSqlHealthCheck(dbContext);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            healthCheck.CheckHealthAsync(new HealthCheckContext(), cancellationSource.Token));
    }
}

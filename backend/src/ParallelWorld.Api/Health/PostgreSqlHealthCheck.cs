using Microsoft.Extensions.Diagnostics.HealthChecks;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Api.Health;

public sealed class PostgreSqlHealthCheck(ParallelWorldDbContext dbContext) : IHealthCheck
{
    public const string Name = "postgresql";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}

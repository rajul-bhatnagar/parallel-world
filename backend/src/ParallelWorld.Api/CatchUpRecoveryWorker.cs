using ParallelWorld.Application.Simulation;

namespace ParallelWorld.Api;

public sealed class CatchUpRecoveryWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<CatchUpRecoveryWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int ClaimBatchSize = 8;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PollInterval, timeProvider, stoppingToken);
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISimulationRepository>();
                var worldIds = await repository.ListRecoverableCatchUpWorldIdsAsync(
                    timeProvider.GetUtcNow(), ClaimBatchSize, stoppingToken);
                foreach (var worldId in worldIds)
                {
                    await using var workScope = scopeFactory.CreateAsyncScope();
                    await workScope.ServiceProvider.GetRequiredService<ISimulationService>()
                        .ProcessCatchUpAsync(worldId, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    "CatchUp recovery failed with {ExceptionType}.",
                    exception.GetType().Name);
            }
        }
    }
}

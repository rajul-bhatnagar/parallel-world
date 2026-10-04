using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParallelWorld.Application.Messaging;

namespace ParallelWorld.AI;

public sealed class MessageWordingWorker(IServiceScopeFactory scopeFactory,
    ILogger<MessageWordingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IMessagingRepository>();
                var work = await repository.GetPendingWordingWorkAsync(stoppingToken);
                if (work is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                var generator = scope.ServiceProvider.GetRequiredService<IMessageWordingGenerator>();
                var wording = await generator.GenerateAsync(work.Request, stoppingToken);
                await repository.FinalizeReplyAsync(
                    work.OwnerUserId, work.Request.PlannedReplyId, wording, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError("Private message wording recovery failed with {ExceptionType}.",
                    exception.GetType().Name);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}

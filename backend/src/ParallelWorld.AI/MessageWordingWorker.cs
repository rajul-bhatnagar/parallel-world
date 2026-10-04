using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParallelWorld.Application.Memory;
using ParallelWorld.Application.Messaging;
using ParallelWorld.Domain.Memory;

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

                var memoryService = scope.ServiceProvider.GetRequiredService<IMemoryService>();
                _ = await memoryService.CreateAsync(new CreateMemoryCommand(
                    work.Request.WorldId,
                    work.Request.CharacterId,
                    MemoryType.Event,
                    MemoryAuthorityType.GameplayEvent,
                    MemorySubjectType.Actor,
                    work.Request.SubjectActorId,
                    null,
                    null,
                    "Received and replied to a private message from the player.",
                    MemorySourceType.GameplayEvent,
                    work.Request.SourceGameplayEventId,
                    null), stoppingToken);
                var memoryContext = await memoryService.RecallForMessageWordingAsync(
                    work.Request.WorldId,
                    work.Request.CharacterId,
                    work.Request.SubjectActorId,
                    work.Request.PlannedReplyId,
                    stoppingToken);
                var request = work.Request with { MemoryContext = memoryContext };
                var generator = scope.ServiceProvider.GetRequiredService<IMessageWordingGenerator>();
                var wording = await generator.GenerateAsync(request, stoppingToken);
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

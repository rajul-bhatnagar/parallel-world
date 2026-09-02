using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Application.Common;

namespace ParallelWorld.Application.Worlds;

public sealed class WorldService(
    IWorldRepository repository,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier failureClassifier,
    TimeProvider timeProvider) : IWorldService
{
    public async Task<WorldResult<WorldSummary>> CreateAsync(
        CreateWorldCommand command,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                return await CreateOnceAsync(command, cancellationToken);
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return WorldResult<WorldSummary>.Fail(new ServiceFailure(
            "concurrency_conflict",
            409,
            "The world could not be created concurrently."));
    }

    public Task<IReadOnlyList<WorldSummary>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken) => repository.ListAsync(userId, cancellationToken);

    public async Task<WorldResult<WorldSummary>> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var world = await repository.FindAsync(userId, null, cancellationToken);
        return world is null ? NotAvailable() : WorldResult<WorldSummary>.Success(world);
    }

    public async Task<WorldResult<WorldSummary>> GetAsync(
        Guid userId,
        Guid worldId,
        CancellationToken cancellationToken)
    {
        var world = await repository.FindAsync(userId, worldId, cancellationToken);
        return world is null ? NotAvailable() : WorldResult<WorldSummary>.Success(world);
    }

    private async Task<WorldResult<WorldSummary>> CreateOnceAsync(
        CreateWorldCommand command,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.Serializable,
            cancellationToken);
        if (await repository.FindAsync(command.UserId, null, cancellationToken) is not null)
        {
            return WorldResult<WorldSummary>.Fail(new ServiceFailure(
                "world_already_exists",
                409,
                "The MVP world already exists."));
        }

        await repository.EnsureUserExistsAsync(command.UserId, cancellationToken);
        var aggregate = WorldAggregateFactory.Create(command.UserId, command.Name, now);
        repository.Add(aggregate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return WorldResult<WorldSummary>.Success(ToSummary(aggregate));
    }

    private static WorldSummary ToSummary(WorldAggregate aggregate) => new(
        aggregate.World.Id,
        aggregate.World.Name,
        aggregate.World.Status.ToString().ToLowerInvariant(),
        aggregate.World.CurrentWorldTime,
        new PlayerSummary(aggregate.PlayerActor.Id, aggregate.PlayerProfile.DisplayName),
        aggregate.World.CreatedAt);

    private static WorldResult<WorldSummary> NotAvailable() =>
        WorldResult<WorldSummary>.Fail(new ServiceFailure(
            "resource_not_available",
            404,
            "The requested world is not available."));
}

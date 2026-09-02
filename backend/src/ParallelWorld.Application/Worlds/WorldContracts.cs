using ParallelWorld.Application.Authentication;
using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Application.Worlds;

public sealed record CreateWorldCommand(
    Guid UserId,
    string Name);

public sealed record WorldResult<T>(T? Value, ServiceFailure? Failure)
{
    public bool IsSuccess => Failure is null;

    public static WorldResult<T> Success(T value) => new(value, null);

    public static WorldResult<T> Fail(ServiceFailure failure) => new(default, failure);
}

public interface IWorldService
{
    Task<WorldResult<WorldSummary>> CreateAsync(
        CreateWorldCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WorldSummary>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<WorldResult<WorldSummary>> GetCurrentAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<WorldResult<WorldSummary>> GetAsync(
        Guid userId,
        Guid worldId,
        CancellationToken cancellationToken);
}

public interface IWorldRepository
{
    Task<IReadOnlyList<WorldSummary>> ListAsync(Guid ownerUserId, CancellationToken cancellationToken);

    Task<WorldSummary?> FindAsync(
        Guid ownerUserId,
        Guid? worldId,
        CancellationToken cancellationToken);

    Task EnsureUserExistsAsync(Guid userId, CancellationToken cancellationToken);

    void Add(WorldAggregate aggregate);
}

public sealed record WorldAggregate(
    GameWorld World,
    WorldSettings Settings,
    WorldSimulationState SimulationState,
    PlayerProfile PlayerProfile,
    Actor PlayerActor);

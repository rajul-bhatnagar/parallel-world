using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Relationships;

namespace ParallelWorld.Application.Relationships;

public sealed record ApplyRelationshipEventCommand(Guid WorldId, Guid SourceActorId, Guid TargetActorId, Guid GameplayEventId,
    string EventType, bool IsPublicNegative, bool IsPositive, DateOnly GameDate, DateTimeOffset OccurredAt, int RuleVersion, string IdempotencyKey);
public sealed record RelationshipSummary(Guid ActorId, string DisplayName, string Handle, string State, DateTimeOffset UpdatedAtUtc);
public sealed record RelationshipHistoryItem(Guid Id, string EventType, string ReasonCode, DateTimeOffset OccurredAtUtc);
public sealed record RelationshipDetails(RelationshipSummary Summary, IReadOnlyList<RelationshipHistoryItem> History);
public sealed record RelationshipResult<T>(T? Value, ServiceFailure? Failure) { public bool IsSuccess => Failure is null; public static RelationshipResult<T> Success(T value) => new(value, null); public static RelationshipResult<T> Fail(ServiceFailure failure) => new(default, failure); }

public interface IRelationshipRepository
{
    Task<bool> OwnsWorldAsync(Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<Relationship?> ApplyAsync(ApplyRelationshipEventCommand command, RelationshipValues baseDelta, CancellationToken cancellationToken);
    Task<IReadOnlyList<RelationshipSummary>> ListAsync(Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<RelationshipDetails?> FindAsync(Guid userId, Guid worldId, Guid actorId, int historyLimit, CancellationToken cancellationToken);
}
public interface IRelationshipService
{
    Task<Relationship?> ApplyAsync(ApplyRelationshipEventCommand command, CancellationToken cancellationToken = default);
    Task<RelationshipResult<IReadOnlyList<RelationshipSummary>>> ListAsync(Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<RelationshipResult<RelationshipDetails>> FindAsync(Guid userId, Guid worldId, Guid actorId, int historyLimit, CancellationToken cancellationToken);
}

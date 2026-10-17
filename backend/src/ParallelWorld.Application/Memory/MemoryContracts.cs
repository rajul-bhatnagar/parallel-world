using ParallelWorld.Domain.Memory;

namespace ParallelWorld.Application.Memory;

public sealed record PromiseCreationDetails(
    string PromiseType,
    Guid SourceActorId,
    Guid? TargetActorId,
    PromiseDueConditionType DueConditionType,
    DateTimeOffset? DueAtWorldTime,
    Guid? DueGameplayEventId,
    string? DueEventType);

public sealed record CreateMemoryCommand(
    Guid WorldId,
    Guid OwnerCharacterId,
    MemoryType MemoryType,
    MemoryAuthorityType AuthorityType,
    MemorySubjectType SubjectType,
    Guid? SubjectActorId,
    string? SubjectTopicId,
    string? TopicId,
    string StructuredContent,
    MemorySourceType SourceType,
    Guid SourceId,
    PromiseCreationDetails? Promise);

public sealed record MemoryCreationResult(
    bool Created,
    bool IsReplay,
    Guid? MemoryId,
    Guid? EvictedMemoryId,
    string? ReasonCode)
{
    public static MemoryCreationResult Invalid(string reasonCode) =>
        new(false, false, null, null, reasonCode);
}

public sealed record RecallMemoryCommand(
    Guid WorldId,
    Guid OwnerCharacterId,
    MemoryRecallPurpose Purpose,
    MemorySubjectType SubjectType,
    Guid? SubjectActorId,
    string? SubjectTopicId,
    string? TopicId,
    string IdempotencyKey);

public sealed record RecalledMemory(
    Guid MemoryId,
    MemoryType MemoryType,
    string StructuredContent,
    int Score,
    int Rank,
    DateTimeOffset CreatedAtUtc);

public sealed record MemoryRecallResult(
    bool IsValid,
    bool IsReplay,
    string? ReasonCode,
    IReadOnlyList<RecalledMemory> Items)
{
    public static MemoryRecallResult Invalid(string reasonCode) =>
        new(false, false, reasonCode, []);
}

public sealed record TransitionPromiseCommand(
    Guid WorldId,
    Guid PromiseId,
    PromiseStatus TargetStatus,
    Guid? GameplayEventId,
    DateTimeOffset WorldTime);

public interface IMemoryRepository
{
    Task<MemoryCreationResult> CreateAsync(
        CreateMemoryCommand command,
        CancellationToken cancellationToken);

    Task<MemoryRecallResult> RecallAsync(
        RecallMemoryCommand command,
        CancellationToken cancellationToken);

    Task<bool> TransitionPromiseAsync(
        TransitionPromiseCommand command,
        CancellationToken cancellationToken);

    Task<int> ExpireDuePromisesAsync(
        Guid worldId,
        DateTimeOffset worldTime,
        CancellationToken cancellationToken);
}

public interface IMemoryService
{
    Task<MemoryCreationResult> CreateAsync(
        CreateMemoryCommand command,
        CancellationToken cancellationToken = default);

    Task<MemoryRecallResult> RecallAsync(
        RecallMemoryCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> RecallForMessageWordingAsync(
        Guid worldId,
        Guid ownerCharacterId,
        Guid subjectActorId,
        Guid plannedReplyId,
        CancellationToken cancellationToken = default);

    Task<bool> TransitionPromiseAsync(
        TransitionPromiseCommand command,
        CancellationToken cancellationToken = default);

    Task<int> ExpireDuePromisesAsync(
        Guid worldId,
        DateTimeOffset worldTime,
        CancellationToken cancellationToken = default);
}

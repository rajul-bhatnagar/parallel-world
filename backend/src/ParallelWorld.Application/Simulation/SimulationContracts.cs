using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Application.Simulation;

public interface ISimulationService
{
    Task<SimulationTickResult> ProcessOldestDueIntervalAsync(
        Guid worldId,
        CancellationToken cancellationToken = default);

    Task<CatchUpResult> ProcessCatchUpAsync(
        Guid worldId,
        CancellationToken cancellationToken = default);

    Task<CatchUpSummaryView?> GetLatestCatchUpSummaryAsync(
        Guid worldId,
        CancellationToken cancellationToken = default);
}

public interface ISimulationRepository
{
    Task<SimulationWorldSnapshot?> FindSnapshotAsync(
        Guid worldId,
        CancellationToken cancellationToken);

    Task<LockedSimulationWorld?> LockWorldAsync(
        Guid worldId,
        CancellationToken cancellationToken);

    Task<SimulationRun?> FindRunAsync(
        Guid worldId,
        Guid runId,
        CancellationToken cancellationToken);

    Task<SimulationRun?> FindOpenCatchUpRunAsync(Guid worldId, CancellationToken cancellationToken);

    Task<SimulationRun?> LockRunAsync(Guid worldId, Guid runId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> ListRecoverableCatchUpWorldIdsAsync(
        DateTimeOffset observedAtUtc,
        int limit,
        CancellationToken cancellationToken);

    Task<CatchUpSummary?> FindCatchUpSummaryAsync(Guid worldId, Guid runId, CancellationToken cancellationToken);

    Task<CatchUpSummaryView?> GetLatestCatchUpSummaryAsync(Guid worldId, CancellationToken cancellationToken);

    Task<int> CountCatchUpSummaryItemsAsync(Guid worldId, Guid summaryId, CancellationToken cancellationToken);

    Task<bool> HasCatchUpSummaryItemAsync(Guid worldId, Guid summaryId, string itemType, DateOnly gameDate, CancellationToken cancellationToken);

    Task<IReadOnlyList<FollowRuleCandidate>> ListFollowCandidatesAsync(Guid worldId, DateTimeOffset currentGameTime, DateOnly currentGameDate, CancellationToken cancellationToken);

    Guid AddAutonomousFollow(Guid worldId, Guid runId, DateTimeOffset occurredAt, DateTimeOffset occurredGameTime, DateOnly occurredGameDate, int ruleVersion, FollowRuleAction action);

    Guid AddCatchUpAutonomousFollow(
        Guid worldId,
        Guid runId,
        int bucketOrdinal,
        Guid gameplayEventId,
        Guid followId,
        Guid simulationActionId,
        DateTimeOffset occurredAt,
        DateTimeOffset occurredGameTime,
        DateOnly occurredGameDate,
        int ruleVersion,
        FollowRuleAction action);

    void AddRun(
        SimulationRun run,
        SimulationRunCheckpoint checkpoint,
        IReadOnlyCollection<SimulationRuleEvaluation> evaluations);

    void AddCatchUpRun(SimulationRun run, CatchUpSummary summary);

    void AddCatchUpCheckpoint(SimulationRunCheckpoint checkpoint);

    void AddCatchUpSummaryItem(CatchUpSummaryItem item);
}

public sealed record FollowRuleCandidate(Guid SourceActorId, Guid TargetActorId, RelationshipValues Relationship, int InterestOverlap, int TargetReputation, bool IsFollowing, bool HasHistoricalFollow, int QualifiedNegativeEventCount, int StableOrdinal);
public sealed record FollowRuleAction(Guid SourceActorId, Guid TargetActorId, bool DesiredFollowing, string? RelationshipEventType, int Score, int Roll, int StableOrdinal);

public sealed record SimulationWorldSnapshot(
    Guid WorldId,
    WorldStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset CurrentWorldTime,
    DateTimeOffset LastSimulatedAt,
    DateTimeOffset? LastCompletedIntervalEnd,
    DateTimeOffset NextDueAt,
    decimal TimeScale,
    int RuleVersion,
    string DisplayTimeZoneId);

public sealed record LockedSimulationWorld(
    GameWorld World,
    WorldSettings Settings,
    WorldSimulationState SimulationState);

public sealed record SimulationTickResult(
    SimulationTickDisposition Disposition,
    Guid? RunId,
    DateTimeOffset? IntervalStartUtc,
    DateTimeOffset? IntervalEndUtc,
    DateTimeOffset? LastCompletedIntervalEndUtc,
    DateTimeOffset NextDueAtUtc,
    DateTimeOffset CurrentWorldTimeUtc,
    IReadOnlyList<SimulationEvaluationResult> Evaluations,
    string? ErrorCode = null);

public sealed record SimulationEvaluationResult(
    Guid Id,
    string RuleCode,
    string Outcome,
    string ReasonCode);

public enum SimulationTickDisposition
{
    NotDue,
    Completed,
    AlreadyProcessed,
    WorldUnavailable,
    InconsistentState,
}

public sealed record CatchUpResult(
    CatchUpDisposition Disposition,
    Guid? RunId,
    string Status,
    int RequestedIntervals,
    int ProcessedIntervals,
    int RemainingIntervals,
    int ProcessedBuckets,
    DateTimeOffset? ProcessedThroughUtc,
    DateTimeOffset NextDueAtUtc,
    DateTimeOffset CurrentWorldTimeUtc,
    CatchUpSummaryView? Summary,
    string? ErrorCode = null);

public enum CatchUpDisposition
{
    NotDue,
    Processing,
    Partial,
    Completed,
    WorldUnavailable,
    InconsistentState,
}

public sealed record CatchUpSummaryView(
    Guid Id,
    Guid RunId,
    string Status,
    DateTimeOffset FromGameTime,
    DateTimeOffset ToGameTime,
    DateTimeOffset GeneratedAtUtc,
    string Text,
    IReadOnlyList<CatchUpSummaryItemView> Items);

public sealed record CatchUpSummaryItemView(
    Guid Id,
    string ItemType,
    int StableOrdinal,
    string FactCode,
    Guid? ActorId,
    Guid? TargetActorId,
    DateOnly GameDate,
    string Wording,
    DateTimeOffset CreatedAtUtc);

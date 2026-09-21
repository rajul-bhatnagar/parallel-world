using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Application.Simulation;

public interface ISimulationService
{
    Task<SimulationTickResult> ProcessOldestDueIntervalAsync(
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

    void AddRun(
        SimulationRun run,
        SimulationRunCheckpoint checkpoint,
        IReadOnlyCollection<SimulationRuleEvaluation> evaluations);
}

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

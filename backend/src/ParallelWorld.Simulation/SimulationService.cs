using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Simulation;

public sealed class SimulationService(
    ISimulationRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IWorldTimeProjector worldTimeProjector,
    IDeterministicRandomProvider randomProvider,
    IEnumerable<ISimulationRule> rules) : ISimulationService
{
    private readonly IReadOnlyList<ISimulationRule> _rules = rules
        .OrderBy(rule => rule.Priority)
        .ThenBy(rule => rule.Code, StringComparer.Ordinal)
        .ToArray();

    public async Task<SimulationTickResult> ProcessOldestDueIntervalAsync(
        Guid worldId,
        CancellationToken cancellationToken = default)
    {
        var observedAtUtc = timeProvider.GetUtcNow();
        var snapshot = await repository.FindSnapshotAsync(worldId, cancellationToken);
        if (snapshot is null || snapshot.Status != WorldStatus.Active)
        {
            return Unavailable(snapshot);
        }

        var candidateStart = snapshot.LastCompletedIntervalEnd ?? snapshot.CreatedAtUtc;
        var candidateEnd = candidateStart.Add(WorldSimulationState.ActiveIntervalDuration);
        if (snapshot.NextDueAt != candidateEnd)
        {
            return Inconsistent(snapshot, "simulation_due_projection_mismatch");
        }

        if (observedAtUtc < snapshot.NextDueAt)
        {
            return FromSnapshot(SimulationTickDisposition.NotDue, snapshot);
        }

        var observedRunId = DeterministicSimulationIdentity.CreateRunId(
            worldId,
            candidateStart,
            candidateEnd,
            snapshot.RuleVersion);

        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken);
        var locked = await repository.LockWorldAsync(worldId, cancellationToken);
        if (locked is null || locked.World.Status != WorldStatus.Active)
        {
            return Unavailable(snapshot);
        }

        var lockedStart = locked.SimulationState.LastCompletedIntervalEnd ?? locked.World.CreatedAt;
        if (lockedStart != candidateStart)
        {
            return new(
                SimulationTickDisposition.AlreadyProcessed,
                observedRunId,
                candidateStart,
                candidateEnd,
                locked.SimulationState.LastCompletedIntervalEnd,
                locked.SimulationState.NextDueAt,
                locked.World.CurrentWorldTime,
                []);
        }

        if (locked.SimulationState.NextDueAt != candidateEnd)
        {
            return new(
                SimulationTickDisposition.InconsistentState,
                null,
                candidateStart,
                candidateEnd,
                locked.SimulationState.LastCompletedIntervalEnd,
                locked.SimulationState.NextDueAt,
                locked.World.CurrentWorldTime,
                [],
                "simulation_due_projection_mismatch");
        }

        var runId = DeterministicSimulationIdentity.CreateRunId(
            worldId,
            candidateStart,
            candidateEnd,
            locked.Settings.RuleVersion);

        var existingRun = await repository.FindRunAsync(worldId, runId, cancellationToken);
        if (existingRun?.Status == SimulationRunStatus.Completed)
        {
            return new(
                SimulationTickDisposition.AlreadyProcessed,
                runId,
                candidateStart,
                candidateEnd,
                locked.SimulationState.LastCompletedIntervalEnd,
                locked.SimulationState.NextDueAt,
                locked.World.CurrentWorldTime,
                []);
        }

        var deltaTicks = CalculateWorldTimeDeltaTicks(locked.Settings.TimeScale);
        var resultingWorldTime = locked.World.CurrentWorldTime.AddTicks(deltaTicks);
        var localWorldTime = worldTimeProjector.ToLocalWorldTime(
            resultingWorldTime,
            locked.Settings.DisplayTimeZoneId);
        var seed = DeterministicSimulationIdentity.CreateRunSeed(
            locked.World.Seed,
            locked.Settings.RuleVersion,
            candidateStart,
            candidateEnd,
            locked.SimulationState.DeterministicSequence);
        var run = new SimulationRun(
            runId,
            worldId,
            SimulationRunType.ActiveTick,
            candidateStart,
            candidateEnd,
            locked.Settings.TimeScale,
            seed,
            locked.Settings.RuleVersion,
            observedAtUtc,
            RunIdempotencyKey(worldId, candidateStart, candidateEnd, locked.Settings.RuleVersion));
        var context = new SimulationRuleContext(
            worldId,
            runId,
            candidateStart,
            candidateEnd,
            resultingWorldTime,
            localWorldTime,
            locked.Settings.TimeScale,
            locked.Settings.RuleVersion,
            seed,
            SimulationInputAvailability.M08,
            randomProvider);
        var evaluations = _rules.Select(rule =>
        {
            var decision = rule.Evaluate(context);
            return new SimulationRuleEvaluation(
                DeterministicSimulationIdentity.CreateEvaluationId(worldId, runId, rule.Code),
                worldId,
                runId,
                rule.Code,
                decision.Outcome,
                decision.ReasonCode,
                locked.Settings.RuleVersion,
                candidateEnd);
        }).ToArray();
        EnsureRequiredRuleSet(evaluations);

        var checkpoint = new SimulationRunCheckpoint(
            DeterministicSimulationIdentity.CreateCheckpointId(worldId, runId, 0),
            worldId,
            runId,
            candidateStart,
            candidateEnd,
            0,
            candidateEnd,
            $"m08:checkpoint:{runId:N}:0");

        locked.SimulationState.CompleteInterval(candidateStart, candidateEnd, observedAtUtc);
        locked.World.AdvanceSimulation(candidateEnd, deltaTicks, observedAtUtc);
        run.Complete(observedAtUtc);
        repository.AddRun(run, checkpoint, evaluations);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new(
            SimulationTickDisposition.Completed,
            runId,
            candidateStart,
            candidateEnd,
            candidateEnd,
            candidateEnd.Add(WorldSimulationState.ActiveIntervalDuration),
            resultingWorldTime,
            evaluations.Select(evaluation => new SimulationEvaluationResult(
                evaluation.Id,
                evaluation.RuleCode,
                evaluation.Outcome.ToString(),
                evaluation.ReasonCode)).ToArray());
    }

    public static long CalculateWorldTimeDeltaTicks(decimal effectiveTimeScale)
    {
        if (effectiveTimeScale is < WorldSettings.MinimumTimeScale or > WorldSettings.MaximumTimeScale
            || decimal.Round(effectiveTimeScale, 4, MidpointRounding.ToEven) != effectiveTimeScale)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveTimeScale));
        }

        var scaledTicks = WorldSimulationState.ActiveIntervalDuration.Ticks * effectiveTimeScale;
        if (scaledTicks != decimal.Truncate(scaledTicks))
        {
            throw new InvalidOperationException("The scaled world-time delta must resolve to exact ticks.");
        }

        return decimal.ToInt64(scaledTicks);
    }

    private static string RunIdempotencyKey(
        Guid worldId,
        DateTimeOffset start,
        DateTimeOffset end,
        int ruleVersion) => FormattableString.Invariant(
            $"m08:active:{worldId:N}:{start:O}:{end:O}:v{ruleVersion}");

    private static void EnsureRequiredRuleSet(IReadOnlyCollection<SimulationRuleEvaluation> evaluations)
    {
        var codes = evaluations.Select(evaluation => evaluation.RuleCode).ToArray();
        var required = new[]
        {
            SimulationRuleCodes.Act,
            SimulationRuleCodes.Post,
            SimulationRuleCodes.Reply,
            SimulationRuleCodes.React,
            SimulationRuleCodes.Follow,
        };
        if (codes.Length != required.Length
            || !codes.Order(StringComparer.Ordinal).SequenceEqual(required.Order(StringComparer.Ordinal)))
        {
            throw new InvalidOperationException("M08 requires exactly one evaluation for each registered rule.");
        }
    }

    private static SimulationTickResult FromSnapshot(
        SimulationTickDisposition disposition,
        SimulationWorldSnapshot snapshot) => new(
            disposition,
            null,
            null,
            null,
            snapshot.LastCompletedIntervalEnd,
            snapshot.NextDueAt,
            snapshot.CurrentWorldTime,
            []);

    private static SimulationTickResult Unavailable(SimulationWorldSnapshot? snapshot) => new(
        SimulationTickDisposition.WorldUnavailable,
        null,
        null,
        null,
        snapshot?.LastCompletedIntervalEnd,
        snapshot?.NextDueAt ?? default,
        snapshot?.CurrentWorldTime ?? default,
        []);

    private static SimulationTickResult Inconsistent(
        SimulationWorldSnapshot snapshot,
        string errorCode) => new(
            SimulationTickDisposition.InconsistentState,
            null,
            null,
            null,
            snapshot.LastCompletedIntervalEnd,
            snapshot.NextDueAt,
            snapshot.CurrentWorldTime,
            [],
            errorCode);
}

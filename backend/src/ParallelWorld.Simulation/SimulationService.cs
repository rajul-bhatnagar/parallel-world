using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Memory;
using ParallelWorld.Application.Relationships;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Simulation;

public sealed class SimulationService(
    ISimulationRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IWorldTimeProjector worldTimeProjector,
    IRelationshipService relationshipService,
    IRomanceService romanceService,
    IMemoryService memoryService,
    IDeterministicRandomProvider randomProvider,
    IEnumerable<ISimulationRule> rules) : ISimulationService
{
    public const int CatchUpMaximumElapsedDays = 30;
    public const int CatchUpDetailedDays = 7;
    public const int CatchUpDetailedBucketHours = 6;
    public const int CatchUpMaximumBucketsPerInvocation = 8;
    public const int CatchUpMaximumSummaryItems = 20;
    private static readonly TimeSpan CatchUpLeaseDuration = TimeSpan.FromMinutes(2);

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

        var openCatchUp = await repository.FindOpenCatchUpRunAsync(
            worldId, cancellationToken);
        if (openCatchUp is not null)
        {
            return new(
                SimulationTickDisposition.AlreadyProcessed,
                openCatchUp.Id,
                openCatchUp.IntervalStart,
                openCatchUp.IntervalEnd,
                locked.SimulationState.LastCompletedIntervalEnd,
                locked.SimulationState.NextDueAt,
                locked.World.CurrentWorldTime,
                []);
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
        var gameDate = DateOnly.FromDateTime(localWorldTime.DateTime);
        var followCandidates = await repository.ListFollowCandidatesAsync(
            worldId,
            resultingWorldTime,
            gameDate,
            cancellationToken);
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
            SimulationInputAvailability.M10,
            randomProvider,
            followCandidates);
        FollowRuleAction? followAction = null;
        var evaluations = _rules.Select(rule =>
        {
            var decision = rule.Evaluate(context);
            followAction ??= decision.FollowAction;
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
        Guid? autonomousFollowEventId = null;
        if (followAction is not null)
        {
            autonomousFollowEventId = repository.AddAutonomousFollow(worldId, runId, candidateEnd, resultingWorldTime, gameDate, locked.Settings.RuleVersion, followAction);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (autonomousFollowEventId is Guid relationshipGameplayEventId && followAction?.RelationshipEventType is string relationshipEventType)
        {
            await relationshipService.ApplyAsync(new(worldId, followAction.SourceActorId, followAction.TargetActorId,
                relationshipGameplayEventId, relationshipEventType, false, relationshipEventType == "Re-follow",
                gameDate, candidateEnd, locked.Settings.RuleVersion,
                $"m10:relationship:{relationshipGameplayEventId:N}"), cancellationToken);
        }
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

    public async Task<CatchUpResult> ProcessCatchUpAsync(
        Guid worldId,
        CancellationToken cancellationToken = default)
    {
        var observedAtUtc = timeProvider.GetUtcNow();
        var snapshot = await repository.FindSnapshotAsync(worldId, cancellationToken);
        if (snapshot is null || snapshot.Status != WorldStatus.Active)
        {
            return CatchUpUnavailable(snapshot);
        }

        var candidateStart = snapshot.LastCompletedIntervalEnd ?? snapshot.CreatedAtUtc;
        if (snapshot.NextDueAt != candidateStart.Add(WorldSimulationState.ActiveIntervalDuration))
        {
            return CatchUpInconsistent(snapshot, "simulation_due_projection_mismatch");
        }
        if (observedAtUtc < snapshot.NextDueAt)
        {
            return CatchUpFromSnapshot(CatchUpDisposition.NotDue, snapshot);
        }

        var leaseOwnerId = Guid.NewGuid();
        SimulationRun claimedRun;
        CatchUpSummary claimedSummary;
        await using (var claimTransaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken))
        {
            var locked = await repository.LockWorldAsync(worldId, cancellationToken);
            if (locked is null || locked.World.Status != WorldStatus.Active)
            {
                return CatchUpUnavailable(snapshot);
            }

            var authoritativeStart = locked.SimulationState.LastCompletedIntervalEnd
                ?? locked.World.CreatedAt;
            if (locked.SimulationState.NextDueAt
                != authoritativeStart.Add(WorldSimulationState.ActiveIntervalDuration))
            {
                return CatchUpInconsistent(snapshot, "simulation_due_projection_mismatch");
            }

            var open = await repository.FindOpenCatchUpRunAsync(worldId, cancellationToken);
            if (open is not null)
            {
                if (open.ProcessedThrough != authoritativeStart)
                {
                    return CatchUpInconsistent(snapshot, "catchup_checkpoint_cursor_mismatch");
                }
                claimedRun = open;
                claimedSummary = await repository.FindCatchUpSummaryAsync(
                    worldId, open.Id, cancellationToken)
                    ?? throw new InvalidOperationException("A CatchUp run requires its summary.");
            }
            else
            {
                var eligibleIntervals = EligibleIntervalCount(authoritativeStart, observedAtUtc);
                if (eligibleIntervals == 0)
                {
                    return new(
                        CatchUpDisposition.NotDue, null, "not_due", 0, 0, 0, 0,
                        authoritativeStart, locked.SimulationState.NextDueAt,
                        locked.World.CurrentWorldTime, null);
                }

                var requestedIntervals = Math.Min(
                    eligibleIntervals,
                    CatchUpMaximumElapsedDays * 24 * 4);
                var rangeEnd = authoritativeStart.AddTicks(
                    requestedIntervals * WorldSimulationState.ActiveIntervalDuration.Ticks);
                var runId = DeterministicSimulationIdentity.CreateCatchUpRunId(
                    worldId, authoritativeStart, rangeEnd, locked.Settings.RuleVersion);
                var seed = DeterministicSimulationIdentity.CreateRunSeed(
                    locked.World.Seed,
                    locked.Settings.RuleVersion,
                    authoritativeStart,
                    rangeEnd,
                    locked.SimulationState.DeterministicSequence);
                claimedRun = new(
                    runId, worldId, SimulationRunType.CatchUp, authoritativeStart,
                    rangeEnd, locked.Settings.TimeScale, seed,
                    locked.Settings.RuleVersion, observedAtUtc,
                    CatchUpRunIdempotencyKey(
                        worldId, authoritativeStart, rangeEnd, locked.Settings.RuleVersion));
                claimedSummary = new(
                    DeterministicSimulationIdentity.CreateCatchUpSummaryId(worldId, runId),
                    worldId, runId, locked.World.CurrentWorldTime, observedAtUtc,
                    $"m15:summary:{runId:N}");
                repository.AddCatchUpRun(claimedRun, claimedSummary);
            }

            if (!claimedRun.TryClaim(leaseOwnerId, observedAtUtc, CatchUpLeaseDuration))
            {
                await claimTransaction.CommitAsync(cancellationToken);
                return await CatchUpResultAsync(
                    CatchUpDisposition.Processing, claimedRun, locked, 0, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await claimTransaction.CommitAsync(cancellationToken);
        }

        var processedBuckets = 0;
        try
        {
            while (processedBuckets < CatchUpMaximumBucketsPerInvocation)
            {
                unitOfWork.ClearTrackedChanges();
                await using var bucketTransaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.ReadCommitted,
                    cancellationToken);
                var locked = await repository.LockWorldAsync(worldId, cancellationToken)
                    ?? throw new InvalidOperationException("The CatchUp world disappeared.");
                var run = await repository.LockRunAsync(worldId, claimedRun.Id, cancellationToken)
                    ?? throw new InvalidOperationException("The CatchUp run disappeared.");
                var summary = await repository.FindCatchUpSummaryAsync(
                    worldId, run.Id, cancellationToken)
                    ?? throw new InvalidOperationException("The CatchUp summary disappeared.");

                if (run.ProcessedThrough == run.IntervalEnd)
                {
                    run.CompleteCatchUp(leaseOwnerId, timeProvider.GetUtcNow());
                    var count = await repository.CountCatchUpSummaryItemsAsync(
                        worldId, summary.Id, cancellationToken);
                    summary.Update(
                        locked.World.CurrentWorldTime,
                        CatchUpSummaryStatus.Completed,
                        timeProvider.GetUtcNow(),
                        count);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await bucketTransaction.CommitAsync(cancellationToken);
                    claimedRun = run;
                    break;
                }

                run.RenewLease(leaseOwnerId, timeProvider.GetUtcNow(), CatchUpLeaseDuration);
                var buckets = BuildCatchUpBuckets(run.IntervalStart, run.IntervalEnd);
                var bucketOrdinal = buckets.FindIndex(bucket => bucket.Start == run.ProcessedThrough);
                if (bucketOrdinal < 0)
                {
                    throw new InvalidOperationException("The CatchUp checkpoint is not on a bucket boundary.");
                }
                var bucket = buckets[bucketOrdinal];
                var checkpoint = SimulationRunCheckpoint.Pending(
                    DeterministicSimulationIdentity.CreateCheckpointId(worldId, run.Id, bucketOrdinal),
                    worldId, run.Id, bucket.Start, bucket.End, bucketOrdinal,
                    $"m15:checkpoint:{run.Id:N}:{bucketOrdinal}");
                repository.AddCatchUpCheckpoint(checkpoint);

                var intervalCount = checked((int)((bucket.End - bucket.Start).Ticks
                    / WorldSimulationState.ActiveIntervalDuration.Ticks));
                var worldTimeDeltaTicks = CalculateWorldTimeDeltaTicks(run.EffectiveTimeScale, intervalCount);
                var resultingWorldTime = locked.World.CurrentWorldTime.AddTicks(worldTimeDeltaTicks);
                var localWorldTime = worldTimeProjector.ToLocalWorldTime(
                    resultingWorldTime, locked.Settings.DisplayTimeZoneId);
                var gameDate = DateOnly.FromDateTime(localWorldTime.DateTime);
                _ = await romanceService.ExpireDueAsync(
                    worldId, resultingWorldTime, timeProvider.GetUtcNow(), cancellationToken);
                _ = await memoryService.ExpireDuePromisesAsync(
                    worldId, resultingWorldTime, cancellationToken);
                var bucketSeed = DeterministicSimulationIdentity.CreateBucketSeed(
                    run.Seed, run.Id, bucketOrdinal);
                var followCandidates = await repository.ListFollowCandidatesAsync(
                    worldId, resultingWorldTime, gameDate, cancellationToken);
                var followRule = _rules.Single(rule => rule.Code == SimulationRuleCodes.Follow);
                var decision = followRule.Evaluate(new(
                    worldId, run.Id, bucket.Start, bucket.End, resultingWorldTime,
                    localWorldTime, run.EffectiveTimeScale, run.RuleVersion,
                    bucketSeed, SimulationInputAvailability.M10, randomProvider,
                    followCandidates));

                Guid? relationshipGameplayEventId = null;
                var summaryItemAdded = false;
                if (decision.FollowAction is FollowRuleAction followAction)
                {
                    var actionOrdinal = checked((bucketOrdinal * 100_000)
                        + followAction.StableOrdinal);
                    var gameplayEventId = DeterministicSimulationIdentity.CreateCatchUpEventId(
                        worldId, run.Id, bucketOrdinal, followAction.SourceActorId,
                        followAction.TargetActorId, followAction.DesiredFollowing);
                    relationshipGameplayEventId = repository.AddCatchUpAutonomousFollow(
                        worldId, run.Id, bucketOrdinal, gameplayEventId,
                        DeterministicSimulationIdentity.CreateCatchUpFollowId(
                            worldId, run.Id, bucketOrdinal, followAction.SourceActorId,
                            followAction.TargetActorId),
                        DeterministicSimulationIdentity.CreateActionId(
                            worldId, run.Id, actionOrdinal), bucket.End,
                        resultingWorldTime, gameDate, run.RuleVersion, followAction);
                    var itemCount = await repository.CountCatchUpSummaryItemsAsync(
                        worldId, summary.Id, cancellationToken);
                    var hasFamilyItem = await repository.HasCatchUpSummaryItemAsync(
                        worldId, summary.Id, "follow", gameDate, cancellationToken);
                    if (itemCount < CatchUpMaximumSummaryItems && !hasFamilyItem)
                    {
                        var wording = followAction.DesiredFollowing
                            ? "A character followed someone in your world."
                            : "A character unfollowed someone in your world.";
                        repository.AddCatchUpSummaryItem(new(
                            DeterministicSimulationIdentity.CreateCatchUpSummaryItemId(
                                worldId, run.Id, itemCount, relationshipGameplayEventId.Value),
                            worldId, summary.Id, relationshipGameplayEventId.Value,
                            "follow", itemCount,
                            followAction.DesiredFollowing ? "follow_started" : "follow_ended",
                            followAction.SourceActorId, followAction.TargetActorId,
                            gameDate, wording, bucket.End));
                        summaryItemAdded = true;
                    }
                }

                locked.SimulationState.CompleteCatchUpRange(
                    bucket.Start, bucket.End, timeProvider.GetUtcNow());
                locked.World.AdvanceSimulation(
                    bucket.End, worldTimeDeltaTicks, timeProvider.GetUtcNow());
                run.AdvanceCatchUp(leaseOwnerId, bucket.Start, bucket.End);
                checkpoint.Complete(timeProvider.GetUtcNow());
                var committedItemCount = await repository.CountCatchUpSummaryItemsAsync(
                    worldId, summary.Id, cancellationToken)
                    + (summaryItemAdded ? 1 : 0);
                summary.Update(
                    resultingWorldTime,
                    run.ProcessedThrough == run.IntervalEnd
                        ? CatchUpSummaryStatus.Completed
                        : CatchUpSummaryStatus.Processing,
                    timeProvider.GetUtcNow(),
                    committedItemCount);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                if (relationshipGameplayEventId is Guid eventId
                    && decision.FollowAction?.RelationshipEventType is string relationshipEventType)
                {
                    await relationshipService.ApplyAsync(new(
                        worldId,
                        decision.FollowAction.SourceActorId,
                        decision.FollowAction.TargetActorId,
                        eventId,
                        relationshipEventType,
                        false,
                        relationshipEventType == "Re-follow",
                        gameDate,
                        bucket.End,
                        run.RuleVersion,
                        $"m15:relationship:{eventId:N}"), cancellationToken);
                }

                await bucketTransaction.CommitAsync(cancellationToken);
                claimedRun = run;
                processedBuckets++;
            }

            unitOfWork.ClearTrackedChanges();
            await using var finishTransaction = await unitOfWork.BeginTransactionAsync(
                ApplicationIsolationLevel.ReadCommitted,
                cancellationToken);
            var finalWorld = await repository.LockWorldAsync(worldId, cancellationToken)
                ?? throw new InvalidOperationException("The CatchUp world disappeared.");
            var finalRun = await repository.LockRunAsync(worldId, claimedRun.Id, cancellationToken)
                ?? throw new InvalidOperationException("The CatchUp run disappeared.");
            var finalSummary = await repository.FindCatchUpSummaryAsync(
                worldId, finalRun.Id, cancellationToken)
                ?? throw new InvalidOperationException("The CatchUp summary disappeared.");
            var finalCount = await repository.CountCatchUpSummaryItemsAsync(
                worldId, finalSummary.Id, cancellationToken);
            CatchUpDisposition disposition;
            if (finalRun.ProcessedThrough == finalRun.IntervalEnd)
            {
                if (finalRun.Status != SimulationRunStatus.Completed)
                {
                    finalRun.CompleteCatchUp(leaseOwnerId, timeProvider.GetUtcNow());
                }
                finalSummary.Update(
                    finalWorld.World.CurrentWorldTime,
                    CatchUpSummaryStatus.Completed,
                    timeProvider.GetUtcNow(), finalCount);
                disposition = CatchUpDisposition.Completed;
            }
            else
            {
                finalRun.MarkPartial(leaseOwnerId, timeProvider.GetUtcNow());
                finalSummary.Update(
                    finalWorld.World.CurrentWorldTime,
                    CatchUpSummaryStatus.Partial,
                    timeProvider.GetUtcNow(), finalCount);
                disposition = CatchUpDisposition.Partial;
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await finishTransaction.CommitAsync(cancellationToken);
            return await CatchUpResultAsync(
                disposition, finalRun, finalWorld, processedBuckets, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            await MarkCatchUpFailedAsync(worldId, claimedRun.Id, leaseOwnerId, cancellationToken);
            throw;
        }
    }

    public Task<CatchUpSummaryView?> GetLatestCatchUpSummaryAsync(
        Guid worldId,
        CancellationToken cancellationToken = default) =>
        repository.GetLatestCatchUpSummaryAsync(worldId, cancellationToken);

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

    public static long CalculateWorldTimeDeltaTicks(
        decimal effectiveTimeScale,
        int intervalCount) => checked(
            CalculateWorldTimeDeltaTicks(effectiveTimeScale) * intervalCount);

    public static List<CatchUpBucket> BuildCatchUpBuckets(
        DateTimeOffset start,
        DateTimeOffset end)
    {
        if (start.Offset != TimeSpan.Zero || end.Offset != TimeSpan.Zero || end <= start)
            throw new ArgumentException("CatchUp boundaries must be increasing UTC timestamps.");
        if ((end - start).Ticks % WorldSimulationState.ActiveIntervalDuration.Ticks != 0)
            throw new ArgumentException("CatchUp boundaries must align to complete 15-minute intervals.");

        var buckets = new List<CatchUpBucket>();
        var detailedStart = end.AddDays(-CatchUpDetailedDays);
        if (detailedStart < start) detailedStart = start;
        var cursor = start;
        var ordinal = 0;
        while (cursor < detailedStart)
        {
            var bucketEnd = cursor.AddDays(1);
            if (bucketEnd > detailedStart) bucketEnd = detailedStart;
            buckets.Add(new(cursor, bucketEnd, ordinal++, CatchUpBucketKind.Daily));
            cursor = bucketEnd;
        }
        while (cursor < end)
        {
            var bucketEnd = cursor.AddHours(CatchUpDetailedBucketHours);
            if (bucketEnd > end) bucketEnd = end;
            buckets.Add(new(cursor, bucketEnd, ordinal++, CatchUpBucketKind.Detailed));
            cursor = bucketEnd;
        }
        return buckets;
    }

    private static string RunIdempotencyKey(
        Guid worldId,
        DateTimeOffset start,
        DateTimeOffset end,
        int ruleVersion) => FormattableString.Invariant(
            $"m08:active:{worldId:N}:{start:O}:{end:O}:v{ruleVersion}");

    private static string CatchUpRunIdempotencyKey(
        Guid worldId,
        DateTimeOffset start,
        DateTimeOffset end,
        int ruleVersion) => FormattableString.Invariant(
            $"m15:catchup:{worldId:N}:{start:O}:{end:O}:v{ruleVersion}");

    private async Task MarkCatchUpFailedAsync(
        Guid worldId,
        Guid runId,
        Guid leaseOwnerId,
        CancellationToken cancellationToken)
    {
        unitOfWork.ClearTrackedChanges();
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken);
        var run = await repository.LockRunAsync(worldId, runId, cancellationToken);
        if (run?.LeaseOwnerId == leaseOwnerId)
        {
            run.FailCatchUpRetryable(
                leaseOwnerId, "catchup_bucket_failed", timeProvider.GetUtcNow());
            var summary = await repository.FindCatchUpSummaryAsync(
                worldId, runId, cancellationToken);
            if (summary is not null)
            {
                var count = await repository.CountCatchUpSummaryItemsAsync(
                    worldId, summary.Id, cancellationToken);
                summary.Update(summary.ToGameTime, CatchUpSummaryStatus.FailedRetryable,
                    timeProvider.GetUtcNow(), count);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task<CatchUpResult> CatchUpResultAsync(
        CatchUpDisposition disposition,
        SimulationRun run,
        LockedSimulationWorld locked,
        int processedBuckets,
        CancellationToken cancellationToken)
    {
        var summary = await repository.GetLatestCatchUpSummaryAsync(
            run.WorldId, cancellationToken);
        return new(
            disposition, run.Id, run.Status.ToString().ToLowerInvariant(),
            run.RequestedIntervalCount, run.ProcessedIntervalCount,
            run.RemainingIntervalCount, processedBuckets, run.ProcessedThrough,
            locked.SimulationState.NextDueAt, locked.World.CurrentWorldTime,
            summary, run.ErrorCode);
    }

    private static int EligibleIntervalCount(
        DateTimeOffset start,
        DateTimeOffset observedAtUtc)
    {
        if (observedAtUtc <= start) return 0;
        return checked((int)((observedAtUtc - start).Ticks
            / WorldSimulationState.ActiveIntervalDuration.Ticks));
    }

    private static CatchUpResult CatchUpFromSnapshot(
        CatchUpDisposition disposition,
        SimulationWorldSnapshot snapshot) => new(
            disposition, null, disposition.ToString().ToLowerInvariant(),
            0, 0, 0, 0, snapshot.LastCompletedIntervalEnd,
            snapshot.NextDueAt, snapshot.CurrentWorldTime, null);

    private static CatchUpResult CatchUpUnavailable(
        SimulationWorldSnapshot? snapshot) => new(
            CatchUpDisposition.WorldUnavailable, null, "world_unavailable",
            0, 0, 0, 0, snapshot?.LastCompletedIntervalEnd,
            snapshot?.NextDueAt ?? default,
            snapshot?.CurrentWorldTime ?? default, null);

    private static CatchUpResult CatchUpInconsistent(
        SimulationWorldSnapshot snapshot,
        string errorCode) => new(
            CatchUpDisposition.InconsistentState, null, "inconsistent_state",
            0, 0, 0, 0, snapshot.LastCompletedIntervalEnd,
            snapshot.NextDueAt, snapshot.CurrentWorldTime, null, errorCode);

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

public sealed record CatchUpBucket(
    DateTimeOffset Start,
    DateTimeOffset End,
    int Ordinal,
    CatchUpBucketKind Kind);

public enum CatchUpBucketKind
{
    Daily,
    Detailed,
}

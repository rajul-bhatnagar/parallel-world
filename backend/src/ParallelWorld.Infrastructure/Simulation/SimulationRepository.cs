using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Simulation;

internal sealed class SimulationRepository(ParallelWorldDbContext dbContext) : ISimulationRepository
{
    public Task<SimulationWorldSnapshot?> FindSnapshotAsync(
        Guid worldId,
        CancellationToken cancellationToken) =>
        (from world in dbContext.GameWorlds.AsNoTracking()
         join settings in dbContext.WorldSettings.AsNoTracking() on world.Id equals settings.WorldId
         join state in dbContext.WorldSimulationStates.AsNoTracking() on world.Id equals state.WorldId
         where world.Id == worldId
         select new SimulationWorldSnapshot(
             world.Id,
             world.Status,
             world.CreatedAt,
             world.CurrentWorldTime,
             world.LastSimulatedAt,
             state.LastCompletedIntervalEnd,
             state.NextDueAt,
             settings.TimeScale,
             settings.RuleVersion,
             settings.DisplayTimeZoneId))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<LockedSimulationWorld?> LockWorldAsync(
        Guid worldId,
        CancellationToken cancellationToken)
    {
        var state = await dbContext.WorldSimulationStates.FromSqlInterpolated(
                $"SELECT * FROM world_simulation_states WHERE world_id = {worldId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (state is null)
        {
            return null;
        }

        var world = await dbContext.GameWorlds.FromSqlInterpolated(
                $"SELECT * FROM game_worlds WHERE id = {worldId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        var settings = await dbContext.WorldSettings.FromSqlInterpolated(
                $"SELECT * FROM world_settings WHERE world_id = {worldId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        return world is null || settings is null
            ? null
            : new LockedSimulationWorld(world, settings, state);
    }

    public Task<SimulationRun?> FindRunAsync(
        Guid worldId,
        Guid runId,
        CancellationToken cancellationToken) =>
        dbContext.SimulationRuns.SingleOrDefaultAsync(
            run => run.WorldId == worldId && run.Id == runId,
            cancellationToken);

    public Task<SimulationRun?> FindOpenCatchUpRunAsync(
        Guid worldId,
        CancellationToken cancellationToken) =>
        dbContext.SimulationRuns
            .Where(run => run.WorldId == worldId
                && run.RunType == SimulationRunType.CatchUp
                && (run.Status == SimulationRunStatus.Running
                    || run.Status == SimulationRunStatus.Partial
                    || run.Status == SimulationRunStatus.FailedRetryable))
            .OrderBy(run => run.IntervalStart)
            .ThenBy(run => run.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<SimulationRun?> LockRunAsync(
        Guid worldId,
        Guid runId,
        CancellationToken cancellationToken) =>
        dbContext.SimulationRuns.FromSqlInterpolated(
                $"SELECT * FROM simulation_runs WHERE world_id = {worldId} AND id = {runId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListRecoverableCatchUpWorldIdsAsync(
        DateTimeOffset observedAtUtc,
        int limit,
        CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(limit));
        return await dbContext.SimulationRuns.AsNoTracking()
            .Where(run => run.RunType == SimulationRunType.CatchUp
                && (run.Status == SimulationRunStatus.Partial
                    || run.Status == SimulationRunStatus.FailedRetryable
                    || (run.Status == SimulationRunStatus.Running
                        && run.LeaseExpiresAt <= observedAtUtc)))
            .GroupBy(run => run.WorldId)
            .Select(group => new
            {
                WorldId = group.Key,
                OldestIntervalStart = group.Min(run => run.IntervalStart),
            })
            .OrderBy(candidate => candidate.OldestIntervalStart)
            .ThenBy(candidate => candidate.WorldId)
            .Select(candidate => candidate.WorldId)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }

    public Task<CatchUpSummary?> FindCatchUpSummaryAsync(
        Guid worldId,
        Guid runId,
        CancellationToken cancellationToken) =>
        dbContext.CatchUpSummaries.SingleOrDefaultAsync(
            summary => summary.WorldId == worldId && summary.SimulationRunId == runId,
            cancellationToken);

    public async Task<CatchUpSummaryView?> GetLatestCatchUpSummaryAsync(
        Guid worldId,
        CancellationToken cancellationToken)
    {
        var summary = await dbContext.CatchUpSummaries.AsNoTracking()
            .Where(x => x.WorldId == worldId)
            .OrderByDescending(x => x.GeneratedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (summary is null) return null;
        var items = await dbContext.CatchUpSummaryItems.AsNoTracking()
            .Where(x => x.WorldId == worldId && x.CatchUpSummaryId == summary.Id)
            .OrderBy(x => x.StableOrdinal)
            .Select(x => new CatchUpSummaryItemView(
                x.Id, x.ItemType, x.StableOrdinal, x.FactCode, x.ActorId,
                x.TargetActorId, x.GameDate, x.Wording, x.CreatedAt))
            .ToArrayAsync(cancellationToken);
        return new(summary.Id, summary.SimulationRunId, summary.Status.ToString().ToLowerInvariant(),
            summary.FromGameTime, summary.ToGameTime, summary.GeneratedAt, summary.Text, items);
    }

    public Task<int> CountCatchUpSummaryItemsAsync(
        Guid worldId,
        Guid summaryId,
        CancellationToken cancellationToken) =>
        dbContext.CatchUpSummaryItems.CountAsync(
            x => x.WorldId == worldId && x.CatchUpSummaryId == summaryId,
            cancellationToken);

    public Task<bool> HasCatchUpSummaryItemAsync(
        Guid worldId,
        Guid summaryId,
        string itemType,
        DateOnly gameDate,
        CancellationToken cancellationToken) =>
        dbContext.CatchUpSummaryItems.AnyAsync(
            x => x.WorldId == worldId && x.CatchUpSummaryId == summaryId
                && x.ItemType == itemType && x.GameDate == gameDate,
            cancellationToken);

    public async Task<IReadOnlyList<FollowRuleCandidate>> ListFollowCandidatesAsync(Guid worldId, DateTimeOffset currentGameTime, DateOnly currentGameDate, CancellationToken cancellationToken)
    {
        var followCutoff = currentGameTime.AddDays(-7);
        var refollowCutoff = currentGameTime.AddDays(-14);
        var negativeEventCutoff = currentGameDate.AddDays(-7);
        var actors = await (from actor in dbContext.Actors.AsNoTracking()
                            join character in dbContext.Characters.AsNoTracking()
                                on new { actor.WorldId, actor.CharacterId }
                                equals new { character.WorldId, CharacterId = (Guid?)character.Id }
                            where actor.WorldId == worldId
                                && actor.ActorType == ActorType.Character
                                && actor.Status == ActorStatus.Active
                                && actor.CharacterId != null
                            orderby actor.Id
                            select new { ActorId = actor.Id, CharacterId = character.Id, character.Reputation })
            .ToListAsync(cancellationToken);
        var actorIds = actors.Select(x => x.ActorId).ToArray();
        var characterIds = actors.Select(x => x.CharacterId).ToArray();
        var relationships = (await dbContext.Relationships.AsNoTracking()
                .Where(x => x.WorldId == worldId && actorIds.Contains(x.SourceActorId) && actorIds.Contains(x.TargetActorId))
                .ToListAsync(cancellationToken))
            .ToDictionary(x => (x.SourceActorId, x.TargetActorId));
        var follows = await dbContext.Follows.AsNoTracking()
            .Where(x => x.WorldId == worldId && actorIds.Contains(x.FollowerActorId) && actorIds.Contains(x.FollowedActorId))
            .ToListAsync(cancellationToken);
        var followLookup = follows.ToLookup(x => (x.FollowerActorId, x.FollowedActorId));
        var dailyCounts = follows
            .GroupBy(x => x.FollowerActorId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(follow => (follow.StartedGameDate == currentGameDate ? 1 : 0)
                    + (follow.EndedGameDate == currentGameDate ? 1 : 0)));
        var qualifiedNegativeCounts = (await dbContext.RelationshipEvents.AsNoTracking()
                .Where(x => x.WorldId == worldId
                    && actorIds.Contains(x.SourceActorId)
                    && actorIds.Contains(x.TargetActorId)
                    && x.IsQualifiedNegative
                    && x.OccurredGameDate >= negativeEventCutoff)
                .Select(x => new { x.SourceActorId, x.TargetActorId })
                .ToListAsync(cancellationToken))
            .GroupBy(x => (x.SourceActorId, x.TargetActorId))
            .ToDictionary(x => x.Key, x => x.Count());
        var interests = (await dbContext.CharacterInterests.AsNoTracking()
                .Where(x => x.WorldId == worldId && characterIds.Contains(x.CharacterId))
                .Select(x => new { x.CharacterId, x.TopicId })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.CharacterId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.TopicId).ToArray());

        var candidates = new List<FollowRuleCandidate>();
        var stableOrdinal = 0;
        foreach (var source in actors)
        {
            foreach (var target in actors)
            {
                var ordinal = stableOrdinal++;
                if (source.ActorId == target.ActorId || dailyCounts.GetValueOrDefault(source.ActorId) >= 2)
                {
                    continue;
                }

                var history = followLookup[(source.ActorId, target.ActorId)].ToArray();
                var active = history.SingleOrDefault(x => x.EndedAt is null);
                if (active is not null && active.StartedGameTime > followCutoff)
                {
                    continue;
                }
                if (active is null && history.Length > 0
                    && history.Max(x => x.EndedGameTime) is DateTimeOffset lastEndedGameTime
                    && lastEndedGameTime > refollowCutoff)
                {
                    continue;
                }

                var values = relationships.GetValueOrDefault((source.ActorId, target.ActorId))?.Values
                    ?? RelationshipValues.Initial;
                candidates.Add(new(
                    source.ActorId,
                    target.ActorId,
                    values,
                    RelationshipMechanics.InterestOverlap(
                        interests.GetValueOrDefault(source.CharacterId) ?? [],
                        interests.GetValueOrDefault(target.CharacterId) ?? []),
                    target.Reputation,
                    active is not null,
                    history.Length > 0,
                    qualifiedNegativeCounts.GetValueOrDefault((source.ActorId, target.ActorId)),
                    ordinal));
            }
        }

        return candidates;
    }

    public Guid AddAutonomousFollow(Guid worldId, Guid runId, DateTimeOffset occurredAt, DateTimeOffset occurredGameTime, DateOnly occurredGameDate, int ruleVersion, FollowRuleAction action)
    {
        var key = $"m10:follow:{runId:N}:{action.SourceActorId:N}:{action.TargetActorId:N}";
        var eventId = Guid.NewGuid();
        dbContext.GameplayEvents.Add(new GameplayEvent(eventId, worldId, action.DesiredFollowing ? "followStarted" : "followEnded", action.SourceActorId, action.TargetActorId, occurredAt, 50, 0, "follow_01", ruleVersion, key, occurredAt));
        if (action.DesiredFollowing) dbContext.Follows.Add(new Follow(Guid.NewGuid(), worldId, action.SourceActorId, action.TargetActorId, occurredAt, occurredGameTime, occurredGameDate, eventId, key));
        else dbContext.Follows.Single(f => f.WorldId == worldId && f.FollowerActorId == action.SourceActorId && f.FollowedActorId == action.TargetActorId && f.EndedAt == null).End(occurredAt, occurredGameTime, occurredGameDate);
        var simulationAction = new SimulationAction(Guid.NewGuid(), worldId, runId, action.StableOrdinal, action.SourceActorId, action.DesiredFollowing ? "follow" : "unfollow", action.TargetActorId, null, null, null, null, $"follow_01_score_{action.Score}_roll_{action.Roll}", occurredAt, key);
        simulationAction.MarkExecuted(occurredAt); dbContext.SimulationActions.Add(simulationAction);
        return eventId;
    }

    public Guid AddCatchUpAutonomousFollow(
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
        FollowRuleAction action)
    {
        var actionOrdinal = checked((bucketOrdinal * 100_000) + action.StableOrdinal);
        var key = $"m15:follow:{runId:N}:{bucketOrdinal}:{action.SourceActorId:N}:{action.TargetActorId:N}";
        dbContext.GameplayEvents.Add(new GameplayEvent(gameplayEventId, worldId, action.DesiredFollowing ? "followStarted" : "followEnded", action.SourceActorId, action.TargetActorId, occurredAt, 50, 0, "follow_01", ruleVersion, key, occurredAt));
        if (action.DesiredFollowing)
            dbContext.Follows.Add(new Follow(followId, worldId, action.SourceActorId, action.TargetActorId, occurredAt, occurredGameTime, occurredGameDate, gameplayEventId, key));
        else
            dbContext.Follows.Single(f => f.WorldId == worldId && f.FollowerActorId == action.SourceActorId && f.FollowedActorId == action.TargetActorId && f.EndedAt == null).End(occurredAt, occurredGameTime, occurredGameDate);
        var simulationAction = new SimulationAction(simulationActionId, worldId, runId, actionOrdinal, action.SourceActorId, action.DesiredFollowing ? "follow" : "unfollow", action.TargetActorId, null, null, null, null, $"follow_01_score_{action.Score}_roll_{action.Roll}", occurredAt, key);
        simulationAction.MarkExecuted(occurredAt);
        dbContext.SimulationActions.Add(simulationAction);
        return gameplayEventId;
    }

    public void AddRun(
        SimulationRun run,
        SimulationRunCheckpoint checkpoint,
        IReadOnlyCollection<SimulationRuleEvaluation> evaluations)
    {
        dbContext.SimulationRuns.Add(run);
        dbContext.SimulationRunCheckpoints.Add(checkpoint);
        dbContext.SimulationRuleEvaluations.AddRange(evaluations);
    }

    public void AddCatchUpRun(SimulationRun run, CatchUpSummary summary)
    {
        dbContext.SimulationRuns.Add(run);
        dbContext.CatchUpSummaries.Add(summary);
    }

    public void AddCatchUpCheckpoint(SimulationRunCheckpoint checkpoint) =>
        dbContext.SimulationRunCheckpoints.Add(checkpoint);

    public void AddCatchUpSummaryItem(CatchUpSummaryItem item) =>
        dbContext.CatchUpSummaryItems.Add(item);
}

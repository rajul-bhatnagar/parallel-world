using System.Data;
using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Memory;
using ParallelWorld.Domain.Memory;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Memory;

internal sealed class MemoryRepository(
    ParallelWorldDbContext db,
    TimeProvider clock,
    IPersistenceFailureClassifier failures) : IMemoryRepository
{
    public async Task<MemoryCreationResult> CreateAsync(
        CreateMemoryCommand command,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var tx = await db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, ct);

                var owner = await db.Characters
                    .FromSqlInterpolated($"SELECT * FROM characters WHERE world_id = {command.WorldId} AND id = {command.OwnerCharacterId} FOR UPDATE")
                    .SingleOrDefaultAsync(ct);
                if (owner is null)
                {
                    return MemoryCreationResult.Invalid("memory_owner_not_available");
                }

                var prior = await db.MemoryCreationOutcomes.AsNoTracking().SingleOrDefaultAsync(
                    x => x.WorldId == command.WorldId
                        && x.OwnerCharacterId == command.OwnerCharacterId
                        && x.SourceType == command.SourceType
                        && x.SourceId == command.SourceId
                        && x.MemoryType == command.MemoryType,
                    ct);
                if (prior is not null)
                {
                    await tx.CommitAsync(ct);
                    return MapOutcome(prior, true, null);
                }

                var ownerActorId = await db.Actors.AsNoTracking()
                    .Where(x => x.WorldId == command.WorldId
                        && x.ActorType == ActorType.Character
                        && x.CharacterId == command.OwnerCharacterId)
                    .Select(x => (Guid?)x.Id)
                    .SingleOrDefaultAsync(ct);
                if (!ownerActorId.HasValue
                    || !await ValidSourceAsync(command, ownerActorId.Value, ct)
                    || !await ValidSubjectAndTopicAsync(command, ct)
                    || !await ValidPromiseActorsAsync(command, ct))
                {
                    return MemoryCreationResult.Invalid("memory_source_not_available");
                }

                var now = clock.GetUtcNow();
                Guid? evictedId = null;
                var activeCount = await db.CharacterMemories.CountAsync(
                    x => x.WorldId == command.WorldId
                        && x.OwnerCharacterId == command.OwnerCharacterId
                        && x.LifecycleStatus == MemoryLifecycleStatus.Active,
                    ct);
                if (activeCount >= MemoryMechanics.MaximumActiveMemories)
                {
                    var candidate = await db.CharacterMemories
                        .Where(x => x.WorldId == command.WorldId
                            && x.OwnerCharacterId == command.OwnerCharacterId
                            && x.LifecycleStatus == MemoryLifecycleStatus.Active
                            && x.MemoryType != MemoryType.Secret
                            && x.MemoryType != MemoryType.Promise)
                        .OrderBy(x => x.Importance)
                        .ThenBy(x => x.CreatedAtUtc)
                        .ThenBy(x => x.Id)
                        .FirstOrDefaultAsync(ct);
                    if (candidate is null)
                    {
                        var rejected = Outcome(
                            command,
                            MemoryCreationOutcomeType.Rejected,
                            MemoryMechanics.ProtectedCapacityReason,
                            null,
                            now);
                        db.MemoryCreationOutcomes.Add(rejected);
                        await db.SaveChangesAsync(ct);
                        await tx.CommitAsync(ct);
                        return MapOutcome(rejected, false, null);
                    }

                    candidate.Evict(now);
                    evictedId = candidate.Id;
                }

                var source = TypedSource(command);
                var memory = new CharacterMemory(
                    Guid.NewGuid(),
                    command.WorldId,
                    command.OwnerCharacterId,
                    command.MemoryType,
                    command.AuthorityType,
                    command.SubjectType,
                    command.SubjectActorId,
                    command.SubjectTopicId,
                    command.TopicId,
                    command.StructuredContent,
                    command.SourceType,
                    command.SourceId,
                    source.GameplayEventId,
                    source.MessageId,
                    now);
                db.CharacterMemories.Add(memory);

                if (command.MemoryType == MemoryType.Secret)
                {
                    var secret = new Secret(Guid.NewGuid(), command.WorldId, memory.Id, now);
                    db.Secrets.Add(secret);
                    db.SecretKnowers.Add(new SecretKnower(
                        command.WorldId, secret.Id, command.OwnerCharacterId, now));
                }
                else if (command.MemoryType == MemoryType.Promise)
                {
                    var details = command.Promise!;
                    db.Promises.Add(new Promise(
                        Guid.NewGuid(),
                        command.WorldId,
                        memory.Id,
                        details.PromiseType,
                        details.SourceActorId,
                        details.TargetActorId,
                        details.DueConditionType,
                        details.DueAtWorldTime,
                        details.DueGameplayEventId,
                        details.DueEventType,
                        now));
                }

                var created = Outcome(
                    command,
                    MemoryCreationOutcomeType.Created,
                    null,
                    memory.Id,
                    now);
                db.MemoryCreationOutcomes.Add(created);
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return MapOutcome(created, false, evictedId);
            }
            catch (Exception exception) when (failures.IsRetryableConcurrency(exception))
            {
                db.ClearTrackedChanges();
            }
        }

        return MemoryCreationResult.Invalid("memory_concurrency_conflict");
    }

    public async Task<MemoryRecallResult> RecallAsync(
        RecallMemoryCommand command,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var existing = await db.MemoryRecallRequests.AsNoTracking().SingleOrDefaultAsync(
                    x => x.WorldId == command.WorldId
                        && x.CharacterId == command.OwnerCharacterId
                        && x.IdempotencyKey == command.IdempotencyKey,
                    ct);
                if (existing is not null)
                {
                    if (!SameRecall(existing, command))
                    {
                        return MemoryRecallResult.Invalid("memory_recall_key_reused");
                    }

                    return await LoadRecallAsync(existing.Id, true, ct);
                }

                var ownerActorId = await (from character in db.Characters.AsNoTracking()
                                          join actor in db.Actors.AsNoTracking()
                                              on new { character.WorldId, CharacterId = (Guid?)character.Id }
                                              equals new { actor.WorldId, actor.CharacterId }
                                          where character.WorldId == command.WorldId
                                              && character.Id == command.OwnerCharacterId
                                              && actor.ActorType == ActorType.Character
                                          select (Guid?)actor.Id).SingleOrDefaultAsync(ct);
                if (!ownerActorId.HasValue
                    || !await ValidRecallSubjectAndTopicAsync(command, ct))
                {
                    return MemoryRecallResult.Invalid("memory_recall_not_available");
                }

                var candidates = await db.CharacterMemories.AsNoTracking()
                    .Where(x => x.WorldId == command.WorldId
                        && x.OwnerCharacterId == command.OwnerCharacterId
                        && x.LifecycleStatus == MemoryLifecycleStatus.Active
                        && x.Visibility == MemoryVisibility.CharacterPrivate
                        && (command.Purpose != MemoryRecallPurpose.MessageWording
                            || x.MemoryType != MemoryType.Secret))
                    .ToListAsync(ct);
                var actorIds = candidates.Where(x => x.SubjectActorId.HasValue)
                    .Select(x => x.SubjectActorId!.Value).Distinct().ToArray();
                var relationships = await db.Relationships.AsNoTracking()
                    .Where(x => x.WorldId == command.WorldId
                        && x.SourceActorId == ownerActorId.Value
                        && actorIds.Contains(x.TargetActorId))
                    .ToDictionaryAsync(x => x.TargetActorId, x => x.Values, ct);

                var ranked = candidates.Select(memory =>
                    {
                        var subjectMatch = SubjectMatch(memory, command);
                        var topicMatch = command.TopicId is not null
                            && memory.TopicId is not null
                            && string.Equals(command.TopicId, memory.TopicId, StringComparison.Ordinal)
                            ? 100 : 0;
                        var relationshipMatch = memory.SubjectActorId.HasValue
                            ? MemoryMechanics.RelationshipMatch(
                                relationships.GetValueOrDefault(
                                    memory.SubjectActorId.Value,
                                    RelationshipValues.Initial))
                            : 0;
                        return new
                        {
                            Memory = memory,
                            Score = MemoryMechanics.RecallScore(
                                subjectMatch,
                                topicMatch,
                                relationshipMatch,
                                memory.Importance),
                        };
                    })
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Memory.CreatedAtUtc)
                    .ThenByDescending(x => x.Memory.Id)
                    .Take(MemoryMechanics.MaximumRecallCount)
                    .ToArray();

                var now = clock.GetUtcNow();
                var request = new MemoryRecallRequest(
                    Guid.NewGuid(),
                    command.WorldId,
                    command.OwnerCharacterId,
                    command.Purpose,
                    command.SubjectType,
                    command.SubjectActorId,
                    command.SubjectTopicId,
                    command.TopicId,
                    command.IdempotencyKey,
                    now);
                db.MemoryRecallRequests.Add(request);
                for (var index = 0; index < ranked.Length; index++)
                {
                    db.MemoryRecallSelections.Add(new MemoryRecallSelection(
                        command.WorldId,
                        request.Id,
                        ranked[index].Memory.Id,
                        index + 1,
                        ranked[index].Score,
                        now));
                }

                await db.SaveChangesAsync(ct);
                return new MemoryRecallResult(
                    true,
                    false,
                    null,
                    ranked.Select((item, index) => new RecalledMemory(
                        item.Memory.Id,
                        item.Memory.MemoryType,
                        item.Memory.StructuredContent,
                        item.Score,
                        index + 1,
                        item.Memory.CreatedAtUtc)).ToArray());
            }
            catch (Exception exception) when (failures.IsRetryableConcurrency(exception))
            {
                db.ClearTrackedChanges();
            }
        }

        return MemoryRecallResult.Invalid("memory_recall_concurrency_conflict");
    }

    public async Task<bool> TransitionPromiseAsync(
        TransitionPromiseCommand command,
        CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var promise = await db.Promises
            .FromSqlInterpolated($"SELECT * FROM promises WHERE world_id = {command.WorldId} AND id = {command.PromiseId} FOR UPDATE")
            .SingleOrDefaultAsync(ct);
        if (promise is null)
        {
            return false;
        }

        try
        {
            if (command.TargetStatus == PromiseStatus.Expired)
            {
                promise.Expire(command.WorldTime);
            }
            else
            {
                var gameplayEvent = await db.GameplayEvents.AsNoTracking().SingleOrDefaultAsync(
                    x => x.WorldId == command.WorldId && x.Id == command.GameplayEventId,
                    ct);
                if (gameplayEvent is null)
                {
                    return false;
                }

                if (command.TargetStatus == PromiseStatus.Fulfilled)
                {
                    promise.Fulfill(gameplayEvent.Id, gameplayEvent.EventType, gameplayEvent.OccurredAt);
                }
                else if (command.TargetStatus == PromiseStatus.Cancelled
                    && string.Equals(gameplayEvent.EventType, "promise_cancelled", StringComparison.Ordinal))
                {
                    promise.Cancel(gameplayEvent.Id, gameplayEvent.OccurredAt);
                }
                else
                {
                    return false;
                }
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private async Task<bool> ValidSourceAsync(
        CreateMemoryCommand command,
        Guid ownerActorId,
        CancellationToken ct)
    {
        if (command.SourceType == MemorySourceType.GameplayEvent)
        {
            return await db.GameplayEvents.AsNoTracking().AnyAsync(
                x => x.WorldId == command.WorldId && x.Id == command.SourceId
                    && (x.ActorId == ownerActorId || x.TargetActorId == ownerActorId),
                ct);
        }

        return await (from message in db.Messages.AsNoTracking()
                      join participant in db.ConversationParticipants.AsNoTracking()
                          on new { message.WorldId, message.ConversationId, ActorId = ownerActorId }
                          equals new { participant.WorldId, participant.ConversationId, participant.ActorId }
                      where message.WorldId == command.WorldId && message.Id == command.SourceId
                      select message.Id).AnyAsync(ct);
    }

    private async Task<bool> ValidSubjectAndTopicAsync(
        CreateMemoryCommand command,
        CancellationToken ct)
    {
        if (command.SubjectType == MemorySubjectType.Actor
            && !await db.Actors.AsNoTracking().AnyAsync(
                x => x.WorldId == command.WorldId && x.Id == command.SubjectActorId,
                ct))
        {
            return false;
        }

        if (command.SubjectType == MemorySubjectType.Topic
            && !await TopicExistsAsync(command.WorldId, command.SubjectTopicId!, ct))
        {
            return false;
        }

        return command.TopicId is null
            || await TopicExistsAsync(command.WorldId, command.TopicId, ct);
    }

    private async Task<bool> ValidPromiseActorsAsync(
        CreateMemoryCommand command,
        CancellationToken ct)
    {
        if (command.Promise is null)
        {
            return true;
        }

        var ids = new[] { command.Promise.SourceActorId, command.Promise.TargetActorId }
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToArray();
        return await db.Actors.AsNoTracking().CountAsync(
            x => x.WorldId == command.WorldId && ids.Contains(x.Id), ct) == ids.Length;
    }

    private async Task<bool> ValidRecallSubjectAndTopicAsync(
        RecallMemoryCommand command,
        CancellationToken ct)
    {
        if (command.SubjectType == MemorySubjectType.Actor)
        {
            if (!await db.Actors.AsNoTracking().AnyAsync(
                x => x.WorldId == command.WorldId && x.Id == command.SubjectActorId,
                ct))
            {
                return false;
            }
        }
        else if (!await TopicExistsAsync(command.WorldId, command.SubjectTopicId!, ct))
        {
            return false;
        }

        return command.TopicId is null
            || await TopicExistsAsync(command.WorldId, command.TopicId, ct);
    }

    private Task<bool> TopicExistsAsync(Guid worldId, string topicId, CancellationToken ct) =>
        db.CharacterInterests.AsNoTracking().AnyAsync(
            x => x.WorldId == worldId && x.TopicId == topicId, ct);

    private async Task<MemoryRecallResult> LoadRecallAsync(
        Guid requestId,
        bool replay,
        CancellationToken ct)
    {
        var items = await (from selection in db.MemoryRecallSelections.AsNoTracking()
                           join memory in db.CharacterMemories.AsNoTracking()
                               on new { selection.WorldId, Id = selection.MemoryId }
                               equals new { memory.WorldId, memory.Id }
                           where selection.RequestId == requestId
                           orderby selection.Rank
                           select new RecalledMemory(
                               memory.Id,
                               memory.MemoryType,
                               memory.StructuredContent,
                               selection.Score,
                               selection.Rank,
                               memory.CreatedAtUtc)).ToArrayAsync(ct);
        return new(true, replay, null, items);
    }

    private static bool SameRecall(
        MemoryRecallRequest request,
        RecallMemoryCommand command) =>
        request.Purpose == command.Purpose
        && request.SubjectType == command.SubjectType
        && request.SubjectActorId == command.SubjectActorId
        && string.Equals(request.SubjectTopicId, command.SubjectTopicId, StringComparison.Ordinal)
        && string.Equals(request.TopicId, command.TopicId, StringComparison.Ordinal);

    private static int SubjectMatch(CharacterMemory memory, RecallMemoryCommand command) =>
        memory.SubjectType == command.SubjectType
        && (memory.SubjectType == MemorySubjectType.Actor
            ? memory.SubjectActorId == command.SubjectActorId
            : string.Equals(memory.SubjectTopicId, command.SubjectTopicId, StringComparison.Ordinal))
            ? 100 : 0;

    private static (Guid? GameplayEventId, Guid? MessageId) TypedSource(CreateMemoryCommand command) =>
        command.SourceType == MemorySourceType.GameplayEvent
            ? (command.SourceId, null)
            : (null, command.SourceId);

    private static MemoryCreationOutcome Outcome(
        CreateMemoryCommand command,
        MemoryCreationOutcomeType outcome,
        string? reasonCode,
        Guid? memoryId,
        DateTimeOffset now)
    {
        var source = TypedSource(command);
        return new MemoryCreationOutcome(
            Guid.NewGuid(),
            command.WorldId,
            command.OwnerCharacterId,
            command.MemoryType,
            command.SourceType,
            command.SourceId,
            source.GameplayEventId,
            source.MessageId,
            outcome,
            reasonCode,
            memoryId,
            now);
    }

    private static MemoryCreationResult MapOutcome(
        MemoryCreationOutcome outcome,
        bool replay,
        Guid? evictedMemoryId) =>
        new(
            outcome.Outcome == MemoryCreationOutcomeType.Created,
            replay,
            outcome.MemoryId,
            evictedMemoryId,
            outcome.ReasonCode);
}

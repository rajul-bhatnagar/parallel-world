using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.AI;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.AI;

internal sealed class AiGenerationRepository(
    ParallelWorldDbContext dbContext,
    IPersistenceFailureClassifier failureClassifier) : IAiGenerationRepository
{
    private const string IdempotencyConstraint = "ux_ai_generation_requests_world_idempotency_key";
    private const string ActionInputConstraint = "ux_ai_generation_requests_world_action_input";
    private const string PrimaryKeyConstraint = "pk_ai_generation_requests";

    public async Task<AiActionWordingProjection?> ResolveActionWordingProjectionAsync(
        Guid worldId,
        Guid simulationActionId,
        CancellationToken cancellationToken = default)
    {
        var action = await dbContext.SimulationActions.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.WorldId == worldId && candidate.Id == simulationActionId,
            cancellationToken);
        if (action is null)
        {
            return null;
        }

        var actor = await ResolveActorAsync(worldId, action.ActorId, cancellationToken);
        if (actor is null || actor.ActorType != ActorType.Character)
        {
            return null;
        }

        ResolvedActor? target = null;
        if (action.TargetActorId.HasValue)
        {
            target = await ResolveActorAsync(worldId, action.TargetActorId.Value, cancellationToken);
            if (target is null)
            {
                return null;
            }
        }

        var otherActorNames = await (
                from candidate in dbContext.Actors.AsNoTracking()
                join character in dbContext.Characters.AsNoTracking()
                    on new { candidate.WorldId, Id = candidate.CharacterId!.Value }
                    equals new { character.WorldId, Id = character.Id }
                where candidate.WorldId == worldId
                    && candidate.ActorType == ActorType.Character
                    && candidate.Id != action.ActorId
                    && candidate.Id != action.TargetActorId
                orderby candidate.Id
                select character.DisplayName)
            .Take(100)
            .ToArrayAsync(cancellationToken);

        var outcome = action.ActionType.ToLowerInvariant() switch
        {
            "post" => "published the decided post",
            "reply" => "replied to the decided post",
            "react" => "applied the decided reaction",
            "follow" when target is not null => "followed the decided target",
            "act" => "performed the decided action",
            _ => null,
        };
        if (outcome is null)
        {
            return null;
        }

        return new(
            action.WorldId,
            action.Id,
            action.ActionType,
            actor.DisplayName,
            target?.DisplayName,
            null,
            actor.VisibleMood,
            actor.StyleHint,
            action.Stance,
            action.Tone,
            outcome,
            otherActorNames);
    }

    public async Task<AiGenerationRecord?> FindByIdempotencyKeyAsync(
        Guid worldId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.AiGenerationRequests.AsNoTracking().SingleOrDefaultAsync(
            request => request.WorldId == worldId && request.IdempotencyKey == idempotencyKey,
            cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<AiGenerationRecord?> FindByActionAndInputAsync(
        Guid worldId,
        Guid simulationActionId,
        string inputHash,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.AiGenerationRequests.AsNoTracking().SingleOrDefaultAsync(
            request => request.WorldId == worldId
                && request.SimulationActionId == simulationActionId
                && request.InputHash == inputHash,
            cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<AiGenerationRecord> PersistAsync(
        AiGenerationPersistenceData data,
        CancellationToken cancellationToken = default)
    {
        var entity = new AiGenerationRequest(
            data.Id,
            data.WorldId,
            data.SimulationActionId,
            data.Provider,
            data.Model,
            data.AttemptCount,
            data.InputHash,
            data.OutputHash,
            data.PromptTokenCount,
            data.OutputTokenCount,
            data.LatencyMilliseconds,
            data.FailureCode,
            data.FallbackUsed,
            data.PromptTemplateVersion,
            data.FinalizedText,
            data.StartedAtUtc,
            data.CompletedAtUtc,
            data.IdempotencyKey);
        dbContext.AiGenerationRequests.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }
        catch (DbUpdateException exception) when (
            failureClassifier.HasConstraint(exception, IdempotencyConstraint)
            || failureClassifier.HasConstraint(exception, ActionInputConstraint)
            || failureClassifier.HasConstraint(exception, PrimaryKeyConstraint))
        {
            dbContext.ClearTrackedChanges();
            var byIdempotency = await dbContext.AiGenerationRequests.AsNoTracking().SingleOrDefaultAsync(
                request => request.WorldId == data.WorldId
                    && request.IdempotencyKey == data.IdempotencyKey,
                cancellationToken);
            if (byIdempotency is not null)
            {
                if (!string.Equals(byIdempotency.InputHash, data.InputHash, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "The AI generation idempotency key was reused with different input.",
                        exception);
                }

                return Map(byIdempotency);
            }

            var byActionInput = await dbContext.AiGenerationRequests.AsNoTracking().SingleOrDefaultAsync(
                request => request.WorldId == data.WorldId
                    && request.SimulationActionId == data.SimulationActionId
                    && request.InputHash == data.InputHash,
                cancellationToken);
            if (byActionInput is null)
            {
                throw;
            }

            return Map(byActionInput);
        }
    }

    private static AiGenerationRecord Map(AiGenerationRequest entity) => new(
        entity.Id,
        entity.WorldId,
        entity.SimulationActionId,
        entity.Provider,
        entity.Model,
        entity.AttemptCount,
        entity.InputHash,
        entity.OutputHash,
        entity.PromptTokenCount,
        entity.OutputTokenCount,
        entity.LatencyMilliseconds,
        entity.FailureCode,
        entity.FallbackUsed,
        entity.PromptTemplateVersion,
        entity.FinalizedText,
        entity.StartedAtUtc,
        entity.CompletedAtUtc,
        entity.IdempotencyKey);

    private async Task<ResolvedActor?> ResolveActorAsync(
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        var actor = await dbContext.Actors.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.WorldId == worldId && candidate.Id == actorId,
            cancellationToken);
        if (actor is null)
        {
            return null;
        }

        if (actor.ActorType == ActorType.Character && actor.CharacterId.HasValue)
        {
            return await dbContext.Characters.AsNoTracking()
                .Where(character => character.WorldId == worldId
                    && character.Id == actor.CharacterId.Value)
                .Select(character => new ResolvedActor(
                    actor.ActorType,
                    character.DisplayName,
                    character.CurrentMoodType.ToString().ToLowerInvariant(),
                    character.WritingStyle))
                .SingleOrDefaultAsync(cancellationToken);
        }

        if (actor.ActorType == ActorType.Player && actor.PlayerProfileId.HasValue)
        {
            return await dbContext.PlayerProfiles.AsNoTracking()
                .Where(profile => profile.WorldId == worldId
                    && profile.Id == actor.PlayerProfileId.Value)
                .Select(profile => new ResolvedActor(
                    actor.ActorType,
                    profile.DisplayName,
                    null,
                    null))
                .SingleOrDefaultAsync(cancellationToken);
        }

        return actor.ActorType == ActorType.System
            ? new(actor.ActorType, "System", null, null)
            : null;
    }

    private sealed record ResolvedActor(
        ActorType ActorType,
        string DisplayName,
        string? VisibleMood,
        string? StyleHint);
}

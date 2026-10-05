using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Common;
using ParallelWorld.Application.Relationships;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Relationships;

internal sealed class RomanceRepository(ParallelWorldDbContext db, TimeProvider timeProvider) : IRomanceRepository
{
    public Task<RelationshipResult<RomanticInvitationView>> CreatePlayerInvitationAsync(
        Guid userId, Guid worldId, Guid targetCharacterActorId, string idempotencyKey, CancellationToken ct) =>
        CreateAsync(userId, worldId, null, targetCharacterActorId, idempotencyKey, true, ct);

    public Task<RelationshipResult<RomanticInvitationView>> CreateAutonomousInvitationAsync(
        Guid worldId, Guid initiatorActorId, Guid targetActorId, string idempotencyKey, CancellationToken ct) =>
        CreateAsync(null, worldId, initiatorActorId, targetActorId, idempotencyKey, false, ct);

    public async Task<RelationshipResult<RomanticInvitationView>> ResolvePlayerOutcomeAsync(
        Guid userId, Guid worldId, Guid invitationId, bool accept, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var world = await LockWorldAsync(worldId, userId, ct);
        if (world is null) return Fail<RomanticInvitationView>("world_not_found", 404, "World was not found.");
        var invitationIdentity = await db.RomanticInvitations.AsNoTracking()
            .Where(x => x.WorldId == worldId && x.Id == invitationId)
            .Select(x => new { x.InitiatorActorId, x.TargetActorId }).SingleOrDefaultAsync(ct);
        if (invitationIdentity is null) return Fail<RomanticInvitationView>("invitation_not_found", 404, "Invitation was not found.");
        await LockActorsAsync(worldId, invitationIdentity.InitiatorActorId, invitationIdentity.TargetActorId, ct);
        var invitation = await db.RomanticInvitations.SingleAsync(x => x.WorldId == worldId && x.Id == invitationId, ct);
        var pair = await db.RomanticRelationships.SingleAsync(x => x.WorldId == worldId && x.Id == invitation.RomanticRelationshipId, ct);
        var target = await db.Actors.AsNoTracking().SingleAsync(x => x.WorldId == worldId && x.Id == invitation.TargetActorId, ct);
        var initiator = await db.Actors.AsNoTracking().SingleAsync(x => x.WorldId == worldId && x.Id == invitation.InitiatorActorId, ct);
        var characterId = initiator.ActorType == ActorType.Character ? initiator.CharacterId : target.CharacterId;
        if (target.ActorType != ActorType.Player)
            return Fail<RomanticInvitationView>("player_outcome_not_allowed", 409, "Only the Player target can decide this invitation.");
        if (invitation.Status != RomanticInvitationStatus.Pending)
        {
            await tx.CommitAsync(ct);
            return RelationshipResult<RomanticInvitationView>.Success(View(invitation, pair, characterId));
        }
        var utcNow = timeProvider.GetUtcNow();
        if (world.CurrentWorldTime >= invitation.ExpiresAtWorldTime)
        {
            Expire(invitation, pair, utcNow, world.CurrentWorldTime);
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
            return RelationshipResult<RomanticInvitationView>.Success(View(invitation, pair, characterId));
        }
        if (accept && await HasDatingConflictAsync(worldId, invitation.InitiatorActorId, invitation.TargetActorId, pair.Id, ct))
            return Fail<RomanticInvitationView>(RomanceReasonCodes.ExclusivityConflict, 409, "A Dating exclusivity conflict exists.");
        if (accept) Accept(invitation, pair, null, null, RomanceReasonCodes.PlayerAccepted, utcNow, world.CurrentWorldTime);
        else Reject(invitation, pair, null, null, RomanceReasonCodes.PlayerRejected, utcNow, world.CurrentWorldTime);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return RelationshipResult<RomanticInvitationView>.Success(View(invitation, pair, characterId));
    }

    public async Task<RelationshipResult<IReadOnlyList<RomanticInvitationView>>> ListAsync(Guid userId, Guid worldId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var world = await LockWorldAsync(worldId, userId, ct);
        if (world is null) return Fail<IReadOnlyList<RomanticInvitationView>>("world_not_found", 404, "World was not found.");
        var due = db.RomanticInvitations.AsNoTracking()
            .Where(x => x.WorldId == worldId && x.Status == RomanticInvitationStatus.Pending && x.ExpiresAtWorldTime <= world.CurrentWorldTime);
        var dueActorIds = await due.Select(x => x.InitiatorActorId).Union(due.Select(x => x.TargetActorId)).OrderBy(x => x).ToArrayAsync(ct);
        foreach (var actorId in dueActorIds)
            _ = await db.Actors.FromSqlInterpolated($"SELECT * FROM actors WHERE world_id = {worldId} AND id = {actorId} FOR UPDATE").SingleAsync(ct);
        await ExpireDueAsync(worldId, world.CurrentWorldTime, ct);
        var playerId = await PlayerActorIdAsync(worldId, ct);
        var rows = await (from invitation in db.RomanticInvitations.AsNoTracking()
                          join pair in db.RomanticRelationships.AsNoTracking() on new { invitation.WorldId, Id = invitation.RomanticRelationshipId } equals new { pair.WorldId, pair.Id }
                          join initiator in db.Actors.AsNoTracking() on new { invitation.WorldId, Id = invitation.InitiatorActorId } equals new { initiator.WorldId, initiator.Id }
                          join target in db.Actors.AsNoTracking() on new { invitation.WorldId, Id = invitation.TargetActorId } equals new { target.WorldId, target.Id }
                          where invitation.WorldId == worldId && (invitation.InitiatorActorId == playerId || invitation.TargetActorId == playerId)
                          orderby invitation.CreatedAtWorldTime descending, invitation.Id descending
                          select new { invitation, pair, CharacterId = initiator.ActorType == ActorType.Character ? initiator.CharacterId : target.CharacterId }).ToListAsync(ct);
        await tx.CommitAsync(ct);
        return RelationshipResult<IReadOnlyList<RomanticInvitationView>>.Success(rows.Select(x => View(x.invitation, x.pair, x.CharacterId)).ToArray());
    }

    public async Task<RelationshipResult<IReadOnlyList<RomanticHistoryItem>>> HistoryAsync(Guid userId, Guid worldId, Guid otherActorId, CancellationToken ct)
    {
        var owned = await db.GameWorlds.AsNoTracking().AnyAsync(x => x.Id == worldId && x.OwnerUserId == userId, ct);
        if (!owned) return Fail<IReadOnlyList<RomanticHistoryItem>>("world_not_found", 404, "World was not found.");
        var playerId = await PlayerActorIdAsync(worldId, ct);
        otherActorId = await db.Actors.AsNoTracking().Where(x => x.WorldId == worldId && x.ActorType == ActorType.Character && x.CharacterId == otherActorId).Select(x => x.Id).SingleOrDefaultAsync(ct);
        if (otherActorId == Guid.Empty) return Fail<IReadOnlyList<RomanticHistoryItem>>("character_not_found", 404, "Character was not found.");
        var pairIds = RomanticRelationship.Canonical(playerId, otherActorId);
        var pair = await db.RomanticRelationships.AsNoTracking().SingleOrDefaultAsync(
            x => x.WorldId == worldId && x.ActorAId == pairIds.ActorAId && x.ActorBId == pairIds.ActorBId, ct);
        if (pair is null) return RelationshipResult<IReadOnlyList<RomanticHistoryItem>>.Success([]);
        var rows = await db.RomanticStatusHistory.AsNoTracking().Where(x => x.WorldId == worldId && x.RomanticRelationshipId == pair.Id)
            .OrderByDescending(x => x.OccurredAtWorldTime).ThenByDescending(x => x.Id).ToListAsync(ct);
        return RelationshipResult<IReadOnlyList<RomanticHistoryItem>>.Success(rows.Select(x => new RomanticHistoryItem(
            x.Id, x.EpisodeId, Name(x.FromStatus), Name(x.ToStatus), x.ReasonCode, x.OccurredAtUtc, x.OccurredAtWorldTime)).ToArray());
    }

    private async Task<RelationshipResult<RomanticInvitationView>> CreateAsync(
        Guid? userId, Guid worldId, Guid? requestedInitiatorId, Guid requestedTargetId,
        string idempotencyKey, bool playerInitiated, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var world = await LockWorldAsync(worldId, userId, ct);
        if (world is null) return Fail<RomanticInvitationView>("world_not_found", 404, "World was not found.");
        var initiatorId = requestedInitiatorId ?? await PlayerActorIdAsync(worldId, ct);
        var targetId = playerInitiated
            ? await db.Actors.AsNoTracking().Where(x => x.WorldId == worldId && x.ActorType == ActorType.Character && x.CharacterId == requestedTargetId).Select(x => x.Id).SingleOrDefaultAsync(ct)
            : requestedTargetId;
        if (targetId == Guid.Empty) return Fail<RomanticInvitationView>("character_not_found", 404, "Character was not found.");
        var actors = await LockActorsAsync(worldId, initiatorId, targetId, ct);
        if (actors.Count != 2) return Fail<RomanticInvitationView>(RomanceReasonCodes.RomanceIncompatible, 409, "The actors are not romantically compatible.");
        var initiator = actors.Single(x => x.Id == initiatorId); var target = actors.Single(x => x.Id == targetId);
        if (playerInitiated && (initiator.ActorType != ActorType.Player || target.ActorType != ActorType.Character))
            return Fail<RomanticInvitationView>("sender_not_authorized", 403, "The Player can invite only a Character in the owned world.");
        var duplicate = await db.RomanticInvitations.SingleOrDefaultAsync(x => x.WorldId == worldId && x.IdempotencyKey == idempotencyKey, ct);
        if (duplicate is not null)
        {
            var duplicatePair = await db.RomanticRelationships.SingleAsync(x => x.WorldId == worldId && x.Id == duplicate.RomanticRelationshipId, ct);
            await tx.CommitAsync(ct); return RelationshipResult<RomanticInvitationView>.Success(View(duplicate, duplicatePair,
                initiator.ActorType == ActorType.Character ? initiator.CharacterId : target.CharacterId));
        }
        await ExpireDueForActorsAsync(worldId, world.CurrentWorldTime, initiatorId, targetId, ct);
        var hasDating = await HasDatingConflictAsync(worldId, initiatorId, targetId, null, ct);
        var hasPending = await db.RomanticInvitations.AnyAsync(x => x.WorldId == worldId && x.Status == RomanticInvitationStatus.Pending
            && (x.InitiatorActorId == initiatorId || x.TargetActorId == initiatorId || x.InitiatorActorId == targetId || x.TargetActorId == targetId), ct);
        var settings = await db.WorldSettings.AsNoTracking().SingleAsync(x => x.WorldId == worldId, ct);
        var initiatorData = await ActorRomanceDataAsync(initiator, ct); var targetData = await ActorRomanceDataAsync(target, ct);
        var canonical = RomanticRelationship.Canonical(initiatorId, targetId);
        var pair = await db.RomanticRelationships.SingleOrDefaultAsync(x => x.WorldId == worldId && x.ActorAId == canonical.ActorAId && x.ActorBId == canonical.ActorBId, ct);
        var stateConflict = pair is not null && pair.Status is not RomanticStatus.None and not RomanticStatus.RomanticInterest;
        var gate = RomanceMechanics.Gate(new(settings.RomanceEnabled, initiatorData.Mode, targetData.Mode,
            initiator.Status == ActorStatus.Active && initiatorData.Active, target.Status == ActorStatus.Active && targetData.Active,
            initiator.WorldId == target.WorldId, initiatorId != targetId, hasDating || hasPending, stateConflict));
        if (gate is not null) return RuleFailure<RomanticInvitationView>(gate);
        var cooldownStart = world.CurrentWorldTime.Subtract(RomanceMechanics.InvitationCooldown);
        if (await db.RomanticInvitations.AnyAsync(x => x.WorldId == worldId && x.InitiatorActorId == initiatorId
            && x.TargetActorId == targetId && x.CreatedAtWorldTime > cooldownStart, ct)) return RuleFailure<RomanticInvitationView>(RomanceReasonCodes.CooldownActive);
        var initiationValues = await ValuesAsync(worldId, initiatorId, targetId, ct);
        var initiation = RomanceMechanics.EvaluateInitiation(initiationValues, initiatorData.Openness);
        if (!initiation.Eligible) return RuleFailure<RomanticInvitationView>(initiation.ReasonCode!);
        var utcNow = timeProvider.GetUtcNow();
        pair ??= new RomanticRelationship(Guid.NewGuid(), worldId, initiatorId, targetId, world.CurrentWorldTime, utcNow);
        if (db.Entry(pair).State == EntityState.Detached) db.RomanticRelationships.Add(pair);
        var invitation = new RomanticInvitation(Guid.NewGuid(), worldId, pair.Id, Guid.NewGuid(), initiatorId, targetId,
            initiation.Score, utcNow, world.CurrentWorldTime, settings.RuleVersion, idempotencyKey);
        pair.MarkInvitationPending(world.CurrentWorldTime, utcNow); db.RomanticInvitations.Add(invitation);
        AddHistory(invitation, RomanticStatus.None, RomanticStatus.InvitationPending, "invitation_created", utcNow, world.CurrentWorldTime, "pending");
        if (target.ActorType == ActorType.Character)
        {
            var offset = DeterministicOffset(worldId, invitation.Id);
            var acceptance = RomanceMechanics.EvaluateAcceptance(await ValuesAsync(worldId, targetId, initiatorId, ct), targetData.Openness, offset);
            if (acceptance.Eligible) Accept(invitation, pair, acceptance.Score, offset, RomanceReasonCodes.CharacterAccepted, utcNow, world.CurrentWorldTime);
            else Reject(invitation, pair, acceptance.Score, offset, acceptance.ReasonCode!, utcNow, world.CurrentWorldTime);
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return RelationshipResult<RomanticInvitationView>.Success(View(invitation, pair,
            initiator.ActorType == ActorType.Character ? initiator.CharacterId : target.CharacterId));
    }

    private async Task<GameWorld?> LockWorldAsync(Guid worldId, Guid? userId, CancellationToken ct) =>
        (await db.GameWorlds.FromSqlInterpolated($"SELECT * FROM game_worlds WHERE id = {worldId} FOR UPDATE").ToListAsync(ct))
            .SingleOrDefault(x => userId is null || x.OwnerUserId == userId);

    private Task<List<Actor>> LockActorsAsync(Guid worldId, Guid left, Guid right, CancellationToken ct) =>
        db.Actors.FromSqlInterpolated($"SELECT * FROM actors WHERE world_id = {worldId} AND id IN ({left}, {right}) ORDER BY id FOR UPDATE").ToListAsync(ct);

    private async Task<ActorRomanceData> ActorRomanceDataAsync(Actor actor, CancellationToken ct)
    {
        if (actor.ActorType == ActorType.Player)
        {
            var profile = await db.PlayerProfiles.AsNoTracking().SingleAsync(x => x.WorldId == actor.WorldId && x.Id == actor.PlayerProfileId, ct);
            return new(profile.RomancePreferenceMode, RomanceMechanics.PlayerRomanticOpenness, true);
        }
        if (actor.ActorType == ActorType.Character)
        {
            var row = await (from character in db.Characters.AsNoTracking()
                             join traits in db.CharacterTraits.AsNoTracking() on new { character.WorldId, CharacterId = character.Id } equals new { traits.WorldId, traits.CharacterId }
                             where character.WorldId == actor.WorldId && character.Id == actor.CharacterId
                             select new ActorRomanceData(character.RomancePreferenceMode, traits.RomanticOpenness, character.Status == CharacterStatus.Active)).SingleAsync(ct);
            return row;
        }
        return new(RomancePreferenceMode.Disabled, 0, false);
    }

    private async Task<RelationshipValues> ValuesAsync(Guid worldId, Guid source, Guid target, CancellationToken ct) =>
        (await db.Relationships.SingleOrDefaultAsync(x => x.WorldId == worldId && x.SourceActorId == source && x.TargetActorId == target, ct))?.Values ?? RelationshipValues.Initial;

    private Task<bool> HasDatingConflictAsync(Guid worldId, Guid left, Guid right, Guid? excludedPairId, CancellationToken ct) =>
        db.RomanticRelationships.AnyAsync(x => x.WorldId == worldId && x.Status == RomanticStatus.Dating
            && (excludedPairId == null || x.Id != excludedPairId)
            && (x.ActorAId == left || x.ActorBId == left || x.ActorAId == right || x.ActorBId == right), ct);

    private void Accept(RomanticInvitation invitation, RomanticRelationship pair, int? score, int? offset, string reason, DateTimeOffset utcNow, DateTimeOffset worldTime)
    {
        pair.StartDating(worldTime, utcNow); invitation.Accept(score, offset, reason, utcNow, worldTime);
        ApplyCommitment(invitation.WorldId, invitation.InitiatorActorId, invitation.TargetActorId, utcNow);
        invitation.MarkCommitmentApplied(); AddHistory(invitation, RomanticStatus.InvitationPending, RomanticStatus.Dating, reason, utcNow, worldTime, "dating");
    }

    private void Reject(RomanticInvitation invitation, RomanticRelationship pair, int? score, int? offset, string reason, DateTimeOffset utcNow, DateTimeOffset worldTime)
    {
        pair.RejectOrExpire(worldTime, utcNow); invitation.Reject(score, offset, reason, utcNow, worldTime);
        AddHistory(invitation, RomanticStatus.InvitationPending, RomanticStatus.None, reason, utcNow, worldTime, "rejected");
    }

    private void Expire(RomanticInvitation invitation, RomanticRelationship pair, DateTimeOffset utcNow, DateTimeOffset worldTime)
    {
        pair.RejectOrExpire(worldTime, utcNow); invitation.Expire(utcNow, worldTime);
        AddHistory(invitation, RomanticStatus.InvitationPending, RomanticStatus.None, RomanceReasonCodes.InvitationExpired, utcNow, worldTime, "expired");
    }

    private void ApplyCommitment(Guid worldId, Guid left, Guid right, DateTimeOffset utcNow)
    {
        foreach (var direction in new[] { (Source: left, Target: right), (Source: right, Target: left) })
        {
            var relationship = db.Relationships.Local.SingleOrDefault(x => x.WorldId == worldId && x.SourceActorId == direction.Source && x.TargetActorId == direction.Target)
                ?? db.Relationships.SingleOrDefault(x => x.WorldId == worldId && x.SourceActorId == direction.Source && x.TargetActorId == direction.Target);
            if (relationship is null)
            {
                relationship = new Relationship(Guid.NewGuid(), worldId, direction.Source, direction.Target, utcNow);
                db.Relationships.Add(relationship);
            }
            relationship.Apply(new RelationshipValues(0, 0, 0, 0, 0, 0, 0, 0, RomanceMechanics.CommitmentOnDating), utcNow);
        }
    }

    private void AddHistory(RomanticInvitation invitation, RomanticStatus from, RomanticStatus to, string reason, DateTimeOffset utcNow, DateTimeOffset worldTime, string suffix) =>
        db.RomanticStatusHistory.Add(new RomanticStatusHistory(Guid.NewGuid(), invitation.WorldId, invitation.RomanticRelationshipId,
            invitation.EpisodeId, invitation.Id, from, to, invitation.InitiatorActorId, reason, utcNow, worldTime,
            invitation.RuleVersion, $"{invitation.IdempotencyKey}:{suffix}"));

    private async Task ExpireDueAsync(Guid worldId, DateTimeOffset worldTime, CancellationToken ct)
    {
        var due = await db.RomanticInvitations.Where(x => x.WorldId == worldId && x.Status == RomanticInvitationStatus.Pending && x.ExpiresAtWorldTime <= worldTime).ToListAsync(ct);
        if (due.Count == 0) return;
        var utcNow = timeProvider.GetUtcNow();
        foreach (var invitation in due)
        {
            var pair = await db.RomanticRelationships.SingleAsync(x => x.WorldId == worldId && x.Id == invitation.RomanticRelationshipId, ct);
            Expire(invitation, pair, utcNow, worldTime);
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task ExpireDueForActorsAsync(Guid worldId, DateTimeOffset worldTime, Guid left, Guid right, CancellationToken ct)
    {
        var due = await db.RomanticInvitations.Where(x => x.WorldId == worldId && x.Status == RomanticInvitationStatus.Pending && x.ExpiresAtWorldTime <= worldTime
            && (x.InitiatorActorId == left || x.TargetActorId == left || x.InitiatorActorId == right || x.TargetActorId == right)).ToListAsync(ct);
        var utcNow = timeProvider.GetUtcNow();
        foreach (var invitation in due)
        {
            var pair = await db.RomanticRelationships.SingleAsync(x => x.WorldId == worldId && x.Id == invitation.RomanticRelationshipId, ct);
            Expire(invitation, pair, utcNow, worldTime);
        }
        if (due.Count > 0) await db.SaveChangesAsync(ct);
    }

    private Task<Guid> PlayerActorIdAsync(Guid worldId, CancellationToken ct) =>
        db.Actors.AsNoTracking().Where(x => x.WorldId == worldId && x.ActorType == ActorType.Player).Select(x => x.Id).SingleAsync(ct);

    private static int DeterministicOffset(Guid worldId, Guid invitationId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"m13|rom-02|{worldId:N}|{invitationId:N}"));
        return hash[0] % 11 - 5;
    }

    private static RomanticInvitationView View(RomanticInvitation invitation, RomanticRelationship pair, Guid? characterId) => new(
        invitation.Id, invitation.EpisodeId, characterId, invitation.InitiatorActorId, invitation.TargetActorId, invitation.DateType,
        Name(invitation.Status), Name(pair.Status), invitation.ReasonCode, invitation.CreatedAtUtc, invitation.CreatedAtWorldTime,
        invitation.ExpiresAtWorldTime, invitation.ResolvedAtUtc, invitation.ResolvedAtWorldTime);

    private static string Name<T>(T value) where T : struct, Enum =>
        string.Concat(value.ToString().Select((c, i) => char.IsUpper(c) && i > 0 ? $"_{char.ToLowerInvariant(c)}" : char.ToLowerInvariant(c).ToString()));

    private static RelationshipResult<T> RuleFailure<T>(string reason) => Fail<T>(reason, 409, "The romantic action is not eligible.");
    private static RelationshipResult<T> Fail<T>(string code, int status, string detail) => RelationshipResult<T>.Fail(new ServiceFailure(code, status, detail));
    private sealed record ActorRomanceData(RomancePreferenceMode Mode, int Openness, bool Active);
}

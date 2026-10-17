using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Relationships;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Relationships;

internal sealed class RelationshipRepository(ParallelWorldDbContext db) : IRelationshipRepository
{
    public Task<bool> OwnsWorldAsync(Guid userId, Guid worldId, CancellationToken ct) =>
        db.GameWorlds.AsNoTracking().AnyAsync(x => x.Id == worldId && x.OwnerUserId == userId, ct);
    public async Task<Relationship?> ApplyAsync(ApplyRelationshipEventCommand c, RelationshipValues baseDelta, CancellationToken ct)
    {
        var ownsTransaction = db.Database.CurrentTransaction is null;
        await using var tx = ownsTransaction ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct) : null;
        var actors = await db.Actors.FromSqlInterpolated($"SELECT * FROM actors WHERE world_id = {c.WorldId} AND id IN ({c.SourceActorId}, {c.TargetActorId}) ORDER BY id FOR UPDATE").ToListAsync(ct);
        if (actors.Count != 2 || actors.Any(x => x.Status != ActorStatus.Active)) return null;
        var gameplayEvent = await db.GameplayEvents.SingleOrDefaultAsync(x => x.WorldId == c.WorldId && x.Id == c.GameplayEventId, ct); if (gameplayEvent is null) return null;
        var duplicate = await db.RelationshipEvents.AnyAsync(x => x.WorldId == c.WorldId && x.IdempotencyKey == c.IdempotencyKey, ct);
        if (duplicate) { if (tx is not null) await tx.CommitAsync(ct); return await db.Relationships.SingleAsync(x => x.WorldId == c.WorldId && x.SourceActorId == c.SourceActorId && x.TargetActorId == c.TargetActorId, ct); }
        var relationship = await db.Relationships.SingleOrDefaultAsync(x => x.WorldId == c.WorldId && x.SourceActorId == c.SourceActorId && x.TargetActorId == c.TargetActorId, ct);
        if (relationship is null) { relationship = new(Guid.NewGuid(), c.WorldId, c.SourceActorId, c.TargetActorId, c.OccurredAt); db.Relationships.Add(relationship); }
        var ledger = await db.RelationshipDailyChangeLedgers.SingleOrDefaultAsync(x => x.WorldId == c.WorldId && x.SourceActorId == c.SourceActorId && x.TargetActorId == c.TargetActorId && x.GameDate == c.GameDate, ct);
        if (ledger is null) { ledger = new(c.WorldId, c.SourceActorId, c.TargetActorId, c.GameDate, c.OccurredAt); db.RelationshipDailyChangeLedgers.Add(ledger); }
        var sinceGameDate = c.GameDate.AddDays(-7);
        var repeated = c.IsPositive ? await db.RelationshipEvents.CountAsync(x => x.WorldId == c.WorldId && x.SourceActorId == c.SourceActorId && x.TargetActorId == c.TargetActorId && x.EventType == c.EventType && x.OccurredGameDate >= sinceGameDate, ct) : 0;
        var modified = RelationshipMechanics.ApplyModifiers(baseDelta, c.IsPublicNegative, repeated);
        var severeBypass = gameplayEvent.Importance >= 90 && !ledger.SevereBypassUsed;
        var final = severeBypass ? modified : RelationshipMechanics.ApplyDailyCap(modified, ledger.Used);
        var before = relationship.Apply(final, c.OccurredAt); if (severeBypass) ledger.UseSevereBypass(c.OccurredAt); else ledger.Add(final, c.OccurredAt);
        db.RelationshipEvents.Add(new(Guid.NewGuid(), c.WorldId, relationship.Id, c.SourceActorId, c.TargetActorId, c.GameplayEventId, c.EventType, baseDelta, final, JsonSerializer.Serialize(before), JsonSerializer.Serialize(relationship.Values), "rel_01", c.OccurredAt, c.GameDate, RelationshipEventMatrix.IsQualifiedNegative(c.EventType), c.RuleVersion, c.IdempotencyKey));
        await db.SaveChangesAsync(ct); if (tx is not null) await tx.CommitAsync(ct); return relationship;
    }

    public async Task<IReadOnlyList<RelationshipSummary>> ListAsync(Guid userId, Guid worldId, CancellationToken ct)
    {
        var rows = await BaseQuery(userId, worldId, null).ToListAsync(ct);
        return rows.Select(x => Summary(x.Relationship, x.Actor.Id, x.Character.DisplayName, x.Character.Handle)).ToArray();
    }
    public async Task<RelationshipDetails?> FindAsync(Guid userId, Guid worldId, Guid actorId, int historyLimit, CancellationToken ct)
    {
        var row = await BaseQuery(userId, worldId, actorId).SingleOrDefaultAsync(ct); if (row is null) return null;
        var events = await db.RelationshipEvents.AsNoTracking().Where(x => x.WorldId == worldId && x.RelationshipId == row.Relationship.Id).OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id).Take(historyLimit).ToListAsync(ct);
        return new(Summary(row.Relationship, row.Actor.Id, row.Character.DisplayName, row.Character.Handle), events.Select(x => new RelationshipHistoryItem(x.Id, x.EventType, x.ReasonCode, x.OccurredAt)).ToArray());
    }
    private IQueryable<Row> BaseQuery(Guid userId, Guid worldId, Guid? actorId) =>
        from w in db.GameWorlds.AsNoTracking()
        join source in db.Actors.AsNoTracking() on w.Id equals source.WorldId
        join r in db.Relationships.AsNoTracking()
            on new { source.WorldId, SourceActorId = source.Id } equals new { r.WorldId, r.SourceActorId }
        join target in db.Actors.AsNoTracking()
            on new { r.WorldId, Id = r.TargetActorId } equals new { target.WorldId, target.Id }
        join character in db.Characters.AsNoTracking()
            on new { target.WorldId, target.CharacterId } equals new { character.WorldId, CharacterId = (Guid?)character.Id }
        where w.Id == worldId
            && w.OwnerUserId == userId
            && source.ActorType == ActorType.Player
            && (actorId == null || target.CharacterId == actorId)
        orderby r.UpdatedAt descending, character.Handle
        select new Row(r, target, character);
    private static RelationshipSummary Summary(Relationship r, Guid actorId, string name, string handle) => new(actorId, name, handle, RelationshipMechanics.Label(r.Values), r.UpdatedAt);
    private sealed record Row(Relationship Relationship, Actor Actor, Domain.Characters.Character Character);
}

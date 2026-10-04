using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Messaging;
using ParallelWorld.Domain.Messaging;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Messaging;

internal sealed class MessagingRepository(ParallelWorldDbContext db, TimeProvider clock,
    IPersistenceFailureClassifier failures) : IMessagingRepository
{
    private const string DirectOperation = "messaging.create-direct";

    public Task<bool> OwnsWorldAsync(Guid userId, Guid worldId, CancellationToken ct) =>
        db.GameWorlds.AsNoTracking().AnyAsync(x => x.Id == worldId && x.OwnerUserId == userId, ct);

    public async Task<ConversationPageData> ListAsync(Guid userId, Guid worldId, ConversationCursorValue? after, int take, CancellationToken ct)
    {
        var cursor = after.GetValueOrDefault();
        var ids = await (from w in db.GameWorlds.AsNoTracking()
                         join c in db.Conversations.AsNoTracking() on w.Id equals c.WorldId
                         where w.Id == worldId && w.OwnerUserId == userId && c.IsActive
                             && (after == null || c.LastMessageAt < cursor.LastMessageAtUtc
                                 || (c.LastMessageAt == cursor.LastMessageAtUtc && c.Id.CompareTo(cursor.Id) < 0))
                         orderby c.LastMessageAt descending, c.Id descending
                         select c.Id).Take(take).ToArrayAsync(ct);
        var items = new List<ConversationSummary>(ids.Length);
        foreach (var id in ids)
        {
            var details = await FindAsync(userId, worldId, id, ct);
            if (details is not null) items.Add(details.Conversation);
        }
        return new(items);
    }

    public async Task<DirectConversationResult?> CreateOrGetDirectAsync(Guid userId, Guid worldId, Guid characterId, string key, string hash, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
                var replay = await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId && x.Operation == DirectOperation && x.IdempotencyKey == key, ct);
                if (replay is not null)
                {
                    if (!string.Equals(replay.RequestHash, hash, StringComparison.Ordinal)) throw new MessagingIdempotencyConflictException();
                    var replayed = await FindAsync(userId, worldId, replay.ResponseResourceId, ct);
                    if (replayed is null) return null;
                    await tx.CommitAsync(ct);
                    return new(replayed, false);
                }

                var actors = await (from w in db.GameWorlds
                                    join p in db.Actors on w.Id equals p.WorldId
                                    join a in db.Actors on w.Id equals a.WorldId
                                    join c in db.Characters on new { a.WorldId, a.CharacterId } equals new { c.WorldId, CharacterId = (Guid?)c.Id }
                                    where w.Id == worldId && w.OwnerUserId == userId
                                        && p.ActorType == ActorType.Player && p.Status == ActorStatus.Active
                                        && a.ActorType == ActorType.Character && a.Status == ActorStatus.Active && c.Id == characterId
                                    select new { Player = p, CharacterActor = a }).SingleOrDefaultAsync(ct);
                if (actors is null) return null;

                var existing = await db.Conversations.SingleOrDefaultAsync(x => x.WorldId == worldId && x.PlayerActorId == actors.Player.Id && x.CharacterActorId == actors.CharacterActor.Id && x.IsActive, ct);
                var created = existing is null;
                var conversation = existing ?? new Conversation(Guid.NewGuid(), worldId, actors.Player.Id, actors.CharacterActor.Id, clock.GetUtcNow());
                if (created)
                {
                    db.Conversations.Add(conversation);
                    db.ConversationParticipants.AddRange(
                        new ConversationParticipant(worldId, conversation.Id, actors.Player.Id, conversation.CreatedAt),
                        new ConversationParticipant(worldId, conversation.Id, actors.CharacterActor.Id, conversation.CreatedAt));
                }
                db.IdempotencyRecords.Add(new IdempotencyRecord(Guid.NewGuid(), userId, worldId, DirectOperation, key, hash, conversation.Id, clock.GetUtcNow()));
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                var details = await FindAsync(userId, worldId, conversation.Id, ct);
                return details is null ? null : new(details, created);
            }
            catch (Exception ex) when (failures.IsRetryableConcurrency(ex)) { db.ClearTrackedChanges(); }
        }
        return null;
    }

    public async Task<ConversationDetails?> FindAsync(Guid userId, Guid worldId, Guid conversationId, CancellationToken ct)
    {
        var row = await (from w in db.GameWorlds.AsNoTracking()
                         join c in db.Conversations.AsNoTracking() on w.Id equals c.WorldId
                         join actor in db.Actors.AsNoTracking() on new { c.WorldId, Id = c.CharacterActorId } equals new { actor.WorldId, actor.Id }
                         join character in db.Characters.AsNoTracking() on new { actor.WorldId, actor.CharacterId } equals new { character.WorldId, CharacterId = (Guid?)character.Id }
                         join participant in db.ConversationParticipants.AsNoTracking() on new { c.WorldId, ConversationId = c.Id, ActorId = c.PlayerActorId } equals new { participant.WorldId, participant.ConversationId, participant.ActorId }
                         where w.Id == worldId && w.OwnerUserId == userId && c.Id == conversationId && c.IsActive
                         select new { Conversation = c, Actor = actor, Character = character, Participant = participant }).SingleOrDefaultAsync(ct);
        if (row is null) return null;
        var latest = await db.Messages.AsNoTracking().Where(x => x.WorldId == worldId && x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var lastPlan = await db.PlannedReplies.AsNoTracking().Where(x => x.WorldId == worldId && x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var unread = await UnreadCountAsync(row.Conversation, row.Participant, ct);
        var summary = new ConversationSummary(row.Conversation.Id, "direct",
            new(row.Actor.Id, row.Character.Id, row.Character.DisplayName, row.Character.Handle),
            row.Conversation.CreatedAt, row.Conversation.LastMessageAt,
            latest is null ? null : Preview(latest.Content), unread, Status(lastPlan));
        return new(summary, row.Conversation.PlayerActorId, row.Participant.LastReadMessageId, row.Participant.LastReadAt);
    }

    public async Task<IReadOnlyList<ConversationMessage>?> ListMessagesAsync(Guid userId, Guid worldId, Guid conversationId, MessageCursorValue? after, int take, CancellationToken ct)
    {
        var conversation = await OwnedConversationAsync(userId, worldId, conversationId, false, ct);
        if (conversation is null) return null;
        var cursor = after.GetValueOrDefault();
        return await db.Messages.AsNoTracking().Where(x => x.WorldId == worldId && x.ConversationId == conversationId
                && (after == null || x.CreatedAt < cursor.CreatedAtUtc || (x.CreatedAt == cursor.CreatedAtUtc && x.Id.CompareTo(cursor.Id) < 0)))
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Select(x => new ConversationMessage(x.Id, x.ConversationId, x.SenderActorId,
                x.SenderActorId == conversation.PlayerActorId ? "player" : "character", x.Content, x.CreatedAt,
                x.DeliveryStatus.ToString().ToLower(), x.ClientOperationId)).Take(take).ToArrayAsync(ct);
    }

    public async Task<PersistedSend?> SendAsync(Guid userId, Guid worldId, Guid conversationId, string body, Guid clientMessageId, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
                var conversation = await OwnedConversationAsync(userId, worldId, conversationId, true, ct);
                if (conversation is null) return null;
                var existing = await db.Messages.SingleOrDefaultAsync(x => x.WorldId == worldId && x.SenderActorId == conversation.PlayerActorId && x.ClientOperationId == clientMessageId, ct);
                if (existing is not null)
                {
                    if (existing.ConversationId != conversationId || !string.Equals(existing.Content, body, StringComparison.Ordinal)) throw new MessagingIdempotencyConflictException();
                    var existingPlan = await db.PlannedReplies.SingleAsync(x => x.WorldId == worldId && x.SourceMessageId == existing.Id && x.RecipientActorId == conversation.CharacterActorId, ct);
                    var wording = existingPlan.Status == PlannedReplyStatus.Planned ? await WordingRequestAsync(conversation, existingPlan, existing.Content, ct) : null;
                    await tx.CommitAsync(ct);
                    return new(Map(existing, "player"), existingPlan.Id, Status(existingPlan)!, wording, true);
                }

                var world = await db.GameWorlds.SingleAsync(x => x.Id == worldId, ct);
                var settings = await db.WorldSettings.SingleAsync(x => x.WorldId == worldId, ct);
                var traits = await (from actor in db.Actors
                                    join character in db.Characters on new { actor.WorldId, actor.CharacterId } equals new { character.WorldId, CharacterId = (Guid?)character.Id }
                                    join candidate in db.CharacterTraits on new { character.WorldId, CharacterId = character.Id } equals new { candidate.WorldId, candidate.CharacterId }
                                    where actor.WorldId == worldId && actor.Id == conversation.CharacterActorId
                                    select candidate).SingleAsync(ct);
                var relationship = await db.Relationships.AsNoTracking().SingleOrDefaultAsync(x => x.WorldId == worldId && x.SourceActorId == conversation.CharacterActorId && x.TargetActorId == conversation.PlayerActorId, ct);
                var values = relationship?.Values ?? RelationshipValues.Initial;
                var messageId = Guid.NewGuid();
                var now = clock.GetUtcNow();
                var score = MessageReplyMechanics.Score(values, traits.Sociability);
                var roll = MessageReplyMechanics.Roll(worldId, world.Seed, settings.RuleVersion, messageId, conversation.CharacterActorId, conversation.PlayerActorId);
                var eventId = Guid.NewGuid();
                var message = new Message(messageId, worldId, conversationId, conversation.PlayerActorId, eventId, null, clientMessageId, body, MessageDeliveryStatus.Delivered, now);
                var plan = new PlannedReply(Guid.NewGuid(), worldId, conversationId, messageId, conversation.CharacterActorId,
                    MessageReplyMechanics.Urgency, MessageReplyMechanics.ConflictAvoidancePenalty, score, roll, roll < score, now, now);
                db.GameplayEvents.Add(new GameplayEvent(eventId, worldId, "private_message", conversation.PlayerActorId, conversation.CharacterActorId, now, 50, 0, "player_private_message", settings.RuleVersion, $"message:{messageId:N}", now));
                db.Messages.Add(message); db.PlannedReplies.Add(plan); conversation.RecordMessage(now);
                await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
                var wordingRequest = plan.Status == PlannedReplyStatus.Planned ? await WordingRequestAsync(conversation, plan, body, ct) : null;
                return new(Map(message, "player"), plan.Id, Status(plan)!, wordingRequest, false);
            }
            catch (Exception ex) when (failures.IsRetryableConcurrency(ex)) { db.ClearTrackedChanges(); }
        }
        return null;
    }

    public async Task<string?> FinalizeReplyAsync(Guid userId, Guid plannedReplyId, MessageWordingResult wording, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var planInfo = await (from p in db.PlannedReplies.AsNoTracking()
                              join w in db.GameWorlds.AsNoTracking() on p.WorldId equals w.Id
                              where p.Id == plannedReplyId && w.OwnerUserId == userId
                              select new { p.WorldId, p.ConversationId }).SingleOrDefaultAsync(ct);
        if (planInfo is null) return null;
        var plan = await db.PlannedReplies.FromSqlInterpolated($"SELECT * FROM planned_replies WHERE world_id = {planInfo.WorldId} AND id = {plannedReplyId} FOR UPDATE").SingleAsync(ct);
        if (plan.Status != PlannedReplyStatus.Planned) { await tx.CommitAsync(ct); return Status(plan); }
        var conversation = await db.Conversations.FromSqlInterpolated($"SELECT * FROM conversations WHERE world_id = {plan.WorldId} AND id = {plan.ConversationId} FOR UPDATE").SingleAsync(ct);
        var settings = await db.WorldSettings.AsNoTracking().SingleAsync(x => x.WorldId == plan.WorldId, ct);
        var now = clock.GetUtcNow();
        var eventId = Guid.NewGuid();
        var reply = new Message(Guid.NewGuid(), plan.WorldId, plan.ConversationId, plan.RecipientActorId, eventId, null, null,
            wording.Text, MessageDeliveryStatus.Delivered, now);
        db.GameplayEvents.Add(new GameplayEvent(eventId, plan.WorldId, "private_message_reply", plan.RecipientActorId,
            conversation.PlayerActorId, now, 50, 0, "msg_02_reply", settings.RuleVersion, $"msg-02-reply:{plan.Id:N}", now));
        db.Messages.Add(reply); conversation.RecordMessage(now); plan.Complete(wording.FallbackUsed, now);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Status(plan);
    }

    public async Task<PendingMessageWordingWork?> GetPendingWordingWorkAsync(CancellationToken ct)
    {
        var pending = await (from plan in db.PlannedReplies.AsNoTracking()
                             join world in db.GameWorlds.AsNoTracking() on plan.WorldId equals world.Id
                             where plan.Status == PlannedReplyStatus.Planned && plan.DueAt <= clock.GetUtcNow()
                             orderby plan.DueAt, plan.Id
                             select new { Plan = plan, world.OwnerUserId }).FirstOrDefaultAsync(ct);
        if (pending is null) return null;
        var conversation = await db.Conversations.AsNoTracking().SingleAsync(
            x => x.WorldId == pending.Plan.WorldId && x.Id == pending.Plan.ConversationId, ct);
        var source = await db.Messages.AsNoTracking().SingleAsync(
            x => x.WorldId == pending.Plan.WorldId && x.Id == pending.Plan.SourceMessageId, ct);
        var request = await WordingRequestAsync(conversation, pending.Plan, source.Content, ct);
        return new(pending.OwnerUserId, request);
    }

    public async Task<bool> MarkReadAsync(Guid userId, Guid worldId, Guid conversationId, Guid messageId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var conversation = await OwnedConversationAsync(userId, worldId, conversationId, false, ct);
        if (conversation is null) return false;
        var target = await db.Messages.AsNoTracking().SingleOrDefaultAsync(x => x.WorldId == worldId && x.ConversationId == conversationId && x.Id == messageId, ct);
        if (target is null) return false;
        var participant = await db.ConversationParticipants.SingleAsync(x => x.WorldId == worldId && x.ConversationId == conversationId && x.ActorId == conversation.PlayerActorId, ct);
        Message? current = null;
        if (participant.LastReadMessageId.HasValue)
            current = await db.Messages.AsNoTracking().SingleAsync(x => x.WorldId == worldId && x.ConversationId == conversationId && x.Id == participant.LastReadMessageId.Value, ct);
        if (current is null || target.CreatedAt > current.CreatedAt || (target.CreatedAt == current.CreatedAt && target.Id.CompareTo(current.Id) > 0))
            participant.MarkRead(target.Id, clock.GetUtcNow());
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return true;
    }

    private async Task<Conversation?> OwnedConversationAsync(Guid userId, Guid worldId, Guid id, bool forUpdate, CancellationToken ct)
    {
        if (!await db.GameWorlds.AsNoTracking().AnyAsync(x => x.Id == worldId && x.OwnerUserId == userId, ct)) return null;
        return forUpdate
            ? await db.Conversations.FromSqlInterpolated($"SELECT * FROM conversations WHERE world_id = {worldId} AND id = {id} AND is_active = TRUE FOR UPDATE").SingleOrDefaultAsync(ct)
            : await db.Conversations.AsNoTracking().SingleOrDefaultAsync(x => x.WorldId == worldId && x.Id == id && x.IsActive, ct);
    }

    private async Task<MessageWordingRequest> WordingRequestAsync(Conversation c, PlannedReply p, string body, CancellationToken ct)
    {
        var row = await (from a in db.Actors.AsNoTracking()
                         join character in db.Characters.AsNoTracking() on new { a.WorldId, a.CharacterId } equals new { character.WorldId, CharacterId = (Guid?)character.Id }
                         where a.WorldId == c.WorldId && a.Id == c.CharacterActorId
                         select new { character.DisplayName, character.CurrentMoodType, character.WritingStyle }).SingleAsync(ct);
        var playerName = await (from a in db.Actors.AsNoTracking()
                                join profile in db.PlayerProfiles.AsNoTracking() on new { a.WorldId, a.PlayerProfileId } equals new { profile.WorldId, PlayerProfileId = (Guid?)profile.Id }
                                where a.WorldId == c.WorldId && a.Id == c.PlayerActorId
                                select profile.DisplayName).SingleAsync(ct);
        return new(c.WorldId, c.Id, p.Id, c.CharacterActorId, row.DisplayName, playerName,
            row.CurrentMoodType.ToString().ToLowerInvariant(), row.WritingStyle, body, 500);
    }

    private async Task<int> UnreadCountAsync(Conversation c, ConversationParticipant participant, CancellationToken ct)
    {
        if (!participant.LastReadMessageId.HasValue)
            return await db.Messages.CountAsync(x => x.WorldId == c.WorldId && x.ConversationId == c.Id && x.SenderActorId == c.CharacterActorId, ct);
        var cursor = await db.Messages.AsNoTracking().SingleAsync(x => x.WorldId == c.WorldId && x.ConversationId == c.Id && x.Id == participant.LastReadMessageId.Value, ct);
        return await db.Messages.CountAsync(x => x.WorldId == c.WorldId && x.ConversationId == c.Id && x.SenderActorId == c.CharacterActorId
            && (x.CreatedAt > cursor.CreatedAt || (x.CreatedAt == cursor.CreatedAt && x.Id.CompareTo(cursor.Id) > 0)), ct);
    }

    private static ConversationMessage Map(Message x, string sender) => new(x.Id, x.ConversationId, x.SenderActorId, sender, x.Content, x.CreatedAt, x.DeliveryStatus.ToString().ToLowerInvariant(), x.ClientOperationId);
    private static string? Status(PlannedReply? p) => p?.Status.ToString().ToLowerInvariant();
    private static string Preview(string value) => value.Length <= 120 ? value : value[..120];
}

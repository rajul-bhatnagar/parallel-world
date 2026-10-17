using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Notifications;
using ParallelWorld.Domain.Notifications;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Notifications;

internal sealed class NotificationProjectionWriter(ParallelWorldDbContext db)
    : INotificationProjectionWriter
{
    public Task AddPrivateMessageAsync(
        Guid worldId, Guid recipientUserId, Guid recipientActorId,
        Guid gameplayEventId, DateTimeOffset createdAt, CancellationToken cancellationToken) =>
        AddAsync(worldId, recipientUserId, recipientActorId,
            NotificationSourceType.GameplayEvent, gameplayEventId, gameplayEventId,
            NotificationCategory.PrivateMessage, NotificationPriority.High, createdAt,
            cancellationToken);

    public async Task AddCatchUpSummaryAsync(
        Guid worldId, Guid recipientUserId, Guid catchUpSummaryId,
        DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        var summary = db.CatchUpSummaries.Local.FirstOrDefault(x =>
            x.WorldId == worldId && x.Id == catchUpSummaryId)
            ?? await db.CatchUpSummaries.AsNoTracking().SingleOrDefaultAsync(x =>
                x.WorldId == worldId && x.Id == catchUpSummaryId, cancellationToken);
        if (summary?.Status != CatchUpSummaryStatus.Completed) return;
        await AddAsync(worldId, recipientUserId, null,
            NotificationSourceType.CatchUpSummary, catchUpSummaryId, null,
            NotificationCategory.CatchUpSummary, NotificationPriority.Normal, createdAt,
            cancellationToken);
    }

    public async Task<bool> ProjectReplyAsync(
        Guid worldId, Guid replyId, CancellationToken cancellationToken)
    {
        var source = await (from reply in db.Posts.AsNoTracking()
                            join author in db.Actors.AsNoTracking()
                                on new { reply.WorldId, Id = reply.AuthorActorId }
                                equals new { author.WorldId, author.Id }
                            join parent in db.Posts.AsNoTracking()
                                on new { reply.WorldId, Id = reply.ParentPostId }
                                equals new { parent.WorldId, Id = (Guid?)parent.Id }
                            join parentAuthor in db.Actors.AsNoTracking()
                                on new { parent.WorldId, Id = parent.AuthorActorId }
                                equals new { parentAuthor.WorldId, parentAuthor.Id }
                            join world in db.GameWorlds.AsNoTracking() on reply.WorldId equals world.Id
                            where reply.WorldId == worldId && reply.Id == replyId
                                && reply.ParentPostId != null
                            select new
                            {
                                Reply = reply,
                                ReplyAuthorType = author.ActorType,
                                ParentAuthorType = parentAuthor.ActorType,
                                ParentAuthorId = parentAuthor.Id,
                                world.OwnerUserId,
                            }).SingleOrDefaultAsync(cancellationToken);
        if (source is null || source.ReplyAuthorType != ActorType.Character
            || source.ParentAuthorType != ActorType.Player)
            return false;

        await AddAsync(worldId, source.OwnerUserId, source.ParentAuthorId,
            NotificationSourceType.GameplayEvent, source.Reply.GameplayEventId,
            source.Reply.GameplayEventId, NotificationCategory.Reply,
            NotificationPriority.High, source.Reply.CreatedAt, cancellationToken);
        return true;
    }

    private async Task AddAsync(
        Guid worldId, Guid recipientUserId, Guid? recipientActorId,
        NotificationSourceType sourceType, Guid sourceId, Guid? gameplayEventId,
        NotificationCategory category, NotificationPriority priority,
        DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        if (await db.Notifications.AnyAsync(x => x.WorldId == worldId
            && x.RecipientUserId == recipientUserId && x.SourceType == sourceType
            && x.SourceId == sourceId && x.Category == category, cancellationToken)) return;

        var key = $"m16:{recipientUserId:N}:{sourceType}:{sourceId:N}:{category}".ToLowerInvariant();
        db.Notifications.Add(new Notification(
            Guid.NewGuid(), worldId, recipientUserId, recipientActorId,
            sourceType, sourceId, gameplayEventId, category, priority,
            createdAt, null, key));
    }
}

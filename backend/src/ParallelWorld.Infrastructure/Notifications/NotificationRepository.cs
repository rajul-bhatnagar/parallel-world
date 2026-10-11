using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Notifications;
using ParallelWorld.Domain.Notifications;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Notifications;

internal sealed class NotificationRepository(ParallelWorldDbContext db) : INotificationRepository
{
    public Task<bool> OwnsWorldAsync(Guid userId, Guid worldId, CancellationToken cancellationToken) =>
        db.GameWorlds.AsNoTracking().AnyAsync(x => x.Id == worldId && x.OwnerUserId == userId, cancellationToken);

    public async Task<IReadOnlyList<NotificationItem>> ListAsync(
        Guid userId, Guid worldId, NotificationCursorValue? after, int take,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var cursorCreatedAt = after?.CreatedAtUtc ?? default;
        var cursorId = after?.Id ?? default;
        var rows = await db.Notifications.AsNoTracking()
            .Where(x => x.WorldId == worldId && x.RecipientUserId == userId
                && (x.ExpiresAt == null || x.ExpiresAt > now)
                && (after == null || x.CreatedAt < cursorCreatedAt
                    || (x.CreatedAt == cursorCreatedAt && x.Id.CompareTo(cursorId) < 0)))
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Take(take).ToArrayAsync(cancellationToken);

        var eventIds = rows.Where(x => x.GameplayEventId.HasValue)
            .Select(x => x.GameplayEventId!.Value).Distinct().ToArray();
        var messageResources = await db.Messages.AsNoTracking()
            .Where(x => x.WorldId == worldId && eventIds.Contains(x.GameplayEventId))
            .ToDictionaryAsync(x => x.GameplayEventId, x => x.ConversationId, cancellationToken);
        var replyResources = await db.Posts.AsNoTracking()
            .Where(x => x.WorldId == worldId && eventIds.Contains(x.GameplayEventId))
            .ToDictionaryAsync(x => x.GameplayEventId, x => x.Id, cancellationToken);

        return rows.Select(x => new NotificationItem(
            x.Id,
            Category(x.Category),
            Title(x.Category),
            ResourceId(x, messageResources, replyResources),
            x.ReadAt is not null,
            x.CreatedAt)).ToArray();
    }

    public Task<int> CountUnreadAsync(Guid userId, Guid worldId, DateTimeOffset now,
        CancellationToken cancellationToken) => db.Notifications.AsNoTracking().CountAsync(
            x => x.WorldId == worldId && x.RecipientUserId == userId && x.ReadAt == null
                && (x.ExpiresAt == null || x.ExpiresAt > now), cancellationToken);

    public async Task<bool> MarkReadAsync(Guid userId, Guid worldId, Guid notificationId,
        DateTimeOffset readAt, CancellationToken cancellationToken)
    {
        var item = await db.Notifications.SingleOrDefaultAsync(
            x => x.Id == notificationId && x.WorldId == worldId && x.RecipientUserId == userId
                && (x.ExpiresAt == null || x.ExpiresAt > readAt), cancellationToken);
        if (item is null) return false;
        item.MarkRead(readAt);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllReadAsync(Guid userId, Guid worldId, DateTimeOffset readAt,
        CancellationToken cancellationToken)
    {
        if (!await OwnsWorldAsync(userId, worldId, cancellationToken)) return false;
        var items = await db.Notifications.Where(x => x.WorldId == worldId
            && x.RecipientUserId == userId && x.ReadAt == null
            && (x.ExpiresAt == null || x.ExpiresAt > readAt)).ToArrayAsync(cancellationToken);
        foreach (var item in items) item.MarkRead(readAt);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Guid ResourceId(Notification item,
        IReadOnlyDictionary<Guid, Guid> messageResources,
        IReadOnlyDictionary<Guid, Guid> replyResources) => item.Category switch
        {
            NotificationCategory.PrivateMessage when item.GameplayEventId is Guid eventId
                && messageResources.TryGetValue(eventId, out var conversationId) => conversationId,
            NotificationCategory.Reply when item.GameplayEventId is Guid eventId
                && replyResources.TryGetValue(eventId, out var replyId) => replyId,
            NotificationCategory.CatchUpSummary => item.SourceId,
            _ => item.SourceId,
        };

    private static string Category(NotificationCategory category) => category switch
    {
        NotificationCategory.PrivateMessage => "privateMessage",
        NotificationCategory.CatchUpSummary => "catchUpSummary",
        _ => "reply",
    };

    private static string Title(NotificationCategory category) => category switch
    {
        NotificationCategory.PrivateMessage => "New private message",
        NotificationCategory.CatchUpSummary => "While you were away",
        _ => "New reply",
    };
}

using ParallelWorld.Application.Common;

namespace ParallelWorld.Application.Notifications;

public sealed record NotificationItem(
    Guid Id,
    string Category,
    string Title,
    Guid ResourceId,
    bool IsRead,
    DateTimeOffset CreatedAtUtc);

public sealed record NotificationPage(
    IReadOnlyList<NotificationItem> Items,
    string? NextCursor,
    bool HasMore);

public sealed record NotificationCursorValue(DateTimeOffset CreatedAtUtc, Guid Id);
public readonly record struct NotificationCursorDecodeResult(bool IsValid, NotificationCursorValue? Value);

public sealed record NotificationResult<T>(T? Value, ServiceFailure? Failure)
{
    public bool IsSuccess => Failure is null;
    public static NotificationResult<T> Success(T value) => new(value, null);
    public static NotificationResult<T> Fail(ServiceFailure failure) => new(default, failure);
}

public interface INotificationCursorCodec
{
    string Encode(Guid worldId, NotificationCursorValue value);
    NotificationCursorDecodeResult Decode(string? cursor, Guid worldId);
}

public interface INotificationRepository
{
    Task<bool> OwnsWorldAsync(Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotificationItem>> ListAsync(Guid userId, Guid worldId,
        NotificationCursorValue? after, int take, DateTimeOffset now, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid userId, Guid worldId, DateTimeOffset now, CancellationToken cancellationToken);
    Task<bool> MarkReadAsync(Guid userId, Guid worldId, Guid notificationId,
        DateTimeOffset readAt, CancellationToken cancellationToken);
    Task<bool> MarkAllReadAsync(Guid userId, Guid worldId, DateTimeOffset readAt,
        CancellationToken cancellationToken);
}

public interface INotificationProjectionWriter
{
    Task AddPrivateMessageAsync(Guid worldId, Guid recipientUserId, Guid recipientActorId,
        Guid gameplayEventId, DateTimeOffset createdAt, CancellationToken cancellationToken);
    Task AddCatchUpSummaryAsync(Guid worldId, Guid recipientUserId, Guid catchUpSummaryId,
        DateTimeOffset createdAt, CancellationToken cancellationToken);
    Task<bool> ProjectReplyAsync(Guid worldId, Guid replyId, CancellationToken cancellationToken);
}

public interface INotificationService
{
    Task<NotificationResult<NotificationPage>> ListAsync(Guid userId, Guid worldId,
        int limit, string? cursor, CancellationToken cancellationToken);
    Task<NotificationResult<int>> CountUnreadAsync(Guid userId, Guid worldId,
        CancellationToken cancellationToken);
    Task<NotificationResult<bool>> MarkReadAsync(Guid userId, Guid worldId,
        Guid notificationId, CancellationToken cancellationToken);
    Task<NotificationResult<bool>> MarkAllReadAsync(Guid userId, Guid worldId,
        CancellationToken cancellationToken);
}

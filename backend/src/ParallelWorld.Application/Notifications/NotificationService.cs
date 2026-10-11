using ParallelWorld.Application.Common;

namespace ParallelWorld.Application.Notifications;

public sealed class NotificationService(
    INotificationRepository repository,
    INotificationCursorCodec cursorCodec,
    TimeProvider clock) : INotificationService
{
    public async Task<NotificationResult<NotificationPage>> ListAsync(
        Guid userId, Guid worldId, int limit, string? cursor, CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 100)
            return Invalid<NotificationPage>("Limit must be between 1 and 100.");
        var decoded = cursorCodec.Decode(cursor, worldId);
        if (!decoded.IsValid)
            return Invalid<NotificationPage>("The cursor is invalid.", "invalid_cursor");
        if (!await repository.OwnsWorldAsync(userId, worldId, cancellationToken))
            return Missing<NotificationPage>();

        var rows = await repository.ListAsync(userId, worldId, decoded.Value,
            limit + 1, clock.GetUtcNow(), cancellationToken);
        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).ToArray();
        var next = hasMore && items.Length > 0
            ? cursorCodec.Encode(worldId, new(items[^1].CreatedAtUtc, items[^1].Id))
            : null;
        return NotificationResult<NotificationPage>.Success(new(items, next, hasMore));
    }

    public async Task<NotificationResult<int>> CountUnreadAsync(
        Guid userId, Guid worldId, CancellationToken cancellationToken)
    {
        if (!await repository.OwnsWorldAsync(userId, worldId, cancellationToken))
            return Missing<int>();
        return NotificationResult<int>.Success(await repository.CountUnreadAsync(
            userId, worldId, clock.GetUtcNow(), cancellationToken));
    }

    public async Task<NotificationResult<bool>> MarkReadAsync(
        Guid userId, Guid worldId, Guid notificationId, CancellationToken cancellationToken) =>
        await repository.MarkReadAsync(userId, worldId, notificationId,
            clock.GetUtcNow(), cancellationToken)
            ? NotificationResult<bool>.Success(true)
            : Missing<bool>();

    public async Task<NotificationResult<bool>> MarkAllReadAsync(
        Guid userId, Guid worldId, CancellationToken cancellationToken) =>
        await repository.MarkAllReadAsync(userId, worldId, clock.GetUtcNow(), cancellationToken)
            ? NotificationResult<bool>.Success(true)
            : Missing<bool>();

    private static NotificationResult<T> Invalid<T>(string title, string code = "validation_failed") =>
        NotificationResult<T>.Fail(new ServiceFailure(code, 400, title));
    private static NotificationResult<T> Missing<T>() => NotificationResult<T>.Fail(new(
        "resource_not_available", 404, "The requested resource is not available."));
}

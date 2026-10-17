using ParallelWorld.Domain.Notifications;

namespace ParallelWorld.UnitTests;

public sealed class NotificationTests
{
    [Fact]
    public void TypedSourceShapeAndReadAreDeterministic()
    {
        var now = new DateTimeOffset(2026, 10, 11, 0, 0, 0, TimeSpan.Zero);
        var sourceId = Guid.NewGuid();
        var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            NotificationSourceType.CatchUpSummary, sourceId, null,
            NotificationCategory.CatchUpSummary, NotificationPriority.Normal,
            now, null, "m16:test");

        notification.MarkRead(now.AddMinutes(1));
        notification.MarkRead(now.AddMinutes(2));

        Assert.Equal(now.AddMinutes(1), notification.ReadAt);
        Assert.Throws<ArgumentException>(() => new Notification(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            NotificationSourceType.CatchUpSummary, sourceId, Guid.NewGuid(),
            NotificationCategory.CatchUpSummary, NotificationPriority.Normal,
            now, null, "m16:invalid"));
    }
}

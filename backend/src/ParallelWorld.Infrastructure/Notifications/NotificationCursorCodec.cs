using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using ParallelWorld.Application.Notifications;

namespace ParallelWorld.Infrastructure.Notifications;

internal sealed class NotificationCursorCodec(IDataProtectionProvider provider) : INotificationCursorCodec
{
    private readonly IDataProtector protector = provider.CreateProtector("ParallelWorld.Notifications.Cursor.v1");

    public string Encode(Guid worldId, NotificationCursorValue value) =>
        protector.Protect($"1|{worldId:N}|{value.CreatedAtUtc.UtcTicks}|{value.Id:N}");

    public NotificationCursorDecodeResult Decode(string? cursor, Guid worldId)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return new(true, null);
        try
        {
            var values = protector.Unprotect(cursor).Split('|');
            if (values.Length != 4 || values[0] != "1"
                || !Guid.TryParseExact(values[1], "N", out var scopedWorldId) || scopedWorldId != worldId
                || !long.TryParse(values[2], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                || ticks < DateTimeOffset.MinValue.UtcTicks || ticks > DateTimeOffset.MaxValue.UtcTicks
                || !Guid.TryParseExact(values[3], "N", out var id))
                return new(false, null);
            return new(true, new(new DateTimeOffset(ticks, TimeSpan.Zero), id));
        }
        catch (CryptographicException)
        {
            return new(false, null);
        }
    }
}

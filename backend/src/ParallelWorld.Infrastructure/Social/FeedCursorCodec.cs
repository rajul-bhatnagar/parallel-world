using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using ParallelWorld.Application.Social;

namespace ParallelWorld.Infrastructure.Social;

internal sealed class FeedCursorCodec(IDataProtectionProvider dataProtectionProvider) : IFeedCursorCodec
{
    private const int Version = 1;
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        "ParallelWorld.Social.FeedCursor.v1");

    public string Encode(Guid worldId, FeedCursorValue value) => _protector.Protect(
        FormattableString.Invariant(
            $"{Version}|{worldId:N}|{value.CreatedAtUtc.UtcTicks}|{value.Id:N}"));

    public FeedCursorDecodeResult Decode(string? cursor, Guid worldId)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return new(true, null);
        }

        try
        {
            var values = _protector.Unprotect(cursor).Split('|');
            if (values.Length != 4
                || !int.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var version)
                || version != Version
                || !Guid.TryParseExact(values[1], "N", out var cursorWorldId)
                || cursorWorldId != worldId
                || !long.TryParse(values[2], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                || ticks < DateTimeOffset.MinValue.UtcTicks
                || ticks > DateTimeOffset.MaxValue.UtcTicks
                || !Guid.TryParseExact(values[3], "N", out var postId))
            {
                return new(false, null);
            }

            return new(true, new FeedCursorValue(new DateTimeOffset(ticks, TimeSpan.Zero), postId));
        }
        catch (CryptographicException)
        {
            return new(false, null);
        }
    }
}

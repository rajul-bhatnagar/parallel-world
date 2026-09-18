using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using ParallelWorld.Application.Social;

namespace ParallelWorld.Infrastructure.Social;

internal sealed class ReplyCursorCodec(IDataProtectionProvider dataProtectionProvider) : IReplyCursorCodec
{
    private const int Version = 1;
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(
        "ParallelWorld.Social.ReplyCursor.v1");

    public string Encode(Guid worldId, Guid parentPostId, ReplyCursorValue value) => _protector.Protect(
        FormattableString.Invariant(
            $"{Version}|{worldId:N}|{parentPostId:N}|{value.CreatedAtUtc.UtcTicks}|{value.Id:N}"));

    public ReplyCursorDecodeResult Decode(string? cursor, Guid worldId, Guid parentPostId)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return new(true, null);
        }

        try
        {
            var values = _protector.Unprotect(cursor).Split('|');
            if (values.Length != 5
                || !int.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var version)
                || version != Version
                || !Guid.TryParseExact(values[1], "N", out var cursorWorldId)
                || cursorWorldId != worldId
                || !Guid.TryParseExact(values[2], "N", out var cursorParentId)
                || cursorParentId != parentPostId
                || !long.TryParse(values[3], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                || ticks < DateTimeOffset.MinValue.UtcTicks
                || ticks > DateTimeOffset.MaxValue.UtcTicks
                || !Guid.TryParseExact(values[4], "N", out var postId))
            {
                return new(false, null);
            }

            return new(true, new ReplyCursorValue(new DateTimeOffset(ticks, TimeSpan.Zero), postId));
        }
        catch (CryptographicException)
        {
            return new(false, null);
        }
    }
}

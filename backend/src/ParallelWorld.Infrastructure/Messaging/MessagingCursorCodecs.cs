using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using ParallelWorld.Application.Messaging;

namespace ParallelWorld.Infrastructure.Messaging;

internal sealed class ConversationCursorCodec(IDataProtectionProvider provider) : IConversationCursorCodec
{
    private readonly IDataProtector protector = provider.CreateProtector("ParallelWorld.Messaging.ConversationCursor.v1");
    public string Encode(Guid worldId, ConversationCursorValue value) => protector.Protect($"1|{worldId:N}|{value.LastMessageAtUtc.UtcTicks}|{value.Id:N}");
    public CursorDecodeResult<ConversationCursorValue> Decode(string? cursor, Guid worldId)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return new(true, null);
        try
        {
            var v = protector.Unprotect(cursor).Split('|');
            if (v.Length != 4 || v[0] != "1" || !Guid.TryParseExact(v[1], "N", out var w) || w != worldId
                || !long.TryParse(v[2], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                || ticks < DateTimeOffset.MinValue.UtcTicks || ticks > DateTimeOffset.MaxValue.UtcTicks
                || !Guid.TryParseExact(v[3], "N", out var id)) return new(false, null);
            return new(true, new(new DateTimeOffset(ticks, TimeSpan.Zero), id));
        }
        catch (CryptographicException) { return new(false, null); }
    }
}

internal sealed class MessageCursorCodec(IDataProtectionProvider provider) : IMessageCursorCodec
{
    private readonly IDataProtector protector = provider.CreateProtector("ParallelWorld.Messaging.MessageCursor.v1");
    public string Encode(Guid worldId, Guid conversationId, MessageCursorValue value) => protector.Protect($"1|{worldId:N}|{conversationId:N}|{value.CreatedAtUtc.UtcTicks}|{value.Id:N}");
    public CursorDecodeResult<MessageCursorValue> Decode(string? cursor, Guid worldId, Guid conversationId)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return new(true, null);
        try
        {
            var v = protector.Unprotect(cursor).Split('|');
            if (v.Length != 5 || v[0] != "1" || !Guid.TryParseExact(v[1], "N", out var w) || w != worldId
                || !Guid.TryParseExact(v[2], "N", out var c) || c != conversationId
                || !long.TryParse(v[3], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                || ticks < DateTimeOffset.MinValue.UtcTicks || ticks > DateTimeOffset.MaxValue.UtcTicks
                || !Guid.TryParseExact(v[4], "N", out var id)) return new(false, null);
            return new(true, new(new DateTimeOffset(ticks, TimeSpan.Zero), id));
        }
        catch (CryptographicException) { return new(false, null); }
    }
}

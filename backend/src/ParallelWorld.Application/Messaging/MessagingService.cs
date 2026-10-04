using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ParallelWorld.Application.Memory;
using ParallelWorld.Domain.Memory;

namespace ParallelWorld.Application.Messaging;

public sealed class MessagingService(IMessagingRepository repository, IConversationCursorCodec conversationCursors,
    IMessageCursorCodec messageCursors, IMessageWordingGenerator wordingGenerator,
    IMemoryService memoryService) : IMessagingService
{
    public async Task<MessagingResult<ConversationPage>> ListAsync(Guid userId, Guid worldId, int limit, string? cursor, CancellationToken ct)
    {
        if (limit is < 1 or > 50) return Invalid<ConversationPage>("Limit must be between 1 and 50.");
        var decoded = conversationCursors.Decode(cursor, worldId);
        if (!decoded.IsValid) return Invalid<ConversationPage>("The cursor is invalid.", "invalid_cursor");
        if (!await repository.OwnsWorldAsync(userId, worldId, ct)) return Missing<ConversationPage>();
        var rows = await repository.ListAsync(userId, worldId, decoded.Value, limit + 1, ct);
        var hasMore = rows.Items.Count > limit;
        var items = rows.Items.Take(limit).ToArray();
        var next = hasMore && items.Length > 0 ? conversationCursors.Encode(worldId, new(items[^1].LastMessageAtUtc, items[^1].Id)) : null;
        return MessagingResult<ConversationPage>.Success(new(items, next, hasMore));
    }

    public async Task<MessagingResult<DirectConversationResult>> CreateOrGetDirectAsync(Guid userId, Guid worldId, Guid characterId, string key, CancellationToken ct)
    {
        if (characterId == Guid.Empty || !ValidKey(key)) return Invalid<DirectConversationResult>("A valid character and idempotency key are required.");
        var hash = Hash($"direct|{worldId:N}|{characterId:N}");
        try
        {
            var result = await repository.CreateOrGetDirectAsync(userId, worldId, characterId, key, hash, ct);
            return result is null ? Missing<DirectConversationResult>() : MessagingResult<DirectConversationResult>.Success(result);
        }
        catch (MessagingIdempotencyConflictException) { return Conflict<DirectConversationResult>("The idempotency key was already used for another request.", "idempotency_key_reused"); }
    }

    public async Task<MessagingResult<ConversationDetails>> GetAsync(Guid userId, Guid worldId, Guid conversationId, CancellationToken ct)
    {
        var value = await repository.FindAsync(userId, worldId, conversationId, ct);
        return value is null ? Missing<ConversationDetails>() : MessagingResult<ConversationDetails>.Success(value);
    }

    public async Task<MessagingResult<MessagePage>> GetMessagesAsync(Guid userId, Guid worldId, Guid conversationId, int limit, string? cursor, CancellationToken ct)
    {
        if (limit is < 1 or > 50) return Invalid<MessagePage>("Limit must be between 1 and 50.");
        var decoded = messageCursors.Decode(cursor, worldId, conversationId);
        if (!decoded.IsValid) return Invalid<MessagePage>("The cursor is invalid.", "invalid_cursor");
        var rows = await repository.ListMessagesAsync(userId, worldId, conversationId, decoded.Value, limit + 1, ct);
        if (rows is null) return Missing<MessagePage>();
        var hasMore = rows.Count > limit;
        var items = rows.Take(limit).ToArray();
        var next = hasMore && items.Length > 0 ? messageCursors.Encode(worldId, conversationId, new(items[^1].CreatedAtUtc, items[^1].Id)) : null;
        return MessagingResult<MessagePage>.Success(new(items, next, hasMore));
    }

    public async Task<MessagingResult<SendMessageResult>> SendAsync(Guid userId, Guid worldId, Guid conversationId, string body, Guid clientMessageId, string key, CancellationToken ct)
    {
        var normalized = body?.Trim() ?? string.Empty;
        if (clientMessageId == Guid.Empty || !ValidKey(key) || string.IsNullOrWhiteSpace(normalized)
            || new StringInfo(normalized).LengthInTextElements > Domain.Messaging.Message.MaximumContentCharacters)
            return Invalid<SendMessageResult>("Body, clientMessageId, and idempotency key are required; body must not exceed 2000 characters.");
        if (!string.Equals(key, clientMessageId.ToString(), StringComparison.OrdinalIgnoreCase)
            && !string.Equals(key, clientMessageId.ToString("N"), StringComparison.OrdinalIgnoreCase))
            return Invalid<SendMessageResult>("The idempotency key must identify the client message.");
        PersistedSend? persisted;
        try { persisted = await repository.SendAsync(userId, worldId, conversationId, normalized, clientMessageId, ct); }
        catch (MessagingIdempotencyConflictException) { return Conflict<SendMessageResult>("The client message ID was already used with different content.", "client_message_id_reused"); }
        if (persisted is null) return Missing<SendMessageResult>();
        var status = persisted.CharacterReplyStatus;
        if (persisted.WordingRequest is not null)
        {
            _ = await memoryService.CreateAsync(new CreateMemoryCommand(
                persisted.WordingRequest.WorldId,
                persisted.WordingRequest.CharacterId,
                MemoryType.Event,
                MemoryAuthorityType.GameplayEvent,
                MemorySubjectType.Actor,
                persisted.WordingRequest.SubjectActorId,
                null,
                null,
                "Received and replied to a private message from the player.",
                MemorySourceType.GameplayEvent,
                persisted.WordingRequest.SourceGameplayEventId,
                null), ct);
            var memoryContext = await memoryService.RecallForMessageWordingAsync(
                persisted.WordingRequest.WorldId,
                persisted.WordingRequest.CharacterId,
                persisted.WordingRequest.SubjectActorId,
                persisted.WordingRequest.PlannedReplyId,
                ct);
            var wordingRequest = persisted.WordingRequest with { MemoryContext = memoryContext };
            var wording = await wordingGenerator.GenerateAsync(wordingRequest, ct);
            status = await repository.FinalizeReplyAsync(userId, persisted.PlannedReplyId, wording, ct) ?? status;
        }
        return MessagingResult<SendMessageResult>.Success(new(persisted.Message, status, persisted.IsReplay));
    }

    public async Task<MessagingResult<bool>> MarkReadAsync(Guid userId, Guid worldId, Guid conversationId, Guid messageId, CancellationToken ct) =>
        await repository.MarkReadAsync(userId, worldId, conversationId, messageId, ct)
            ? MessagingResult<bool>.Success(true) : Missing<bool>();

    private static bool ValidKey(string value) => value.Length is >= 8 and <= 100 && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or ':' or '-');
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static MessagingResult<T> Invalid<T>(string title, string code = "validation_failed") => MessagingResult<T>.Fail(new(code, 400, title));
    private static MessagingResult<T> Missing<T>() => MessagingResult<T>.Fail(new("resource_not_available", 404, "The requested resource is not available."));
    private static MessagingResult<T> Conflict<T>(string title, string code) => MessagingResult<T>.Fail(new(code, 409, title));
}

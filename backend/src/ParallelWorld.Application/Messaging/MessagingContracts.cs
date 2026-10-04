using ParallelWorld.Application.Common;

namespace ParallelWorld.Application.Messaging;

public sealed record ConversationCharacter(Guid ActorId, Guid CharacterId, string DisplayName, string Handle);
public sealed record ConversationMessage(Guid Id, Guid ConversationId, Guid SenderActorId, string SenderType,
    string Body, DateTimeOffset CreatedAtUtc, string DeliveryStatus, Guid? ClientMessageId);
public sealed record ConversationSummary(Guid Id, string Type, ConversationCharacter Character,
    DateTimeOffset CreatedAtUtc, DateTimeOffset LastMessageAtUtc, string? LastMessagePreview,
    int UnreadCount, string? CharacterReplyStatus);
public sealed record ConversationDetails(ConversationSummary Conversation, Guid PlayerActorId,
    Guid? LastReadMessageId, DateTimeOffset? LastReadAtUtc);
public sealed record ConversationPage(IReadOnlyList<ConversationSummary> Items, string? NextCursor, bool HasMore);
public sealed record MessagePage(IReadOnlyList<ConversationMessage> Items, string? NextCursor, bool HasMore);
public sealed record DirectConversationResult(ConversationDetails Conversation, bool Created);
public sealed record SendMessageResult(ConversationMessage Message, string CharacterReplyStatus, bool IsReplay);
public sealed record MessagingResult<T>(T? Value, ServiceFailure? Failure)
{
    public bool IsSuccess => Failure is null;
    public static MessagingResult<T> Success(T value) => new(value, null);
    public static MessagingResult<T> Fail(ServiceFailure failure) => new(default, failure);
}
public sealed class MessagingIdempotencyConflictException : Exception { }

public readonly record struct ConversationCursorValue(DateTimeOffset LastMessageAtUtc, Guid Id);
public readonly record struct MessageCursorValue(DateTimeOffset CreatedAtUtc, Guid Id);
public readonly record struct CursorDecodeResult<T>(bool IsValid, T? Value) where T : struct;

public interface IConversationCursorCodec
{
    string Encode(Guid worldId, ConversationCursorValue value);
    CursorDecodeResult<ConversationCursorValue> Decode(string? cursor, Guid worldId);
}

public interface IMessageCursorCodec
{
    string Encode(Guid worldId, Guid conversationId, MessageCursorValue value);
    CursorDecodeResult<MessageCursorValue> Decode(string? cursor, Guid worldId, Guid conversationId);
}

public sealed record PersistedSend(ConversationMessage Message, Guid PlannedReplyId,
    string CharacterReplyStatus, MessageWordingRequest? WordingRequest, bool IsReplay);
public sealed record MessageWordingRequest(Guid WorldId, Guid ConversationId, Guid PlannedReplyId,
    Guid CharacterActorId, string CharacterDisplayName, string PlayerDisplayName, string VisibleMood,
    string StyleHint, string SourceMessageBody, int MaxOutputLength, Guid CharacterId,
    Guid SubjectActorId, Guid SourceGameplayEventId, IReadOnlyList<string> MemoryContext);
public sealed record MessageWordingResult(string Text, bool FallbackUsed, string? FailureCode);
public sealed record PendingMessageWordingWork(Guid OwnerUserId, MessageWordingRequest Request);

public interface IMessageWordingGenerator
{
    Task<MessageWordingResult> GenerateAsync(MessageWordingRequest request, CancellationToken cancellationToken);
}

public interface IMessagingRepository
{
    Task<bool> OwnsWorldAsync(Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<ConversationPageData> ListAsync(Guid userId, Guid worldId, ConversationCursorValue? after, int take, CancellationToken cancellationToken);
    Task<DirectConversationResult?> CreateOrGetDirectAsync(Guid userId, Guid worldId, Guid characterId, string idempotencyKey, string requestHash, CancellationToken cancellationToken);
    Task<ConversationDetails?> FindAsync(Guid userId, Guid worldId, Guid conversationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConversationMessage>?> ListMessagesAsync(Guid userId, Guid worldId, Guid conversationId, MessageCursorValue? after, int take, CancellationToken cancellationToken);
    Task<PersistedSend?> SendAsync(Guid userId, Guid worldId, Guid conversationId, string body, Guid clientMessageId, CancellationToken cancellationToken);
    Task<PendingMessageWordingWork?> GetPendingWordingWorkAsync(CancellationToken cancellationToken);
    Task<string?> FinalizeReplyAsync(Guid userId, Guid plannedReplyId, MessageWordingResult wording, CancellationToken cancellationToken);
    Task<bool> MarkReadAsync(Guid userId, Guid worldId, Guid conversationId, Guid messageId, CancellationToken cancellationToken);
}

public sealed record ConversationPageData(IReadOnlyList<ConversationSummary> Items);

public interface IMessagingService
{
    Task<MessagingResult<ConversationPage>> ListAsync(Guid userId, Guid worldId, int limit, string? cursor, CancellationToken cancellationToken);
    Task<MessagingResult<DirectConversationResult>> CreateOrGetDirectAsync(Guid userId, Guid worldId, Guid characterId, string idempotencyKey, CancellationToken cancellationToken);
    Task<MessagingResult<ConversationDetails>> GetAsync(Guid userId, Guid worldId, Guid conversationId, CancellationToken cancellationToken);
    Task<MessagingResult<MessagePage>> GetMessagesAsync(Guid userId, Guid worldId, Guid conversationId, int limit, string? cursor, CancellationToken cancellationToken);
    Task<MessagingResult<SendMessageResult>> SendAsync(Guid userId, Guid worldId, Guid conversationId, string body, Guid clientMessageId, string idempotencyKey, CancellationToken cancellationToken);
    Task<MessagingResult<bool>> MarkReadAsync(Guid userId, Guid worldId, Guid conversationId, Guid messageId, CancellationToken cancellationToken);
}

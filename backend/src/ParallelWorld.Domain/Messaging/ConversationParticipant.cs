namespace ParallelWorld.Domain.Messaging;

public sealed class ConversationParticipant
{
    private ConversationParticipant() { }

    public ConversationParticipant(Guid worldId, Guid conversationId, Guid actorId, DateTimeOffset joinedAt)
    {
        WorldId = worldId;
        ConversationId = conversationId;
        ActorId = actorId;
        JoinedAt = joinedAt;
    }

    public Guid WorldId { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid ActorId { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public DateTimeOffset? LeftAt { get; private set; }
    public Guid? LastReadMessageId { get; private set; }
    public DateTimeOffset? LastReadAt { get; private set; }

    public void MarkRead(Guid messageId, DateTimeOffset readAt)
    {
        LastReadMessageId = messageId;
        LastReadAt = readAt;
    }
}

using System.Globalization;

namespace ParallelWorld.Domain.Messaging;

public sealed class Message
{
    public const int MaximumContentCharacters = 2000;
    private Message() { Content = string.Empty; }

    public Message(Guid id, Guid worldId, Guid conversationId, Guid senderActorId,
        Guid gameplayEventId, Guid? simulationActionId, Guid? clientOperationId,
        string content, MessageDeliveryStatus deliveryStatus, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        var normalized = content.Trim();
        if (new StringInfo(normalized).LengthInTextElements > MaximumContentCharacters)
            throw new ArgumentOutOfRangeException(nameof(content));
        Id = id;
        WorldId = worldId;
        ConversationId = conversationId;
        SenderActorId = senderActorId;
        GameplayEventId = gameplayEventId;
        SimulationActionId = simulationActionId;
        ClientOperationId = clientOperationId;
        Content = normalized;
        DeliveryStatus = deliveryStatus;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SenderActorId { get; private set; }
    public Guid GameplayEventId { get; private set; }
    public Guid? SimulationActionId { get; private set; }
    public Guid? ClientOperationId { get; private set; }
    public string Content { get; private set; }
    public MessageDeliveryStatus DeliveryStatus { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? EditedAt { get; private set; }
    public long Version { get; private set; }
}

public enum MessageDeliveryStatus { Delivered }

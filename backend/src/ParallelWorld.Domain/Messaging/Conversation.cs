namespace ParallelWorld.Domain.Messaging;

public sealed class Conversation
{
    private Conversation() { }

    public Conversation(Guid id, Guid worldId, Guid playerActorId, Guid characterActorId, DateTimeOffset createdAt)
    {
        if (playerActorId == characterActorId) throw new ArgumentException("Direct participants must be distinct.");
        Id = id;
        WorldId = worldId;
        ConversationType = ConversationType.Direct;
        PlayerActorId = playerActorId;
        CharacterActorId = characterActorId;
        CreatedAt = createdAt;
        LastMessageAt = createdAt;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public ConversationType ConversationType { get; private set; }
    public Guid PlayerActorId { get; private set; }
    public Guid CharacterActorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastMessageAt { get; private set; }
    public bool IsActive { get; private set; }
    public long Version { get; private set; }

    public void RecordMessage(DateTimeOffset createdAt)
    {
        if (createdAt > LastMessageAt) LastMessageAt = createdAt;
        Version++;
    }
}

public enum ConversationType { Direct }

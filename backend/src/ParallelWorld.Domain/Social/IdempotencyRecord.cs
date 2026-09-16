namespace ParallelWorld.Domain.Social;

public sealed class IdempotencyRecord
{
    private IdempotencyRecord()
    {
        Operation = string.Empty;
        IdempotencyKey = string.Empty;
        RequestHash = string.Empty;
        Status = string.Empty;
    }

    public IdempotencyRecord(
        Guid id,
        Guid userId,
        Guid worldId,
        string operation,
        string idempotencyKey,
        string requestHash,
        Guid responseResourceId,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestHash);

        Id = id;
        UserId = userId;
        WorldId = worldId;
        Operation = operation;
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;
        Status = "completed";
        ResponseCode = 201;
        ResponseResourceId = responseResourceId;
        CreatedAt = createdAt;
        ExpiresAt = DateTimeOffset.MaxValue;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? WorldId { get; private set; }
    public string Operation { get; private set; }
    public string IdempotencyKey { get; private set; }
    public string RequestHash { get; private set; }
    public string Status { get; private set; }
    public int ResponseCode { get; private set; }
    public Guid ResponseResourceId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public long Version { get; private set; }
}

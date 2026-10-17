namespace ParallelWorld.Domain.Accounts;

public sealed class AccountRecoveryCode
{
    private AccountRecoveryCode() => CodeHash = string.Empty;

    public AccountRecoveryCode(Guid id, Guid userId, string codeHash, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        CodeHash = codeHash;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public long Version { get; private set; }
    public bool IsActive => ConsumedAt is null && RevokedAt is null;

    public void Consume(DateTimeOffset now) { ConsumedAt ??= now; Version++; }
    public void Revoke(DateTimeOffset now) { RevokedAt ??= now; Version++; }
}

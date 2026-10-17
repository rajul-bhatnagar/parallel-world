namespace ParallelWorld.Domain.Accounts;

public sealed class RefreshToken
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(
        Guid id,
        Guid userId,
        Guid deviceInstallationId,
        string tokenHash,
        Guid rotationFamilyId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        DeviceInstallationId = deviceInstallationId;
        TokenHash = tokenHash;
        RotationFamilyId = rotationFamilyId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid DeviceInstallationId { get; private set; }

    public string TokenHash { get; private set; }

    public Guid RotationFamilyId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsActive(DateTimeOffset now) =>
        ConsumedAt is null && RevokedAt is null && now < ExpiresAt;

    public void Consume(Guid replacementTokenId, DateTimeOffset consumedAt)
    {
        ConsumedAt = consumedAt;
        ReplacedByTokenId = replacementTokenId;
    }

    public void Revoke(DateTimeOffset revokedAt) => RevokedAt ??= revokedAt;
}

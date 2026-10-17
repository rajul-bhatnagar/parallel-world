namespace ParallelWorld.Domain.Accounts;

public sealed class GuestBootstrapOperation
{
    private GuestBootstrapOperation()
    {
        ProofHash = string.Empty;
    }

    public GuestBootstrapOperation(
        Guid id,
        string proofHash,
        Guid userId,
        Guid deviceInstallationId,
        Guid refreshTokenFamilyId,
        DateTimeOffset createdAt,
        DateTimeOffset completedAt,
        DateTimeOffset expiresAt)
    {
        Id = id;
        ProofHash = proofHash;
        UserId = userId;
        DeviceInstallationId = deviceInstallationId;
        RefreshTokenFamilyId = refreshTokenFamilyId;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }

    public string ProofHash { get; private set; }

    public Guid UserId { get; private set; }

    public Guid DeviceInstallationId { get; private set; }

    public Guid RefreshTokenFamilyId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? RecoveryConsumedAt { get; private set; }

    public bool CanRecover(DateTimeOffset now) =>
        CompletedAt is not null && RecoveryConsumedAt is null && now <= ExpiresAt;

    public void ConsumeRecovery(DateTimeOffset consumedAt)
    {
        if (!CanRecover(consumedAt))
        {
            throw new InvalidOperationException("The bootstrap recovery is not available.");
        }

        RecoveryConsumedAt = consumedAt;
    }
}

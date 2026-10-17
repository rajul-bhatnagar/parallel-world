using ParallelWorld.Domain.Accounts;

namespace ParallelWorld.UnitTests;

public sealed class AuthenticationDomainTests
{
    [Fact]
    public void BootstrapRecovery_IsAvailableOnceThroughTheExactExpiryBoundary()
    {
        var completedAt = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);
        var operation = new GuestBootstrapOperation(
            Guid.NewGuid(),
            "hash-only",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            completedAt,
            completedAt,
            completedAt.AddMinutes(10));

        Assert.True(operation.CanRecover(completedAt.AddMinutes(10)));
        operation.ConsumeRecovery(completedAt.AddMinutes(10));
        Assert.False(operation.CanRecover(completedAt.AddMinutes(10)));
        Assert.Throws<InvalidOperationException>(() => operation.ConsumeRecovery(completedAt));
    }

    [Fact]
    public void BootstrapRecovery_AfterExpiry_IsRejected()
    {
        var completedAt = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);
        var operation = new GuestBootstrapOperation(
            Guid.NewGuid(),
            "hash-only",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            completedAt,
            completedAt,
            completedAt.AddMinutes(10));

        Assert.False(operation.CanRecover(completedAt.AddMinutes(10).AddTicks(1)));
    }

    [Fact]
    public void RefreshToken_ConsumptionAndRevocationAreTerminalForActivity()
    {
        var createdAt = new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);
        var token = new RefreshToken(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hash-only",
            Guid.NewGuid(),
            createdAt,
            createdAt.AddDays(30));

        Assert.True(token.IsActive(createdAt));
        var replacementId = Guid.NewGuid();
        token.Consume(replacementId, createdAt.AddMinutes(1));
        token.Revoke(createdAt.AddMinutes(2));

        Assert.False(token.IsActive(createdAt.AddMinutes(2)));
        Assert.Equal(replacementId, token.ReplacedByTokenId);
        Assert.Equal(createdAt.AddMinutes(1), token.ConsumedAt);
        Assert.Equal(createdAt.AddMinutes(2), token.RevokedAt);
    }
}

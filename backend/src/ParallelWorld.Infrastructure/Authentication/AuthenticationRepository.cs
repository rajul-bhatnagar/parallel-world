using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Domain.Accounts;
using ParallelWorld.Domain.Social;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Authentication;

internal sealed class AuthenticationRepository(ParallelWorldDbContext dbContext)
    : IAuthenticationRepository
{
    public Task<GuestBootstrapOperation?> LockBootstrapAsync(
        string proofHash,
        CancellationToken cancellationToken) => dbContext.GuestBootstrapOperations
        .FromSqlInterpolated(
            $"SELECT * FROM guest_bootstrap_operations WHERE proof_hash = {proofHash} FOR UPDATE")
        .SingleOrDefaultAsync(cancellationToken);

    public Task<Guid?> FindRefreshTokenUserIdAsync(
        string tokenHash,
        CancellationToken cancellationToken) => dbContext.RefreshTokens
        .AsNoTracking()
        .Where(entity => entity.TokenHash == tokenHash)
        .Select(entity => (Guid?)entity.UserId)
        .SingleOrDefaultAsync(cancellationToken);

    public async Task LockUserAsync(Guid userId, CancellationToken cancellationToken) =>
        _ = await dbContext.Users
            .FromSqlInterpolated($"SELECT * FROM users WHERE id = {userId} FOR UPDATE")
            .SingleAsync(cancellationToken);

    public Task<User?> FindUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<User?> LockUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Users.FromSqlInterpolated($"SELECT * FROM users WHERE normalized_email = {normalizedEmail} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users.SingleAsync(x => x.Id == userId, cancellationToken);

    public Task<DeviceInstallation?> LockInstallationAsync(Guid userId, string installationPublicId, CancellationToken cancellationToken) =>
        dbContext.DeviceInstallations.FromSqlInterpolated($"SELECT * FROM device_installations WHERE user_id = {userId} AND installation_public_id = {installationPublicId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<DeviceInstallation?> LockGuestInstallationAsync(string installationPublicId, CancellationToken cancellationToken) =>
        dbContext.DeviceInstallations.FromSqlInterpolated($"SELECT di.* FROM device_installations AS di INNER JOIN users AS u ON u.id = di.user_id WHERE di.installation_public_id = {installationPublicId} AND u.account_type = 'guest' FOR UPDATE OF di LIMIT 1")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<AccountRecoveryCode?> LockActiveRecoveryCodeAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.AccountRecoveryCodes.FromSqlInterpolated($"SELECT * FROM account_recovery_codes WHERE user_id = {userId} AND consumed_at IS NULL AND revoked_at IS NULL FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

    public Task<IdempotencyRecord?> FindUpgradeOperationAsync(Guid userId, string idempotencyKey, CancellationToken cancellationToken) =>
        dbContext.IdempotencyRecords.SingleOrDefaultAsync(x => x.UserId == userId && x.Operation == "auth.upgrade" && x.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<IReadOnlyList<SessionSummary>> ListSessionsAsync(Guid userId, Guid? currentFamilyId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var rows = await (from token in dbContext.RefreshTokens.AsNoTracking()
                          join installation in dbContext.DeviceInstallations.AsNoTracking() on token.DeviceInstallationId equals installation.Id
                          where token.UserId == userId && token.ConsumedAt == null && token.RevokedAt == null && token.ExpiresAt > now
                          orderby installation.LastSeenAt descending, token.RotationFamilyId
                          select new { token.RotationFamilyId, installation.Platform, installation.AppVersion, installation.LastSeenAt })
            .ToListAsync(cancellationToken);
        return rows.GroupBy(x => x.RotationFamilyId).Select(x => x.First())
            .Select(x => new SessionSummary(x.RotationFamilyId, x.Platform, x.AppVersion, x.LastSeenAt, currentFamilyId == x.RotationFamilyId)).ToList();
    }

    public Task<RefreshToken?> LockRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken) => dbContext.RefreshTokens
        .FromSqlInterpolated(
            $"SELECT * FROM refresh_tokens WHERE token_hash = {tokenHash} FOR UPDATE")
        .SingleOrDefaultAsync(cancellationToken);

    public Task<DeviceInstallation> GetInstallationAsync(
        Guid id,
        CancellationToken cancellationToken) => dbContext.DeviceInstallations
        .SingleAsync(entity => entity.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> LockFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken) => await dbContext.RefreshTokens
        .FromSqlInterpolated(
            $"SELECT * FROM refresh_tokens WHERE rotation_family_id = {familyId} AND revoked_at IS NULL FOR UPDATE")
        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> LockUnrevokedUserTokensAsync(
        Guid userId,
        CancellationToken cancellationToken) => await dbContext.RefreshTokens
        .FromSqlInterpolated(
            $"SELECT * FROM refresh_tokens WHERE user_id = {userId} AND revoked_at IS NULL FOR UPDATE")
        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> LockActiveFamilyHistoriesAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken) => await dbContext.RefreshTokens
        .FromSqlInterpolated($"""
            SELECT historical.*
            FROM refresh_tokens AS historical
            WHERE historical.user_id = {userId}
              AND EXISTS (
                  SELECT 1
                  FROM refresh_tokens AS active
                  WHERE active.user_id = {userId}
                    AND active.rotation_family_id = historical.rotation_family_id
                    AND active.consumed_at IS NULL
                    AND active.revoked_at IS NULL
                    AND active.expires_at > {now})
            FOR UPDATE
            """)
        .ToListAsync(cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);

    public void Add(DeviceInstallation installation) => dbContext.DeviceInstallations.Add(installation);

    public void Add(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

    public void Add(GuestBootstrapOperation operation) => dbContext.GuestBootstrapOperations.Add(operation);

    public void Add(AccountRecoveryCode recoveryCode) => dbContext.AccountRecoveryCodes.Add(recoveryCode);
    public void Add(IdempotencyRecord operation) => dbContext.IdempotencyRecords.Add(operation);
}

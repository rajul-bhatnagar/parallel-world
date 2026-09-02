using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Domain.Accounts;
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
}

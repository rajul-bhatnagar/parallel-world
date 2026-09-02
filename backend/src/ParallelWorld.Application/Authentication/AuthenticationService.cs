using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Common;
using ParallelWorld.Application.Worlds;
using ParallelWorld.Domain.Accounts;

namespace ParallelWorld.Application.Authentication;

public sealed class AuthenticationService(
    IAuthenticationRepository repository,
    IWorldRepository worldRepository,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier failureClassifier,
    IOpaqueSecretService secretService,
    IAccessTokenIssuer accessTokenIssuer,
    IAuthenticationRateLimiter rateLimiter,
    AuthenticationPolicy policy,
    TimeProvider timeProvider) : IAuthenticationService, ISessionAdministrationService
{
    private const int GuestLimit = 10;
    private const int RefreshFamilyLimit = 30;
    private const int InvalidRefreshLimit = 10;
    private const int LogoutLimit = 30;

    public async Task<AuthenticationResult<GuestSession>> BootstrapGuestAsync(
        GuestBootstrapCommand command,
        CancellationToken cancellationToken)
    {
        var rateDecision = rateLimiter.Acquire("guest", command.IpAddress, GuestLimit);
        if (!rateDecision.IsAllowed)
        {
            return AuthenticationResult<GuestSession>.Fail(RateLimited(rateDecision));
        }

        var proofHash = secretService.Hash(command.GuestBootstrapProof);
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                return await BootstrapOnceAsync(command, proofHash, cancellationToken);
            }
            catch (Exception exception) when (
                failureClassifier.HasConstraint(exception, "ux_guest_bootstrap_operations_proof_hash")
                || failureClassifier.IsSerializationFailure(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
            catch (Exception exception) when (
                failureClassifier.HasConstraint(exception, "ux_device_installations_public_id"))
            {
                return AuthenticationResult<GuestSession>.Fail(new ServiceFailure(
                    "duplicate_request",
                    409,
                    "The guest bootstrap could not be completed."));
            }
        }

        return AuthenticationResult<GuestSession>.Fail(new ServiceFailure(
            "concurrency_conflict",
            409,
            "The guest bootstrap could not be completed concurrently."));
    }

    public async Task<AuthenticationResult<TokenPair>> RefreshAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                return await RefreshOnceAsync(command, cancellationToken);
            }
            catch (Exception exception) when (failureClassifier.IsSerializationFailure(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return AuthenticationResult<TokenPair>.Fail(new ServiceFailure(
            "concurrency_conflict",
            409,
            "The refresh credential could not be processed concurrently."));
    }

    public async Task<AuthenticationResult<bool>> LogoutAsync(
        LogoutSessionCommand command,
        CancellationToken cancellationToken)
    {
        var rateDecision = rateLimiter.Acquire("logout", command.UserId.ToString(), LogoutLimit);
        if (!rateDecision.IsAllowed)
        {
            return AuthenticationResult<bool>.Fail(RateLimited(rateDecision));
        }

        var now = timeProvider.GetUtcNow();
        var tokenHash = secretService.Hash(command.RefreshToken);
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken);
        await repository.LockUserAsync(command.UserId, cancellationToken);
        var token = await repository.LockRefreshTokenAsync(tokenHash, cancellationToken);
        if (token is null || token.UserId != command.UserId)
        {
            return AuthenticationResult<bool>.Fail(new ServiceFailure(
                "resource_not_available",
                404,
                "The requested session is not available."));
        }

        await RevokeFamilyAsync(token.RotationFamilyId, now, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return AuthenticationResult<bool>.Success(true);
    }

    public async Task RevokeAllFamiliesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken);
        await repository.LockUserAsync(userId, cancellationToken);
        foreach (var token in await repository.LockUnrevokedUserTokensAsync(userId, cancellationToken))
        {
            token.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<TokenPair> CreateFamilyAsync(
        Guid userId,
        string installationId,
        string platform,
        string appVersion,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken);
        await repository.LockUserAsync(userId, cancellationToken);

        var activeFamilyHistories = await repository.LockActiveFamilyHistoriesAsync(
            userId,
            now,
            cancellationToken);
        var activeFamilies = activeFamilyHistories
            .GroupBy(entity => entity.RotationFamilyId)
            .OrderBy(group => group.Min(entity => entity.CreatedAt))
            .ToList();
        if (activeFamilies.Count >= 5)
        {
            foreach (var token in activeFamilies[0].Where(entity => entity.IsActive(now)))
            {
                token.Revoke(now);
            }
        }

        var deviceId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var rawRefreshToken = secretService.Generate();
        repository.Add(new DeviceInstallation(
            deviceId,
            userId,
            installationId,
            platform,
            appVersion,
            now));
        var refreshToken = CreateRefreshToken(userId, deviceId, familyId, rawRefreshToken, now);
        repository.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return CreateTokenPair(userId, rawRefreshToken, refreshToken.ExpiresAt);
    }

    private async Task<AuthenticationResult<TokenPair>> RefreshOnceAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var tokenHash = secretService.Hash(command.RefreshToken);
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.ReadCommitted,
            cancellationToken);
        var userId = await repository.FindRefreshTokenUserIdAsync(tokenHash, cancellationToken);
        if (userId is null)
        {
            return InvalidRefreshFailure(command.IpAddress);
        }

        await repository.LockUserAsync(userId.Value, cancellationToken);
        var token = await repository.LockRefreshTokenAsync(tokenHash, cancellationToken)
            ?? throw new InvalidOperationException("Refresh token disappeared while its user was locked.");

        if (token.ConsumedAt is not null || token.ReplacedByTokenId is not null)
        {
            await RevokeFamilyAsync(token.RotationFamilyId, now, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            var invalidRateDecision = rateLimiter.Acquire(
                "invalid-refresh",
                command.IpAddress,
                InvalidRefreshLimit);
            return invalidRateDecision.IsAllowed
                ? AuthenticationResult<TokenPair>.Fail(new ServiceFailure(
                    "refresh_token_reused",
                    401,
                    "The refresh credential was already used."))
                : AuthenticationResult<TokenPair>.Fail(RateLimited(invalidRateDecision));
        }

        if (token.RevokedAt is not null)
        {
            return InvalidRefreshFailure(command.IpAddress);
        }

        if (now >= token.ExpiresAt)
        {
            return InvalidRefreshFailure(command.IpAddress, new ServiceFailure(
                "refresh_token_expired",
                401,
                "The refresh credential has expired."));
        }

        var installation = await repository.GetInstallationAsync(
            token.DeviceInstallationId,
            cancellationToken);
        if (installation.RevokedAt is not null)
        {
            return InvalidRefreshFailure(command.IpAddress, new ServiceFailure(
                "installation_revoked",
                401,
                "The installation session is unavailable."));
        }

        var familyRateDecision = rateLimiter.Acquire(
            "refresh-family",
            token.RotationFamilyId.ToString(),
            RefreshFamilyLimit);
        if (!familyRateDecision.IsAllowed)
        {
            return AuthenticationResult<TokenPair>.Fail(RateLimited(familyRateDecision));
        }

        var replacementRaw = secretService.Generate();
        var replacement = CreateRefreshToken(
            token.UserId,
            token.DeviceInstallationId,
            token.RotationFamilyId,
            replacementRaw,
            now);
        token.Consume(replacement.Id, now);
        repository.Add(replacement);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return AuthenticationResult<TokenPair>.Success(CreateTokenPair(
            token.UserId,
            replacementRaw,
            replacement.ExpiresAt));
    }

    private async Task<AuthenticationResult<GuestSession>> BootstrapOnceAsync(
        GuestBootstrapCommand command,
        string proofHash,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await unitOfWork.BeginTransactionAsync(
            ApplicationIsolationLevel.Serializable,
            cancellationToken);
        var existing = await repository.LockBootstrapAsync(proofHash, cancellationToken);
        if (existing is not null)
        {
            return await RecoverBootstrapAsync(existing, now, transaction, cancellationToken);
        }

        var userId = Guid.NewGuid();
        var installationId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var rawRefreshToken = secretService.Generate();
        var refreshToken = CreateRefreshToken(userId, installationId, familyId, rawRefreshToken, now);
        var worldAggregate = WorldAggregateFactory.Create(userId, command.WorldName, now);
        var bootstrap = new GuestBootstrapOperation(
            Guid.NewGuid(),
            proofHash,
            userId,
            installationId,
            familyId,
            now,
            now,
            now.AddMinutes(policy.BootstrapRecoveryMinutes));

        repository.Add(new User(userId, now));
        repository.Add(new DeviceInstallation(
            installationId,
            userId,
            command.InstallationId,
            command.Platform,
            command.AppVersion,
            now));
        repository.Add(refreshToken);
        repository.Add(bootstrap);
        worldRepository.Add(worldAggregate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return AuthenticationResult<GuestSession>.Success(new GuestSession(
            CreateTokenPair(userId, rawRefreshToken, refreshToken.ExpiresAt),
            new AuthenticatedUser(userId, "guest"),
            ToSummary(worldAggregate),
            false));
    }

    private async Task<AuthenticationResult<GuestSession>> RecoverBootstrapAsync(
        GuestBootstrapOperation bootstrap,
        DateTimeOffset now,
        IApplicationTransaction transaction,
        CancellationToken cancellationToken)
    {
        if (!bootstrap.CanRecover(now))
        {
            return AuthenticationResult<GuestSession>.Fail(new ServiceFailure(
                "authentication_required",
                401,
                "The guest bootstrap recovery is unavailable."));
        }

        foreach (var token in (await repository.LockFamilyAsync(
            bootstrap.RefreshTokenFamilyId,
            cancellationToken)).Where(entity => entity.IsActive(now)))
        {
            token.Revoke(now);
        }

        var rawRefreshToken = secretService.Generate();
        var replacement = CreateRefreshToken(
            bootstrap.UserId,
            bootstrap.DeviceInstallationId,
            bootstrap.RefreshTokenFamilyId,
            rawRefreshToken,
            now);
        repository.Add(replacement);
        bootstrap.ConsumeRecovery(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var world = await worldRepository.FindAsync(bootstrap.UserId, null, cancellationToken)
            ?? throw new InvalidOperationException("Completed bootstrap world was not found.");
        return AuthenticationResult<GuestSession>.Success(new GuestSession(
            CreateTokenPair(bootstrap.UserId, rawRefreshToken, replacement.ExpiresAt),
            new AuthenticatedUser(bootstrap.UserId, "guest"),
            world,
            true));
    }

    private RefreshToken CreateRefreshToken(
        Guid userId,
        Guid deviceInstallationId,
        Guid familyId,
        string rawToken,
        DateTimeOffset now) => new(
            Guid.NewGuid(),
            userId,
            deviceInstallationId,
            secretService.Hash(rawToken),
            familyId,
            now,
            now.AddDays(policy.RefreshTokenLifetimeDays));

    private TokenPair CreateTokenPair(Guid userId, string rawRefreshToken, DateTimeOffset refreshExpiresAt)
    {
        var accessToken = accessTokenIssuer.Issue(userId);
        return new TokenPair(accessToken.Value, accessToken.ExpiresAt, rawRefreshToken, refreshExpiresAt);
    }

    private async Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var token in await repository.LockFamilyAsync(familyId, cancellationToken))
        {
            token.Revoke(now);
        }
    }

    private AuthenticationResult<TokenPair> InvalidRefreshFailure(
        string ipAddress,
        ServiceFailure? failure = null)
    {
        var decision = rateLimiter.Acquire("invalid-refresh", ipAddress, InvalidRefreshLimit);
        return decision.IsAllowed
            ? AuthenticationResult<TokenPair>.Fail(failure ?? new ServiceFailure(
                "authentication_required", 401, "The refresh credential is invalid."))
            : AuthenticationResult<TokenPair>.Fail(RateLimited(decision));
    }

    private static WorldSummary ToSummary(WorldAggregate aggregate) => new(
        aggregate.World.Id,
        aggregate.World.Name,
        aggregate.World.Status.ToString().ToLowerInvariant(),
        aggregate.World.CurrentWorldTime,
        new PlayerSummary(aggregate.PlayerActor.Id, aggregate.PlayerProfile.DisplayName),
        aggregate.World.CreatedAt);

    private static ServiceFailure RateLimited(RateLimitDecision decision) => new(
        "rate_limit_exceeded", 429, "Too many requests.", decision.RetryAfterSeconds);
}

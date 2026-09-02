using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Accounts;

namespace ParallelWorld.Application.Authentication;

public sealed record GuestBootstrapCommand(
    string InstallationId,
    string Platform,
    string AppVersion,
    string GuestBootstrapProof,
    string WorldName,
    string IpAddress);

public sealed record RefreshSessionCommand(string RefreshToken, string IpAddress);

public sealed record LogoutSessionCommand(Guid UserId, string RefreshToken, string IpAddress);

public sealed record TokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record AuthenticatedUser(Guid Id, string AccountType);

public sealed record PlayerSummary(Guid ActorId, string DisplayName);

public sealed record WorldSummary(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset CurrentGameTime,
    PlayerSummary Player,
    DateTimeOffset CreatedAt);

public sealed record GuestSession(
    TokenPair Tokens,
    AuthenticatedUser User,
    WorldSummary World,
    bool IsRecovery);

public sealed record AuthenticationResult<T>(T? Value, ServiceFailure? Failure)
{
    public bool IsSuccess => Failure is null;

    public static AuthenticationResult<T> Success(T value) => new(value, null);

    public static AuthenticationResult<T> Fail(ServiceFailure failure) => new(default, failure);
}

public interface IAuthenticationService
{
    Task<AuthenticationResult<GuestSession>> BootstrapGuestAsync(
        GuestBootstrapCommand command,
        CancellationToken cancellationToken);

    Task<AuthenticationResult<TokenPair>> RefreshAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken);

    Task<AuthenticationResult<bool>> LogoutAsync(
        LogoutSessionCommand command,
        CancellationToken cancellationToken);
}

public interface ISessionAdministrationService
{
    Task RevokeAllFamiliesAsync(Guid userId, CancellationToken cancellationToken);

    Task<TokenPair> CreateFamilyAsync(
        Guid userId,
        string installationId,
        string platform,
        string appVersion,
        CancellationToken cancellationToken);
}

public sealed record AuthenticationPolicy(
    int RefreshTokenLifetimeDays,
    int BootstrapRecoveryMinutes);

public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAt);

public sealed record RateLimitDecision(bool IsAllowed, int? RetryAfterSeconds);

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(Guid userId);
}

public interface IOpaqueSecretService
{
    string Generate();

    string Hash(string value);
}

public interface IAuthenticationRateLimiter
{
    RateLimitDecision Acquire(string policy, string partition, int permitLimit);
}

public interface IAuthenticationRepository
{
    Task<GuestBootstrapOperation?> LockBootstrapAsync(string proofHash, CancellationToken cancellationToken);

    Task<Guid?> FindRefreshTokenUserIdAsync(string tokenHash, CancellationToken cancellationToken);

    Task LockUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<RefreshToken?> LockRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);

    Task<DeviceInstallation> GetInstallationAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> LockFamilyAsync(Guid familyId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> LockUnrevokedUserTokensAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> LockActiveFamilyHistoriesAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    void Add(User user);

    void Add(DeviceInstallation installation);

    void Add(RefreshToken refreshToken);

    void Add(GuestBootstrapOperation operation);
}

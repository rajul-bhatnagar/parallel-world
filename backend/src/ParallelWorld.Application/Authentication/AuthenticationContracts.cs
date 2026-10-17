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
public sealed record RegisterAccountCommand(string Email, string Password, string IpAddress);
public sealed record LoginCommand(string Email, string Password, string InstallationId, string Platform, string AppVersion, string IpAddress);
public sealed record UpgradeGuestCommand(Guid UserId, string Email, string Password, string IdempotencyKey);
public sealed record RecoverAccountCommand(string Email, string RecoveryCode, string NewPassword, string IpAddress);
public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword);
public sealed record SessionSummary(Guid Id, string Platform, string AppVersion, DateTimeOffset LastSeenAt, bool IsCurrent);
public sealed record RegisteredAccount(AuthenticatedUser User, WorldSummary World, string? RecoveryCode);
public sealed record LoginSession(TokenPair Tokens, AuthenticatedUser User, WorldSummary World);

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

    Task<AuthenticationResult<RegisteredAccount>> RegisterAsync(RegisterAccountCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult<LoginSession>> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult<RegisteredAccount>> UpgradeAsync(UpgradeGuestCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult<string>> RecoverAsync(RecoverAccountCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult<bool>> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult<string>> RegenerateRecoveryCodeAsync(Guid userId, CancellationToken cancellationToken);
    Task<AuthenticationResult<IReadOnlyList<SessionSummary>>> ListSessionsAsync(Guid userId, Guid? currentFamilyId, CancellationToken cancellationToken);
    Task<AuthenticationResult<bool>> RevokeSessionAsync(Guid userId, Guid familyId, CancellationToken cancellationToken);
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
    IssuedAccessToken Issue(Guid userId, Guid familyId);
}

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string hash, string password);
    bool VerifyOrDummy(User? user, string? hash, string value);
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
    Task<User?> FindUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<User?> LockUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<DeviceInstallation?> LockInstallationAsync(Guid userId, string installationPublicId, CancellationToken cancellationToken);
    Task<DeviceInstallation?> LockGuestInstallationAsync(string installationPublicId, CancellationToken cancellationToken);
    Task<AccountRecoveryCode?> LockActiveRecoveryCodeAsync(Guid userId, CancellationToken cancellationToken);
    Task<ParallelWorld.Domain.Social.IdempotencyRecord?> FindUpgradeOperationAsync(Guid userId, string idempotencyKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<SessionSummary>> ListSessionsAsync(Guid userId, Guid? currentFamilyId, DateTimeOffset now, CancellationToken cancellationToken);

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
    void Add(AccountRecoveryCode recoveryCode);
    void Add(ParallelWorld.Domain.Social.IdempotencyRecord operation);
}

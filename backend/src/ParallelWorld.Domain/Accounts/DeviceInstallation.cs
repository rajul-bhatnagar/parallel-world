namespace ParallelWorld.Domain.Accounts;

public sealed class DeviceInstallation
{
    private DeviceInstallation()
    {
        InstallationPublicId = string.Empty;
        Platform = string.Empty;
        AppVersion = string.Empty;
    }

    public DeviceInstallation(
        Guid id,
        Guid userId,
        string installationPublicId,
        string platform,
        string appVersion,
        DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        InstallationPublicId = installationPublicId;
        Platform = platform;
        AppVersion = appVersion;
        LastSeenAt = createdAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string InstallationPublicId { get; private set; }

    public string Platform { get; private set; }

    public string AppVersion { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public void Revoke(DateTimeOffset revokedAt) => RevokedAt ??= revokedAt;
}

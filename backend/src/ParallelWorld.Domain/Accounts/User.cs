namespace ParallelWorld.Domain.Accounts;

public sealed class User
{
    private User()
    {
    }

    public User(Guid id, DateTimeOffset createdAt)
    {
        Id = id;
        AccountType = AccountType.Guest;
        Status = AccountStatus.Active;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public AccountType AccountType { get; private set; }

    public string? Email { get; private set; }

    public string? NormalizedEmail { get; private set; }

    public string? PasswordHash { get; private set; }

    public AccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }

    public bool IsRegistered => AccountType == AccountType.Registered;

    public void Register(string email, string normalizedEmail, string passwordHash, DateTimeOffset now)
    {
        if (IsRegistered)
        {
            throw new InvalidOperationException("The account is already registered.");
        }

        AccountType = AccountType.Registered;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        UpdatedAt = now;
        Version++;
    }

    public void ChangePassword(string passwordHash, DateTimeOffset now)
    {
        if (!IsRegistered)
        {
            throw new InvalidOperationException("Only registered accounts have passwords.");
        }

        PasswordHash = passwordHash;
        UpdatedAt = now;
        Version++;
    }
}

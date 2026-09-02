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
}

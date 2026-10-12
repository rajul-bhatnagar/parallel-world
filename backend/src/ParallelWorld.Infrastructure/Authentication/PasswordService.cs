using Microsoft.AspNetCore.Identity;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Domain.Accounts;

namespace ParallelWorld.Infrastructure.Authentication;

internal sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();
    private readonly User _dummyUser = new(Guid.Empty, DateTimeOffset.UnixEpoch);
    private readonly string _dummyHash;

    public PasswordService() => _dummyHash = _hasher.HashPassword(_dummyUser, "m17-stable-dummy-proof");

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);
    public bool Verify(User user, string hash, string password) =>
        _hasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Failed;

    public bool VerifyOrDummy(User? user, string? hash, string value) =>
        Verify(user ?? _dummyUser, hash ?? _dummyHash, value);
}

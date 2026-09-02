using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Domain.Accounts;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence;

public sealed class ParallelWorldDbContext(DbContextOptions<ParallelWorldDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<DeviceInstallation> DeviceInstallations => Set<DeviceInstallation>();

    public DbSet<GuestBootstrapOperation> GuestBootstrapOperations => Set<GuestBootstrapOperation>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<GameWorld> GameWorlds => Set<GameWorld>();

    public DbSet<WorldSettings> WorldSettings => Set<WorldSettings>();

    public DbSet<WorldSimulationState> WorldSimulationStates => Set<WorldSimulationState>();

    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();

    public DbSet<Actor> Actors => Set<Actor>();

    public async Task<IApplicationTransaction> BeginTransactionAsync(
        ApplicationIsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        var isolation = isolationLevel switch
        {
            ApplicationIsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
            ApplicationIsolationLevel.Serializable => IsolationLevel.Serializable,
            _ => throw new ArgumentOutOfRangeException(nameof(isolationLevel)),
        };
        return new ApplicationTransaction(
            await Database.BeginTransactionAsync(isolation, cancellationToken));
    }

    public void ClearTrackedChanges() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParallelWorldDbContext).Assembly);
}

internal sealed class ApplicationTransaction(IDbContextTransaction transaction) : IApplicationTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}

using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Domain.Accounts;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
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

    public DbSet<Character> Characters => Set<Character>();

    public DbSet<CharacterTraits> CharacterTraits => Set<CharacterTraits>();

    public DbSet<CharacterInterest> CharacterInterests => Set<CharacterInterest>();

    public DbSet<CharacterOpinion> CharacterOpinions => Set<CharacterOpinion>();

    public DbSet<CharacterSchedule> CharacterSchedules => Set<CharacterSchedule>();

    public DbSet<GameplayEvent> GameplayEvents => Set<GameplayEvent>();

    public DbSet<Post> Posts => Set<Post>();

    public DbSet<PostReaction> PostReactions => Set<PostReaction>();

    public DbSet<Follow> Follows => Set<Follow>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public DbSet<SimulationRun> SimulationRuns => Set<SimulationRun>();

    public DbSet<SimulationRunCheckpoint> SimulationRunCheckpoints => Set<SimulationRunCheckpoint>();

    public DbSet<SimulationRuleEvaluation> SimulationRuleEvaluations => Set<SimulationRuleEvaluation>();

    public DbSet<SimulationAction> SimulationActions => Set<SimulationAction>();

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

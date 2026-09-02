using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Application.Worlds;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Worlds;

internal sealed class WorldRepository(ParallelWorldDbContext dbContext) : IWorldRepository
{
    public async Task<IReadOnlyList<WorldSummary>> ListAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        await Query(ownerUserId, null).ToListAsync(cancellationToken);

    public Task<WorldSummary?> FindAsync(
        Guid ownerUserId,
        Guid? worldId,
        CancellationToken cancellationToken) =>
        Query(ownerUserId, worldId).FirstOrDefaultAsync(cancellationToken);

    public async Task EnsureUserExistsAsync(Guid userId, CancellationToken cancellationToken) =>
        _ = await dbContext.Users.SingleAsync(entity => entity.Id == userId, cancellationToken);

    public void Add(WorldAggregate aggregate)
    {
        dbContext.GameWorlds.Add(aggregate.World);
        dbContext.WorldSettings.Add(aggregate.Settings);
        dbContext.WorldSimulationStates.Add(aggregate.SimulationState);
        dbContext.PlayerProfiles.Add(aggregate.PlayerProfile);
        dbContext.Actors.Add(aggregate.PlayerActor);
    }

    private IQueryable<WorldSummary> Query(Guid ownerUserId, Guid? worldId) =>
        from world in dbContext.GameWorlds.AsNoTracking()
        join actor in dbContext.Actors.AsNoTracking() on world.Id equals actor.WorldId
        join profile in dbContext.PlayerProfiles.AsNoTracking()
            on new { actor.WorldId, actor.PlayerProfileId }
            equals new { profile.WorldId, PlayerProfileId = (Guid?)profile.Id }
        where world.OwnerUserId == ownerUserId
            && actor.ActorType == ActorType.Player
            && (worldId == null || world.Id == worldId)
        orderby world.CreatedAt
        select new WorldSummary(
            world.Id,
            world.Name,
            world.Status == WorldStatus.Active ? "active" : "archived",
            world.CurrentWorldTime,
            new PlayerSummary(actor.Id, profile.DisplayName),
            world.CreatedAt);
}

using System.Security.Cryptography;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Application.Worlds;

internal static class WorldAggregateFactory
{
    public static WorldAggregate Create(Guid userId, string name, DateTimeOffset now)
    {
        var worldId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var seed = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(sizeof(long))) & long.MaxValue;
        var world = new GameWorld(worldId, userId, name, seed, now);
        var profile = new PlayerProfile(profileId, worldId, now);
        var actor = new Actor(Guid.NewGuid(), worldId, profileId, now);

        return new WorldAggregate(
            world,
            new WorldSettings(Guid.NewGuid(), worldId, now),
            new WorldSimulationState(Guid.NewGuid(), worldId, now),
            profile,
            actor);
    }
}

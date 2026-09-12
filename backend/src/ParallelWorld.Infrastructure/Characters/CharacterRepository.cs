using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Characters;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Characters;

internal sealed class CharacterRepository(ParallelWorldDbContext dbContext) : ICharacterRepository
{
    public async Task<CharacterSeedContext?> FindSeedContextAsync(
        Guid ownerUserId,
        Guid worldId,
        bool forUpdate,
        CancellationToken cancellationToken)
    {
        var worlds = forUpdate
            ? dbContext.GameWorlds.FromSqlInterpolated(
                $"SELECT * FROM game_worlds WHERE id = {worldId} AND owner_user_id = {ownerUserId} FOR UPDATE")
            : dbContext.GameWorlds.AsNoTracking().Where(world =>
                world.Id == worldId && world.OwnerUserId == ownerUserId);

        return await (
            from world in worlds
            join settings in dbContext.WorldSettings on world.Id equals settings.WorldId
            select new CharacterSeedContext(
                world.Id,
                world.Seed,
                settings.RuleVersion,
                world.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid worldId, CancellationToken cancellationToken) =>
        dbContext.Characters.CountAsync(character => character.WorldId == worldId, cancellationToken);

    public void Add(CharacterCast cast)
    {
        dbContext.Characters.AddRange(cast.Characters);
        dbContext.Actors.AddRange(cast.Actors);
        dbContext.CharacterTraits.AddRange(cast.Traits);
        dbContext.CharacterInterests.AddRange(cast.Interests);
        dbContext.CharacterOpinions.AddRange(cast.Opinions);
        dbContext.CharacterSchedules.AddRange(cast.Schedules);
    }

    public async Task<IReadOnlyList<CharacterSummary>> ListAsync(
        Guid worldId,
        CharacterStatus? status,
        Guid? afterId,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from character in dbContext.Characters.AsNoTracking()
            join actor in dbContext.Actors.AsNoTracking()
                on new { character.WorldId, CharacterId = (Guid?)character.Id }
                equals new { actor.WorldId, actor.CharacterId }
            where character.WorldId == worldId
                && actor.ActorType == ActorType.Character
                && actor.Status == ActorStatus.Active
                && (status == null || character.Status == status)
                && (afterId == null || character.Id.CompareTo(afterId.Value) > 0)
            orderby character.Id
            select new
            {
                character.Id,
                character.DisplayName,
                character.Handle,
                character.Profession,
                character.CurrentMoodType,
            })
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new CharacterSummary(
            row.Id,
            row.DisplayName,
            row.Handle,
            row.Profession,
            row.CurrentMoodType.ToString().ToLowerInvariant(),
            false)).ToArray();
    }

    public async Task<CharacterDetails?> FindDetailsAsync(
        Guid ownerUserId,
        Guid worldId,
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var row = await (
            from world in dbContext.GameWorlds.AsNoTracking()
            join character in dbContext.Characters.AsNoTracking() on world.Id equals character.WorldId
            join actor in dbContext.Actors.AsNoTracking()
                on new { character.WorldId, CharacterId = (Guid?)character.Id }
                equals new { actor.WorldId, actor.CharacterId }
            where world.Id == worldId
                && world.OwnerUserId == ownerUserId
                && character.Id == characterId
                && actor.ActorType == ActorType.Character
                && actor.Status == ActorStatus.Active
            select new
            {
                character.Id,
                character.DisplayName,
                character.Handle,
                character.Bio,
                character.Age,
                character.Profession,
                character.Archetype,
                character.CurrentMoodType,
            }).SingleOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return null;
        }

        var interests = await dbContext.CharacterInterests.AsNoTracking()
            .Where(interest => interest.WorldId == worldId && interest.CharacterId == characterId)
            .OrderByDescending(interest => interest.Strength)
            .ThenBy(interest => interest.TopicId)
            .Select(interest => new CharacterInterestSummary(interest.TopicId))
            .ToListAsync(cancellationToken);
        var schedule = await dbContext.CharacterSchedules.AsNoTracking()
            .Where(item => item.WorldId == worldId
                && item.CharacterId == characterId
                && item.Status == CharacterScheduleStatus.Active)
            .OrderBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartLocalTime)
            .Select(item => new CharacterScheduleSummary(
                item.DayOfWeek,
                item.StartLocalTime,
                item.EndLocalTime,
                item.Activity))
            .ToListAsync(cancellationToken);

        return new CharacterDetails(
            row.Id,
            row.DisplayName,
            row.Handle,
            row.Bio,
            row.Age,
            row.Profession,
            row.Archetype,
            row.CurrentMoodType.ToString().ToLowerInvariant(),
            interests,
            schedule);
    }
}

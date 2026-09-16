using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Social;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.Infrastructure.Social;

internal sealed class SocialRepository(ParallelWorldDbContext dbContext) : ISocialRepository
{
    public async Task<FeedSeedContext?> FindSeedContextAsync(
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
            select new FeedSeedContext(
                world.Id,
                world.Seed,
                settings.RuleVersion,
                world.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountSeedPostsAsync(Guid worldId, CancellationToken cancellationToken) =>
        (from post in dbContext.Posts
         join gameplayEvent in dbContext.GameplayEvents
             on new { post.WorldId, Id = post.GameplayEventId }
             equals new { gameplayEvent.WorldId, gameplayEvent.Id }
         where post.WorldId == worldId && gameplayEvent.ReasonCode == SeedPostFactory.SeedReasonCode
         select post).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<CharacterPostAuthor>> ListCharacterAuthorsAsync(
        Guid worldId,
        int take,
        CancellationToken cancellationToken) =>
        await (
            from actor in dbContext.Actors.AsNoTracking()
            join character in dbContext.Characters.AsNoTracking()
                on new { actor.WorldId, actor.CharacterId }
                equals new { character.WorldId, CharacterId = (Guid?)character.Id }
            where actor.WorldId == worldId
                && actor.ActorType == ActorType.Character
                && actor.Status == ActorStatus.Active
            orderby character.Handle, actor.Id
            select new CharacterPostAuthor(actor.Id, character.DisplayName, character.Handle))
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<PlayerPostAuthor?> FindPlayerAuthorAsync(
        Guid ownerUserId,
        Guid worldId,
        CancellationToken cancellationToken) =>
        await (
            from world in dbContext.GameWorlds.AsNoTracking()
            join settings in dbContext.WorldSettings.AsNoTracking() on world.Id equals settings.WorldId
            join actor in dbContext.Actors.AsNoTracking() on world.Id equals actor.WorldId
            join profile in dbContext.PlayerProfiles.AsNoTracking()
                on new { actor.WorldId, actor.PlayerProfileId }
                equals new { profile.WorldId, PlayerProfileId = (Guid?)profile.Id }
            where world.Id == worldId
                && world.OwnerUserId == ownerUserId
                && actor.ActorType == ActorType.Player
                && actor.Status == ActorStatus.Active
            select new PlayerPostAuthor(
                actor.Id,
                profile.DisplayName,
                profile.Handle,
                settings.RuleVersion))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<(string RequestHash, FeedPost Post)?> FindIdempotentPostAsync(
        Guid userId,
        string operation,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.IdempotencyRecords.AsNoTracking()
            .Where(item => item.UserId == userId
                && item.Operation == operation
                && item.IdempotencyKey == idempotencyKey)
            .Select(item => new { item.RequestHash, item.WorldId, item.ResponseResourceId })
            .SingleOrDefaultAsync(cancellationToken);
        if (record?.WorldId is not Guid worldId)
        {
            return null;
        }

        var post = await PostRows(worldId, record.ResponseResourceId)
            .SingleAsync(cancellationToken);
        return (record.RequestHash, ToFeedPost(post));
    }

    public async Task<IReadOnlyList<FeedPost>> ListFeedAsync(
        Guid worldId,
        FeedCursorValue? after,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await PostRows(worldId, after: after)
            .Take(take)
            .ToListAsync(cancellationToken);
        return rows.Select(ToFeedPost).ToArray();
    }

    public void AddSeedPosts(SeedPostSet seedPosts)
    {
        dbContext.GameplayEvents.AddRange(seedPosts.Events);
        dbContext.Posts.AddRange(seedPosts.Posts);
    }

    public void AddPlayerPost(
        GameplayEvent gameplayEvent,
        Post post,
        IdempotencyRecord idempotencyRecord)
    {
        dbContext.GameplayEvents.Add(gameplayEvent);
        dbContext.Posts.Add(post);
        dbContext.IdempotencyRecords.Add(idempotencyRecord);
    }

    private IQueryable<FeedPostRow> PostRows(
        Guid worldId,
        Guid? postId = null,
        FeedCursorValue? after = null)
    {
        var posts = dbContext.Posts.AsNoTracking().Where(post =>
            post.WorldId == worldId
            && post.DeletedAt == null
            && (postId == null || post.Id == postId)
            && (after == null
                || post.CreatedAt < after.CreatedAtUtc
                || (post.CreatedAt == after.CreatedAtUtc && post.Id.CompareTo(after.Id) < 0)));

        return
        from post in posts
        join actor in dbContext.Actors.AsNoTracking()
            on new { post.WorldId, Id = post.AuthorActorId }
            equals new { actor.WorldId, actor.Id }
        join playerProfile in dbContext.PlayerProfiles.AsNoTracking()
            on new { actor.WorldId, actor.PlayerProfileId }
            equals new { playerProfile.WorldId, PlayerProfileId = (Guid?)playerProfile.Id }
            into playerProfiles
        from playerProfile in playerProfiles.DefaultIfEmpty()
        join character in dbContext.Characters.AsNoTracking()
            on new { actor.WorldId, actor.CharacterId }
            equals new { character.WorldId, CharacterId = (Guid?)character.Id }
            into characters
        from character in characters.DefaultIfEmpty()
        orderby post.CreatedAt descending, post.Id descending
        select new FeedPostRow(
            post.Id,
            post.WorldId,
            actor.Id,
            actor.ActorType,
            actor.ActorType == ActorType.Player
                ? playerProfile!.DisplayName
                : actor.ActorType == ActorType.Character
                    ? character!.DisplayName
                    : "Parallel World",
            actor.ActorType == ActorType.Player
                ? playerProfile!.Handle
                : actor.ActorType == ActorType.Character
                    ? character!.Handle
                    : "system",
            post.Content,
            post.CreatedAt,
            post.ParentPostId,
            post.LikeCount,
            post.ReplyCount,
            post.Visibility,
            post.DeletedAt);
    }

    private static FeedPost ToFeedPost(FeedPostRow row) => new(
        row.Id,
        row.WorldId,
        new FeedAuthor(
            row.ActorId,
            row.DisplayName,
            row.Handle,
            row.ActorType.ToString().ToLowerInvariant()),
        row.Content,
        row.CreatedAt,
        row.ParentPostId,
        new FeedCounts(row.LikeCount, row.ReplyCount),
        row.Visibility.ToString().ToLowerInvariant());

    private sealed record FeedPostRow(
        Guid Id,
        Guid WorldId,
        Guid ActorId,
        ActorType ActorType,
        string DisplayName,
        string Handle,
        string Content,
        DateTimeOffset CreatedAt,
        Guid? ParentPostId,
        int LikeCount,
        int ReplyCount,
        PostVisibility Visibility,
        DateTimeOffset? DeletedAt);
}

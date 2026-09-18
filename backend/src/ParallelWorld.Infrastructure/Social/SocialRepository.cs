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

        var playerActorId = await dbContext.Actors.AsNoTracking()
            .Where(actor => actor.WorldId == worldId && actor.ActorType == ActorType.Player)
            .Select(actor => actor.Id)
            .SingleAsync(cancellationToken);
        var post = await PostRows(worldId, playerActorId, record.ResponseResourceId)
            .SingleAsync(cancellationToken);
        return (record.RequestHash, ToFeedPost(post));
    }

    public async Task<IReadOnlyList<FeedPost>> ListFeedAsync(
        Guid worldId,
        Guid playerActorId,
        FeedCursorValue? after,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await PostRows(worldId, playerActorId)
            .Where(post => after == null
                || post.CreatedAt < after.CreatedAtUtc
                || (post.CreatedAt == after.CreatedAtUtc && post.Id.CompareTo(after.Id) < 0))
            .OrderByDescending(post => post.CreatedAt)
            .ThenByDescending(post => post.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
        return rows.Select(ToFeedPost).ToArray();
    }

    public async Task<FeedPost?> FindPostAsync(
        Guid worldId,
        Guid postId,
        Guid playerActorId,
        CancellationToken cancellationToken)
    {
        var row = await PostRows(worldId, playerActorId, postId)
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : ToFeedPost(row);
    }

    public async Task<IReadOnlyList<FeedPost>> ListRepliesAsync(
        Guid worldId,
        Guid parentPostId,
        Guid playerActorId,
        ReplyCursorValue? after,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await PostRows(worldId, playerActorId)
            .Where(post => post.ParentPostId == parentPostId
                && (after == null
                    || post.CreatedAt > after.CreatedAtUtc
                    || (post.CreatedAt == after.CreatedAtUtc && post.Id.CompareTo(after.Id) > 0)))
            .OrderBy(post => post.CreatedAt)
            .ThenBy(post => post.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
        return rows.Select(ToFeedPost).ToArray();
    }

    public async Task<PostActionTarget?> FindPostForUpdateAsync(
        Guid worldId,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var post = await dbContext.Posts.FromSqlInterpolated(
                $"SELECT * FROM posts WHERE world_id = {worldId} AND id = {postId} AND deleted_at IS NULL FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (post is null)
        {
            return null;
        }

        var depth = 0;
        var parentId = post.ParentPostId;
        while (parentId is Guid currentParentId)
        {
            depth++;
            if (depth > 2)
            {
                break;
            }

            parentId = await dbContext.Posts.AsNoTracking()
                .Where(parent => parent.WorldId == worldId && parent.Id == currentParentId)
                .Select(parent => parent.ParentPostId)
                .SingleOrDefaultAsync(cancellationToken);
        }

        return new(post, depth);
    }

    public Task<PostReaction?> FindReactionAsync(
        Guid worldId,
        Guid postId,
        Guid actorId,
        CancellationToken cancellationToken) =>
        dbContext.PostReactions.SingleOrDefaultAsync(reaction =>
            reaction.WorldId == worldId
            && reaction.PostId == postId
            && reaction.ActorId == actorId
            && reaction.ReactionType == ReactionType.Like,
            cancellationToken);

    public async Task<bool> LockCharacterActorAsync(
        Guid worldId,
        Guid actorId,
        CancellationToken cancellationToken) =>
        await dbContext.Actors.FromSqlInterpolated(
                $"SELECT * FROM actors WHERE world_id = {worldId} AND id = {actorId} AND actor_type = 'character' AND status = 'active' FOR UPDATE")
            .AnyAsync(cancellationToken);

    public Task<Follow?> FindActiveFollowAsync(
        Guid worldId,
        Guid followerActorId,
        Guid followedActorId,
        CancellationToken cancellationToken) =>
        dbContext.Follows.SingleOrDefaultAsync(follow =>
            follow.WorldId == worldId
            && follow.FollowerActorId == followerActorId
            && follow.FollowedActorId == followedActorId
            && follow.EndedAt == null,
            cancellationToken);

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

    public void AddReaction(GameplayEvent gameplayEvent, PostReaction reaction)
    {
        dbContext.GameplayEvents.Add(gameplayEvent);
        dbContext.PostReactions.Add(reaction);
    }

    public void RemoveReaction(PostReaction reaction) => dbContext.PostReactions.Remove(reaction);

    public void AddFollow(GameplayEvent gameplayEvent, Follow follow)
    {
        dbContext.GameplayEvents.Add(gameplayEvent);
        dbContext.Follows.Add(follow);
    }

    private IQueryable<FeedPostRow> PostRows(
        Guid worldId,
        Guid playerActorId,
        Guid? postId = null)
    {
        var posts = dbContext.Posts.AsNoTracking().Where(post =>
            post.WorldId == worldId
            && post.DeletedAt == null
            && (postId == null || post.Id == postId));

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
        select new FeedPostRow
        {
            Id = post.Id,
            WorldId = post.WorldId,
            ActorId = actor.Id,
            ActorType = actor.ActorType,
            DisplayName = actor.ActorType == ActorType.Player
                ? playerProfile!.DisplayName
                : actor.ActorType == ActorType.Character
                    ? character!.DisplayName
                    : "Parallel World",
            Handle = actor.ActorType == ActorType.Player
                ? playerProfile!.Handle
                : actor.ActorType == ActorType.Character
                    ? character!.Handle
                    : "system",
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            ParentPostId = post.ParentPostId,
            LikeCount = post.LikeCount,
            ReplyCount = post.ReplyCount,
            IsLiked = dbContext.PostReactions.Any(reaction =>
                reaction.WorldId == post.WorldId
                && reaction.PostId == post.Id
                && reaction.ActorId == playerActorId
                && reaction.ReactionType == ReactionType.Like),
            IsFollowed = actor.ActorType == ActorType.Character && dbContext.Follows.Any(follow =>
                follow.WorldId == post.WorldId
                && follow.FollowerActorId == playerActorId
                && follow.FollowedActorId == actor.Id
                && follow.EndedAt == null),
            Visibility = post.Visibility,
            DeletedAt = post.DeletedAt,
        };
    }

    private static FeedPost ToFeedPost(FeedPostRow row) => new(
        row.Id,
        row.WorldId,
        new FeedAuthor(
            row.ActorId,
            row.DisplayName,
            row.Handle,
            row.ActorType.ToString().ToLowerInvariant(),
            row.IsFollowed),
        row.Content,
        row.CreatedAt,
        row.ParentPostId,
        new FeedCounts(row.LikeCount, row.ReplyCount),
        row.IsLiked ? "like" : null,
        row.Visibility.ToString().ToLowerInvariant());

    private sealed class FeedPostRow
    {
        public Guid Id { get; init; }

        public Guid WorldId { get; init; }

        public Guid ActorId { get; init; }

        public ActorType ActorType { get; init; }

        public required string DisplayName { get; init; }

        public required string Handle { get; init; }

        public required string Content { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public Guid? ParentPostId { get; init; }

        public int LikeCount { get; init; }

        public int ReplyCount { get; init; }

        public bool IsLiked { get; init; }

        public bool IsFollowed { get; init; }

        public PostVisibility Visibility { get; init; }

        public DateTimeOffset? DeletedAt { get; init; }
    }
}

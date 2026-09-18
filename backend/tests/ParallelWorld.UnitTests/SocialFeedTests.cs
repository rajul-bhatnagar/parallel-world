using System.Globalization;
using ParallelWorld.Application.Social;
using ParallelWorld.Domain.Social;

namespace ParallelWorld.UnitTests;

public sealed class SocialFeedTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Post_EnforcesContentLengthAndParentInvariant()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => CreatePost(id, "   "));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePost(id, new string('x', 501)));
        Assert.Equal(
            500,
            new StringInfo(CreatePost(
                id,
                string.Concat(Enumerable.Repeat("e\u0301", 500))).Content).LengthInTextElements);
        Assert.Throws<ArgumentException>(() => new Post(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            id,
            Guid.NewGuid(),
            "Valid content",
            CreatedAt));
    }

    [Fact]
    public void SeedPosts_AreDeterministicAndWorldScoped()
    {
        var worldId = Guid.Parse("4f520745-bf70-4795-a384-4dc86c5ea6c5");
        var context = new FeedSeedContext(worldId, 90210, 1, CreatedAt);
        var authors = Authors(worldId);

        var first = SeedPostFactory.Create(context, authors);
        var second = SeedPostFactory.Create(context, authors);
        var foreign = SeedPostFactory.Create(
            context with { WorldId = Guid.Parse("43415b43-6361-437f-a76c-118105d4834d") },
            Authors(Guid.Parse("43415b43-6361-437f-a76c-118105d4834d")));

        Assert.Equal(SeedPostFactory.SeedPostCount, first.Posts.Count);
        Assert.Equal(first.Posts.Select(item => item.Id), second.Posts.Select(item => item.Id));
        Assert.Equal(first.Events.Select(item => item.Id), second.Events.Select(item => item.Id));
        Assert.Empty(first.Posts.Select(item => item.Id).Intersect(foreign.Posts.Select(item => item.Id)));
        Assert.All(first.Posts, post =>
        {
            Assert.Equal(worldId, post.WorldId);
            Assert.Null(post.ParentPostId);
            Assert.Equal(PostVisibility.World, post.Visibility);
        });
    }

    [Fact]
    public void Post_CountersAndFollowHistoryEnforceDomainInvariants()
    {
        var post = CreatePost(Guid.NewGuid(), "Counters");
        post.AddLike();
        post.AddReply();

        Assert.Equal(1, post.LikeCount);
        Assert.Equal(1, post.ReplyCount);
        post.RemoveLike();
        Assert.Equal(0, post.LikeCount);
        Assert.Throws<InvalidOperationException>(post.RemoveLike);

        var actorId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new Follow(
            Guid.NewGuid(),
            Guid.NewGuid(),
            actorId,
            actorId,
            CreatedAt,
            Guid.NewGuid(),
            "operation"));
        var follow = new Follow(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CreatedAt,
            Guid.NewGuid(),
            "operation");
        follow.End(CreatedAt.AddMinutes(1));
        follow.End(CreatedAt.AddMinutes(2));
        Assert.Equal(CreatedAt.AddMinutes(1), follow.EndedAt);
    }

    private static Post CreatePost(Guid id, string content) => new(
        id,
        Guid.NewGuid(),
        Guid.NewGuid(),
        null,
        Guid.NewGuid(),
        content,
        CreatedAt);

    private static CharacterPostAuthor[] Authors(Guid worldId) =>
    [
        new(Stable(worldId, 1), "Amara", "amara"),
        new(Stable(worldId, 2), "Elias", "elias"),
        new(Stable(worldId, 3), "Hana", "hana"),
    ];

    private static Guid Stable(Guid worldId, byte suffix)
    {
        var bytes = worldId.ToByteArray();
        bytes[^1] = suffix;
        return new Guid(bytes);
    }
}

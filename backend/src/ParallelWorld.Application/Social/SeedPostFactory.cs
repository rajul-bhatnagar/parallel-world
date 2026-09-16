using System.Security.Cryptography;
using System.Text;
using ParallelWorld.Domain.Social;

namespace ParallelWorld.Application.Social;

public static class SeedPostFactory
{
    public const int SeedPostCount = 3;
    public const string SeedReasonCode = "m06_seed_post";

    private static readonly string[] Contents =
    [
        "A quiet start, and plenty to notice.",
        "Making a little room today for something new.",
        "Some ordinary moments are worth sharing.",
    ];

    public static SeedPostSet Create(
        FeedSeedContext context,
        IReadOnlyList<CharacterPostAuthor> authors)
    {
        if (authors.Count != SeedPostCount)
        {
            throw new ArgumentException($"Exactly {SeedPostCount} character authors are required.", nameof(authors));
        }

        var events = new List<GameplayEvent>(SeedPostCount);
        var posts = new List<Post>(SeedPostCount);
        for (var index = 0; index < SeedPostCount; index++)
        {
            var author = authors[index];
            var eventId = StableGuid(context, "event", index);
            var postId = StableGuid(context, "post", index);
            var createdAt = context.WorldCreatedAt.AddMinutes(index - SeedPostCount);
            events.Add(new GameplayEvent(
                eventId,
                context.WorldId,
                "postCreated",
                author.ActorId,
                null,
                createdAt,
                0,
                0,
                SeedReasonCode,
                context.RuleVersion,
                $"m06:seed-post:{postId:N}",
                createdAt));
            posts.Add(new Post(
                postId,
                context.WorldId,
                author.ActorId,
                null,
                eventId,
                Contents[index],
                createdAt));
        }

        return new SeedPostSet(events, posts);
    }

    private static Guid StableGuid(FeedSeedContext context, string category, int ordinal)
    {
        var input = Encoding.UTF8.GetBytes(FormattableString.Invariant(
            $"parallel-world:m06:{context.WorldId:N}:{context.WorldSeed}:{context.RuleVersion}:{category}:{ordinal}"));
        var bytes = SHA256.HashData(input)[..16];
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes);
    }
}

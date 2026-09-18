using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ParallelWorld.Domain.Social;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class SocialActionsTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Like_PutAndDeleteAreIdempotentAndKeepCounterConsistent()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(client, guest.World.Id)).Items
            .First(post => post.Author.ActorType == "character");

        var first = await SetLikeAsync(client, guest.World.Id, target.Id);
        var repeated = await SetLikeAsync(client, guest.World.Id, target.Id);
        var current = await first.Content.ReadFromJsonAsync<M07ReactionState>();
        var repeatedState = await repeated.Content.ReadFromJsonAsync<M07ReactionState>();
        var afterLike = await GetPostAsync(client, guest.World.Id, target.Id);
        var removed = await client.DeleteAsync(
            $"/api/v1/worlds/{guest.World.Id}/posts/{target.Id}/reaction");
        var repeatedRemove = await client.DeleteAsync(
            $"/api/v1/worlds/{guest.World.Id}/posts/{target.Id}/reaction");
        var afterRemove = await GetPostAsync(client, guest.World.Id, target.Id);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        Assert.True(current!.Active);
        Assert.Equal(1, current.LikeCount);
        Assert.Equal(current, repeatedState);
        Assert.Equal("like", afterLike.CurrentPlayerReaction);
        Assert.Equal(1, afterLike.Counts.Likes);
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, repeatedRemove.StatusCode);
        Assert.Null(afterRemove.CurrentPlayerReaction);
        Assert.Equal(0, afterRemove.Counts.Likes);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Empty(await db.PostReactions.Where(reaction => reaction.PostId == target.Id).ToListAsync());
    }

    [Fact]
    public async Task Like_ConcurrentPutCreatesOneSourceRowAndSelfReactionIsRejected()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(client, guest.World.Id)).Items
            .First(post => post.Author.ActorType == "character");

        var concurrent = await Task.WhenAll(
            SetLikeAsync(client, guest.World.Id, target.Id),
            SetLikeAsync(client, guest.World.Id, target.Id));
        var own = await CreatePostAsync(client, guest.World.Id, "Own post");
        var selfReaction = await SetLikeAsync(client, guest.World.Id, own.Id);

        Assert.All(concurrent, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        Assert.Equal(HttpStatusCode.BadRequest, selfReaction.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.PostReactions.Where(reaction => reaction.PostId == target.Id).ToListAsync());
        Assert.Equal(1, (await db.Posts.SingleAsync(post => post.Id == target.Id)).LikeCount);
    }

    [Fact]
    public async Task Like_ConcurrentPutAndDeleteLeaveSourceRowAndCounterConsistent()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(client, guest.World.Id)).Items
            .First(post => post.Author.ActorType == "character");

        var results = await Task.WhenAll(
            SetLikeAsync(client, guest.World.Id, target.Id),
            client.DeleteAsync($"/api/v1/worlds/{guest.World.Id}/posts/{target.Id}/reaction"));

        Assert.Contains(results, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Contains(results, response => response.StatusCode == HttpStatusCode.NoContent);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var sourceCount = await db.PostReactions.CountAsync(reaction => reaction.PostId == target.Id);
        var cachedCount = (await db.Posts.SingleAsync(post => post.Id == target.Id)).LikeCount;
        Assert.InRange(sourceCount, 0, 1);
        Assert.Equal(sourceCount, cachedCount);
    }

    [Fact]
    public async Task Replies_AreDirectOldestFirstPaginatedAndDepthBounded()
    {
        await using var factory = await CreateFactoryAsync(new MutableTimeProvider(FixedNow));
        var (client, guest) = await BootstrapAsync(factory);
        var root = (await GetFeedAsync(client, guest.World.Id)).Items[0];
        var direct = new List<M07Post>();
        for (var index = 0; index < 3; index++)
        {
            direct.Add(await CreateReplyAsync(client, guest.World.Id, root.Id, $"Direct {index}"));
        }

        var firstPage = await GetRepliesAsync(client, guest.World.Id, root.Id, 2);
        var repeatedFirstPage = await GetRepliesAsync(client, guest.World.Id, root.Id, 2);
        var secondPage = await GetRepliesAsync(client, guest.World.Id, root.Id, 2, firstPage.NextCursor);
        var depthTwo = await CreateReplyAsync(client, guest.World.Id, direct[0].Id, "Depth two");
        var depthThree = await SendReplyAsync(client, guest.World.Id, depthTwo.Id, "Too deep");
        var depthTwoChildren = await GetRepliesAsync(client, guest.World.Id, depthTwo.Id);
        var rootDetailResponse = await client.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/posts/{root.Id}");
        var rootJson = JsonDocument.Parse(await rootDetailResponse.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(firstPage.Items.Select(post => post.Id), repeatedFirstPage.Items.Select(post => post.Id));
        Assert.NotNull(firstPage.NextCursor);
        Assert.NotNull(repeatedFirstPage.NextCursor);
        Assert.Equal(firstPage.HasMore, repeatedFirstPage.HasMore);
        Assert.True(firstPage.HasMore);
        Assert.Single(secondPage.Items);
        Assert.Empty(firstPage.Items.Select(post => post.Id).Intersect(secondPage.Items.Select(post => post.Id)));
        Assert.Equal(
            direct.Select(post => post.Id).OrderBy(id => id.ToString("N"), StringComparer.Ordinal),
            firstPage.Items.Concat(secondPage.Items).Select(post => post.Id));
        Assert.All(firstPage.Items.Concat(secondPage.Items), post => Assert.Equal(root.Id, post.Parent!.Id));
        Assert.Equal(HttpStatusCode.BadRequest, depthThree.StatusCode);
        Assert.Equal("invalid_state_transition", (await depthThree.Content.ReadFromJsonAsync<M07Problem>())!.Code);
        Assert.Empty(depthTwoChildren.Items);
        Assert.False(rootJson.TryGetProperty("replies", out _));
        Assert.False(rootJson.TryGetProperty("items", out _));
        Assert.Equal(3, (await GetPostAsync(client, guest.World.Id, root.Id)).Counts.Replies);
        Assert.Equal(1, (await GetPostAsync(client, guest.World.Id, direct[0].Id)).Counts.Replies);
    }

    [Fact]
    public async Task Replies_AreIdempotentConcurrentAndCursorBoundToParentAndWorld()
    {
        await using var factory = await CreateFactoryAsync(new MutableTimeProvider(FixedNow));
        var (firstClient, firstGuest) = await BootstrapAsync(factory);
        var (secondClient, secondGuest) = await BootstrapAsync(factory);
        var root = (await GetFeedAsync(firstClient, firstGuest.World.Id)).Items[0];
        var otherParent = (await GetFeedAsync(firstClient, firstGuest.World.Id)).Items[1];
        var secondRoot = (await GetFeedAsync(secondClient, secondGuest.World.Id)).Items[0];
        var key = Guid.NewGuid().ToString();
        var clientId = Guid.NewGuid();
        var replayResponses = await Task.WhenAll(
            SendReplyAsync(firstClient, firstGuest.World.Id, root.Id, "Same", key, clientId),
            SendReplyAsync(firstClient, firstGuest.World.Id, root.Id, "Same", key, clientId));
        var simultaneous = await Task.WhenAll(
            SendReplyAsync(firstClient, firstGuest.World.Id, root.Id, "One"),
            SendReplyAsync(firstClient, firstGuest.World.Id, root.Id, "Two"));
        var page = await GetRepliesAsync(firstClient, firstGuest.World.Id, root.Id, 1);
        var wrongParent = await firstClient.GetAsync(
            $"/api/v1/worlds/{firstGuest.World.Id}/posts/{otherParent.Id}/replies?cursor={Uri.EscapeDataString(page.NextCursor!)}");
        var wrongWorld = await secondClient.GetAsync(
            $"/api/v1/worlds/{secondGuest.World.Id}/posts/{secondRoot.Id}/replies?cursor={Uri.EscapeDataString(page.NextCursor!)}");
        var tamperedCursor = page.NextCursor![..^1] + (page.NextCursor[^1] == 'A' ? 'B' : 'A');
        var tampered = await firstClient.GetAsync(
            $"/api/v1/worlds/{firstGuest.World.Id}/posts/{root.Id}/replies?cursor={Uri.EscapeDataString(tamperedCursor)}");
        var malformed = await firstClient.GetAsync(
            $"/api/v1/worlds/{firstGuest.World.Id}/posts/{root.Id}/replies?cursor=not-a-cursor");

        Assert.All(replayResponses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.All(simultaneous, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        var replayPosts = await Task.WhenAll(replayResponses.Select(response =>
            response.Content.ReadFromJsonAsync<M07Post>()));
        Assert.Equal(replayPosts[0]!.Id, replayPosts[1]!.Id);
        Assert.Equal(HttpStatusCode.BadRequest, wrongParent.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, wrongWorld.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tampered.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        Assert.Equal(3, (await GetPostAsync(firstClient, firstGuest.World.Id, root.Id)).Counts.Replies);
    }

    [Fact]
    public async Task Follow_PutDeleteAndRefollowPreserveHistoryAndRejectInvalidTargets()
    {
        await using var factory = await CreateFactoryAsync();
        var (ownerClient, owner) = await BootstrapAsync(factory);
        var (foreignClient, foreign) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(ownerClient, owner.World.Id)).Items
            .First(post => post.Author.ActorType == "character");
        var foreignTarget = (await GetFeedAsync(foreignClient, foreign.World.Id)).Items
            .First(post => post.Author.ActorType == "character");

        var first = await ownerClient.PutAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{target.Author.ActorId}/follow",
            null);
        var repeated = await ownerClient.PutAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{target.Author.ActorId}/follow",
            null);
        var unfollow = await ownerClient.DeleteAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{target.Author.ActorId}/follow");
        var repeatedUnfollow = await ownerClient.DeleteAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{target.Author.ActorId}/follow");
        var refollow = await ownerClient.PutAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{target.Author.ActorId}/follow",
            null);
        var self = await ownerClient.PutAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{owner.World.Player.ActorId}/follow",
            null);
        var crossWorld = await ownerClient.PutAsync(
            $"/api/v1/worlds/{owner.World.Id}/actors/{foreignTarget.Author.ActorId}/follow",
            null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, unfollow.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, repeatedUnfollow.StatusCode);
        Assert.Equal(HttpStatusCode.OK, refollow.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, self.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, crossWorld.StatusCode);
        Assert.True((await GetPostAsync(ownerClient, owner.World.Id, target.Id)).Author.IsFollowed);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var history = await db.Follows.Where(follow =>
            follow.WorldId == owner.World.Id
            && follow.FollowedActorId == target.Author.ActorId).ToListAsync();
        Assert.Equal(2, history.Count);
        Assert.Single(history, follow => follow.EndedAt is null);
        Assert.Single(history, follow => follow.EndedAt is not null);
    }

    [Fact]
    public async Task Follow_ConcurrentPutCreatesOneActiveEdge()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(client, guest.World.Id)).Items
            .First(post => post.Author.ActorType == "character");

        var responses = await Task.WhenAll(
            client.PutAsync($"/api/v1/worlds/{guest.World.Id}/actors/{target.Author.ActorId}/follow", null),
            client.PutAsync($"/api/v1/worlds/{guest.World.Id}/actors/{target.Author.ActorId}/follow", null));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.Follows.Where(follow =>
            follow.WorldId == guest.World.Id && follow.EndedAt == null).ToListAsync());
    }

    [Fact]
    public async Task Follow_ConcurrentPutAndDeleteLeaveAtMostOneActiveHistoricalEdge()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(client, guest.World.Id)).Items
            .First(post => post.Author.ActorType == "character");
        var route = $"/api/v1/worlds/{guest.World.Id}/actors/{target.Author.ActorId}/follow";

        var results = await Task.WhenAll(client.PutAsync(route, null), client.DeleteAsync(route));

        Assert.Contains(results, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Contains(results, response => response.StatusCode == HttpStatusCode.NoContent);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var history = await db.Follows.Where(follow =>
            follow.WorldId == guest.World.Id
            && follow.FollowerActorId == guest.World.Player.ActorId
            && follow.FollowedActorId == target.Author.ActorId).ToListAsync();
        Assert.Single(history);
        Assert.InRange(history.Count(follow => follow.EndedAt is null), 0, 1);
        Assert.Equal(
            history.Any(follow => follow.EndedAt is null),
            (await GetPostAsync(client, guest.World.Id, target.Id)).Author.IsFollowed);
    }

    [Fact]
    public async Task SocialActions_RejectUnsupportedReactionAndClientSelectedActorFields()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var target = (await GetFeedAsync(client, guest.World.Id)).Items
            .First(post => post.Author.ActorType == "character");

        var unsupported = await client.PutAsJsonAsync(
            $"/api/v1/worlds/{guest.World.Id}/posts/{target.Id}/reaction",
            new { type = "dislike" });
        var impersonatedReaction = await client.PutAsJsonAsync(
            $"/api/v1/worlds/{guest.World.Id}/posts/{target.Id}/reaction",
            new { type = "like", actorId = target.Author.ActorId });
        var impersonatedReply = await SendReplyWithActorAsync(
            client,
            guest.World.Id,
            target.Id,
            target.Author.ActorId);

        Assert.Equal(HttpStatusCode.BadRequest, unsupported.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, impersonatedReaction.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, impersonatedReply.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Empty(await db.PostReactions.Where(reaction => reaction.PostId == target.Id).ToListAsync());
        Assert.Empty(await db.Posts.Where(post => post.ParentPostId == target.Id).ToListAsync());
    }

    [Fact]
    public async Task SocialActions_HideForeignResourcesAndDatabaseRejectsCrossWorldLinks()
    {
        await using var factory = await CreateFactoryAsync();
        var (firstClient, first) = await BootstrapAsync(factory);
        var (secondClient, second) = await BootstrapAsync(factory);
        var firstPost = (await GetFeedAsync(firstClient, first.World.Id)).Items[0];
        var secondPost = (await GetFeedAsync(secondClient, second.World.Id)).Items[0];

        var foreignRead = await secondClient.GetAsync(
            $"/api/v1/worlds/{first.World.Id}/posts/{firstPost.Id}");
        var foreignReply = await SendReplyAsync(
            secondClient,
            first.World.Id,
            firstPost.Id,
            "Foreign");
        var foreignReaction = await SetLikeAsync(secondClient, first.World.Id, firstPost.Id);

        Assert.Equal(HttpStatusCode.NotFound, foreignRead.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreignReply.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreignReaction.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var firstEvent = await db.GameplayEvents.FirstAsync(item => item.WorldId == first.World.Id);
        db.PostReactions.Add(new PostReaction(
            Guid.NewGuid(),
            first.World.Id,
            secondPost.Id,
            first.World.Player.ActorId,
            ReactionType.Like,
            firstEvent.Id,
            FixedNow));
        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, FindPostgres(failure).SqlState);
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync(TimeProvider? timeProvider = null)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(connectionString, timeProvider);
    }

    private static async Task<(HttpClient Client, M03GuestResponse Guest)> BootstrapAsync(M03ApiFactory factory)
    {
        var client = factory.CreateClient();
        var response = await client.BootstrapAsync(M03TestClient.NewSecret());
        response.EnsureSuccessStatusCode();
        var guest = await response.Content.ReadFromJsonAsync<M03GuestResponse>();
        client.Authenticate(guest!.AccessToken);
        return (client, guest);
    }

    private static async Task<M07FeedPage> GetFeedAsync(HttpClient client, Guid worldId) =>
        (await client.GetFromJsonAsync<M07FeedPage>($"/api/v1/worlds/{worldId}/feed"))!;

    private static async Task<M07Post> GetPostAsync(HttpClient client, Guid worldId, Guid postId) =>
        (await client.GetFromJsonAsync<M07Post>($"/api/v1/worlds/{worldId}/posts/{postId}"))!;

    private static async Task<M07Post> CreatePostAsync(HttpClient client, Guid worldId, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/worlds/{worldId}/posts")
        {
            Content = JsonContent.Create(new { content, clientPostId = Guid.NewGuid() }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<M07Post>())!;
    }

    private static Task<HttpResponseMessage> SetLikeAsync(HttpClient client, Guid worldId, Guid postId) =>
        client.PutAsJsonAsync(
            $"/api/v1/worlds/{worldId}/posts/{postId}/reaction",
            new { type = "like" });

    private static async Task<M07Post> CreateReplyAsync(
        HttpClient client,
        Guid worldId,
        Guid parentPostId,
        string content)
    {
        var response = await SendReplyAsync(client, worldId, parentPostId, content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<M07Post>())!;
    }

    private static Task<HttpResponseMessage> SendReplyAsync(
        HttpClient client,
        Guid worldId,
        Guid parentPostId,
        string content,
        string? key = null,
        Guid? clientPostId = null)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/worlds/{worldId}/posts/{parentPostId}/replies")
        {
            Content = JsonContent.Create(new
            {
                content,
                clientPostId = clientPostId ?? Guid.NewGuid(),
            }),
        };
        request.Headers.Add("Idempotency-Key", key ?? Guid.NewGuid().ToString());
        return client.SendAsync(request);
    }

    private static Task<HttpResponseMessage> SendReplyWithActorAsync(
        HttpClient client,
        Guid worldId,
        Guid parentPostId,
        Guid actorId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/worlds/{worldId}/posts/{parentPostId}/replies")
        {
            Content = JsonContent.Create(new
            {
                content = "Impersonated reply",
                clientPostId = Guid.NewGuid(),
                actorId,
            }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return client.SendAsync(request);
    }

    private static async Task<M07FeedPage> GetRepliesAsync(
        HttpClient client,
        Guid worldId,
        Guid parentPostId,
        int limit = 20,
        string? cursor = null)
    {
        var suffix = $"?limit={limit}"
            + (cursor is null ? string.Empty : $"&cursor={Uri.EscapeDataString(cursor)}");
        return (await client.GetFromJsonAsync<M07FeedPage>(
            $"/api/v1/worlds/{worldId}/posts/{parentPostId}/replies{suffix}"))!;
    }

    private static PostgresException FindPostgres(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }

            if (current.InnerException is null)
            {
                break;
            }
        }

        throw new InvalidOperationException("A PostgreSQL exception was expected.");
    }
}

internal sealed record M07FeedPage(IReadOnlyList<M07Post> Items, string? NextCursor, bool HasMore);

internal sealed record M07Post(
    Guid Id,
    Guid WorldId,
    M07Author Author,
    string Content,
    DateTimeOffset CreatedAtUtc,
    M07Parent? Parent,
    M07Counts Counts,
    string? CurrentPlayerReaction,
    string Visibility);

internal sealed record M07Author(
    Guid ActorId,
    string DisplayName,
    string Handle,
    string ActorType,
    bool IsFollowed);

internal sealed record M07Parent(Guid Id);

internal sealed record M07Counts(int Likes, int Replies);

internal sealed record M07ReactionState(Guid PostId, string Type, bool Active, int LikeCount);

internal sealed record M07Problem(string Code);

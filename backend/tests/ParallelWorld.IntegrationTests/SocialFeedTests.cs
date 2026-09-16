using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ParallelWorld.Application.Social;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class SocialFeedTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 9, 12, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Feed_InitializesDeterministicCharacterPostsOnce()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);

        var first = await GetFeedAsync(client, guest.World.Id);
        var second = await GetFeedAsync(client, guest.World.Id);

        Assert.Equal(3, first.Items.Count);
        Assert.Equal(first.Items.Select(item => item.Id), second.Items.Select(item => item.Id));
        Assert.Equal(first.Items.Select(item => item.Content), second.Items.Select(item => item.Content));
        Assert.All(first.Items, item => Assert.Equal("character", item.Author.ActorType));
        Assert.Equal(
            first.Items.OrderByDescending(item => item.CreatedAtUtc).ThenByDescending(item => item.Id),
            first.Items);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(3, await db.Posts.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(3, await db.GameplayEvents.CountAsync(item =>
            item.WorldId == guest.World.Id && item.ReasonCode == "m06_seed_post"));
    }

    [Fact]
    public async Task Feed_ConcurrentInitializationCreatesExactlyOneSeedSet()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);

        var pages = await Task.WhenAll(Enumerable.Range(0, 6)
            .Select(_ => GetFeedAsync(client, guest.World.Id)));

        Assert.All(pages, page => Assert.Equal(SeedPostFactory.SeedPostCount, page.Items.Count));
        Assert.All(pages, page => Assert.Equal(
            pages[0].Items.Select(item => item.Id),
            page.Items.Select(item => item.Id)));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(
            SeedPostFactory.SeedPostCount,
            await db.Posts.CountAsync(item => item.WorldId == guest.World.Id));
    }

    [Fact]
    public async Task CreatePost_DerivesPlayerAuthorAndReplaysSameRequest()
    {
        var clock = new MutableTimeProvider(FixedNow);
        await using var factory = await CreateFactoryAsync(clock);
        var (client, guest) = await BootstrapAsync(factory);
        var key = Guid.NewGuid().ToString();
        var clientPostId = Guid.NewGuid();

        var firstResponse = await CreatePostAsync(client, guest.World.Id, key, clientPostId, "  Hello, world.  ");
        var secondResponse = await CreatePostAsync(client, guest.World.Id, key, clientPostId, "  Hello, world.  ");
        var first = await firstResponse.Content.ReadFromJsonAsync<M06FeedPost>();
        var second = await secondResponse.Content.ReadFromJsonAsync<M06FeedPost>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal("true", secondResponse.Headers.GetValues("Idempotency-Replayed").Single());
        Assert.NotNull(first);
        Assert.Equal(first!.Id, second!.Id);
        Assert.Equal("Hello, world.", first.Content);
        Assert.Equal(guest.World.Player.ActorId, first.Author.ActorId);
        Assert.Equal("player", first.Author.ActorType);
        Assert.Equal(FixedNow, first.CreatedAtUtc);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.Posts.Where(item => item.Id == first.Id).ToListAsync());
        Assert.Single(await db.IdempotencyRecords.Where(item =>
            item.UserId == guest.User.Id && item.IdempotencyKey == key).ToListAsync());
        Assert.Single(await db.GameplayEvents.Where(item =>
            item.WorldId == guest.World.Id && item.ActorId == guest.World.Player.ActorId
                && item.ReasonCode == "player_post").ToListAsync());
    }

    [Fact]
    public async Task CreatePost_ConcurrentRetryCreatesOnePost()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var key = Guid.NewGuid().ToString();
        var clientPostId = Guid.NewGuid();

        var responses = await Task.WhenAll(
            CreatePostAsync(client, guest.World.Id, key, clientPostId, "One operation"),
            CreatePostAsync(client, guest.World.Id, key, clientPostId, "One operation"));
        var posts = await Task.WhenAll(responses.Select(response =>
            response.Content.ReadFromJsonAsync<M06FeedPost>()));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Created, response.StatusCode));
        Assert.All(posts, post => Assert.Equal(posts[0]!.Id, post!.Id));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.IdempotencyRecords.Where(item =>
            item.UserId == guest.User.Id && item.IdempotencyKey == key).ToListAsync());
        Assert.Single(await db.Posts.Where(item => item.Id == posts[0]!.Id).ToListAsync());
    }

    [Fact]
    public async Task CreatePost_RejectsImpersonationValidationAndChangedReplay()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var key = Guid.NewGuid().ToString();
        var clientPostId = Guid.NewGuid();

        var first = await CreatePostAsync(client, guest.World.Id, key, clientPostId, "Original");
        var changed = await CreatePostAsync(client, guest.World.Id, key, clientPostId, "Changed");
        var impersonation = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/worlds/{guest.World.Id}/posts")
        {
            Content = JsonContent.Create(new
            {
                content = "Forged",
                clientPostId = Guid.NewGuid(),
                actorId = Guid.NewGuid(),
                userId = Guid.NewGuid(),
            }),
        };
        impersonation.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var impersonationResponse = await client.SendAsync(impersonation);
        var missingKey = await client.PostAsJsonAsync(
            $"/api/v1/worlds/{guest.World.Id}/posts",
            new { content = "Missing key", clientPostId = Guid.NewGuid() });
        var longContent = await CreatePostAsync(
            client,
            guest.World.Id,
            Guid.NewGuid().ToString(),
            Guid.NewGuid(),
            new string('x', 501));
        var maximumGraphemes = await CreatePostAsync(
            client,
            guest.World.Id,
            Guid.NewGuid().ToString(),
            Guid.NewGuid(),
            string.Concat(Enumerable.Repeat("e\u0301", 500)));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
        Assert.Equal("idempotency_key_reused", (await changed.Content.ReadFromJsonAsync<M06Problem>())!.Code);
        Assert.Equal(HttpStatusCode.BadRequest, impersonationResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingKey.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, longContent.StatusCode);
        Assert.Equal(HttpStatusCode.Created, maximumGraphemes.StatusCode);
    }

    [Fact]
    public async Task Feed_OrdersEqualTimestampsByDescendingId()
    {
        await using var factory = await CreateFactoryAsync(new MutableTimeProvider(FixedNow));
        var (client, guest) = await BootstrapAsync(factory);
        _ = await GetFeedAsync(client, guest.World.Id);
        var lower = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var higher = Guid.Parse("00000000-0000-0000-0000-000000000002");
        await AddPostAsync(factory, guest.World.Id, guest.World.Player.ActorId, lower, FixedNow, "Lower");
        await AddPostAsync(factory, guest.World.Id, guest.World.Player.ActorId, higher, FixedNow, "Higher");

        var first = await GetFeedAsync(client, guest.World.Id, 5);
        var repeated = await GetFeedAsync(client, guest.World.Id, 5);

        Assert.Equal(new[] { higher, lower }, first.Items.Take(2).Select(item => item.Id));
        Assert.Equal(first.Items.Select(item => item.Id), repeated.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Feed_CursorContinuesWithoutDuplicatesWhenNewPostArrives()
    {
        await using var factory = await CreateFactoryAsync(new MutableTimeProvider(FixedNow));
        var (client, guest) = await BootstrapAsync(factory);
        _ = await GetFeedAsync(client, guest.World.Id);
        for (var index = 1; index <= 4; index++)
        {
            await AddPostAsync(
                factory,
                guest.World.Id,
                guest.World.Player.ActorId,
                Guid.Parse($"00000000-0000-0000-0000-{index:D12}"),
                FixedNow.AddMinutes(index),
                $"Post {index}");
        }

        var first = await GetFeedAsync(client, guest.World.Id, 2);
        Assert.True(first.HasMore);
        Assert.NotNull(first.NextCursor);
        await AddPostAsync(
            factory,
            guest.World.Id,
            guest.World.Player.ActorId,
            Guid.Parse("00000000-0000-0000-0000-000000000099"),
            FixedNow.AddHours(1),
            "New arrival");

        var second = await GetFeedAsync(client, guest.World.Id, 2, first.NextCursor);

        Assert.Empty(first.Items.Select(item => item.Id).Intersect(second.Items.Select(item => item.Id)));
        Assert.DoesNotContain(second.Items, item => item.Content == "New arrival");
        Assert.All(second.Items, item => Assert.True(
            item.CreatedAtUtc < first.Items[^1].CreatedAtUtc
                || (item.CreatedAtUtc == first.Items[^1].CreatedAtUtc
                    && item.Id.CompareTo(first.Items[^1].Id) < 0)));
    }

    [Fact]
    public async Task Feed_RejectsInvalidTamperedAndWrongWorldCursors()
    {
        await using var factory = await CreateFactoryAsync();
        var (firstClient, firstGuest) = await BootstrapAsync(factory);
        var (secondClient, secondGuest) = await BootstrapAsync(factory);
        var firstPage = await GetFeedAsync(firstClient, firstGuest.World.Id, 1);
        var secondPage = await GetFeedAsync(secondClient, secondGuest.World.Id, 1);
        var cursor = firstPage.NextCursor!;
        var midpoint = cursor.Length / 2;
        var tampered = cursor[..midpoint]
            + (cursor[midpoint] == 'A' ? 'B' : 'A')
            + cursor[(midpoint + 1)..];

        var invalid = await firstClient.GetAsync($"/api/v1/worlds/{firstGuest.World.Id}/feed?cursor=invalid");
        var tamperedResponse = await firstClient.GetAsync(
            $"/api/v1/worlds/{firstGuest.World.Id}/feed?cursor={Uri.EscapeDataString(tampered)}");
        var wrongWorld = await secondClient.GetAsync(
            $"/api/v1/worlds/{secondGuest.World.Id}/feed?cursor={Uri.EscapeDataString(cursor)}");

        Assert.True(secondPage.HasMore);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tamperedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, wrongWorld.StatusCode);
        var problems = await Task.WhenAll(new[] { invalid, tamperedResponse, wrongWorld }
            .Select(response => response.Content.ReadFromJsonAsync<M06Problem>()));
        Assert.All(problems, problem => Assert.Equal("invalid_cursor", problem!.Code));
    }

    [Fact]
    public async Task Feed_ValidatesLimitsSupportsEmptyAndHidesForeignWorlds()
    {
        await using var factory = await CreateFactoryAsync();
        var (ownerClient, owner) = await BootstrapAsync(factory);
        var (foreignClient, _) = await BootstrapAsync(factory);
        _ = await GetFeedAsync(ownerClient, owner.World.Id);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE posts SET deleted_at = {FixedNow} WHERE world_id = {owner.World.Id}");
        }

        var empty = await GetFeedAsync(ownerClient, owner.World.Id);
        var zero = await ownerClient.GetAsync($"/api/v1/worlds/{owner.World.Id}/feed?limit=0");
        var tooLarge = await ownerClient.GetAsync($"/api/v1/worlds/{owner.World.Id}/feed?limit=51");
        var unknown = await ownerClient.GetAsync($"/api/v1/worlds/{owner.World.Id}/feed?sort=popular");
        var foreign = await foreignClient.GetAsync($"/api/v1/worlds/{owner.World.Id}/feed");

        Assert.Empty(empty.Items);
        Assert.False(empty.HasMore);
        Assert.Null(empty.NextCursor);
        Assert.Equal(HttpStatusCode.BadRequest, zero.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLarge.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal("resource_not_available", (await foreign.Content.ReadFromJsonAsync<M06Problem>())!.Code);
    }

    [Fact]
    public async Task PostgreSql_RejectsCrossWorldAuthorParentAndNegativeCounts()
    {
        await using var factory = await CreateFactoryAsync();
        var (firstClient, first) = await BootstrapAsync(factory);
        var (secondClient, second) = await BootstrapAsync(factory);
        var firstFeed = await GetFeedAsync(firstClient, first.World.Id);
        var secondFeed = await GetFeedAsync(secondClient, second.World.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var firstEvent = await db.GameplayEvents.FirstAsync(item => item.WorldId == first.World.Id);
        db.Posts.Add(new Post(
            Guid.NewGuid(),
            first.World.Id,
            secondFeed.Items[0].Author.ActorId,
            null,
            firstEvent.Id,
            "Cross-world author",
            FixedNow));
        var authorFailure = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, FindPostgres(authorFailure).SqlState);
        db.ChangeTracker.Clear();

        var firstActorId = firstFeed.Items[0].Author.ActorId;
        var eventId = Guid.NewGuid();
        db.GameplayEvents.Add(new GameplayEvent(
            eventId,
            first.World.Id,
            "postCreated",
            firstActorId,
            null,
            FixedNow,
            0,
            0,
            "test_post",
            1,
            $"test:{eventId:N}",
            FixedNow));
        db.Posts.Add(new Post(
            Guid.NewGuid(),
            first.World.Id,
            firstActorId,
            secondFeed.Items[0].Id,
            eventId,
            "Cross-world parent",
            FixedNow));
        var parentFailure = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, FindPostgres(parentFailure).SqlState);
        db.ChangeTracker.Clear();

        var countFailure = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE posts SET like_count = -1 WHERE id = {firstFeed.Items[0].Id}"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, countFailure.SqlState);
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

    private static Task<HttpResponseMessage> CreatePostAsync(
        HttpClient client,
        Guid worldId,
        string idempotencyKey,
        Guid clientPostId,
        string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/worlds/{worldId}/posts")
        {
            Content = JsonContent.Create(new { content, clientPostId }),
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        return client.SendAsync(request);
    }

    private static async Task<M06FeedPage> GetFeedAsync(
        HttpClient client,
        Guid worldId,
        int? limit = null,
        string? cursor = null)
    {
        var query = new List<string>();
        if (limit is not null)
        {
            query.Add($"limit={limit}");
        }

        if (cursor is not null)
        {
            query.Add($"cursor={Uri.EscapeDataString(cursor)}");
        }

        var suffix = query.Count == 0 ? string.Empty : $"?{string.Join('&', query)}";
        return (await client.GetFromJsonAsync<M06FeedPage>($"/api/v1/worlds/{worldId}/feed{suffix}"))!;
    }

    private static async Task AddPostAsync(
        M03ApiFactory factory,
        Guid worldId,
        Guid actorId,
        Guid postId,
        DateTimeOffset createdAt,
        string content)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var eventId = Guid.NewGuid();
        db.GameplayEvents.Add(new GameplayEvent(
            eventId,
            worldId,
            "postCreated",
            actorId,
            null,
            createdAt,
            0,
            0,
            "test_post",
            1,
            $"test:{eventId:N}",
            createdAt));
        db.Posts.Add(new Post(postId, worldId, actorId, null, eventId, content, createdAt));
        await db.SaveChangesAsync();
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

internal sealed record M06FeedPage(
    IReadOnlyList<M06FeedPost> Items,
    string? NextCursor,
    bool HasMore);

internal sealed record M06FeedPost(
    Guid Id,
    Guid WorldId,
    M06FeedAuthor Author,
    string Content,
    DateTimeOffset CreatedAtUtc,
    M06PostParent? Parent,
    M06FeedCounts Counts,
    string Visibility);

internal sealed record M06FeedAuthor(Guid ActorId, string DisplayName, string Handle, string ActorType);

internal sealed record M06PostParent(Guid Id);

internal sealed record M06FeedCounts(int Likes, int Replies);

internal sealed record M06Problem(string Code);

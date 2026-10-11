using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Notifications;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Notifications;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class NotificationTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 10, 11, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ReplyProjection_UsesImmediateParentAndReplayDoesNotDuplicate()
    {
        await using var factory = await CreateFactoryAsync(
            new MutableTimeProvider(CreatedAt.AddMinutes(1)));
        var (client, guest) = await BootstrapAsync(factory);
        _ = await client.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{guest.World.Id}/characters");
        Guid[] eligibleReplyIds;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            var writer = scope.ServiceProvider.GetRequiredService<INotificationProjectionWriter>();
            var player = await db.Actors.SingleAsync(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Player);
            var character = await db.Actors
                .Where(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Character)
                .OrderBy(x => x.Id)
                .FirstAsync();
            var posts = new List<Post>();
            Post AddPost(Guid author, Guid? parent, int ordinal)
            {
                var eventId = Guid.NewGuid();
                var post = new Post(Guid.NewGuid(), guest.World.Id, author, parent, eventId,
                    $"M16 reply fixture {ordinal}", CreatedAt.AddSeconds(ordinal));
                db.GameplayEvents.Add(new GameplayEvent(eventId, guest.World.Id, "replyCreated",
                    author, null, post.CreatedAt, 0, 0, "m16_reply_fixture", 1,
                    $"m16:test:reply:{ordinal}", post.CreatedAt));
                posts.Add(post);
                return post;
            }

            var playerRoot = AddPost(player.Id, null, 1);
            var characterRoot = AddPost(character.Id, null, 2);
            var directToPlayerPost = AddPost(character.Id, playerRoot.Id, 3);
            var playerReply = AddPost(player.Id, characterRoot.Id, 4);
            var directToPlayerReply = AddPost(character.Id, playerReply.Id, 5);
            var characterReply = AddPost(character.Id, characterRoot.Id, 6);
            var characterToCharacter = AddPost(character.Id, characterReply.Id, 7);
            var elsewhereInPlayerRoot = AddPost(character.Id, directToPlayerPost.Id, 8);
            var playerAuthored = AddPost(player.Id, characterRoot.Id, 9);
            db.Posts.AddRange(posts);
            await db.SaveChangesAsync();

            foreach (var reply in new[]
                     {
                         directToPlayerPost, directToPlayerReply, characterToCharacter,
                         elsewhereInPlayerRoot, playerAuthored,
                     })
                await writer.ProjectReplyAsync(guest.World.Id, reply.Id, default);
            await db.SaveChangesAsync();
            eligibleReplyIds = [directToPlayerPost.Id, directToPlayerReply.Id];

            await writer.ProjectReplyAsync(guest.World.Id, directToPlayerPost.Id, default);
            await db.SaveChangesAsync();
            Assert.Equal(2, await db.Notifications.CountAsync(x => x.WorldId == guest.World.Id));
            Assert.All(await db.Notifications.ToArrayAsync(), item =>
            {
                Assert.Equal(NotificationCategory.Reply, item.Category);
                Assert.Equal(NotificationSourceType.GameplayEvent, item.SourceType);
                Assert.Equal(item.SourceId, item.GameplayEventId);
            });
        }

        var first = await client.GetFromJsonAsync<M16NotificationPage>(
            $"/api/v1/worlds/{guest.World.Id}/notifications?limit=1");
        Assert.Single(first!.Items);
        Assert.True(first.HasMore);
        Assert.NotNull(first.NextCursor);
        Assert.Contains(first.Items[0].ResourceId, eligibleReplyIds);
        var second = await client.GetFromJsonAsync<M16NotificationPage>(
            $"/api/v1/worlds/{guest.World.Id}/notifications?limit=1&cursor={Uri.EscapeDataString(first.NextCursor!)}");
        Assert.Single(second!.Items);
        Assert.Empty(first.Items.Select(x => x.Id).Intersect(second.Items.Select(x => x.Id)));

        var count = await client.GetFromJsonAsync<M16UnreadCount>(
            $"/api/v1/worlds/{guest.World.Id}/notifications/unread-count");
        Assert.Equal(2, count!.Count);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync(
            $"/api/v1/worlds/{guest.World.Id}/notifications/{first.Items[0].Id}/read", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync(
            $"/api/v1/worlds/{guest.World.Id}/notifications/{first.Items[0].Id}/read", null)).StatusCode);

        var (foreignClient, _) = await BootstrapAsync(factory);
        Assert.Equal(HttpStatusCode.NotFound, (await foreignClient.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/notifications")).StatusCode);
    }

    [Fact]
    public async Task CatchUp_NotifiesOnlyFinalizedSummaryAndRetryReusesTypedSource()
    {
        var clock = new MutableTimeProvider(CreatedAt.AddDays(3));
        await using var factory = await CreateFactoryAsync(clock);
        var (_, guest) = await BootstrapAsync(factory);
        await RebaseWorldAsync(factory, guest.World.Id, CreatedAt);

        await using (var firstScope = factory.Services.CreateAsyncScope())
        {
            var first = await firstScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
            Assert.Equal(CatchUpDisposition.Partial, first.Disposition);
        }
        await using (var assertionScope = factory.Services.CreateAsyncScope())
        {
            var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            Assert.Empty(await db.Notifications.Where(x => x.WorldId == guest.World.Id).ToArrayAsync());
        }

        await using (var secondScope = factory.Services.CreateAsyncScope())
        {
            var second = await secondScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);
            Assert.Equal(CatchUpDisposition.Completed, second.Disposition);
        }
        await using (var retryScope = factory.Services.CreateAsyncScope())
            _ = await retryScope.ServiceProvider.GetRequiredService<ISimulationService>()
                .ProcessCatchUpAsync(guest.World.Id);

        await using var finalScope = factory.Services.CreateAsyncScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var notification = Assert.Single(await finalDb.Notifications
            .Where(x => x.WorldId == guest.World.Id).ToArrayAsync());
        var summary = await finalDb.CatchUpSummaries.SingleAsync(x => x.WorldId == guest.World.Id);
        Assert.Equal(NotificationSourceType.CatchUpSummary, notification.SourceType);
        Assert.Equal(NotificationCategory.CatchUpSummary, notification.Category);
        Assert.Equal(summary.Id, notification.SourceId);
        Assert.Null(notification.GameplayEventId);
        Assert.Equal(CatchUpSummaryStatus.Completed, summary.Status);
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync(TimeProvider clock)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(connectionString, clock);
    }

    private static async Task<(HttpClient Client, M03GuestResponse Guest)> BootstrapAsync(M03ApiFactory factory)
    {
        var client = factory.CreateClient();
        var response = await client.BootstrapAsync(M03TestClient.NewSecret());
        response.EnsureSuccessStatusCode();
        var guest = (await response.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        client.Authenticate(guest.AccessToken);
        return (client, guest);
    }

    private static async Task RebaseWorldAsync(M03ApiFactory factory, Guid worldId, DateTimeOffset createdAt)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE game_worlds SET created_at = {createdAt}, current_world_time = {createdAt},
                last_simulated_at = {createdAt}, updated_at = {createdAt} WHERE id = {worldId};
            UPDATE world_simulation_states SET created_at = {createdAt}, updated_at = {createdAt},
                next_due_at = {createdAt.AddMinutes(15)}, last_completed_interval_end = NULL,
                deterministic_sequence = 0 WHERE world_id = {worldId};
            """);
    }
}

internal sealed record M16NotificationItem(Guid Id, string Category, string Title,
    Guid ResourceId, bool IsRead, DateTimeOffset CreatedAtUtc);
internal sealed record M16NotificationPage(IReadOnlyList<M16NotificationItem> Items,
    string? NextCursor, bool HasMore);
internal sealed record M16UnreadCount(int Count);

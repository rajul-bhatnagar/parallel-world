using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Relationships;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;
using ParallelWorld.Simulation;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class RelationshipPersistenceTests
{
    [Theory]
    [InlineData(24)]
    [InlineData(1)]
    [InlineData(48)]
    public async Task RepetitionWindow_UsesGameDatesInsteadOfUtcSpacing(int utcHoursPerGameDay)
    {
        await using var factory = await CreateFactoryAsync();
        var client = factory.CreateClient();
        using var bootstrap = await client.BootstrapAsync(M03TestClient.NewSecret());
        var guest = (await bootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        client.Authenticate(guest.AccessToken);
        (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")).EnsureSuccessStatusCode();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IRelationshipService>();
        var actors = await db.Actors
            .Where(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Character)
            .OrderBy(x => x.Id)
            .Take(2)
            .Select(x => x.Id)
            .ToArrayAsync();
        var currentGameDate = new DateOnly(2032, 6, 20);
        var utcOrigin = new DateTimeOffset(2026, 10, 3, 0, 0, 0, TimeSpan.Zero);

        async Task Apply(Guid source, Guid target, DateOnly gameDate, DateTimeOffset utc)
        {
            var eventId = Guid.NewGuid();
            db.GameplayEvents.Add(new(eventId, guest.World.Id, "helpfulReply", source, target, utc, 50, 10, "authoritative_helpful_reply", 1, $"test:{eventId:N}", utc));
            await db.SaveChangesAsync();
            await service.ApplyAsync(new(guest.World.Id, source, target, eventId, "Helpful reply", false, true, gameDate, utc, 1, $"rel:{eventId:N}"));
        }

        var insideDates = new[] { currentGameDate.AddDays(-7), currentGameDate.AddDays(-3), currentGameDate.AddDays(-1), currentGameDate };
        for (var index = 0; index < insideDates.Length; index++)
        {
            await Apply(actors[0], actors[1], insideDates[index], utcOrigin.AddHours(index * utcHoursPerGameDay));
        }

        var outsideDates = new[] { currentGameDate.AddDays(-10), currentGameDate.AddDays(-9), currentGameDate.AddDays(-8), currentGameDate };
        for (var index = 0; index < outsideDates.Length; index++)
        {
            await Apply(actors[1], actors[0], outsideDates[index], utcOrigin.AddMinutes(index));
        }

        var inside = await db.Relationships.SingleAsync(x => x.WorldId == guest.World.Id && x.SourceActorId == actors[0] && x.TargetActorId == actors[1]);
        var outside = await db.Relationships.SingleAsync(x => x.WorldId == guest.World.Id && x.SourceActorId == actors[1] && x.TargetActorId == actors[0]);
        Assert.Equal(57, inside.Trust);
        Assert.Equal(58, outside.Trust);
    }

    [Fact]
    public async Task FollowCandidates_ResolveLazyDefaultsAndRespectFollowCooldowns()
    {
        await using var factory = await CreateFactoryAsync();
        var client = factory.CreateClient();
        using var bootstrap = await client.BootstrapAsync(M03TestClient.NewSecret());
        var guest = (await bootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        client.Authenticate(guest.AccessToken);
        (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")).EnsureSuccessStatusCode();
        var currentGameTime = new DateTimeOffset(2026, 10, 2, 18, 30, 0, TimeSpan.Zero);
        var currentGameDate = new DateOnly(2026, 10, 3);

        await using var scope = factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISimulationRepository>();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var actorIds = await db.Actors
            .Where(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Character)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToArrayAsync();

        var initial = await repository.ListFollowCandidatesAsync(guest.World.Id, currentGameTime, currentGameDate, default);
        Assert.Equal(actorIds.Length * (actorIds.Length - 1), initial.Count);
        Assert.All(initial, candidate => Assert.Equal(RelationshipValues.Initial, candidate.Relationship));

        var eventId = Guid.NewGuid();
        db.GameplayEvents.Add(new(
            eventId,
            guest.World.Id,
            "followStarted",
            actorIds[0],
            actorIds[1],
            DateTimeOffset.UtcNow.AddDays(-30),
            50,
            0,
            "follow_01",
            1,
            $"test:{eventId:N}",
            DateTimeOffset.UtcNow.AddDays(-30)));
        db.Follows.Add(new(
            Guid.NewGuid(),
            guest.World.Id,
            actorIds[0],
            actorIds[1],
            DateTimeOffset.UtcNow.AddDays(-30),
            currentGameTime.AddDays(-1),
            currentGameDate.AddDays(-1),
            eventId,
            $"test:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var duringCooldown = await repository.ListFollowCandidatesAsync(guest.World.Id, currentGameTime, currentGameDate, default);
        Assert.DoesNotContain(duringCooldown, x => x.SourceActorId == actorIds[0] && x.TargetActorId == actorIds[1]);
    }

    [Fact]
    public async Task FollowCandidates_CountOnlyQualifiedNegativesWithinSevenGameDays()
    {
        await using var factory = await CreateFactoryAsync();
        var client = factory.CreateClient();
        using var bootstrap = await client.BootstrapAsync(M03TestClient.NewSecret());
        var guest = (await bootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        client.Authenticate(guest.AccessToken);
        (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")).EnsureSuccessStatusCode();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IRelationshipService>();
        var repository = scope.ServiceProvider.GetRequiredService<ISimulationRepository>();
        var actors = await db.Actors
            .Where(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Character)
            .OrderBy(x => x.Id)
            .Take(2)
            .Select(x => x.Id)
            .ToArrayAsync();
        var currentGameTime = new DateTimeOffset(2032, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var currentGameDate = new DateOnly(2032, 6, 20);
        var auditTime = new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

        async Task Apply(string eventType, DateOnly gameDate, int ordinal)
        {
            var eventId = Guid.NewGuid();
            var occurredAt = auditTime.AddMinutes(ordinal);
            db.GameplayEvents.Add(new(eventId, guest.World.Id, eventType, actors[0], actors[1], occurredAt, 50, 10, "authoritative_relationship_event", 1, $"test:{eventId:N}", occurredAt));
            await db.SaveChangesAsync();
            await service.ApplyAsync(new(guest.World.Id, actors[0], actors[1], eventId, eventType, false, eventType == "Helpful reply", gameDate, occurredAt, 1, $"rel:{eventId:N}"));
        }

        await Apply("Insult", currentGameDate.AddDays(-8), 0);
        await Apply("Insult", currentGameDate.AddDays(-6), 1);
        await Apply("Insult", currentGameDate.AddDays(-2), 2);
        await Apply("Insult", currentGameDate.AddDays(-1), 3);
        await Apply("Helpful reply", currentGameDate, 4);

        var followEventId = Guid.NewGuid();
        db.GameplayEvents.Add(new(followEventId, guest.World.Id, "followStarted", actors[0], actors[1], auditTime, 50, 0, "follow_01", 1, $"test:{followEventId:N}", auditTime));
        db.Follows.Add(new(Guid.NewGuid(), guest.World.Id, actors[0], actors[1], auditTime, currentGameTime.AddDays(-8), currentGameDate.AddDays(-8), followEventId, $"follow:{followEventId:N}"));
        await db.SaveChangesAsync();

        var candidates = await repository.ListFollowCandidatesAsync(guest.World.Id, currentGameTime, currentGameDate, default);
        var candidate = Assert.Single(candidates, x => x.SourceActorId == actors[0] && x.TargetActorId == actors[1]);
        Assert.Equal(3, candidate.QualifiedNegativeEventCount);
        var decision = new ParallelWorld.Simulation.FollowSimulationRule().Evaluate(new(
            guest.World.Id,
            Guid.NewGuid(),
            auditTime,
            auditTime.AddMinutes(15),
            currentGameTime,
            currentGameTime,
            1,
            1,
            42,
            ParallelWorld.Simulation.SimulationInputAvailability.M10,
            new ZeroRandom(),
            [candidate]));
        Assert.False(decision.FollowAction!.DesiredFollowing);
    }

    [Fact]
    public async Task PlayerFollowSeparation_UnfollowAndRefollowApplyExactDirectionalRows()
    {
        await using var factory = await CreateFactoryAsync(); var client = factory.CreateClient();
        using var bootstrap = await client.BootstrapAsync(M03TestClient.NewSecret()); var guest = (await bootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!; client.Authenticate(guest.AccessToken);
        (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")).EnsureSuccessStatusCode();
        Guid target;
        await using (var scope = factory.Services.CreateAsyncScope()) { var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>(); target = await db.Actors.Where(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Character).OrderBy(x => x.Id).Select(x => x.Id).FirstAsync(); }
        using var follow = await client.PutAsync($"/api/v1/worlds/{guest.World.Id}/actors/{target}/follow", null); Assert.Equal(HttpStatusCode.OK, follow.StatusCode);
        await using (var scope = factory.Services.CreateAsyncScope()) { var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>(); Assert.Empty(await db.Relationships.Where(x => x.WorldId == guest.World.Id).ToListAsync()); }
        using var unfollow = await client.DeleteAsync($"/api/v1/worlds/{guest.World.Id}/actors/{target}/follow"); Assert.Equal(HttpStatusCode.NoContent, unfollow.StatusCode);
        using var refollow = await client.PutAsync($"/api/v1/worlds/{guest.World.Id}/actors/{target}/follow", null); Assert.Equal(HttpStatusCode.OK, refollow.StatusCode);
        await using var assertion = factory.Services.CreateAsyncScope(); var assertionDb = assertion.ServiceProvider.GetRequiredService<ParallelWorldDbContext>(); var relationship = await assertionDb.Relationships.SingleAsync(x => x.WorldId == guest.World.Id);
        Assert.Equal(12, relationship.Familiarity); Assert.Equal(49, relationship.Trust); Assert.Equal(49, relationship.Respect); Assert.Equal(19, relationship.Affection); Assert.Equal(15, relationship.Comfort); Assert.Equal(0, relationship.Rivalry);
        Assert.Equal(2, await assertionDb.RelationshipEvents.CountAsync(x => x.RelationshipId == relationship.Id)); Assert.Equal(2, await assertionDb.Follows.CountAsync(x => x.WorldId == guest.World.Id));
        var direct = await assertion.ServiceProvider.GetRequiredService<IRelationshipService>().ListAsync(guest.User.Id, guest.World.Id, default);
        Assert.True(direct.IsSuccess);

        using var list = await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/relationships");
        var projection = await list.Content.ReadAsStringAsync();
        Assert.True(list.StatusCode == HttpStatusCode.OK, projection);
        Assert.Contains("stranger", projection, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("familiarity", projection, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("beforeValues", projection, StringComparison.OrdinalIgnoreCase);

        using var foreignClient = factory.CreateClient();
        using var foreignBootstrap = await foreignClient.BootstrapAsync(M03TestClient.NewSecret());
        var foreign = (await foreignBootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!;
        foreignClient.Authenticate(foreign.AccessToken);
        using var hidden = await foreignClient.GetAsync($"/api/v1/worlds/{guest.World.Id}/relationships");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }

    [Fact]
    public async Task ConcurrentApprovedEvents_AreAppliedExactlyOnceWithoutLostUpdate()
    {
        await using var factory = await CreateFactoryAsync(); var client = factory.CreateClient(); using var bootstrap = await client.BootstrapAsync(M03TestClient.NewSecret()); var guest = (await bootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!; client.Authenticate(guest.AccessToken); (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")).EnsureSuccessStatusCode();
        Guid source; Guid target; var eventIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await using (var scope = factory.Services.CreateAsyncScope()) { var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>(); var actors = await db.Actors.Where(x => x.WorldId == guest.World.Id && x.ActorType == ActorType.Character).OrderBy(x => x.Id).Take(2).Select(x => x.Id).ToArrayAsync(); source = actors[0]; target = actors[1]; foreach (var id in eventIds) db.GameplayEvents.Add(new(id, guest.World.Id, "helpfulReply", source, target, DateTimeOffset.UtcNow, 50, 10, "authoritative_helpful_reply", 1, $"test:{id:N}", DateTimeOffset.UtcNow)); await db.SaveChangesAsync(); }
        async Task Apply(Guid id) { await using var scope = factory.Services.CreateAsyncScope(); var service = scope.ServiceProvider.GetRequiredService<IRelationshipService>(); await service.ApplyAsync(new(guest.World.Id, source, target, id, "Helpful reply", false, true, new DateOnly(2026, 10, 2), DateTimeOffset.UtcNow, 1, $"rel:{id:N}")); }
        await Task.WhenAll(eventIds.Select(Apply)); await Apply(eventIds[0]);
        await using var assertion = factory.Services.CreateAsyncScope(); var db2 = assertion.ServiceProvider.GetRequiredService<ParallelWorldDbContext>(); var relationship = await db2.Relationships.SingleAsync(x => x.WorldId == guest.World.Id && x.SourceActorId == source && x.TargetActorId == target);
        Assert.Equal(12, relationship.Familiarity); Assert.Equal(54, relationship.Trust); Assert.Equal(52, relationship.Respect); Assert.Equal(22, relationship.Affection); Assert.Equal(19, relationship.Comfort); Assert.Equal(2, await db2.RelationshipEvents.CountAsync(x => x.RelationshipId == relationship.Id)); Assert.False(await db2.Relationships.AnyAsync(x => x.SourceActorId == target && x.TargetActorId == source));
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync() { var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Default"); Assert.False(string.IsNullOrWhiteSpace(connection)); return await M03ApiFactory.CreateAsync(connection, TimeProvider.System); }
    private sealed class ZeroRandom : IDeterministicRandomProvider
    { public int NextInt(DeterministicChoiceCoordinates coordinates, int inclusiveMinimum, int exclusiveMaximum) => inclusiveMinimum; }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Relationships;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class RomancePersistenceTests
{
    [Fact]
    public async Task PlayerInvitation_AcceptsImmediatelyAndAppliesBilateralCommitmentOnce()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 1, bidirectional: true);
        var key = $"m13-{Guid.NewGuid():N}";

        using var response = await InviteAsync(client, guest.World.Id, actors.CharacterIds[0], key);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<InvitationResponse>();
        Assert.Equal("CasualDate", body!.DateType);
        Assert.Equal("accepted", body.Status);
        Assert.Equal("dating", body.RomanticStatus);

        using var replay = await InviteAsync(client, guest.World.Id, actors.CharacterIds[0], key);
        Assert.Equal(HttpStatusCode.Created, replay.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var directions = await db.Relationships.Where(x => x.WorldId == guest.World.Id
            && ((x.SourceActorId == actors.Player && x.TargetActorId == actors.Characters[0])
                || (x.SourceActorId == actors.Characters[0] && x.TargetActorId == actors.Player))).ToListAsync();
        Assert.Equal(2, directions.Count);
        Assert.All(directions, x => Assert.Equal(10, x.Commitment));
        Assert.Equal(1, await db.RomanticInvitations.CountAsync(x => x.WorldId == guest.World.Id));
        Assert.Equal(2, await db.RomanticStatusHistory.CountAsync(x => x.WorldId == guest.World.Id));
    }

    [Fact]
    public async Task CharacterInvitation_ToPlayerWaitsForExplicitOutcomeAndExpiresByWorldTime()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 2, bidirectional: true);
        Guid invitationId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRomanceService>();
            var created = await service.InviteAutonomouslyAsync(guest.World.Id, actors.Characters[0], actors.Player, $"m13-{Guid.NewGuid():N}", default);
            Assert.True(created.IsSuccess);
            Assert.Equal("pending", created.Value!.Status);
            invitationId = created.Value.Id;
        }

        using var accept = await client.PostAsJsonAsync($"/api/v1/worlds/{guest.World.Id}/date-invitations/{invitationId}/outcome", new { decision = "accept" });
        Assert.Equal(HttpStatusCode.OK, accept.StatusCode);
        Assert.Equal("accepted", (await accept.Content.ReadFromJsonAsync<InvitationResponse>())!.Status);

        await using var secondFactory = await CreateFactoryAsync();
        var (secondClient, secondGuest) = await BootstrapAsync(secondFactory);
        var secondActors = await SeedEligibleRelationshipsAsync(secondFactory, secondGuest.World.Id, 1, bidirectional: true);
        Guid expiringId;
        await using (var scope = secondFactory.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRomanceService>();
            var created = await service.InviteAutonomouslyAsync(secondGuest.World.Id, secondActors.Characters[0], secondActors.Player, $"m13-{Guid.NewGuid():N}", default);
            expiringId = created.Value!.Id;
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            var world = await db.GameWorlds.SingleAsync(x => x.Id == secondGuest.World.Id);
            world.AdvanceSimulation(world.LastSimulatedAt.AddMinutes(15), TimeSpan.FromHours(25).Ticks, DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }
        using var list = await secondClient.GetAsync($"/api/v1/worlds/{secondGuest.World.Id}/date-invitations");
        list.EnsureSuccessStatusCode();
        var listText = await list.Content.ReadAsStringAsync();
        Assert.Contains(expiringId.ToString(), listText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("expired", listText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConcurrentInvitations_AllowOnlyOneDatingPartner()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 2, bidirectional: true);
        var calls = actors.CharacterIds.Select((character, index) => InviteAsync(client, guest.World.Id, character, $"m13-concurrent-{index}-{Guid.NewGuid():N}")).ToArray();
        var responses = await Task.WhenAll(calls);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
        foreach (var response in responses) response.Dispose();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(1, await db.RomanticRelationships.CountAsync(x => x.WorldId == guest.World.Id && x.Status == RomanticStatus.Dating));
        Assert.Equal(1, await db.RomanticInvitations.CountAsync(x => x.WorldId == guest.World.Id));
    }

    [Fact]
    public async Task CharacterRejection_AppliesNoCommitmentAndStartsFourteenDayCooldown()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 1, bidirectional: false);
        using var rejected = await InviteAsync(client, guest.World.Id, actors.CharacterIds[0], $"m13-{Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.Created, rejected.StatusCode);
        Assert.Equal("rejected", (await rejected.Content.ReadFromJsonAsync<InvitationResponse>())!.Status);
        using var cooldown = await InviteAsync(client, guest.World.Id, actors.CharacterIds[0], $"m13-{Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.Conflict, cooldown.StatusCode);
        Assert.Contains("invitation-cooldown-active", await cooldown.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.DoesNotContain(await db.Relationships.Where(x => x.WorldId == guest.World.Id).ToListAsync(), x => x.Commitment != 0);
    }

    [Fact]
    public async Task PlayerCanExplicitlyRejectPendingInvitationWithoutRelationshipDelta()
    {
        await using var factory = await CreateFactoryAsync(); var (client, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 1, bidirectional: true); Guid invitationId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<IRomanceService>()
                .InviteAutonomouslyAsync(guest.World.Id, actors.Characters[0], actors.Player, $"m13-{Guid.NewGuid():N}", default);
            invitationId = result.Value!.Id;
        }
        using var response = await client.PostAsJsonAsync($"/api/v1/worlds/{guest.World.Id}/date-invitations/{invitationId}/outcome", new { decision = "reject" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("rejected", (await response.Content.ReadFromJsonAsync<InvitationResponse>())!.Status);
        await using var assertion = factory.Services.CreateAsyncScope();
        Assert.DoesNotContain(await assertion.ServiceProvider.GetRequiredService<ParallelWorldDbContext>().Relationships.Where(x => x.WorldId == guest.World.Id).ToListAsync(), x => x.Commitment != 0);
    }

    [Fact]
    public async Task ConcurrentCharacterInvitations_AllowOnlyOnePendingInvitationForPlayer()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 2, bidirectional: true);
        async Task<RelationshipResult<RomanticInvitationView>> Invite(Guid initiator)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IRomanceService>()
                .InviteAutonomouslyAsync(guest.World.Id, initiator, actors.Player, $"m13-{Guid.NewGuid():N}", default);
        }
        var results = await Task.WhenAll(actors.Characters.Select(Invite));
        Assert.Single(results, x => x.IsSuccess && x.Value!.Status == "pending");
        Assert.Single(results, x => !x.IsSuccess && x.Failure!.Code == RomanceReasonCodes.ExclusivityConflict);
        await using var assertion = factory.Services.CreateAsyncScope();
        Assert.Equal(1, await assertion.ServiceProvider.GetRequiredService<ParallelWorldDbContext>().RomanticInvitations.CountAsync(x => x.WorldId == guest.World.Id && x.Status == RomanticInvitationStatus.Pending));
    }

    [Fact]
    public async Task RomanceEndpoints_EnforceWorldOwnershipAndHideRawScores()
    {
        await using var factory = await CreateFactoryAsync();
        var (owner, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 1, bidirectional: true);
        using var created = await InviteAsync(owner, guest.World.Id, actors.CharacterIds[0], $"m13-{Guid.NewGuid():N}");
        created.EnsureSuccessStatusCode();
        var projection = await created.Content.ReadAsStringAsync();
        Assert.DoesNotContain("initiationScore", projection, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("acceptanceScore", projection, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("seededOffset", projection, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("commitment", projection, StringComparison.OrdinalIgnoreCase);

        var (foreign, _) = await BootstrapAsync(factory);
        using var hidden = await foreign.GetAsync($"/api/v1/worlds/{guest.World.Id}/date-invitations");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }

    [Fact]
    public async Task RomanticHistory_PreservesDistinctSeededEpisodesWithoutEnablingTransitions()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, guest) = await BootstrapAsync(factory);
        var actors = await SeedEligibleRelationshipsAsync(factory, guest.World.Id, 1, bidirectional: false);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var pair = new RomanticRelationship(Guid.NewGuid(), guest.World.Id, actors.Player, actors.Characters[0], guest.World.CurrentGameTimeUtc, DateTimeOffset.UtcNow);
        db.RomanticRelationships.Add(pair);
        var firstEpisode = Guid.NewGuid(); var secondEpisode = Guid.NewGuid();
        db.RomanticStatusHistory.AddRange(
            new RomanticStatusHistory(Guid.NewGuid(), guest.World.Id, pair.Id, firstEpisode, null, RomanticStatus.InvitationPending, RomanticStatus.Dating, actors.Player, "seeded_start", DateTimeOffset.UtcNow, guest.World.CurrentGameTimeUtc, 1, $"history:{Guid.NewGuid():N}"),
            new RomanticStatusHistory(Guid.NewGuid(), guest.World.Id, pair.Id, firstEpisode, null, RomanticStatus.Dating, RomanticStatus.FormerPartner, actors.Player, "seeded_end", DateTimeOffset.UtcNow, guest.World.CurrentGameTimeUtc.AddDays(1), 1, $"history:{Guid.NewGuid():N}"),
            new RomanticStatusHistory(Guid.NewGuid(), guest.World.Id, pair.Id, secondEpisode, null, RomanticStatus.InvitationPending, RomanticStatus.Dating, actors.Player, "seeded_restart", DateTimeOffset.UtcNow, guest.World.CurrentGameTimeUtc.AddDays(2), 1, $"history:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();
        var rows = await db.RomanticStatusHistory.AsNoTracking().Where(x => x.RomanticRelationshipId == pair.Id).ToListAsync();
        Assert.Equal(3, rows.Count); Assert.Equal(2, rows.Select(x => x.EpisodeId).Distinct().Count());
        Assert.Throws<InvalidOperationException>(() => pair.RejectOrExpire(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
    }

    private static async Task<(HttpClient Client, M03GuestResponse Guest)> BootstrapAsync(M03ApiFactory factory)
    {
        var client = factory.CreateClient(); using var bootstrap = await client.BootstrapAsync(M03TestClient.NewSecret());
        var guest = (await bootstrap.Content.ReadFromJsonAsync<M03GuestResponse>())!; client.Authenticate(guest.AccessToken);
        (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")).EnsureSuccessStatusCode();
        return (client, guest);
    }

    private static async Task<ActorSet> SeedEligibleRelationshipsAsync(M03ApiFactory factory, Guid worldId, int characterCount, bool bidirectional)
    {
        await using var scope = factory.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var player = await db.Actors.Where(x => x.WorldId == worldId && x.ActorType == ActorType.Player).Select(x => x.Id).SingleAsync();
        var characters = await db.Actors.Where(x => x.WorldId == worldId && x.ActorType == ActorType.Character).OrderBy(x => x.Id).Take(characterCount).Select(x => new { ActorId = x.Id, CharacterId = x.CharacterId!.Value }).ToArrayAsync();
        foreach (var character in characters)
        {
            AddEligible(db, worldId, player, character.ActorId);
            if (bidirectional) AddEligible(db, worldId, character.ActorId, player);
        }
        await db.SaveChangesAsync(); return new(player, characters.Select(x => x.ActorId).ToArray(), characters.Select(x => x.CharacterId).ToArray());
    }

    private static void AddEligible(ParallelWorldDbContext db, Guid worldId, Guid source, Guid target)
    {
        var relationship = new Relationship(Guid.NewGuid(), worldId, source, target, DateTimeOffset.UtcNow);
        relationship.Apply(new RelationshipValues(70, 30, 0, 0, 65, 0, 0, 90, 0), DateTimeOffset.UtcNow);
        db.Relationships.Add(relationship);
    }

    private static Task<HttpResponseMessage> InviteAsync(HttpClient client, Guid worldId, Guid target, string key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/worlds/{worldId}/relationships/{target}/date-invitations") { Content = JsonContent.Create(new { }) };
        request.Headers.Add("Idempotency-Key", key);
        return client.SendAsync(request);
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        return await M03ApiFactory.CreateAsync(connection, TimeProvider.System);
    }

    private sealed record ActorSet(Guid Player, Guid[] Characters, Guid[] CharacterIds);
    private sealed record InvitationResponse(Guid Id, string DateType, string Status, string RomanticStatus);
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class CharacterCatalogueTests
{
    [Fact]
    public async Task List_InitializesExactlyTenCharacterActorsAndDetailsOnce()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);

        var first = await client.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{guest.World.Id}/characters");
        var second = await client.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{guest.World.Id}/characters");

        Assert.NotNull(first);
        Assert.Equal(10, first.Items.Count);
        Assert.Equal(first.Items.Select(item => item.Id), second!.Items.Select(item => item.Id));
        Assert.Equal(10, first.Items.Select(item => item.Handle).Distinct().Count());
        Assert.False(first.HasMore);
        Assert.Null(first.NextCursor);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(10, await db.Characters.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(10, await db.Actors.CountAsync(item =>
            item.WorldId == guest.World.Id && item.ActorType == ActorType.Character));
        var playerActor = await db.Actors.SingleAsync(item =>
            item.WorldId == guest.World.Id && item.ActorType == ActorType.Player);
        Assert.NotNull(playerActor.PlayerProfileId);
        Assert.Null(playerActor.CharacterId);
        var characterIds = await db.Characters
            .Where(item => item.WorldId == guest.World.Id)
            .Select(item => item.Id)
            .ToHashSetAsync();
        var actorCharacterIds = await db.Actors
            .Where(item => item.WorldId == guest.World.Id && item.ActorType == ActorType.Character)
            .Select(item => item.CharacterId!.Value)
            .ToHashSetAsync();
        Assert.True(characterIds.SetEquals(actorCharacterIds));
        Assert.Equal(10, await db.CharacterTraits.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(20, await db.CharacterInterests.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(20, await db.CharacterOpinions.CountAsync(item => item.WorldId == guest.World.Id));
        Assert.Equal(10, await db.CharacterSchedules.CountAsync(item => item.WorldId == guest.World.Id));
    }

    [Fact]
    public async Task List_ValidatesDocumentedFiltersAndRejectsUnknownFilters()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);

        var badLimit = await client.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/characters?limit=101");
        var badStatus = await client.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/characters?status=unknown");
        var unknownFilter = await client.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/characters?sort=name");

        Assert.Equal(HttpStatusCode.BadRequest, badLimit.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badStatus.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknownFilter.StatusCode);
    }

    [Fact]
    public async Task List_UsesOpaqueScopedCursorWithoutDuplicates()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);

        var first = await client.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{guest.World.Id}/characters?limit=3");
        Assert.NotNull(first);
        Assert.Equal(3, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.False(string.IsNullOrWhiteSpace(first.NextCursor));

        var second = await client.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{guest.World.Id}/characters?limit=3&cursor={Uri.EscapeDataString(first.NextCursor!)}");
        Assert.NotNull(second);
        Assert.Equal(3, second.Items.Count);
        Assert.Empty(first.Items.Select(item => item.Id).Intersect(second.Items.Select(item => item.Id)));

        var invalid = await client.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/characters?cursor=not-a-cursor");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("invalid_cursor", (await invalid.Content.ReadFromJsonAsync<M05Problem>())!.Code);
    }

    [Fact]
    public async Task Detail_ExposesOnlyApprovedPlayerVisibleProjection()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);
        var page = await client.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{guest.World.Id}/characters");

        var response = await client.GetAsync(
            $"/api/v1/worlds/{guest.World.Id}/characters/{page!.Items[0].Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.True(root.TryGetProperty("bio", out _));
        Assert.True(root.TryGetProperty("interests", out var interests));
        Assert.Equal(2, interests.GetArrayLength());
        Assert.True(root.TryGetProperty("schedule", out var schedule));
        Assert.Single(schedule.EnumerateArray());

        foreach (var hidden in new[]
        {
            "activityLevel", "influence", "popularity", "writingStyle", "traits", "opinions",
            "strength", "position", "confidence", "intensity", "worldId", "actorId", "version",
        })
        {
            Assert.DoesNotContain($"\"{hidden}\"", json, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ForeignWorldAndCharacterIds_ReturnOwnershipSafeNotFound()
    {
        await using var factory = await CreateFactoryAsync();
        var (ownerClient, owner) = await BootstrapAsync(factory);
        var (foreignClient, foreign) = await BootstrapAsync(factory);
        var ownerPage = await ownerClient.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{owner.World.Id}/characters");

        var foreignWorld = await foreignClient.GetAsync(
            $"/api/v1/worlds/{owner.World.Id}/characters");
        var foreignCharacter = await foreignClient.GetAsync(
            $"/api/v1/worlds/{foreign.World.Id}/characters/{ownerPage!.Items[0].Id}");

        Assert.Equal(HttpStatusCode.NotFound, foreignWorld.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreignCharacter.StatusCode);
        Assert.Equal("resource_not_available", (await foreignWorld.Content.ReadFromJsonAsync<M05Problem>())!.Code);
        Assert.Equal("resource_not_available", (await foreignCharacter.Content.ReadFromJsonAsync<M05Problem>())!.Code);
    }

    [Fact]
    public async Task ConcurrentFirstLoads_CreateOneCompleteCast()
    {
        await using var factory = await CreateFactoryAsync();
        var (client, guest) = await BootstrapAsync(factory);

        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
            client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters")));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(10, await db.Characters.CountAsync(item => item.WorldId == guest.World.Id));
    }

    [Fact]
    public async Task SameSeedDifferentWorlds_InitializeIndependentWorldScopedCasts()
    {
        await using var factory = await CreateFactoryAsync();
        var (firstClient, first) = await BootstrapAsync(factory);
        var (secondClient, second) = await BootstrapAsync(factory);
        const long sharedSeed = 90210;

        await using (var setupScope = factory.Services.CreateAsyncScope())
        {
            var setupDb = setupScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await setupDb.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE game_worlds SET seed = {sharedSeed} WHERE id = {first.World.Id} OR id = {second.World.Id}");
            var ruleVersions = await setupDb.WorldSettings
                .Where(item => item.WorldId == first.World.Id || item.WorldId == second.World.Id)
                .Select(item => item.RuleVersion)
                .ToListAsync();
            Assert.Single(ruleVersions.Distinct());
        }

        var firstPage = await firstClient.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{first.World.Id}/characters");
        var secondPage = await secondClient.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{second.World.Id}/characters");

        Assert.Equal(10, firstPage!.Items.Count);
        Assert.Equal(10, secondPage!.Items.Count);
        Assert.Empty(firstPage.Items.Select(item => item.Id).Intersect(secondPage.Items.Select(item => item.Id)));
        Assert.Equal(
            firstPage.Items.OrderBy(item => item.Handle).Select(item => new
            {
                item.DisplayName,
                item.Handle,
                item.Profession,
                item.VisibleMood,
            }),
            secondPage.Items.OrderBy(item => item.Handle).Select(item => new
            {
                item.DisplayName,
                item.Handle,
                item.Profession,
                item.VisibleMood,
            }));

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var worldIds = new[] { first.World.Id, second.World.Id };
        Assert.Equal(20, await db.Characters.CountAsync(item => worldIds.Contains(item.WorldId)));
        Assert.Equal(20, await db.Actors.CountAsync(item =>
            worldIds.Contains(item.WorldId) && item.ActorType == ActorType.Character));
        Assert.Equal(20, await db.CharacterTraits.CountAsync(item => worldIds.Contains(item.WorldId)));
        Assert.Equal(40, await db.CharacterInterests.CountAsync(item => worldIds.Contains(item.WorldId)));
        Assert.Equal(40, await db.CharacterOpinions.CountAsync(item => worldIds.Contains(item.WorldId)));
        Assert.Equal(20, await db.CharacterSchedules.CountAsync(item => worldIds.Contains(item.WorldId)));
    }

    [Fact]
    public async Task PostgreSql_RejectsOutOfRangeTraitsAndCrossWorldActorDetail()
    {
        await using var factory = await CreateFactoryAsync();
        var (firstClient, first) = await BootstrapAsync(factory);
        var (_, second) = await BootstrapAsync(factory);
        var page = await firstClient.GetFromJsonAsync<M05CharacterPage>(
            $"/api/v1/worlds/{first.World.Id}/characters");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var traitId = await db.CharacterTraits
            .Where(item => item.WorldId == first.World.Id)
            .Select(item => item.Id)
            .FirstAsync();
        var bounds = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE character_traits SET humour = 101 WHERE id = {traitId}"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, bounds.SqlState);

        var crossWorld = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE actors SET world_id = {second.World.Id} WHERE world_id = {first.World.Id} AND character_id = {page!.Items[0].Id}"));
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, crossWorld.SqlState);
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(connectionString);
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
}

internal sealed record M05CharacterPage(
    IReadOnlyList<M05CharacterSummary> Items,
    string? NextCursor,
    bool HasMore);

internal sealed record M05CharacterSummary(
    Guid Id,
    string DisplayName,
    string Handle,
    string Profession,
    string VisibleMood,
    bool IsFollowed);

internal sealed record M05Problem(string Code);

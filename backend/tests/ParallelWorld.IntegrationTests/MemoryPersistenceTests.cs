using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Memory;
using ParallelWorld.Domain.Memory;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class MemoryPersistenceTests
{
    [Fact]
    public async Task Capacity_EvictsOnlyNonProtectedAndNeverExceedsOneHundred()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, guest) = await BootstrapAsync(factory);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var actors = await ActorsAsync(db, guest.World.Id);
        var now = DateTimeOffset.UtcNow;
        for (var index = 0; index < 99; index++)
        {
            SeedMemory(db, actors, MemoryType.Secret, MemoryAuthorityType.StructuredSecret,
                DeterministicGuid(index + 1), now.AddMinutes(index));
        }

        var evictableId = DeterministicGuid(500);
        SeedMemory(db, actors, MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact,
            evictableId, now.AddMinutes(100));
        var source = AddEvent(db, actors, "memorable_event", now.AddMinutes(101));
        await db.SaveChangesAsync();

        var service = scope.ServiceProvider.GetRequiredService<IMemoryService>();
        var result = await service.CreateAsync(EventCommand(actors, source.Id));

        Assert.True(result.Created);
        Assert.Equal(evictableId, result.EvictedMemoryId);
        Assert.Equal(100, await db.CharacterMemories.CountAsync(x =>
            x.WorldId == actors.WorldId && x.OwnerCharacterId == actors.CharacterId
            && x.LifecycleStatus == MemoryLifecycleStatus.Active));
        Assert.Equal(MemoryLifecycleStatus.Evicted,
            (await db.CharacterMemories.SingleAsync(x => x.Id == evictableId)).LifecycleStatus);
        Assert.All(await db.CharacterMemories.Where(x => x.MemoryType == MemoryType.Secret).ToListAsync(),
            memory => Assert.Equal(MemoryLifecycleStatus.Active, memory.LifecycleStatus));
    }

    [Fact]
    public async Task FullyProtectedCapacity_RejectsAndConcurrentReplayReusesOutcome()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, guest) = await BootstrapAsync(factory);
        ActorSet actors;
        Guid sourceId;
        await using (var seedScope = factory.Services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            actors = await ActorsAsync(db, guest.World.Id);
            var now = DateTimeOffset.UtcNow;
            for (var index = 0; index < 100; index++)
            {
                SeedMemory(db, actors, MemoryType.Secret, MemoryAuthorityType.StructuredSecret,
                    DeterministicGuid(index + 1), now.AddMinutes(index));
            }

            sourceId = AddEvent(db, actors, "memorable_event", now.AddMinutes(101)).Id;
            await db.SaveChangesAsync();
        }

        var command = EventCommand(actors, sourceId);
        async Task<MemoryCreationResult> CreateAsync()
        {
            await using var callScope = factory.Services.CreateAsyncScope();
            return await callScope.ServiceProvider.GetRequiredService<IMemoryService>()
                .CreateAsync(command);
        }

        var results = await Task.WhenAll(CreateAsync(), CreateAsync());
        Assert.All(results, result =>
        {
            Assert.False(result.Created);
            Assert.Equal(MemoryMechanics.ProtectedCapacityReason, result.ReasonCode);
        });
        Assert.Contains(results, result => result.IsReplay);

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(100, await verify.CharacterMemories.CountAsync(x =>
            x.WorldId == actors.WorldId && x.OwnerCharacterId == actors.CharacterId
            && x.LifecycleStatus == MemoryLifecycleStatus.Active));
        Assert.Equal(100, await verify.CharacterMemories.CountAsync(x =>
            x.WorldId == actors.WorldId && x.OwnerCharacterId == actors.CharacterId
            && x.MemoryType == MemoryType.Secret && x.LifecycleStatus == MemoryLifecycleStatus.Active));
        var outcome = await verify.MemoryCreationOutcomes.SingleAsync(x =>
            x.WorldId == actors.WorldId && x.SourceId == sourceId);
        Assert.Equal(MemoryCreationOutcomeType.Rejected, outcome.Outcome);
        Assert.Equal(MemoryMechanics.ProtectedCapacityReason, outcome.ReasonCode);
    }

    [Fact]
    public async Task Recall_IsOwnerScopedDeterministicTopEightAndExcludesSecretsFromWording()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, guest) = await BootstrapAsync(factory);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var actors = await ActorsAsync(db, guest.World.Id);
        var createdAt = DateTimeOffset.UtcNow;
        var factIds = Enumerable.Range(1, 9).Select(DeterministicGuid).ToArray();
        foreach (var memoryId in factIds)
        {
            SeedMemory(db, actors, MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact,
                memoryId, createdAt);
        }

        var secretId = DeterministicGuid(99);
        SeedMemory(db, actors, MemoryType.Secret, MemoryAuthorityType.StructuredSecret,
            secretId, createdAt);
        await db.SaveChangesAsync();
        var service = scope.ServiceProvider.GetRequiredService<IMemoryService>();

        var wording = await service.RecallAsync(new RecallMemoryCommand(
            actors.WorldId, actors.CharacterId, MemoryRecallPurpose.MessageWording,
            MemorySubjectType.Actor, actors.PlayerActorId, null, null, "wording:one"));
        Assert.True(wording.IsValid);
        Assert.Equal(8, wording.Items.Count);
        Assert.DoesNotContain(wording.Items, item => item.MemoryId == secretId);
        Assert.Equal(factIds.OrderByDescending(id => id).Take(8),
            wording.Items.Select(item => item.MemoryId));

        var replay = await service.RecallAsync(new RecallMemoryCommand(
            actors.WorldId, actors.CharacterId, MemoryRecallPurpose.MessageWording,
            MemorySubjectType.Actor, actors.PlayerActorId, null, null, "wording:one"));
        Assert.True(replay.IsReplay);
        Assert.Equal(wording.Items.Select(x => x.MemoryId), replay.Items.Select(x => x.MemoryId));

        var internalRecall = await service.RecallAsync(new RecallMemoryCommand(
            actors.WorldId, actors.CharacterId, MemoryRecallPurpose.Internal,
            MemorySubjectType.Actor, actors.PlayerActorId, null, null, "internal:one"));
        Assert.Contains(internalRecall.Items, item => item.MemoryId == secretId);
    }

    [Fact]
    public async Task Recall_UsesExactMatchesAndCannotReadAnotherCharacterMemory()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, guest) = await BootstrapAsync(factory);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var owner = await ActorsAsync(db, guest.World.Id);
        var otherOwner = await ActorsAsync(db, guest.World.Id, 1);
        var topicId = await db.CharacterInterests.AsNoTracking()
            .Where(x => x.WorldId == owner.WorldId)
            .Select(x => x.TopicId)
            .FirstAsync();
        var createdAt = DateTimeOffset.UtcNow;
        var exactId = DeterministicGuid(701);
        var mismatchId = DeterministicGuid(702);
        var privateId = DeterministicGuid(703);
        SeedMemory(db, owner, MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact,
            exactId, createdAt, owner.PlayerActorId, topicId);
        SeedMemory(db, owner, MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact,
            mismatchId, createdAt.AddTicks(-1), owner.CharacterActorId, null);
        SeedMemory(db, otherOwner, MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact,
            privateId, createdAt.AddTicks(1), owner.PlayerActorId, topicId);
        await db.SaveChangesAsync();

        var result = await scope.ServiceProvider.GetRequiredService<IMemoryService>().RecallAsync(
            new RecallMemoryCommand(owner.WorldId, owner.CharacterId,
                MemoryRecallPurpose.Internal, MemorySubjectType.Actor,
                owner.PlayerActorId, null, topicId, "matching:one"));

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Items, item => item.MemoryId == privateId);
        var exact = Assert.Single(result.Items, item => item.MemoryId == exactId);
        Assert.Equal(MemoryMechanics.RecallScore(
            100, 100,
            MemoryMechanics.RelationshipMatch(RelationshipValues.Initial),
            60), exact.Score);
        var mismatch = Assert.Single(result.Items, item => item.MemoryId == mismatchId);
        Assert.Equal(MemoryMechanics.RecallScore(
            0, 0,
            MemoryMechanics.RelationshipMatch(RelationshipValues.Initial),
            60), mismatch.Score);
        Assert.True(exact.Rank < mismatch.Rank);
    }

    [Fact]
    public async Task Promise_UsesAuthoritativeEventsAndCrossWorldCreationIsRejected()
    {
        await using var factory = await CreateFactoryAsync();
        var (_, first) = await BootstrapAsync(factory);
        var (_, second) = await BootstrapAsync(factory);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var actors = await ActorsAsync(db, first.World.Id);
        var foreign = await ActorsAsync(db, second.World.Id);
        var source = AddEvent(db, actors, "promise_created", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        var service = scope.ServiceProvider.GetRequiredService<IMemoryService>();
        var promiseCommand = new CreateMemoryCommand(
            actors.WorldId, actors.CharacterId, MemoryType.Promise,
            MemoryAuthorityType.StructuredPromise, MemorySubjectType.Actor,
            actors.PlayerActorId, null, null, "deliver the parcel",
            MemorySourceType.GameplayEvent, source.Id,
            new PromiseCreationDetails("deliver_item", actors.PlayerActorId,
                actors.CharacterActorId, PromiseDueConditionType.GameplayEvent,
                null, null, "parcel_delivered"));

        var created = await service.CreateAsync(promiseCommand);
        Assert.True(created.Created);
        var replay = await service.CreateAsync(promiseCommand);
        Assert.True(replay.Created);
        Assert.True(replay.IsReplay);
        Assert.Equal(created.MemoryId, replay.MemoryId);
        Assert.Equal(1, await db.CharacterMemories.CountAsync(x =>
            x.WorldId == actors.WorldId && x.SourceId == source.Id
            && x.MemoryType == MemoryType.Promise));
        var promise = await db.Promises.SingleAsync(x => x.MemoryId == created.MemoryId);
        var unrelated = AddEvent(db, actors, "different_event", DateTimeOffset.UtcNow.AddMinutes(1));
        var fulfilled = AddEvent(db, actors, "parcel_delivered", DateTimeOffset.UtcNow.AddMinutes(2));
        await db.SaveChangesAsync();

        Assert.False(await service.TransitionPromiseAsync(new(
            actors.WorldId, promise.Id, PromiseStatus.Fulfilled, unrelated.Id,
            DateTimeOffset.UtcNow.AddMinutes(1))));
        Assert.True(await service.TransitionPromiseAsync(new(
            actors.WorldId, promise.Id, PromiseStatus.Fulfilled, fulfilled.Id,
            DateTimeOffset.UtcNow.AddMinutes(2))));
        Assert.Equal(PromiseStatus.Fulfilled,
            (await db.Promises.AsNoTracking().SingleAsync(x => x.Id == promise.Id)).Status);

        var foreignAttempt = promiseCommand with
        {
            WorldId = foreign.WorldId,
            OwnerCharacterId = actors.CharacterId,
            SourceId = source.Id,
        };
        var rejected = await service.CreateAsync(foreignAttempt);
        Assert.False(rejected.Created);
        Assert.Equal("memory_owner_not_available", rejected.ReasonCode);
    }

    private static void SeedMemory(
        ParallelWorldDbContext db,
        ActorSet actors,
        MemoryType type,
        MemoryAuthorityType authority,
        Guid memoryId,
        DateTimeOffset createdAt,
        Guid? subjectActorId = null,
        string? topicId = null)
    {
        var source = AddEvent(db, actors, $"seed_{type.ToString().ToLowerInvariant()}", createdAt);
        var memory = new CharacterMemory(memoryId, actors.WorldId, actors.CharacterId, type,
            authority, MemorySubjectType.Actor, subjectActorId ?? actors.PlayerActorId, null, topicId,
            $"structured-{memoryId:N}", MemorySourceType.GameplayEvent, source.Id,
            source.Id, null, createdAt);
        db.CharacterMemories.Add(memory);
        if (type == MemoryType.Secret)
        {
            var secret = new Secret(Guid.NewGuid(), actors.WorldId, memory.Id, createdAt);
            db.Secrets.Add(secret);
            db.SecretKnowers.Add(new SecretKnower(
                actors.WorldId, secret.Id, actors.CharacterId, createdAt));
        }
    }

    private static GameplayEvent AddEvent(
        ParallelWorldDbContext db,
        ActorSet actors,
        string eventType,
        DateTimeOffset occurredAt)
    {
        var id = Guid.NewGuid();
        var gameplayEvent = new GameplayEvent(id, actors.WorldId, eventType,
            actors.PlayerActorId, actors.CharacterActorId, occurredAt, 50, 0,
            "m12_test", 1, $"m12:{id:N}", occurredAt);
        db.GameplayEvents.Add(gameplayEvent);
        return gameplayEvent;
    }

    private static CreateMemoryCommand EventCommand(ActorSet actors, Guid sourceId) =>
        new(actors.WorldId, actors.CharacterId, MemoryType.Event,
            MemoryAuthorityType.GameplayEvent, MemorySubjectType.Actor,
            actors.PlayerActorId, null, null, "structured event",
            MemorySourceType.GameplayEvent, sourceId, null);

    private static async Task<ActorSet> ActorsAsync(
        ParallelWorldDbContext db,
        Guid worldId,
        int characterIndex = 0)
    {
        var player = await db.Actors.AsNoTracking().SingleAsync(x =>
            x.WorldId == worldId && x.ActorType == ActorType.Player);
        var characterActor = await db.Actors.AsNoTracking().Where(x =>
            x.WorldId == worldId && x.ActorType == ActorType.Character)
            .OrderBy(x => x.Id).Skip(characterIndex).FirstAsync();
        return new(worldId, characterActor.CharacterId!.Value, characterActor.Id, player.Id);
    }

    private static Guid DeterministicGuid(int value) =>
        Guid.Parse($"00000000-0000-0000-0000-{value:x12}");

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(connectionString);
    }

    private static async Task<(HttpClient Client, M03GuestResponse Guest)> BootstrapAsync(
        M03ApiFactory factory)
    {
        var client = factory.CreateClient();
        var response = await client.BootstrapAsync(M03TestClient.NewSecret());
        response.EnsureSuccessStatusCode();
        var guest = await response.Content.ReadFromJsonAsync<M03GuestResponse>();
        client.Authenticate(guest!.AccessToken);
        (await client.GetAsync($"/api/v1/worlds/{guest.World.Id}/characters"))
            .EnsureSuccessStatusCode();
        return (client, guest);
    }

    private sealed record ActorSet(
        Guid WorldId,
        Guid CharacterId,
        Guid CharacterActorId,
        Guid PlayerActorId);
}

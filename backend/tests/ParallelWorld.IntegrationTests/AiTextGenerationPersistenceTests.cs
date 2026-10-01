using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ParallelWorld.AI;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;
using AiGenerationEntity = ParallelWorld.Domain.Simulation.AiGenerationRequest;
using AiTextRequest = ParallelWorld.AI.AiGenerationRequest;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class AiTextGenerationPersistenceTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task DisabledProvider_PersistsSanitizedFallbackAndReplaysIdempotently()
    {
        await using var factory = await CreateFactoryAsync();
        var guest = await BootstrapAsync(factory);
        var actionId = await AddActionAsync(factory, guest);
        var request = CreateRequest(guest.World.Id, actionId, "m09-persist-1");

        AiGenerationResult first;
        AiGenerationResult second;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var generator = scope.ServiceProvider.GetRequiredService<IAiTextGenerator>();
            first = await generator.GenerateAsync(request);
            second = await generator.GenerateAsync(request);
        }

        Assert.Equal(first, second);
        Assert.True(first.FallbackUsed);
        Assert.Equal("sensitive_input", first.FailureCode);
        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var persisted = await db.AiGenerationRequests.SingleAsync();
        Assert.Equal(guest.World.Id, persisted.WorldId);
        Assert.Equal(actionId, persisted.SimulationActionId);
        Assert.Equal("ollama", persisted.Provider);
        Assert.Equal("qwen3:4b", persisted.Model);
        Assert.Equal("m09-v1", persisted.PromptTemplateVersion);
        Assert.Equal(64, persisted.InputHash.Length);
        Assert.Equal(64, persisted.OutputHash.Length);
        Assert.DoesNotContain("Password=", persisted.FinalizedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token", persisted.FinalizedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConcurrentDuplicateGeneration_PersistsOneTerminalRecord()
    {
        await using var factory = await CreateFactoryAsync();
        var guest = await BootstrapAsync(factory);
        var actionId = await AddActionAsync(factory, guest);
        var request = CreateRequest(guest.World.Id, actionId, "m09-concurrent-1");
        await using var firstScope = factory.Services.CreateAsyncScope();
        await using var secondScope = factory.Services.CreateAsyncScope();

        var results = await Task.WhenAll(
            firstScope.ServiceProvider.GetRequiredService<IAiTextGenerator>().GenerateAsync(request),
            secondScope.ServiceProvider.GetRequiredService<IAiTextGenerator>().GenerateAsync(request));

        Assert.Equal(results[0], results[1]);
        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Single(await db.AiGenerationRequests.ToListAsync());
    }

    [Fact]
    public async Task ReusingIdempotencyKeyForDifferentCanonicalPresentation_IsRejected()
    {
        await using var factory = await CreateFactoryAsync();
        var guest = await BootstrapAsync(factory);
        var actionId = await AddActionAsync(factory, guest);
        var original = CreateRequest(guest.World.Id, actionId, "m09-key-reuse");
        var changed = original with
        {
            Presentation = new("different presentation context", []),
        };

        await using var scope = factory.Services.CreateAsyncScope();
        var generator = scope.ServiceProvider.GetRequiredService<IAiTextGenerator>();
        await generator.GenerateAsync(original);

        await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateAsync(changed));
    }

    [Fact]
    public async Task GenerationReferenceToActionInAnotherWorld_IsRejected()
    {
        await using var factory = await CreateFactoryAsync();
        var first = await BootstrapAsync(factory);
        var second = await BootstrapAsync(factory);
        var actionId = await AddActionAsync(factory, first);
        var hash = new string('a', 64);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        db.AiGenerationRequests.Add(new AiGenerationEntity(
            Guid.NewGuid(),
            second.World.Id,
            actionId,
            "ollama",
            "qwen3:4b",
            0,
            hash,
            hash,
            null,
            null,
            0,
            "provider_disabled",
            true,
            "m09-v1",
            "An update was shared.",
            CreatedAt,
            CreatedAt,
            "m09-cross-world"));

        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, FindPostgres(failure).SqlState);
        Assert.Equal(
            "fk_ai_generation_requests_actions_world_action",
            FindPostgres(failure).ConstraintName);
    }

    [Fact]
    public async Task GeneratorRejectsCrossWorldActionBeforeProviderOrPersistence()
    {
        await using var factory = await CreateFactoryAsync();
        var first = await BootstrapAsync(factory);
        var second = await BootstrapAsync(factory);
        var actionId = await AddActionAsync(factory, first);

        await using var scope = factory.Services.CreateAsyncScope();
        var generator = scope.ServiceProvider.GetRequiredService<IAiTextGenerator>();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => generator.GenerateAsync(
            CreateRequest(second.World.Id, actionId, "m09-cross-world-generator")));

        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Empty(await db.AiGenerationRequests.ToListAsync());
    }

    private static AiTextRequest CreateRequest(Guid worldId, Guid actionId, string key) => new(
        worldId,
        actionId,
        key,
        new("Password=do-not-persist; Bearer secret-token", []),
        500);

    private static async Task<Guid> AddActionAsync(M03ApiFactory factory, M03GuestResponse guest)
    {
        var runId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var run = new SimulationRun(
            runId,
            guest.World.Id,
            SimulationRunType.ActiveTick,
            CreatedAt,
            CreatedAt.AddMinutes(15),
            1m,
            42,
            1,
            CreatedAt,
            $"m09-run-{runId:N}");
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var characterId = Guid.NewGuid();
        var characterActorId = Guid.NewGuid();
        var character = new Character(
            characterId,
            guest.World.Id,
            "Ada",
            $"ada-{characterId:N}"[..30],
            "Fixture character for M09 wording verification.",
            32,
            "Astronomer",
            "Scholar",
            "precise",
            50,
            50,
            50,
            MoodType.Calm,
            CreatedAt);
        var characterActor = Actor.CreateCharacter(
            characterActorId,
            guest.World.Id,
            characterId,
            CreatedAt);
        var action = new SimulationAction(
            actionId,
            guest.World.Id,
            runId,
            0,
            characterActorId,
            "post",
            null,
            null,
            null,
            null,
            "curious",
            "m09_fixture",
            CreatedAt,
            $"m09-action-{actionId:N}");
        db.SimulationRuns.Add(run);
        db.Characters.Add(character);
        db.Actors.Add(characterActor);
        db.SimulationActions.Add(action);
        await db.SaveChangesAsync();
        return actionId;
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        return await M03ApiFactory.CreateAsync(connectionString);
    }

    private static async Task<M03GuestResponse> BootstrapAsync(M03ApiFactory factory)
    {
        using var response = await factory.CreateClient().BootstrapAsync(M03TestClient.NewSecret());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<M03GuestResponse>())!;
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

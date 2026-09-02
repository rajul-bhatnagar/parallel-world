using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Domain.Worlds;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class GuestSessionAndWorldTests
{
    [Fact]
    public async Task FirstBootstrap_CreatesExactlyOneHashOnlyM03Aggregate()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var proof = M03TestClient.NewSecret();

        using var response = await client.BootstrapAsync(proof);
        var responseBody = await response.Content.ReadAsStringAsync();
        var session = await response.Content.ReadFromJsonAsync<M03GuestResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(session);
        Assert.Equal("guest", session.User.AccountType);
        Assert.Equal("Player", session.World.Player.DisplayName);
        Assert.NotEqual(proof, session.RefreshToken);
        Assert.DoesNotContain(proof, responseBody, StringComparison.Ordinal);
        Assert.DoesNotContain("\"seed\"", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.True(WebEncoders.Base64UrlDecode(session.RefreshToken).Length >= 32);
        Assert.InRange(
            session.AccessTokenExpiresAtUtc - DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(14),
            TimeSpan.FromMinutes(15));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(1, await db.DeviceInstallations.CountAsync());
        Assert.Equal(1, await db.GuestBootstrapOperations.CountAsync());
        Assert.Equal(1, await db.RefreshTokens.CountAsync());
        Assert.Equal(1, await db.GameWorlds.CountAsync());
        Assert.Equal(1, await db.WorldSettings.CountAsync());
        Assert.Equal(1, await db.WorldSimulationStates.CountAsync());
        Assert.Equal(1, await db.PlayerProfiles.CountAsync());
        Assert.Equal(1, await db.Actors.CountAsync(entity => entity.ActorType == ActorType.Player));
        Assert.DoesNotContain(
            db.Model.GetEntityTypes(),
            entity => entity.ClrType.Name.Contains("Character", StringComparison.Ordinal));

        var operation = await db.GuestBootstrapOperations.SingleAsync();
        var refresh = await db.RefreshTokens.SingleAsync();
        Assert.NotEqual(proof, operation.ProofHash);
        Assert.NotEqual(session.RefreshToken, refresh.TokenHash);
        Assert.Equal(43, operation.ProofHash.Length);
        Assert.Equal(43, refresh.TokenHash.Length);
        Assert.Equal(TimeSpan.FromMinutes(10), operation.ExpiresAt - operation.CompletedAt!.Value);
        Assert.Equal(TimeSpan.FromDays(30), refresh.ExpiresAt - refresh.CreatedAt);
        Assert.Equal(session.User.Id, (await db.GameWorlds.SingleAsync()).OwnerUserId);
    }

    [Fact]
    public async Task SameProof_RecoversOnceWithNewCredentialsAndSameIdentity()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var proof = M03TestClient.NewSecret();
        var installationId = Guid.NewGuid().ToString();

        using var firstResponse = await client.BootstrapAsync(proof, installationId);
        var first = await firstResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        using var recoveryResponse = await client.BootstrapAsync(proof, Guid.NewGuid().ToString());
        var recovered = await recoveryResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        using var secondRecovery = await client.BootstrapAsync(proof, installationId);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, recoveryResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondRecovery.StatusCode);
        Assert.NotNull(first);
        Assert.NotNull(recovered);
        Assert.Equal(first.User.Id, recovered.User.Id);
        Assert.Equal(first.World.Id, recovered.World.Id);
        Assert.NotEqual(first.AccessToken, recovered.AccessToken);
        Assert.NotEqual(first.RefreshToken, recovered.RefreshToken);

        using var oldRefresh = await client.RefreshAsync(first.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, oldRefresh.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(1, await db.GameWorlds.CountAsync());
        Assert.Equal(1, await db.Actors.CountAsync());
        Assert.Equal(1, await db.RefreshTokens.Select(entity => entity.RotationFamilyId).Distinct().CountAsync());
        Assert.NotNull((await db.GuestBootstrapOperations.SingleAsync()).RecoveryConsumedAt);
    }

    [Fact]
    public async Task ExpiredProofAndInstallationSubstitution_CannotRecoverIdentity()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var proof = M03TestClient.NewSecret();
        var installationId = Guid.NewGuid().ToString();
        using var initial = await client.BootstrapAsync(proof, installationId);
        Assert.Equal(HttpStatusCode.Created, initial.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            var operation = await db.GuestBootstrapOperations.AsNoTracking().SingleAsync();
            var expiredAt = operation.CompletedAt!.Value.AddMilliseconds(1);
            await db.GuestBootstrapOperations.ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.ExpiresAt, expiredAt));
        }

        using var expired = await client.BootstrapAsync(proof, installationId);
        using var substituted = await client.BootstrapAsync(M03TestClient.NewSecret(), installationId);
        using var proofAsBearer = await SendCurrentWorldAsync(client, proof);

        Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, substituted.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, proofAsBearer.StatusCode);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(1, await assertionDb.Users.CountAsync());
        Assert.Equal(1, await assertionDb.GameWorlds.CountAsync());
    }

    [Fact]
    public async Task ConcurrentBootstrapAndRecovery_CreateOneAggregateAndRecoverAtMostOnce()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var firstProof = M03TestClient.NewSecret();

        var firstRace = await Task.WhenAll(
            client.BootstrapAsync(firstProof),
            client.BootstrapAsync(firstProof));
        Assert.Equal(1, firstRace.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.True(firstRace.Count(response => response.IsSuccessStatusCode) <= 2);
        foreach (var response in firstRace)
        {
            response.Dispose();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            Assert.Equal(1, await db.Users.CountAsync());
            Assert.Equal(1, await db.GameWorlds.CountAsync());
            Assert.Equal(1, await db.Actors.CountAsync());
            Assert.Equal(1, await db.GuestBootstrapOperations.CountAsync());
        }

        await factory.ResetDatabaseAsync();
        var recoveryProof = M03TestClient.NewSecret();
        using var initial = await client.BootstrapAsync(recoveryProof);
        Assert.Equal(HttpStatusCode.Created, initial.StatusCode);
        var recoveryRace = await Task.WhenAll(
            client.BootstrapAsync(recoveryProof),
            client.BootstrapAsync(recoveryProof));
        Assert.Equal(1, recoveryRace.Count(response => response.StatusCode == HttpStatusCode.OK));
        foreach (var response in recoveryRace)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task OwnershipQueries_DenyAnotherUsersWorldAndSecondCreation()
    {
        await using var factory = await CreateFactoryAsync();
        using var clientA = factory.CreateClient();
        using var clientB = factory.CreateClient();
        using var responseA = await clientA.BootstrapAsync(M03TestClient.NewSecret(), worldName: "World A");
        using var responseB = await clientB.BootstrapAsync(M03TestClient.NewSecret(), worldName: "World B");
        var sessionA = await responseA.Content.ReadFromJsonAsync<M03GuestResponse>();
        var sessionB = await responseB.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(sessionA);
        Assert.NotNull(sessionB);
        clientA.Authenticate(sessionA.AccessToken);
        clientB.Authenticate(sessionB.AccessToken);

        using var own = await clientA.GetAsync("/api/v1/worlds/current");
        using var list = await clientA.GetAsync("/api/v1/worlds");
        using var foreign = await clientB.GetAsync($"/api/v1/worlds/{sessionA.World.Id}");
        using var secondCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/worlds")
        {
            Content = JsonContent.Create(new { name = "Second World" }),
        };
        secondCreateRequest.Headers.Add("Idempotency-Key", "m03-second-world-attempt");
        using var secondCreate = await clientA.SendAsync(secondCreateRequest);

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var listJson = await list.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, listJson.GetProperty("items").ValueKind);
        Assert.Single(listJson.GetProperty("items").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, listJson.GetProperty("nextCursor").ValueKind);
        Assert.False(listJson.GetProperty("hasMore").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondCreate.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(2, await db.GameWorlds.CountAsync());
        Assert.Equal(2, await db.Actors.CountAsync(entity => entity.ActorType == ActorType.Player));
    }

    [Fact]
    public async Task FamilyCap_UsesOriginalLineageAgeAfterRotation()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var bootstrap = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(bootstrap);

        Guid oldestFamilyId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            oldestFamilyId = (await db.RefreshTokens.SingleAsync()).RotationFamilyId;
        }

        var laterFamilies = new List<TokenPair>();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sessions = scope.ServiceProvider.GetRequiredService<ISessionAdministrationService>();
            for (var index = 0; index < 4; index++)
            {
                laterFamilies.Add(await sessions.CreateFamilyAsync(
                    bootstrap.User.Id,
                    Guid.NewGuid().ToString(),
                    "android",
                    "1.0.0-test",
                    CancellationToken.None));
            }
        }

        using var rotatedResponse = await client.RefreshAsync(bootstrap.RefreshToken);
        var rotatedOldestFamily = await rotatedResponse.Content.ReadFromJsonAsync<M03TokenResponse>();
        Assert.Equal(HttpStatusCode.OK, rotatedResponse.StatusCode);
        Assert.NotNull(rotatedOldestFamily);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sessions = scope.ServiceProvider.GetRequiredService<ISessionAdministrationService>();
            _ = await sessions.CreateFamilyAsync(
                bootstrap.User.Id,
                Guid.NewGuid().ToString(),
                "android",
                "1.0.0-test",
                CancellationToken.None);
        }

        using var oldestLineage = await client.RefreshAsync(rotatedOldestFamily.RefreshToken);
        using var nextOldestLineage = await client.RefreshAsync(laterFamilies[0].RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, oldestLineage.StatusCode);
        Assert.Equal(HttpStatusCode.OK, nextOldestLineage.StatusCode);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var now = DateTimeOffset.UtcNow;
        Assert.Equal(5, await assertionDb.RefreshTokens
            .Where(entity => entity.ConsumedAt == null
                && entity.RevokedAt == null
                && entity.ExpiresAt > now)
            .Select(entity => entity.RotationFamilyId)
            .Distinct()
            .CountAsync());
        Assert.False(await assertionDb.RefreshTokens.AnyAsync(entity =>
            entity.RotationFamilyId == oldestFamilyId
            && entity.ConsumedAt == null
            && entity.RevokedAt == null
            && entity.ExpiresAt > now));
    }

    [Fact]
    public async Task Refresh_IsSingleUseAndReplayRevokesOnlyItsFamily()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var bootstrap = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(bootstrap);

        TokenPair otherFamily;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sessions = scope.ServiceProvider.GetRequiredService<ISessionAdministrationService>();
            otherFamily = await sessions.CreateFamilyAsync(
                bootstrap.User.Id,
                Guid.NewGuid().ToString(),
                "android",
                "1.0.0-test",
                CancellationToken.None);
        }

        using var rotatedResponse = await client.RefreshAsync(bootstrap.RefreshToken);
        var rotated = await rotatedResponse.Content.ReadFromJsonAsync<M03TokenResponse>();
        using var replay = await client.RefreshAsync(bootstrap.RefreshToken);
        using var descendantAfterReplay = await client.RefreshAsync(rotated!.RefreshToken);
        using var unrelated = await client.RefreshAsync(otherFamily.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, rotatedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, descendantAfterReplay.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unrelated.StatusCode);
    }

    [Fact]
    public async Task ExpiredRefreshToken_CannotMintCredentials()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var bootstrap = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(bootstrap);

        var expiredAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            await db.RefreshTokens.ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.CreatedAt, expiredAt.AddDays(-30))
                .SetProperty(entity => entity.ExpiresAt, expiredAt));
        }

        using var expired = await client.RefreshAsync(bootstrap.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
        var body = await expired.Content.ReadAsStringAsync();
        Assert.Contains("refresh_token_expired", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DatabaseConstraint_RejectsCrossWorldPlayerProfileReference()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var first = await client.BootstrapAsync(M03TestClient.NewSecret());
        using var second = await client.BootstrapAsync(M03TestClient.NewSecret());
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var worlds = await db.GameWorlds.OrderBy(entity => entity.CreatedAt).ToListAsync();
        var foreignProfile = await db.PlayerProfiles
            .SingleAsync(entity => entity.WorldId == worlds[1].Id);
        var originalPlayer = await db.Actors.SingleAsync(entity => entity.WorldId == worlds[0].Id);
        db.Actors.Remove(originalPlayer);
        await db.SaveChangesAsync();
        db.Actors.Add(new Actor(
            Guid.NewGuid(),
            worlds[0].Id,
            foreignProfile.Id,
            DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ConcurrentRefresh_SucceedsOnlyOnceAndRejectsIdempotencyReplay()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var bootstrap = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(bootstrap);

        using var idempotentRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh")
        {
            Content = JsonContent.Create(new { refreshToken = bootstrap.RefreshToken }),
        };
        idempotentRequest.Headers.Add("Idempotency-Key", "must-not-be-used");
        using var rejected = await client.SendAsync(idempotentRequest);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);

        var race = await Task.WhenAll(
            client.RefreshAsync(bootstrap.RefreshToken),
            client.RefreshAsync(bootstrap.RefreshToken));
        Assert.Equal(1, race.Count(response => response.StatusCode == HttpStatusCode.OK));
        Assert.Equal(1, race.Count(response => !response.IsSuccessStatusCode));
        foreach (var response in race)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task LogoutAllFamilyRevocationAndFiveFamilyCap_AreEnforced()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var bootstrap = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(bootstrap);
        client.Authenticate(bootstrap.AccessToken);

        using var logout = await client.PostAsJsonAsync(
            "/api/v1/auth/logout",
            new { refreshToken = bootstrap.RefreshToken });
        using var loggedOutRefresh = await client.RefreshAsync(bootstrap.RefreshToken);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, loggedOutRefresh.StatusCode);

        var families = new List<TokenPair>();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sessions = scope.ServiceProvider.GetRequiredService<ISessionAdministrationService>();
            for (var index = 0; index < 6; index++)
            {
                families.Add(await sessions.CreateFamilyAsync(
                    bootstrap.User.Id,
                    Guid.NewGuid().ToString(),
                    "android",
                    "1.0.0-test",
                    CancellationToken.None));
            }
        }

        using var oldest = await client.RefreshAsync(families[0].RefreshToken);
        using var newest = await client.RefreshAsync(families[^1].RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, oldest.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newest.StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sessions = scope.ServiceProvider.GetRequiredService<ISessionAdministrationService>();
            await sessions.RevokeAllFamiliesAsync(bootstrap.User.Id, CancellationToken.None);
        }

        var newestBody = await newest.Content.ReadFromJsonAsync<M03TokenResponse>();
        using var afterAllRevocation = await client.RefreshAsync(newestBody!.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, afterAllRevocation.StatusCode);

        await using var assertionScope = factory.Services.CreateAsyncScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(0, await db.RefreshTokens.CountAsync(entity => entity.RevokedAt == null));
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "ConnectionStrings__Default must identify the isolated M03 PostgreSQL test database.");
        return await M03ApiFactory.CreateAsync(connectionString);
    }

    private static async Task<HttpResponseMessage> SendCurrentWorldAsync(
        HttpClient client,
        string bearer)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/worlds/current");
        request.Headers.Authorization = new("Bearer", bearer);
        return await client.SendAsync(request);
    }
}

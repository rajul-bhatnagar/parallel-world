using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class M17AuthenticationTests
{
    [Fact]
    public async Task Registration_IsHashOnly_Normalized_AndCreatesNoSession()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { email = "  Player@Example.com ", password = "correct horse battery" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var code = json.GetProperty("recoveryCode").GetString();
        Assert.False(string.IsNullOrWhiteSpace(code));
        Assert.False(json.TryGetProperty("accessToken", out _));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var user = await db.Users.SingleAsync();
        Assert.Equal("Player@Example.com", user.Email);
        Assert.Equal("PLAYER@EXAMPLE.COM", user.NormalizedEmail);
        Assert.NotEqual("correct horse battery", user.PasswordHash);
        Assert.Equal(0, await db.RefreshTokens.CountAsync());
        Assert.NotEqual(code, (await db.AccountRecoveryCodes.SingleAsync()).CodeHash);
    }

    [Fact]
    public async Task GuestUpgrade_PreservesIdentityWorldAndCurrentFamily_AndReplaysWithoutCode()
    {
        await using var factory = await CreateFactoryAsync(); using var client = factory.CreateClient();
        using var bootResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var boot = await bootResponse.Content.ReadFromJsonAsync<M03GuestResponse>(); Assert.NotNull(boot);
        client.Authenticate(boot.AccessToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/upgrade") { Content = JsonContent.Create(new { email="guest@example.com", password="guest-password-1" }) };
        request.Headers.Add("Idempotency-Key", "upgrade-1");
        using var upgraded = await client.SendAsync(request); Assert.Equal(HttpStatusCode.OK, upgraded.StatusCode);
        var first = await upgraded.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(boot.User.Id, first.GetProperty("user").GetProperty("id").GetGuid());
        Assert.Equal(boot.World.Id, first.GetProperty("world").GetProperty("id").GetGuid());
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("recoveryCode").GetString()));

        using var replayRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/upgrade") { Content = JsonContent.Create(new { email="guest@example.com", password="guest-password-1" }) };
        replayRequest.Headers.Add("Idempotency-Key", "upgrade-1");
        using var replay = await client.SendAsync(replayRequest); Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        var replayJson=await replay.Content.ReadFromJsonAsync<JsonElement>(); Assert.Equal(JsonValueKind.Null,replayJson.GetProperty("recoveryCode").ValueKind);
        using var changedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/upgrade") { Content = JsonContent.Create(new { email="guest@example.com", password="different-password-1" }) };
        changedRequest.Headers.Add("Idempotency-Key", "upgrade-1");
        using var changed = await client.SendAsync(changedRequest); Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
        using var refresh=await client.RefreshAsync(boot.RefreshToken); Assert.Equal(HttpStatusCode.OK,refresh.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var record = await db.IdempotencyRecords.SingleAsync(item => item.Operation == "auth.upgrade");
        var expected = Convert.ToHexStringLower(SHA256.HashData(
            Encoding.UTF8.GetBytes($"auth.upgrade\0{boot.User.Id:N}\0GUEST@EXAMPLE.COM")));
        var passwordDerived = Convert.ToHexStringLower(SHA256.HashData(
            Encoding.UTF8.GetBytes("GUEST@EXAMPLE.COM\0guest-password-1")));
        Assert.Equal(expected, record.RequestHash);
        Assert.NotEqual(passwordDerived, record.RequestHash);
        Assert.Equal(1, await db.DeviceInstallations.CountAsync(item => item.UserId == boot.User.Id));
    }

    [Fact]
    public async Task Recovery_IsGenericSingleUse_RotatesCode_AndRevokesEveryFamily()
    {
        await using var factory=await CreateFactoryAsync(); using var client=factory.CreateClient();
        using var register=await client.PostAsJsonAsync("/api/v1/auth/register",new{email="recover@example.com",password="old-password-1"});
        var registered=await register.Content.ReadFromJsonAsync<JsonElement>(); var code=registered.GetProperty("recoveryCode").GetString()!;
        using var login=await client.PostAsJsonAsync("/api/v1/auth/login",new{email="RECOVER@example.com",password="old-password-1",installationId=Guid.NewGuid(),platform="android",appVersion="1.0.0"});
        var session=await login.Content.ReadFromJsonAsync<M03GuestResponse>(); Assert.NotNull(session);
        using var recovery=await client.PostAsJsonAsync("/api/v1/auth/recovery",new{email="recover@example.com",recoveryCode=code,newPassword="new-password-1"});
        Assert.Equal(HttpStatusCode.OK,recovery.StatusCode); var next=(await recovery.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("recoveryCode").GetString(); Assert.NotEqual(code,next);
        using var revoked=await client.RefreshAsync(session.RefreshToken); Assert.Equal(HttpStatusCode.Unauthorized,revoked.StatusCode);
        using var replay=await client.PostAsJsonAsync("/api/v1/auth/recovery",new{email="recover@example.com",recoveryCode=code,newPassword="another-pass-1"});
        using var unknown=await client.PostAsJsonAsync("/api/v1/auth/recovery",new{email="unknown@example.com",recoveryCode=code,newPassword="another-pass-1"});
        Assert.Equal(HttpStatusCode.BadRequest,replay.StatusCode); Assert.Equal(HttpStatusCode.BadRequest,unknown.StatusCode);
        Assert.Equal(await ReadFailureCodeAsync(replay), await ReadFailureCodeAsync(unknown));
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
            Assert.Equal(1, await db.Users.CountAsync());
            Assert.Equal(2, await db.AccountRecoveryCodes.CountAsync());
            Assert.Equal(1, await db.RefreshTokens.CountAsync());
        }
        using var oldLogin = await LoginAsync(client, "recover@example.com", "old-password-1", Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        using var newLogin = await LoginAsync(client, "recover@example.com", "new-password-1", Guid.NewGuid());
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task SessionAdministration_IsOwnerScoped_AndPasswordChangeRevokesCurrentFamily()
    {
        await using var factory=await CreateFactoryAsync(); using var owner=factory.CreateClient(); using var other=factory.CreateClient();
        _=await owner.PostAsJsonAsync("/api/v1/auth/register",new{email="owner@example.com",password="owner-password-1"});
        _=await other.PostAsJsonAsync("/api/v1/auth/register",new{email="other@example.com",password="other-password-1"});
        using var ownerLogin=await owner.PostAsJsonAsync("/api/v1/auth/login",new{email="owner@example.com",password="owner-password-1",installationId=Guid.NewGuid(),platform="android",appVersion="1"});
        using var otherLogin=await other.PostAsJsonAsync("/api/v1/auth/login",new{email="other@example.com",password="other-password-1",installationId=Guid.NewGuid(),platform="android",appVersion="1"});
        var ownerSession=await ownerLogin.Content.ReadFromJsonAsync<M03GuestResponse>(); var otherSession=await otherLogin.Content.ReadFromJsonAsync<M03GuestResponse>(); Assert.NotNull(ownerSession); Assert.NotNull(otherSession);
        owner.Authenticate(ownerSession.AccessToken); other.Authenticate(otherSession.AccessToken);
        var otherSessions=await (await other.GetAsync("/api/v1/auth/sessions")).Content.ReadFromJsonAsync<JsonElement>(); var foreignId=otherSessions.GetProperty("items")[0].GetProperty("id").GetGuid();
        using var denied=await owner.DeleteAsync($"/api/v1/auth/sessions/{foreignId}"); Assert.Equal(HttpStatusCode.NotFound,denied.StatusCode);
        var ownerSessions=await (await owner.GetAsync("/api/v1/auth/sessions")).Content.ReadFromJsonAsync<JsonElement>(); Assert.True(ownerSessions.GetProperty("items")[0].GetProperty("isCurrent").GetBoolean());
        using var changed=await owner.PostAsJsonAsync("/api/v1/auth/password/change",new{currentPassword="owner-password-1",newPassword="owner-password-2"}); Assert.Equal(HttpStatusCode.NoContent,changed.StatusCode);
        using var revoked=await owner.RefreshAsync(ownerSession.RefreshToken); Assert.Equal(HttpStatusCode.Unauthorized,revoked.StatusCode);
        using var oldLogin = await LoginAsync(owner, "owner@example.com", "owner-password-1", Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        using var newLogin = await LoginAsync(owner, "owner@example.com", "owner-password-2", Guid.NewGuid());
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownAndWrongPassword_ReturnSamePublicFailure()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        _ = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "known@example.com", password = "known-password-1" });

        using var wrong = await LoginAsync(client, "known@example.com", "wrong-password-1", Guid.NewGuid());
        using var unknown = await LoginAsync(client, "unknown@example.com", "wrong-password-1", Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(await ReadFailureCodeAsync(wrong), await ReadFailureCodeAsync(unknown));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        Assert.Equal(1, await db.Users.CountAsync());
        Assert.Equal(0, await db.RefreshTokens.CountAsync());
    }

    [Fact]
    public async Task PasswordChange_RacingOldPasswordLogin_CannotLeaveAnActiveOldPasswordFamily()
    {
        await using var factory = await CreateFactoryAsync();
        using var accountClient = factory.CreateClient();
        using var loginClient = factory.CreateClient();
        _ = await accountClient.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "change-race@example.com", password = "old-password-1" });
        using var initialLogin = await LoginAsync(
            accountClient,
            "change-race@example.com",
            "old-password-1",
            Guid.NewGuid());
        var initialSession = await initialLogin.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(initialSession);
        accountClient.Authenticate(initialSession.AccessToken);

        var changeTask = accountClient.PostAsJsonAsync(
            "/api/v1/auth/password/change",
            new { currentPassword = "old-password-1", newPassword = "new-password-1" });
        var oldLoginTask = LoginAsync(
            loginClient,
            "change-race@example.com",
            "old-password-1",
            Guid.NewGuid());
        await Task.WhenAll(changeTask, oldLoginTask);
        using var change = await changeTask;
        using var oldLogin = await oldLoginTask;
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);
        Assert.Contains(oldLogin.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Unauthorized });
        if (oldLogin.StatusCode == HttpStatusCode.OK)
        {
            var racedSession = await oldLogin.Content.ReadFromJsonAsync<M03GuestResponse>();
            Assert.NotNull(racedSession);
            using var racedRefresh = await loginClient.RefreshAsync(racedSession.RefreshToken);
            Assert.Equal(HttpStatusCode.Unauthorized, racedRefresh.StatusCode);
        }

        using var oldAfterCommit = await LoginAsync(
            loginClient,
            "change-race@example.com",
            "old-password-1",
            Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Unauthorized, oldAfterCommit.StatusCode);
        using var newAfterCommit = await LoginAsync(
            loginClient,
            "change-race@example.com",
            "new-password-1",
            Guid.NewGuid());
        Assert.Equal(HttpStatusCode.OK, newAfterCommit.StatusCode);
    }

    [Fact]
    public async Task Recovery_RacingOldPasswordLogin_CannotLeaveAnActiveOldPasswordFamily()
    {
        await using var factory = await CreateFactoryAsync();
        using var recoveryClient = factory.CreateClient();
        using var loginClient = factory.CreateClient();
        using var registration = await recoveryClient.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "recovery-race@example.com", password = "old-password-1" });
        var registrationJson = await registration.Content.ReadFromJsonAsync<JsonElement>();
        var recoveryCode = registrationJson.GetProperty("recoveryCode").GetString()!;

        var recoveryTask = recoveryClient.PostAsJsonAsync(
            "/api/v1/auth/recovery",
            new
            {
                email = "recovery-race@example.com",
                recoveryCode,
                newPassword = "new-password-1",
            });
        var oldLoginTask = LoginAsync(
            loginClient,
            "recovery-race@example.com",
            "old-password-1",
            Guid.NewGuid());
        await Task.WhenAll(recoveryTask, oldLoginTask);
        using var recovery = await recoveryTask;
        using var oldLogin = await oldLoginTask;
        Assert.Equal(HttpStatusCode.OK, recovery.StatusCode);
        Assert.Contains(oldLogin.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Unauthorized });
        if (oldLogin.StatusCode == HttpStatusCode.OK)
        {
            var racedSession = await oldLogin.Content.ReadFromJsonAsync<M03GuestResponse>();
            Assert.NotNull(racedSession);
            using var racedRefresh = await loginClient.RefreshAsync(racedSession.RefreshToken);
            Assert.Equal(HttpStatusCode.Unauthorized, racedRefresh.StatusCode);
        }

        using var oldAfterCommit = await LoginAsync(
            loginClient,
            "recovery-race@example.com",
            "old-password-1",
            Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Unauthorized, oldAfterCommit.StatusCode);
        using var newAfterCommit = await LoginAsync(
            loginClient,
            "recovery-race@example.com",
            "new-password-1",
            Guid.NewGuid());
        Assert.Equal(HttpStatusCode.OK, newAfterCommit.StatusCode);
    }

    [Fact]
    public async Task RepeatedLogin_ReusesInstallationAndCreatesDistinctFamilies()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        _ = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "repeat@example.com", password = "repeat-password-1" });
        var installationId = Guid.NewGuid();

        using var first = await LoginAsync(client, "repeat@example.com", "repeat-password-1", installationId);
        using var second = await LoginAsync(client, "repeat@example.com", "repeat-password-1", installationId);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var user = await db.Users.SingleAsync(item => item.NormalizedEmail == "REPEAT@EXAMPLE.COM");
        Assert.Equal(
            1,
            await db.DeviceInstallations.CountAsync(
                item => item.UserId == user.Id && item.InstallationPublicId == installationId.ToString()));
        Assert.Equal(
            2,
            await db.RefreshTokens
                .Where(item => item.UserId == user.Id)
                .Select(item => item.RotationFamilyId)
                .Distinct()
                .CountAsync());
    }

    [Fact]
    public async Task SameInstallationPublicId_CanBelongToDifferentUsers()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        _ = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "first@example.com", password = "first-password-1" });
        _ = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "second@example.com", password = "second-password-1" });
        var installationId = Guid.NewGuid();

        using var first = await LoginAsync(client, "first@example.com", "first-password-1", installationId);
        using var second = await LoginAsync(client, "second@example.com", "second-password-1", installationId);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var installations = await db.DeviceInstallations
            .Where(item => item.InstallationPublicId == installationId.ToString())
            .ToListAsync();
        Assert.Equal(2, installations.Count);
        Assert.Equal(2, installations.Select(item => item.UserId).Distinct().Count());
    }

    [Fact]
    public async Task ConcurrentFirstLogin_ReusesOneInstallationWithoutLosingEitherFamily()
    {
        await using var factory = await CreateFactoryAsync();
        using var registrationClient = factory.CreateClient();
        _ = await registrationClient.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = "concurrent@example.com", password = "concurrent-password-1" });
        var installationId = Guid.NewGuid();
        using var firstClient = factory.CreateClient();
        using var secondClient = factory.CreateClient();

        var responses = await Task.WhenAll(
            LoginAsync(firstClient, "concurrent@example.com", "concurrent-password-1", installationId),
            LoginAsync(secondClient, "concurrent@example.com", "concurrent-password-1", installationId));
        using var first = responses[0];
        using var second = responses[1];
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();
        var user = await db.Users.SingleAsync(item => item.NormalizedEmail == "CONCURRENT@EXAMPLE.COM");
        Assert.Equal(
            1,
            await db.DeviceInstallations.CountAsync(
                item => item.UserId == user.Id && item.InstallationPublicId == installationId.ToString()));
        Assert.Equal(
            2,
            await db.RefreshTokens
                .Where(item => item.UserId == user.Id)
                .Select(item => item.RotationFamilyId)
                .Distinct()
                .CountAsync());
    }

    private static Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password,
        Guid installationId) =>
        client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new
            {
                email,
                password,
                installationId,
                platform = "android",
                appVersion = "1.0.0",
            });

    private static async Task<string?> ReadFailureCodeAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var cs=Environment.GetEnvironmentVariable("ConnectionStrings__Default"); Assert.False(string.IsNullOrWhiteSpace(cs)); return await M03ApiFactory.CreateAsync(cs);
    }
}

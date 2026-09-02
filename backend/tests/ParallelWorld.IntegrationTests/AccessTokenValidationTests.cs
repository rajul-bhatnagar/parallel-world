using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class AccessTokenValidationTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 8, 31, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CurrentAndPreviousRs256Keys_AreAccepted()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var session = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(session);

        using var current = await GetCurrentWorldAsync(client, session.AccessToken);
        using var previous = await GetCurrentWorldAsync(client, CreateRsaToken(
            session.User.Id,
            TestAuthenticationConfiguration.PreviousPrivateKeyPem,
            TestAuthenticationConfiguration.PreviousKeyId));

        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal(HttpStatusCode.OK, previous.StatusCode);
    }

    [Fact]
    public async Task MissingSubjectTokenIdOrKeyId_IsRejected()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var userId = Guid.NewGuid();
        var tokens = new[]
        {
            CreateRsaToken(userId, includeSubject: false),
            CreateRsaToken(userId, includeJti: false),
            CreateRsaToken(userId, includeKeyId: false),
        };

        await AssertRejectedAsync(client, tokens);
    }

    [Fact]
    public async Task WrongAlgorithmSignatureIssuerOrAudience_IsRejected()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        var userId = Guid.NewGuid();
        using var invalidRsa = RSA.Create(2048);
        var tokens = new[]
        {
            CreateHmacToken(userId),
            CreateRsaToken(userId, invalidRsa.ExportPkcs8PrivateKeyPem()),
            CreateRsaToken(userId, issuer: "wrong-issuer"),
            CreateRsaToken(userId, audience: "wrong-audience"),
        };

        await AssertRejectedAsync(client, tokens);
    }

    [Fact]
    public async Task LifetimeValidation_UsesExactThirtySecondExpiryAndNotBeforeSkew()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var session = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(session);

        var expiryBoundary = CreateRsaToken(session.User.Id, expires: FixedNow.AddSeconds(-30));
        var expiryOutside = CreateRsaToken(session.User.Id, expires: FixedNow.AddSeconds(-31));
        var notBeforeBoundary = CreateRsaToken(session.User.Id, notBefore: FixedNow.AddSeconds(30));
        var notBeforeOutside = CreateRsaToken(session.User.Id, notBefore: FixedNow.AddSeconds(31));

        using var acceptedExpiry = await GetCurrentWorldAsync(client, expiryBoundary);
        using var rejectedExpiry = await GetCurrentWorldAsync(client, expiryOutside);
        using var acceptedNotBefore = await GetCurrentWorldAsync(client, notBeforeBoundary);
        using var rejectedNotBefore = await GetCurrentWorldAsync(client, notBeforeOutside);

        Assert.Equal(HttpStatusCode.OK, acceptedExpiry.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, rejectedExpiry.StatusCode);
        Assert.Equal(HttpStatusCode.OK, acceptedNotBefore.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, rejectedNotBefore.StatusCode);
    }

    private static string CreateRsaToken(
        Guid userId,
        string? privateKeyPem = null,
        string? keyId = null,
        string issuer = "parallel-world-api",
        string audience = "parallel-world-mobile",
        DateTimeOffset? expires = null,
        DateTimeOffset? notBefore = null,
        bool includeSubject = true,
        bool includeJti = true,
        bool includeKeyId = true)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem ?? TestAuthenticationConfiguration.CurrentPrivateKeyPem);
        var securityKey = new RsaSecurityKey(rsa)
        {
            KeyId = includeKeyId
                ? keyId ?? TestAuthenticationConfiguration.CurrentKeyId
                : null,
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
        };
        return CreateToken(
            userId,
            new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256),
            issuer,
            audience,
            expires,
            notBefore,
            includeSubject,
            includeJti);
    }

    private static string CreateHmacToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32))
        {
            KeyId = TestAuthenticationConfiguration.CurrentKeyId,
        };
        return CreateToken(
            userId,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            "parallel-world-api",
            "parallel-world-mobile",
            null,
            null,
            true,
            true);
    }

    private static string CreateToken(
        Guid userId,
        SigningCredentials signingCredentials,
        string issuer,
        string audience,
        DateTimeOffset? expires,
        DateTimeOffset? notBefore,
        bool includeSubject,
        bool includeJti)
    {
        var claims = new List<Claim>();
        if (includeSubject)
        {
            claims.Add(new("sub", userId.ToString()));
        }

        if (includeJti)
        {
            claims.Add(new("jti", Guid.NewGuid().ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Audience = audience,
            Expires = (expires ?? FixedNow.AddMinutes(15)).UtcDateTime,
            IssuedAt = FixedNow.AddMinutes(-1).UtcDateTime,
            Issuer = issuer,
            NotBefore = (notBefore ?? FixedNow.AddMinutes(-1)).UtcDateTime,
            SigningCredentials = signingCredentials,
            Subject = new ClaimsIdentity(claims),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static async Task AssertRejectedAsync(HttpClient client, IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            using var response = await GetCurrentWorldAsync(client, token);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "ConnectionStrings__Default must identify the PostgreSQL administrative base connection.");
        return await M03ApiFactory.CreateAsync(
            connectionString,
            new MutableTimeProvider(FixedNow));
    }

    private static async Task<HttpResponseMessage> GetCurrentWorldAsync(HttpClient client, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/worlds/current");
        request.Headers.Authorization = new("Bearer", token);
        return await client.SendAsync(request);
    }
}

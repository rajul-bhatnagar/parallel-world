using System.Net;
using System.Net.Http.Json;
using ParallelWorld.Infrastructure.Authentication;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class AuthenticationRateLimitTests
{
    [Theory]
    [InlineData("guest")]
    [InlineData("invalid-refresh")]
    [InlineData("logout")]
    [InlineData("refresh-family")]
    public void ExpiredPartitions_AreRemovedDuringPeriodicCleanup(string policy)
    {
        var clock = new MutableTimeProvider(new DateTimeOffset(2026, 8, 31, 12, 0, 0, TimeSpan.Zero));
        var limiter = new AuthenticationRateLimiter(clock);

        for (var index = 0; index < 63; index++)
        {
            Assert.True(limiter.Acquire(policy, $"partition-{index}", 10).IsAllowed);
        }

        Assert.Equal(63, limiter.PartitionCount);
        clock.Advance(TimeSpan.FromMinutes(11));
        Assert.True(limiter.Acquire(policy, "new-partition", 10).IsAllowed);
        Assert.Equal(1, limiter.PartitionCount);
    }

    [Fact]
    public async Task GuestLimit_ReturnsStandardProblemDetailsOnEleventhRequest()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        for (var index = 0; index < 10; index++)
        {
            using var accepted = await client.BootstrapAsync(M03TestClient.NewSecret());
            Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        }

        using var limited = await client.BootstrapAsync(M03TestClient.NewSecret());
        await AssertRateLimitAsync(limited);
    }

    [Fact]
    public async Task InvalidRefreshLimit_ReturnsStandardProblemDetailsOnEleventhRequest()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        for (var index = 0; index < 10; index++)
        {
            using var rejected = await client.RefreshAsync(M03TestClient.NewSecret());
            Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
        }

        using var limited = await client.RefreshAsync(M03TestClient.NewSecret());
        await AssertRateLimitAsync(limited);
    }

    [Fact]
    public async Task RefreshFamilyLimit_ReturnsStandardProblemDetailsOnThirtyFirstRotation()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var session = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(session);
        var refreshToken = session.RefreshToken;

        for (var index = 0; index < 30; index++)
        {
            using var rotated = await client.RefreshAsync(refreshToken);
            Assert.Equal(HttpStatusCode.OK, rotated.StatusCode);
            var tokens = await rotated.Content.ReadFromJsonAsync<M03TokenResponse>();
            refreshToken = Assert.IsType<M03TokenResponse>(tokens).RefreshToken;
        }

        using var limited = await client.RefreshAsync(refreshToken);
        await AssertRateLimitAsync(limited);
    }

    [Fact]
    public async Task LogoutLimit_ReturnsStandardProblemDetailsOnThirtyFirstRequest()
    {
        await using var factory = await CreateFactoryAsync();
        using var client = factory.CreateClient();
        using var bootstrapResponse = await client.BootstrapAsync(M03TestClient.NewSecret());
        var session = await bootstrapResponse.Content.ReadFromJsonAsync<M03GuestResponse>();
        Assert.NotNull(session);
        client.Authenticate(session.AccessToken);

        for (var index = 0; index < 30; index++)
        {
            using var accepted = await client.PostAsJsonAsync(
                "/api/v1/auth/logout",
                new { refreshToken = session.RefreshToken });
            Assert.Equal(HttpStatusCode.NoContent, accepted.StatusCode);
        }

        using var limited = await client.PostAsJsonAsync(
            "/api/v1/auth/logout",
            new { refreshToken = session.RefreshToken });
        await AssertRateLimitAsync(limited);
    }

    private static async Task AssertRateLimitAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.NotNull(response.Headers.RetryAfter);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("rate_limit_exceeded", body, StringComparison.Ordinal);
        Assert.Contains("retryAfterSeconds", body, StringComparison.Ordinal);
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "ConnectionStrings__Default must identify the isolated M03 PostgreSQL test database.");
        return await M03ApiFactory.CreateAsync(connectionString);
    }
}

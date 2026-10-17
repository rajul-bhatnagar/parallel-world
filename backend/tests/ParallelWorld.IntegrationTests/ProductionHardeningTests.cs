using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ParallelWorld.Api.Configuration;
using ParallelWorld.Infrastructure.Authentication;

namespace ParallelWorld.IntegrationTests;

public sealed class ProductionHardeningTests
{
    [Fact]
    public async Task Production_RejectsUnknownHostAndOversizedRequest()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        using var unknownHost = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        unknownHost.Headers.Host = "untrusted.example.test";
        using var unknownHostResponse = await client.SendAsync(unknownHost);
        Assert.Equal(HttpStatusCode.BadRequest, unknownHostResponse.StatusCode);

        using var oversized = new StringContent(new string('x', 4_097));
        using var oversizedResponse = await client.PostAsync("/api/v1/auth/login", oversized);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversizedResponse.StatusCode);
    }

    [Fact]
    public async Task Production_UsesSecurityHeadersAndExplicitCorsOrigin()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("Origin", "https://mobile.example.test");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("https://mobile.example.test", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task ForwardedClientAddresses_PartitionAuthenticationRateLimitsByClientIp()
    {
        var firstAddress = await ResolveForwardedAddressAsync("198.51.100.10");
        var secondAddress = await ResolveForwardedAddressAsync("198.51.100.11");
        var limiter = new AuthenticationRateLimiter(TimeProvider.System);

        Assert.True(limiter.Acquire("register", firstAddress, 2).IsAllowed);
        Assert.True(limiter.Acquire("register", firstAddress, 2).IsAllowed);
        Assert.False(limiter.Acquire("register", firstAddress, 2).IsAllowed);
        Assert.True(limiter.Acquire("register", secondAddress, 2).IsAllowed);
    }

    [Fact]
    public async Task Forwarding_TrustsOnlyTheNearestProxyHop()
    {
        var address = await ResolveForwardedAddressAsync("203.0.113.99, 198.51.100.10");

        Assert.Equal("198.51.100.10", address);
    }

    [Fact]
    public async Task Forwarding_DoesNotAlterAuthenticationOrOwnershipState()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], "test"));
        var worldId = Guid.NewGuid();
        var options = new ForwardedHeadersOptions();
        ProductionForwarding.Configure(options);
        RequestDelegate next = context =>
        {
            Assert.Same(principal, context.User);
            Assert.Equal(worldId, context.Items["WorldId"]);
            return Task.CompletedTask;
        };
        var middleware = new ForwardedHeadersMiddleware(
            next,
            NullLoggerFactory.Instance,
            Options.Create(options));
        var context = new DefaultHttpContext
        {
            User = principal,
        };
        context.Items["WorldId"] = worldId;
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.10");
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.10";
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        await middleware.Invoke(context);

        Assert.Same(principal, context.User);
        Assert.Equal(worldId, context.Items["WorldId"]);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                var values = TestAuthenticationConfiguration.CreateProduction(
                    ApiFactory.UnavailableDatabaseConnectionString);
                values["Deployment:MaxRequestBodyBytes"] = "4096";
                configuration.AddInMemoryCollection(values);
            });
        });

    private static async Task<string> ResolveForwardedAddressAsync(string forwardedFor)
    {
        var options = new ForwardedHeadersOptions();
        ProductionForwarding.Configure(options);
        string? resolvedAddress = null;
        RequestDelegate next = context =>
        {
            resolvedAddress = context.Connection.RemoteIpAddress?.ToString();
            return Task.CompletedTask;
        };
        var middleware = new ForwardedHeadersMiddleware(
            next,
            NullLoggerFactory.Instance,
            Options.Create(options));
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.10");
        context.Request.Headers["X-Forwarded-For"] = forwardedFor;
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        await middleware.Invoke(context);

        Assert.Equal("https", context.Request.Scheme);
        return Assert.IsType<string>(resolvedAddress);
    }
}

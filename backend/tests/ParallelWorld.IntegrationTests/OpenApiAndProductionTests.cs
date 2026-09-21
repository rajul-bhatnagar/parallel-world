using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ParallelWorld.IntegrationTests;

public sealed class OpenApiAndProductionTests
{
    [Fact]
    public async Task OpenApi_InDevelopment_IsAvailable()
    {
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        var document = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("/api/v1/dev/worlds/{worldId}/simulate", document, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/openapi/v1.json")]
    [InlineData("/_testing/error")]
    public async Task DevelopmentAndTestingRoutes_InProduction_AreAbsent(string path)
    {
        await using var factory = CreateFactory("Production");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DevelopmentSimulationRoute_InProduction_IsAbsent()
    {
        await using var factory = CreateFactory("Production");
        using var client = factory.CreateClient();

        using var response = await client.PostAsync(
            "/api/v1/dev/worlds/11111111-1111-1111-1111-111111111111/simulate",
            null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DevelopmentSimulationRoute_RequiresAuthentication()
    {
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.PostAsync(
            "/api/v1/dev/worlds/11111111-1111-1111-1111-111111111111/simulate",
            null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(TestAuthenticationConfiguration.Create(
                    ApiFactory.UnavailableDatabaseConnectionString)));
        });
}

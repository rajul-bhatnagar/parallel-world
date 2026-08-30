using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ParallelWorld.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string UnavailableDatabaseConnectionString =
        "Host=127.0.0.1;Port=1;Database=parallel_world_tests;Username=parallel_world;Timeout=1";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = UnavailableDatabaseConnectionString,
            }));
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ParallelWorld.IntegrationTests;

public sealed class ConfigurationTests
{
    [Fact]
    public void Startup_WhenDatabaseConfigurationIsMissing_FailsWithoutPrintingAValue()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(
                        TestAuthenticationConfiguration.Create(string.Empty)));
            });

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        var validationException = FindException<OptionsValidationException>(exception);

        Assert.NotNull(validationException);
        Assert.Contains("ConnectionStrings:Default", validationException.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Password=", validationException.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Startup_WhenSigningKeyConfigurationIsMissing_FailsWithoutPrintingAValue()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    var values = TestAuthenticationConfiguration.Create(
                        ApiFactory.UnavailableDatabaseConnectionString);
                    values["Authentication:CurrentPrivateKeyPem"] = string.Empty;
                    configuration.AddInMemoryCollection(values);
                });
            });

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        var validationException = FindException<OptionsValidationException>(exception);

        Assert.NotNull(validationException);
        Assert.Contains("Authentication configuration", validationException.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE KEY", validationException.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static TException? FindException<TException>(Exception exception)
        where TException : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TException match)
            {
                return match;
            }
        }

        return null;
    }
}

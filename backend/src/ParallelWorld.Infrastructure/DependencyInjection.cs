using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Application.Worlds;
using ParallelWorld.Infrastructure.Authentication;
using ParallelWorld.Infrastructure.Configuration;
using ParallelWorld.Infrastructure.Persistence;
using ParallelWorld.Infrastructure.Worlds;

namespace ParallelWorld.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Configure(options =>
                options.ConnectionString = configuration.GetConnectionString(
                    DatabaseOptions.ConnectionStringName) ?? string.Empty)
            .Validate(
                DatabaseOptions.IsValid,
                $"ConnectionStrings:{DatabaseOptions.ConnectionStringName} must be a valid PostgreSQL "
                    + "connection string containing Host, Database, and Username.")
            .ValidateOnStart();

        services
            .AddOptions<AuthenticationOptions>()
            .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
            .Validate(
                AuthenticationOptions.IsValid,
                "Authentication configuration must provide the accepted issuer, audience, lifetimes, "
                    + "clock skew, current key ID, and protected RSA PEM key material.")
            .ValidateOnStart();

        services.AddDbContext<ParallelWorldDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            options.UseNpgsql(databaseOptions.ConnectionString);
        });

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ParallelWorldDbContext>());
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<JwtKeyMaterial>();
        services.AddSingleton<IOpaqueSecretService, OpaqueSecretService>();
        services.AddSingleton<IAccessTokenIssuer, AccessTokenIssuer>();
        services.AddSingleton<IAuthenticationRateLimiter, AuthenticationRateLimiter>();
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
            return new AuthenticationPolicy(
                options.RefreshTokenLifetimeDays,
                options.BootstrapRecoveryMinutes);
        });
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IWorldRepository, WorldRepository>();
        services.AddSingleton<IPersistenceFailureClassifier, PersistenceFailureClassifier>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<IAuthenticationService>(serviceProvider =>
            serviceProvider.GetRequiredService<AuthenticationService>());
        services.AddScoped<ISessionAdministrationService>(serviceProvider =>
            serviceProvider.GetRequiredService<AuthenticationService>());
        services.AddScoped<IWorldService, WorldService>();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfiguration>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddAuthorization();

        return services;
    }
}

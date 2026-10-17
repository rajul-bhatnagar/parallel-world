using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.AI;
using ParallelWorld.Application.Authentication;
using ParallelWorld.Application.Characters;
using ParallelWorld.Application.Memory;
using ParallelWorld.Application.Messaging;
using ParallelWorld.Application.Notifications;
using ParallelWorld.Application.Relationships;
using ParallelWorld.Application.Simulation;
using ParallelWorld.Application.Social;
using ParallelWorld.Application.Worlds;
using ParallelWorld.Infrastructure.AI;
using ParallelWorld.Infrastructure.Authentication;
using ParallelWorld.Infrastructure.Characters;
using ParallelWorld.Infrastructure.Configuration;
using ParallelWorld.Infrastructure.Memory;
using ParallelWorld.Infrastructure.Messaging;
using ParallelWorld.Infrastructure.Notifications;
using ParallelWorld.Infrastructure.Persistence;
using ParallelWorld.Infrastructure.Relationships;
using ParallelWorld.Infrastructure.Simulation;
using ParallelWorld.Infrastructure.Social;
using ParallelWorld.Infrastructure.Worlds;

namespace ParallelWorld.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var hardenedEnvironment = environment.IsStaging() || environment.IsProduction();
        services
            .AddOptions<DatabaseOptions>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString(
                    DatabaseOptions.ConnectionStringName) ?? string.Empty;
                options.CommandTimeoutSeconds = configuration.GetValue("Database:CommandTimeoutSeconds", 30);
            })
            .Validate(
                options => DatabaseOptions.IsValid(options, hardenedEnvironment),
                $"ConnectionStrings:{DatabaseOptions.ConnectionStringName} must be a valid PostgreSQL "
                    + "connection string containing Host, Database, Username, bounded pooling, and TLS where required.")
            .ValidateOnStart();

        services
            .AddOptions<AuthenticationOptions>()
            .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
            .PostConfigure(options =>
            {
                if (Path.IsPathFullyQualified(options.CurrentPrivateKeyFile)
                    && File.Exists(options.CurrentPrivateKeyFile))
                {
                    try
                    {
                        options.CurrentPrivateKeyPem = File.ReadAllText(options.CurrentPrivateKeyFile);
                    }
                    catch (IOException)
                    {
                        options.CurrentPrivateKeyPem = string.Empty;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        options.CurrentPrivateKeyPem = string.Empty;
                    }
                }
                else if (hardenedEnvironment)
                {
                    options.CurrentPrivateKeyPem = string.Empty;
                }
            })
            .Validate(
                options => AuthenticationOptions.IsValid(options, hardenedEnvironment),
                "Authentication configuration must provide the accepted issuer, audience, lifetimes, "
                    + "clock skew, current key ID, and protected RSA key material from a secret file where required.")
            .ValidateOnStart();

        services.AddDbContext<ParallelWorldDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            options.UseNpgsql(
                databaseOptions.ConnectionString,
                npgsql => npgsql.CommandTimeout(databaseOptions.CommandTimeoutSeconds));
        });
        services.AddDataProtection();

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ParallelWorldDbContext>());
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<JwtKeyMaterial>();
        services.AddSingleton<IOpaqueSecretService, OpaqueSecretService>();
        services.AddSingleton<IPasswordService, PasswordService>();
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
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<ICharacterCatalogueService, CharacterCatalogueService>();
        services.AddSingleton<IFeedCursorCodec, FeedCursorCodec>();
        services.AddSingleton<IReplyCursorCodec, ReplyCursorCodec>();
        services.AddScoped<ISocialRepository, SocialRepository>();
        services.AddScoped<ISocialFeedService, SocialFeedService>();
        services.AddScoped<ISimulationRepository, SimulationRepository>();
        services.AddScoped<IRelationshipRepository, RelationshipRepository>();
        services.AddScoped<IRelationshipService, RelationshipService>();
        services.AddScoped<IRomanceRepository, RomanceRepository>();
        services.AddScoped<IRomanceService, RomanceService>();
        services.AddSingleton<IConversationCursorCodec, ConversationCursorCodec>();
        services.AddSingleton<IMessageCursorCodec, MessageCursorCodec>();
        services.AddScoped<IMessagingRepository, MessagingRepository>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();
        services.AddScoped<IMemoryService, MemoryService>();
        services.AddSingleton<INotificationCursorCodec, NotificationCursorCodec>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationProjectionWriter, NotificationProjectionWriter>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAiGenerationRepository, AiGenerationRepository>();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfiguration>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddAuthorization();

        return services;
    }
}

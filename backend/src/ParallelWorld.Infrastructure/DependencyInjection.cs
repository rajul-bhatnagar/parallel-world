using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Infrastructure.Configuration;
using ParallelWorld.Infrastructure.Persistence;

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

        services.AddDbContext<ParallelWorldDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            options.UseNpgsql(databaseOptions.ConnectionString);
        });

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ParallelWorldDbContext>());

        return services;
    }
}

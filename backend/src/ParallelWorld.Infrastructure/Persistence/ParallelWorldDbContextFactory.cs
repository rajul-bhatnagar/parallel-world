using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ParallelWorld.Infrastructure.Persistence;

public sealed class ParallelWorldDbContextFactory : IDesignTimeDbContextFactory<ParallelWorldDbContext>
{
    public ParallelWorldDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__Default is required for design-time migration commands.");
        }

        var options = new DbContextOptionsBuilder<ParallelWorldDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ParallelWorldDbContext(options);
    }
}

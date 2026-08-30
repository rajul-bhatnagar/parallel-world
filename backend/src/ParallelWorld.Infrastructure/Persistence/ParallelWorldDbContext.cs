using Microsoft.EntityFrameworkCore;
using ParallelWorld.Application.Abstractions.Persistence;

namespace ParallelWorld.Infrastructure.Persistence;

public sealed class ParallelWorldDbContext(DbContextOptions<ParallelWorldDbContext> options)
    : DbContext(options), IUnitOfWork;

using Npgsql;
using ParallelWorld.Application.Abstractions.Persistence;

namespace ParallelWorld.Infrastructure.Persistence;

internal sealed class PersistenceFailureClassifier : IPersistenceFailureClassifier
{
    public bool HasConstraint(Exception exception, string constraintName) =>
        FindPostgresException(exception)?.ConstraintName == constraintName;

    public bool IsSerializationFailure(Exception exception) =>
        FindPostgresException(exception)?.SqlState == PostgresErrorCodes.SerializationFailure;

    public bool IsRetryableConcurrency(Exception exception)
    {
        var postgresException = FindPostgresException(exception);
        return postgresException?.SqlState is PostgresErrorCodes.SerializationFailure
            or PostgresErrorCodes.UniqueViolation;
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }

            if (current.InnerException is null)
            {
                break;
            }
        }

        return null;
    }
}

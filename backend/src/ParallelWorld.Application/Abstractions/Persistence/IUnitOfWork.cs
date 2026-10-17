namespace ParallelWorld.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<IApplicationTransaction> BeginTransactionAsync(
        ApplicationIsolationLevel isolationLevel,
        CancellationToken cancellationToken = default);

    void ClearTrackedChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IApplicationTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}

public enum ApplicationIsolationLevel
{
    ReadCommitted,
    Serializable,
}

public interface IPersistenceFailureClassifier
{
    bool HasConstraint(Exception exception, string constraintName);

    bool IsSerializationFailure(Exception exception);

    bool IsRetryableConcurrency(Exception exception);
}

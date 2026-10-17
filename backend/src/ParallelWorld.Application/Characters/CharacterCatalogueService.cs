using ParallelWorld.Application.Abstractions.Persistence;
using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Characters;

namespace ParallelWorld.Application.Characters;

public sealed class CharacterCatalogueService(
    ICharacterRepository repository,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier failureClassifier) : ICharacterCatalogueService
{
    public async Task<CharacterResult<CharacterPage>> ListAsync(
        Guid userId,
        Guid worldId,
        CharacterStatus? status,
        int limit,
        string? cursor,
        CancellationToken cancellationToken)
    {
        if (limit is < 1 or > 100)
        {
            return Invalid("Limit must be between 1 and 100.");
        }

        var cursorResult = CharacterCursor.TryDecode(cursor, worldId, status);
        if (!cursorResult.IsValid)
        {
            return Invalid("The character cursor is invalid.", "invalid_cursor");
        }

        var initialized = await EnsureInitializedAsync(userId, worldId, cancellationToken);
        if (initialized is not null)
        {
            return CharacterResult<CharacterPage>.Fail(initialized);
        }

        var results = await repository.ListAsync(
            worldId,
            status,
            cursorResult.LastId,
            limit + 1,
            cancellationToken);
        var hasMore = results.Count > limit;
        var items = results.Take(limit).ToArray();
        var nextCursor = hasMore
            ? CharacterCursor.Encode(worldId, status, items[^1].Id)
            : null;
        return CharacterResult<CharacterPage>.Success(new(items, nextCursor, hasMore));
    }

    public async Task<CharacterResult<CharacterDetails>> GetAsync(
        Guid userId,
        Guid worldId,
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var initialized = await EnsureInitializedAsync(userId, worldId, cancellationToken);
        if (initialized is not null)
        {
            return CharacterResult<CharacterDetails>.Fail(initialized);
        }

        var details = await repository.FindDetailsAsync(
            userId,
            worldId,
            characterId,
            cancellationToken);
        return details is null
            ? CharacterResult<CharacterDetails>.Fail(NotAvailable())
            : CharacterResult<CharacterDetails>.Success(details);
    }

    private async Task<ServiceFailure?> EnsureInitializedAsync(
        Guid userId,
        Guid worldId,
        CancellationToken cancellationToken)
    {
        var context = await repository.FindSeedContextAsync(userId, worldId, false, cancellationToken);
        if (context is null)
        {
            return NotAvailable();
        }

        var count = await repository.CountAsync(worldId, cancellationToken);
        if (count == CharacterCastFactory.CastSize)
        {
            return null;
        }

        if (count != 0)
        {
            return IncompleteCatalogue();
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await using var transaction = await unitOfWork.BeginTransactionAsync(
                    ApplicationIsolationLevel.Serializable,
                    cancellationToken);
                context = await repository.FindSeedContextAsync(userId, worldId, true, cancellationToken);
                if (context is null)
                {
                    return NotAvailable();
                }

                count = await repository.CountAsync(worldId, cancellationToken);
                if (count == 0)
                {
                    repository.Add(CharacterCastFactory.Create(context));
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else if (count != CharacterCastFactory.CastSize)
                {
                    return IncompleteCatalogue();
                }

                await transaction.CommitAsync(cancellationToken);
                return null;
            }
            catch (Exception exception) when (failureClassifier.IsRetryableConcurrency(exception))
            {
                unitOfWork.ClearTrackedChanges();
            }
        }

        return new ServiceFailure(
            "concurrency_conflict",
            409,
            "The character catalogue could not be initialized concurrently.");
    }

    private static CharacterResult<CharacterPage> Invalid(string title, string code = "validation_failed") =>
        CharacterResult<CharacterPage>.Fail(new ServiceFailure(code, 400, title));

    private static ServiceFailure NotAvailable() => new(
        "resource_not_available",
        404,
        "The requested resource is not available.");

    private static ServiceFailure IncompleteCatalogue() => new(
        "character_catalogue_incomplete",
        409,
        "The character catalogue is incomplete.");
}

internal readonly record struct CharacterCursorResult(bool IsValid, Guid? LastId);

internal static class CharacterCursor
{
    private const byte Version = 1;

    public static string Encode(Guid worldId, CharacterStatus? status, Guid lastId)
    {
        Span<byte> bytes = stackalloc byte[34];
        bytes[0] = Version;
        worldId.TryWriteBytes(bytes[1..17]);
        bytes[17] = status switch
        {
            null => byte.MaxValue,
            CharacterStatus.Active => 0,
            CharacterStatus.Inactive => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };
        lastId.TryWriteBytes(bytes[18..34]);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static CharacterCursorResult TryDecode(
        string? cursor,
        Guid worldId,
        CharacterStatus? status)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return new(true, null);
        }

        try
        {
            var normalized = cursor.Replace('-', '+').Replace('_', '/');
            normalized = normalized.PadRight((normalized.Length + 3) / 4 * 4, '=');
            var bytes = Convert.FromBase64String(normalized);
            if (bytes.Length != 34 || bytes[0] != Version)
            {
                return new(false, null);
            }

            var cursorWorldId = new Guid(bytes.AsSpan(1, 16));
            var expectedStatus = status switch
            {
                null => byte.MaxValue,
                CharacterStatus.Active => 0,
                CharacterStatus.Inactive => 1,
                _ => byte.MaxValue,
            };
            return cursorWorldId == worldId && bytes[17] == expectedStatus
                ? new(true, new Guid(bytes.AsSpan(18, 16)))
                : new(false, null);
        }
        catch (FormatException)
        {
            return new(false, null);
        }
    }
}

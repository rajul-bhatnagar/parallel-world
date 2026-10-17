using ParallelWorld.Application.Common;
using ParallelWorld.Domain.Characters;

namespace ParallelWorld.Application.Characters;

public sealed record CharacterSeedContext(
    Guid WorldId,
    long WorldSeed,
    int RuleVersion,
    DateTimeOffset WorldCreatedAt);

public sealed record CharacterCast(
    IReadOnlyList<Character> Characters,
    IReadOnlyList<Domain.Worlds.Actor> Actors,
    IReadOnlyList<CharacterTraits> Traits,
    IReadOnlyList<CharacterInterest> Interests,
    IReadOnlyList<CharacterOpinion> Opinions,
    IReadOnlyList<CharacterSchedule> Schedules);

public sealed record CharacterSummary(
    Guid Id,
    string DisplayName,
    string Handle,
    string Profession,
    string VisibleMood,
    bool IsFollowed);

public sealed record CharacterInterestSummary(string TopicId);

public sealed record CharacterScheduleSummary(
    int DayOfWeek,
    TimeOnly StartLocalTime,
    TimeOnly EndLocalTime,
    string Activity);

public sealed record CharacterDetails(
    Guid Id,
    string DisplayName,
    string Handle,
    string Bio,
    int Age,
    string Profession,
    string Archetype,
    string VisibleMood,
    IReadOnlyList<CharacterInterestSummary> Interests,
    IReadOnlyList<CharacterScheduleSummary> Schedule);

public sealed record CharacterPage(
    IReadOnlyList<CharacterSummary> Items,
    string? NextCursor,
    bool HasMore);

public sealed record CharacterResult<T>(T? Value, ServiceFailure? Failure)
{
    public bool IsSuccess => Failure is null;

    public static CharacterResult<T> Success(T value) => new(value, null);

    public static CharacterResult<T> Fail(ServiceFailure failure) => new(default, failure);
}

public interface ICharacterCatalogueService
{
    Task<CharacterResult<CharacterPage>> ListAsync(
        Guid userId,
        Guid worldId,
        CharacterStatus? status,
        int limit,
        string? cursor,
        CancellationToken cancellationToken);

    Task<CharacterResult<CharacterDetails>> GetAsync(
        Guid userId,
        Guid worldId,
        Guid characterId,
        CancellationToken cancellationToken);
}

public interface ICharacterRepository
{
    Task<CharacterSeedContext?> FindSeedContextAsync(
        Guid ownerUserId,
        Guid worldId,
        bool forUpdate,
        CancellationToken cancellationToken);

    Task<int> CountAsync(Guid worldId, CancellationToken cancellationToken);

    void Add(CharacterCast cast);

    Task<IReadOnlyList<CharacterSummary>> ListAsync(
        Guid worldId,
        CharacterStatus? status,
        Guid? afterId,
        int take,
        CancellationToken cancellationToken);

    Task<CharacterDetails?> FindDetailsAsync(
        Guid ownerUserId,
        Guid worldId,
        Guid characterId,
        CancellationToken cancellationToken);
}

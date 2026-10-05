using ParallelWorld.Application.Common;

namespace ParallelWorld.Application.Relationships;

public sealed record RomanticInvitationView(
    Guid Id,
    Guid EpisodeId,
    Guid? CharacterId,
    Guid InitiatorActorId,
    Guid TargetActorId,
    string DateType,
    string Status,
    string RomanticStatus,
    string ReasonCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset CreatedAtWorldTime,
    DateTimeOffset ExpiresAtWorldTime,
    DateTimeOffset? ResolvedAtUtc,
    DateTimeOffset? ResolvedAtWorldTime);

public sealed record RomanticHistoryItem(
    Guid Id,
    Guid EpisodeId,
    string FromStatus,
    string ToStatus,
    string ReasonCode,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset OccurredAtWorldTime);

public interface IRomanceRepository
{
    Task<RelationshipResult<RomanticInvitationView>> CreatePlayerInvitationAsync(
        Guid userId, Guid worldId, Guid targetCharacterActorId, string idempotencyKey, CancellationToken cancellationToken);
    Task<RelationshipResult<RomanticInvitationView>> CreateAutonomousInvitationAsync(
        Guid worldId, Guid initiatorActorId, Guid targetActorId, string idempotencyKey, CancellationToken cancellationToken);
    Task<RelationshipResult<RomanticInvitationView>> ResolvePlayerOutcomeAsync(
        Guid userId, Guid worldId, Guid invitationId, bool accept, CancellationToken cancellationToken);
    Task<RelationshipResult<IReadOnlyList<RomanticInvitationView>>> ListAsync(
        Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<RelationshipResult<IReadOnlyList<RomanticHistoryItem>>> HistoryAsync(
        Guid userId, Guid worldId, Guid otherActorId, CancellationToken cancellationToken);
}

public interface IRomanceService
{
    Task<RelationshipResult<RomanticInvitationView>> InviteCharacterAsync(
        Guid userId, Guid worldId, Guid characterActorId, string idempotencyKey, CancellationToken cancellationToken);
    Task<RelationshipResult<RomanticInvitationView>> InviteAutonomouslyAsync(
        Guid worldId, Guid initiatorActorId, Guid targetActorId, string idempotencyKey, CancellationToken cancellationToken);
    Task<RelationshipResult<RomanticInvitationView>> ResolveAsync(
        Guid userId, Guid worldId, Guid invitationId, string decision, CancellationToken cancellationToken);
    Task<RelationshipResult<IReadOnlyList<RomanticInvitationView>>> ListAsync(
        Guid userId, Guid worldId, CancellationToken cancellationToken);
    Task<RelationshipResult<IReadOnlyList<RomanticHistoryItem>>> HistoryAsync(
        Guid userId, Guid worldId, Guid otherActorId, CancellationToken cancellationToken);
}

public sealed class RomanceService(IRomanceRepository repository) : IRomanceService
{
    public Task<RelationshipResult<RomanticInvitationView>> InviteCharacterAsync(
        Guid userId, Guid worldId, Guid characterActorId, string idempotencyKey, CancellationToken cancellationToken) =>
        ValidKey(idempotencyKey)
            ? repository.CreatePlayerInvitationAsync(userId, worldId, characterActorId, idempotencyKey, cancellationToken)
            : Invalid<RomanticInvitationView>("A valid Idempotency-Key header is required.");

    public Task<RelationshipResult<RomanticInvitationView>> InviteAutonomouslyAsync(
        Guid worldId, Guid initiatorActorId, Guid targetActorId, string idempotencyKey, CancellationToken cancellationToken) =>
        ValidKey(idempotencyKey)
            ? repository.CreateAutonomousInvitationAsync(worldId, initiatorActorId, targetActorId, idempotencyKey, cancellationToken)
            : Invalid<RomanticInvitationView>("A valid idempotency key is required.");

    public Task<RelationshipResult<RomanticInvitationView>> ResolveAsync(
        Guid userId, Guid worldId, Guid invitationId, string decision, CancellationToken cancellationToken) =>
        decision switch
        {
            "accept" => repository.ResolvePlayerOutcomeAsync(userId, worldId, invitationId, true, cancellationToken),
            "reject" => repository.ResolvePlayerOutcomeAsync(userId, worldId, invitationId, false, cancellationToken),
            _ => Invalid<RomanticInvitationView>("Decision must be accept or reject."),
        };

    public Task<RelationshipResult<IReadOnlyList<RomanticInvitationView>>> ListAsync(
        Guid userId, Guid worldId, CancellationToken cancellationToken) => repository.ListAsync(userId, worldId, cancellationToken);

    public Task<RelationshipResult<IReadOnlyList<RomanticHistoryItem>>> HistoryAsync(
        Guid userId, Guid worldId, Guid otherActorId, CancellationToken cancellationToken) => repository.HistoryAsync(userId, worldId, otherActorId, cancellationToken);

    private static bool ValidKey(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 200;
    private static Task<RelationshipResult<T>> Invalid<T>(string detail) => Task.FromResult(
        RelationshipResult<T>.Fail(new ServiceFailure("validation_failed", 400, detail)));
}

using ParallelWorld.Domain.Relationships;

namespace ParallelWorld.Application.Relationships;

public sealed class RelationshipService(IRelationshipRepository repository) : IRelationshipService
{
    public Task<Relationship?> ApplyAsync(ApplyRelationshipEventCommand command, CancellationToken cancellationToken = default)
        => RelationshipEventMatrix.TryGet(command.EventType, out var delta)
            ? repository.ApplyAsync(command, delta, cancellationToken)
            : Task.FromResult<Relationship?>(null);

    public async Task<RelationshipResult<IReadOnlyList<RelationshipSummary>>> ListAsync(Guid userId, Guid worldId, CancellationToken cancellationToken)
    {
        if (!await repository.OwnsWorldAsync(userId, worldId, cancellationToken))
        {
            return RelationshipResult<IReadOnlyList<RelationshipSummary>>.Fail(new("world_not_found", 404, "World was not found."));
        }
        var rows = await repository.ListAsync(userId, worldId, cancellationToken);
        return RelationshipResult<IReadOnlyList<RelationshipSummary>>.Success(rows);
    }

    public async Task<RelationshipResult<RelationshipDetails>> FindAsync(Guid userId, Guid worldId, Guid actorId, int historyLimit, CancellationToken cancellationToken)
    { var row = await repository.FindAsync(userId, worldId, actorId, historyLimit, cancellationToken); return row is null ? RelationshipResult<RelationshipDetails>.Fail(new("relationship_not_found", 404, "Relationship was not found.")) : RelationshipResult<RelationshipDetails>.Success(row); }
}

public static class RelationshipEventMatrix
{
    private static readonly IReadOnlySet<string> QualifiedNegativeEventTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Insult",
        "Public embarrassment",
        "Ignoring direct message",
        "Broken promise",
        "Secret revealed",
        "Failed date",
        "Rejected apology",
        "Repeated argument",
        "Jealous behaviour",
        "Unfollow",
    };
    private static readonly IReadOnlyDictionary<string, RelationshipValues> Rows = new Dictionary<string, RelationshipValues>(StringComparer.OrdinalIgnoreCase)
    {
        ["Helpful reply"] = new(1, 2, 1, 1, 2, 0, 0, 0, 0),
        ["Compliment"] = new(1, 1, 1, 2, 1, 0, 0, 1, 0),
        ["Insult"] = new(1, -5, -4, -3, -3, 5, 0, -2, 0),
        ["Public defence"] = new(2, 7, 5, 3, 2, -2, 0, 1, 0),
        ["Public embarrassment"] = new(2, -8, -5, -4, -6, 6, 1, -3, -1),
        ["Ignoring direct message"] = new(0, -2, 0, -1, -2, 1, 0, 0, 0),
        ["Broken promise"] = new(1, -12, -3, -4, -5, 3, 0, -2, -4),
        ["Kept promise"] = new(1, 6, 4, 2, 3, -1, 0, 1, 2),
        ["Secret shared"] = new(3, 4, 0, 1, 4, 0, 0, 0, 0),
        ["Secret revealed"] = new(2, -15, -6, -5, -8, 7, 1, -3, -5),
        ["Successful date"] = new(4, 3, 2, 4, 4, -1, 0, 6, 3),
        ["Failed date"] = new(2, -2, -1, -2, -3, 1, 0, -5, -1),
        ["Apology offered"] = new(1, 1, 1, 0, 1, -1, 0, 0, 0),
        ["Accepted apology"] = new(2, 3, 2, 2, 3, -2, 0, 0, 1),
        ["Rejected apology"] = new(1, 0, -1, -2, -2, 2, 0, -1, 0),
        ["Repeated argument"] = new(2, -4, -3, -3, -4, 5, 1, -2, -2),
        ["Jealous behaviour"] = new(1, -3, -2, -1, -3, 2, 6, 0, -1),
        ["Support during life event"] = new(3, 5, 4, 4, 4, -1, 0, 1, 1),
        ["Unfollow"] = new(0, -2, -1, -2, -1, 1, 0, 0, 0),
        ["Re-follow"] = new(2, 1, 0, 1, 1, -1, 0, 0, 0),
    };
    public static bool TryGet(string eventType, out RelationshipValues values) => Rows.TryGetValue(eventType, out values);
    public static bool IsQualifiedNegative(string eventType) => QualifiedNegativeEventTypes.Contains(eventType);
}

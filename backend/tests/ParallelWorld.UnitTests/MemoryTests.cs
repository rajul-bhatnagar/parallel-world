using ParallelWorld.Application.Memory;
using ParallelWorld.Domain.Memory;
using ParallelWorld.Domain.Relationships;

namespace ParallelWorld.UnitTests;

public sealed class MemoryTests
{
    [Fact]
    public void M12_HasExactlyFiveTypesAndExactDefaults()
    {
        Assert.Equal(
            [MemoryType.Fact, MemoryType.Preference, MemoryType.Event, MemoryType.Secret, MemoryType.Promise],
            Enum.GetValues<MemoryType>());
        Assert.Equal(60, MemoryMechanics.Importance(MemoryType.Fact));
        Assert.Equal(60, MemoryMechanics.Importance(MemoryType.Preference));
        Assert.Equal(50, MemoryMechanics.Importance(MemoryType.Event));
        Assert.Equal(90, MemoryMechanics.Importance(MemoryType.Secret));
        Assert.Equal(90, MemoryMechanics.Importance(MemoryType.Promise));
        Assert.Equal(90, MemoryMechanics.Confidence(MemoryAuthorityType.StructuredPlayerStatement));
        Assert.Equal(90, MemoryMechanics.Confidence(MemoryAuthorityType.GameplayEvent));
        Assert.Equal(100, MemoryMechanics.Confidence(MemoryAuthorityType.StructuredGameplayFact));
        Assert.Equal(100, MemoryMechanics.Confidence(MemoryAuthorityType.StructuredPreference));
        Assert.Equal(100, MemoryMechanics.Confidence(MemoryAuthorityType.StructuredSecret));
        Assert.Equal(100, MemoryMechanics.Confidence(MemoryAuthorityType.StructuredPromise));

        var ordinary = Entity(MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact);
        Assert.Equal(MemoryVisibility.CharacterPrivate, ordinary.Visibility);
        Assert.Equal(MemoryLifecycleStatus.Active, ordinary.LifecycleStatus);
        Assert.Null(typeof(CharacterMemory).GetProperty("ExpiresAtUtc"));
    }

    [Fact]
    public void Mem02_UsesExactFormulaAndMidpointAwayFromZero()
    {
        Assert.Equal(58, MemoryMechanics.RecallScore(100, 0, 60, 60));
        Assert.Equal(45, MemoryMechanics.RecallScore(0, 100, 50, 50));
        Assert.Equal(13, MemoryMechanics.RecallScore(0, 0, 38, 50));
        Assert.Equal(6, MemoryMechanics.RecallScore(0, 0, 0, 55));
        Assert.Equal(
            RelationshipMechanics.Relevance(RelationshipValues.Initial),
            MemoryMechanics.RelationshipMatch(RelationshipValues.Initial));
    }

    [Fact]
    public async Task AuthoritativeMappingRejectsMismatchedCategoryWithoutRepositoryCall()
    {
        var repository = new FakeMemoryRepository();
        var service = new MemoryService(repository);
        var result = await service.CreateAsync(Command(
            MemoryType.Secret,
            MemoryAuthorityType.StructuredPlayerStatement));

        Assert.False(result.Created);
        Assert.Equal("memory_source_category_invalid", result.ReasonCode);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task NaturalLanguagePromiseWithoutStructuredConditionIsRejected()
    {
        var repository = new FakeMemoryRepository();
        var service = new MemoryService(repository);
        var result = await service.CreateAsync(Command(
            MemoryType.Promise,
            MemoryAuthorityType.StructuredPromise));

        Assert.False(result.Created);
        Assert.Equal("promise_condition_invalid", result.ReasonCode);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public void ProtectedMemoriesCannotBeEvicted()
    {
        var secret = Entity(MemoryType.Secret, MemoryAuthorityType.StructuredSecret);
        var promise = Entity(MemoryType.Promise, MemoryAuthorityType.StructuredPromise);
        var fact = Entity(MemoryType.Fact, MemoryAuthorityType.StructuredGameplayFact);

        Assert.Throws<InvalidOperationException>(() => secret.Evict(DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => promise.Evict(DateTimeOffset.UtcNow));
        fact.Evict(DateTimeOffset.UtcNow);
        Assert.Equal(MemoryLifecycleStatus.Evicted, fact.LifecycleStatus);
    }

    [Fact]
    public void PromiseTransitionsRequireStructuredConditionsAndAllTerminalStates()
    {
        var dueEvent = Guid.NewGuid();
        var fulfilled = new Promise(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "deliver_item",
            Guid.NewGuid(), Guid.NewGuid(), PromiseDueConditionType.GameplayEvent, null, dueEvent,
            "item_delivered", DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            fulfilled.Fulfill(Guid.NewGuid(), "item_delivered", DateTimeOffset.UtcNow));
        fulfilled.Fulfill(dueEvent, "item_delivered", DateTimeOffset.UtcNow);
        Assert.Equal(PromiseStatus.Fulfilled, fulfilled.Status);
        Assert.Throws<InvalidOperationException>(() =>
            fulfilled.Cancel(Guid.NewGuid(), DateTimeOffset.UtcNow));

        var cancelled = new Promise(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "visit",
            Guid.NewGuid(), null, PromiseDueConditionType.GameplayEvent, null, null,
            "visit_completed", DateTimeOffset.UtcNow);
        cancelled.Cancel(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Equal(PromiseStatus.Cancelled, cancelled.Status);

        var dueAt = DateTimeOffset.UtcNow.AddDays(1);
        var expired = new Promise(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "return_item",
            Guid.NewGuid(), null, PromiseDueConditionType.WorldTime, dueAt, null, null,
            DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => expired.Expire(dueAt.AddTicks(-1)));
        expired.Expire(dueAt);
        Assert.Equal(PromiseStatus.Expired, expired.Status);
    }

    [Fact]
    public void MessageReplyDecisionHasNoMemoryInput()
    {
        var score = Domain.Messaging.MessageReplyMechanics.Score(RelationshipValues.Initial, 55);
        Assert.Equal(62, score);
        Assert.DoesNotContain(
            typeof(Domain.Messaging.MessageReplyMechanics).GetMethods()
                .SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace == typeof(CharacterMemory).Namespace);
    }

    private static CreateMemoryCommand Command(
        MemoryType type,
        MemoryAuthorityType authority) =>
        new(Guid.NewGuid(), Guid.NewGuid(), type, authority, MemorySubjectType.Actor,
            Guid.NewGuid(), null, null, "structured", MemorySourceType.Message, Guid.NewGuid(), null);

    private static CharacterMemory Entity(MemoryType type, MemoryAuthorityType authority)
    {
        var source = Guid.NewGuid();
        return new CharacterMemory(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), type, authority,
            MemorySubjectType.Actor, Guid.NewGuid(), null, null, "structured",
            MemorySourceType.GameplayEvent, source, source, null, DateTimeOffset.UtcNow);
    }

    private sealed class FakeMemoryRepository : IMemoryRepository
    {
        public int CreateCalls { get; private set; }

        public Task<MemoryCreationResult> CreateAsync(
            CreateMemoryCommand command,
            CancellationToken cancellationToken)
        {
            CreateCalls++;
            return Task.FromResult(new MemoryCreationResult(true, false, Guid.NewGuid(), null, null));
        }

        public Task<MemoryRecallResult> RecallAsync(
            RecallMemoryCommand command,
            CancellationToken cancellationToken) =>
            Task.FromResult(new MemoryRecallResult(true, false, null, []));

        public Task<bool> TransitionPromiseAsync(
            TransitionPromiseCommand command,
            CancellationToken cancellationToken) => Task.FromResult(true);
    }
}

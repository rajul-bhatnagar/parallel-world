namespace ParallelWorld.Domain.Memory;

public enum MemoryType
{
    Fact,
    Preference,
    Event,
    Secret,
    Promise,
}

public enum MemorySourceType
{
    GameplayEvent,
    Message,
}

public enum MemoryAuthorityType
{
    StructuredGameplayFact,
    StructuredPlayerStatement,
    StructuredPreference,
    GameplayEvent,
    StructuredSecret,
    StructuredPromise,
}

public enum MemorySubjectType
{
    Actor,
    Topic,
}

public enum MemoryVisibility
{
    CharacterPrivate,
}

public enum MemoryLifecycleStatus
{
    Active,
    Evicted,
}

public enum MemoryCreationOutcomeType
{
    Created,
    Rejected,
}

public enum MemoryRecallPurpose
{
    Internal,
    MessageWording,
}

public enum SecretStatus
{
    Active,
}

public enum PromiseStatus
{
    Active,
    Fulfilled,
    Cancelled,
    Expired,
}

public enum PromiseDueConditionType
{
    WorldTime,
    GameplayEvent,
}

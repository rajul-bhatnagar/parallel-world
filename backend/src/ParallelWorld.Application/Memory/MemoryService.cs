using ParallelWorld.Domain.Memory;

namespace ParallelWorld.Application.Memory;

public sealed class MemoryService(IMemoryRepository repository) : IMemoryService
{
    public Task<MemoryCreationResult> CreateAsync(
        CreateMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateCreation(command);
        return error is null
            ? repository.CreateAsync(command, cancellationToken)
            : Task.FromResult(MemoryCreationResult.Invalid(error));
    }

    public Task<MemoryRecallResult> RecallAsync(
        RecallMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateRecall(command);
        return error is null
            ? repository.RecallAsync(command, cancellationToken)
            : Task.FromResult(MemoryRecallResult.Invalid(error));
    }

    public async Task<IReadOnlyList<string>> RecallForMessageWordingAsync(
        Guid worldId,
        Guid ownerCharacterId,
        Guid subjectActorId,
        Guid plannedReplyId,
        CancellationToken cancellationToken = default)
    {
        var result = await RecallAsync(
            new RecallMemoryCommand(
                worldId,
                ownerCharacterId,
                MemoryRecallPurpose.MessageWording,
                MemorySubjectType.Actor,
                subjectActorId,
                null,
                null,
                $"message-wording:{plannedReplyId:N}"),
            cancellationToken);
        return result.IsValid
            ? result.Items.Select(item => item.StructuredContent).ToArray()
            : [];
    }

    public Task<bool> TransitionPromiseAsync(
        TransitionPromiseCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.WorldId == Guid.Empty || command.PromiseId == Guid.Empty
            || command.TargetStatus is PromiseStatus.Active)
        {
            return Task.FromResult(false);
        }

        if (command.TargetStatus is PromiseStatus.Fulfilled or PromiseStatus.Cancelled
            && (!command.GameplayEventId.HasValue || command.GameplayEventId == Guid.Empty))
        {
            return Task.FromResult(false);
        }

        return repository.TransitionPromiseAsync(command, cancellationToken);
    }

    private static string? ValidateCreation(CreateMemoryCommand command)
    {
        if (command.WorldId == Guid.Empty || command.OwnerCharacterId == Guid.Empty
            || command.SourceId == Guid.Empty || string.IsNullOrWhiteSpace(command.StructuredContent)
            || command.StructuredContent.Trim().Length > 500)
        {
            return "memory_source_invalid";
        }

        if (!MemoryMechanics.IsApprovedMapping(command.MemoryType, command.AuthorityType))
        {
            return "memory_source_category_invalid";
        }

        if (command.AuthorityType == MemoryAuthorityType.StructuredPlayerStatement
            && command.SourceType != MemorySourceType.Message
            || command.AuthorityType is MemoryAuthorityType.StructuredGameplayFact
                or MemoryAuthorityType.GameplayEvent
                && command.SourceType != MemorySourceType.GameplayEvent)
        {
            return "memory_source_category_invalid";
        }

        if (!ValidSubject(command.SubjectType, command.SubjectActorId, command.SubjectTopicId))
        {
            return "memory_subject_invalid";
        }

        if (command.MemoryType == MemoryType.Promise)
        {
            if (command.Promise is null || !ValidPromise(command.Promise))
            {
                return "promise_condition_invalid";
            }
        }
        else if (command.Promise is not null)
        {
            return "promise_condition_invalid";
        }

        return null;
    }

    private static string? ValidateRecall(RecallMemoryCommand command)
    {
        if (command.WorldId == Guid.Empty || command.OwnerCharacterId == Guid.Empty
            || string.IsNullOrWhiteSpace(command.IdempotencyKey)
            || command.IdempotencyKey.Length > 200
            || !ValidSubject(command.SubjectType, command.SubjectActorId, command.SubjectTopicId))
        {
            return "memory_recall_invalid";
        }

        return null;
    }

    private static bool ValidSubject(
        MemorySubjectType type,
        Guid? actorId,
        string? topicId) =>
        type switch
        {
            MemorySubjectType.Actor => actorId.HasValue && actorId != Guid.Empty
                && string.IsNullOrWhiteSpace(topicId),
            MemorySubjectType.Topic => actorId is null && !string.IsNullOrWhiteSpace(topicId),
            _ => false,
        };

    private static bool ValidPromise(PromiseCreationDetails promise)
    {
        if (string.IsNullOrWhiteSpace(promise.PromiseType)
            || promise.SourceActorId == Guid.Empty
            || promise.SourceActorId == promise.TargetActorId)
        {
            return false;
        }

        return promise.DueConditionType switch
        {
            PromiseDueConditionType.WorldTime => promise.DueAtWorldTime.HasValue
                && promise.DueGameplayEventId is null
                && string.IsNullOrWhiteSpace(promise.DueEventType),
            PromiseDueConditionType.GameplayEvent => promise.DueAtWorldTime is null
                && (promise.DueGameplayEventId.HasValue
                    || !string.IsNullOrWhiteSpace(promise.DueEventType)),
            _ => false,
        };
    }
}

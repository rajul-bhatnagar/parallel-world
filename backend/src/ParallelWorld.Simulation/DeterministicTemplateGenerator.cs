namespace ParallelWorld.Simulation;

public interface IDeterministicTemplateGenerator
{
    string Generate(string actionType, string actorDisplayName, string topic);
}

public sealed class DeterministicTemplateGenerator : IDeterministicTemplateGenerator
{
    public string Generate(string actionType, string actorDisplayName, string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        return FormattableString.Invariant($"{actorDisplayName} shared a thought about {topic} ({actionType}).");
    }
}

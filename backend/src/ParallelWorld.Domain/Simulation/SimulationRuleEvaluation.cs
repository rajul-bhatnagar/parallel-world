namespace ParallelWorld.Domain.Simulation;

public sealed class SimulationRuleEvaluation
{
    private SimulationRuleEvaluation()
    {
        RuleCode = string.Empty;
        ReasonCode = string.Empty;
    }

    public SimulationRuleEvaluation(
        Guid id,
        Guid worldId,
        Guid simulationRunId,
        string ruleCode,
        SimulationRuleOutcome outcome,
        string reasonCode,
        int ruleVersion,
        DateTimeOffset evaluatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        Id = id;
        WorldId = worldId;
        SimulationRunId = simulationRunId;
        RuleCode = ruleCode;
        Outcome = outcome;
        ReasonCode = reasonCode;
        RuleVersion = ruleVersion;
        EvaluatedAtUtc = evaluatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorldId { get; private set; }
    public Guid SimulationRunId { get; private set; }
    public string RuleCode { get; private set; }
    public SimulationRuleOutcome Outcome { get; private set; }
    public string ReasonCode { get; private set; }
    public int RuleVersion { get; private set; }
    public DateTimeOffset EvaluatedAtUtc { get; private set; }
}

public enum SimulationRuleOutcome
{
    Unavailable,
    Ineligible,
    Eligible,
    Executed,
}

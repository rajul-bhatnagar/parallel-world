using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.UnitTests;

public sealed class RomanceTests
{
    [Fact]
    public void Defaults_EnableRomanceWithAnyEligibleActor()
    {
        var worldId = Guid.NewGuid(); var now = DateTimeOffset.UnixEpoch;
        Assert.True(new WorldSettings(Guid.NewGuid(), worldId, now).RomanceEnabled);
        Assert.Equal(RomancePreferenceMode.AnyEligibleActor, new PlayerProfile(Guid.NewGuid(), worldId, now).RomancePreferenceMode);
    }

    [Theory]
    [InlineData(false, RomancePreferenceMode.AnyEligibleActor, RomancePreferenceMode.AnyEligibleActor, "romance_disabled")]
    [InlineData(true, RomancePreferenceMode.Disabled, RomancePreferenceMode.AnyEligibleActor, "initiator_not_open")]
    [InlineData(true, RomancePreferenceMode.AnyEligibleActor, RomancePreferenceMode.Disabled, "target_not_open")]
    public void Gate_UsesAcceptedReasonPrecedence(bool enabled, RomancePreferenceMode initiator, RomancePreferenceMode target, string expected)
    {
        var reason = RomanceMechanics.Gate(new(enabled, initiator, target, true, true, true, true, true, true));
        Assert.Equal(expected, reason);
    }

    [Fact]
    public void Gate_PrioritizesCompatibilityThenExclusivityThenState()
    {
        Assert.Equal(RomanceReasonCodes.RomanceIncompatible, RomanceMechanics.Gate(new(true, RomancePreferenceMode.AnyEligibleActor, RomancePreferenceMode.AnyEligibleActor, false, true, true, true, true, true)));
        Assert.Equal(RomanceReasonCodes.ExclusivityConflict, RomanceMechanics.Gate(new(true, RomancePreferenceMode.AnyEligibleActor, RomancePreferenceMode.AnyEligibleActor, true, true, true, true, true, true)));
        Assert.Equal(RomanceReasonCodes.RomanticStateConflict, RomanceMechanics.Gate(new(true, RomancePreferenceMode.AnyEligibleActor, RomancePreferenceMode.AnyEligibleActor, true, true, true, true, false, true)));
    }

    [Fact]
    public void Initiation_UsesExactNeutralInputsAndThresholdOrder()
    {
        var values = new RelationshipValues(70, 60, 0, 0, 60, 0, 0, 75, 0);
        var result = RomanceMechanics.EvaluateInitiation(values, 70);
        Assert.True(result.Eligible);
        Assert.Equal(65, result.Score);

        var familiarityFirst = RomanceMechanics.EvaluateInitiation(values with { Familiarity = 54, Trust = 0 }, 0);
        Assert.Equal(RomanceReasonCodes.FamiliarityBelowMinimum, familiarityFirst.ReasonCode);
    }

    [Fact]
    public void Acceptance_UsesExactNeutralInputsAndBoundedOffset()
    {
        var values = new RelationshipValues(0, 60, 0, 0, 60, 0, 0, 75, 0);
        var result = RomanceMechanics.EvaluateAcceptance(values, 70, -5);
        Assert.True(result.Eligible);
        Assert.Equal(65, result.Score);
        Assert.Throws<ArgumentOutOfRangeException>(() => RomanceMechanics.EvaluateAcceptance(values, 70, 6));
    }

    [Fact]
    public void Invitation_IsCasualDateAndExpiresAfterTwentyFourGameHours()
    {
        var worldTime = DateTimeOffset.UnixEpoch;
        var invitation = new RomanticInvitation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 65, worldTime, worldTime, 1, "test-key");
        Assert.Equal("CasualDate", invitation.DateType);
        Assert.Equal(worldTime.AddHours(24), invitation.ExpiresAtWorldTime);
    }

    [Fact]
    public void Pair_IsCanonicalAndCannotEndDatingInM13()
    {
        var first = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var second = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var pair = new RomanticRelationship(Guid.NewGuid(), Guid.NewGuid(), first, second, DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);
        Assert.Equal(second, pair.ActorAId);
        Assert.Equal(first, pair.ActorBId);
        pair.MarkInvitationPending(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);
        pair.StartDating(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch);
        Assert.Throws<InvalidOperationException>(() => pair.RejectOrExpire(DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch));
    }
}

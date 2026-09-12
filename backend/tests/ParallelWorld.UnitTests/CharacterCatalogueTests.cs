using ParallelWorld.Application.Characters;
using ParallelWorld.Domain.Characters;

namespace ParallelWorld.UnitTests;

public sealed class CharacterCatalogueTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 9, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithSameWorldSeedAndRuleVersion_IsReproducible()
    {
        var worldId = Guid.Parse("dd3508da-cc3a-42fb-b775-33c173394cab");
        var context = new CharacterSeedContext(worldId, 120045, 1, CreatedAt);

        var first = CharacterCastFactory.Create(context);
        var second = CharacterCastFactory.Create(context);

        Assert.Equal(CharacterCastFactory.CastSize, first.Characters.Count);
        Assert.Equal(first.Characters.Select(item => item.Id), second.Characters.Select(item => item.Id));
        Assert.Equal(first.Actors.Select(item => item.Id), second.Actors.Select(item => item.Id));
        Assert.Equal(first.Traits.Select(item => item.Id), second.Traits.Select(item => item.Id));
        Assert.Equal(first.Interests.Select(item => item.Id), second.Interests.Select(item => item.Id));
        Assert.Equal(first.Opinions.Select(item => item.Id), second.Opinions.Select(item => item.Id));
        Assert.Equal(first.Schedules.Select(item => item.Id), second.Schedules.Select(item => item.Id));
    }

    [Fact]
    public void Create_WithSameSeedAndRuleVersionInDifferentWorlds_ScopesEveryPersistentId()
    {
        var first = CharacterCastFactory.Create(new(
            Guid.Parse("0a0bdc9a-3c91-4b76-b48c-fbf84763df72"),
            120045,
            1,
            CreatedAt));
        var second = CharacterCastFactory.Create(new(
            Guid.Parse("727fa79d-f2e1-41cf-8bfd-77d9cae17343"),
            120045,
            1,
            CreatedAt));

        Assert.Empty(first.Characters.Select(item => item.Id).Intersect(second.Characters.Select(item => item.Id)));
        Assert.Empty(first.Actors.Select(item => item.Id).Intersect(second.Actors.Select(item => item.Id)));
        Assert.Empty(first.Traits.Select(item => item.Id).Intersect(second.Traits.Select(item => item.Id)));
        Assert.Empty(first.Interests.Select(item => item.Id).Intersect(second.Interests.Select(item => item.Id)));
        Assert.Empty(first.Opinions.Select(item => item.Id).Intersect(second.Opinions.Select(item => item.Id)));
        Assert.Empty(first.Schedules.Select(item => item.Id).Intersect(second.Schedules.Select(item => item.Id)));
        Assert.Equal(
            first.Characters.Select(item => new
            {
                item.DisplayName,
                item.Handle,
                item.Age,
                item.Profession,
                item.Archetype,
                item.CurrentMoodType,
            }),
            second.Characters.Select(item => new
            {
                item.DisplayName,
                item.Handle,
                item.Age,
                item.Profession,
                item.Archetype,
                item.CurrentMoodType,
            }));
    }

    [Fact]
    public void Create_WithDifferentSeed_ProducesDifferentWorldCastIdentity()
    {
        var worldId = Guid.NewGuid();

        var first = CharacterCastFactory.Create(new(worldId, 1001, 1, CreatedAt));
        var second = CharacterCastFactory.Create(new(worldId, 1002, 1, CreatedAt));

        Assert.NotEqual(first.Characters[0].Id, second.Characters[0].Id);
        Assert.Equal(
            first.Characters.Select(item => item.DisplayName),
            second.Characters.Select(item => item.DisplayName));
    }

    [Fact]
    public void Create_ProducesDistinctActorsDetailsAndBoundedStructuredState()
    {
        var worldId = Guid.NewGuid();

        var cast = CharacterCastFactory.Create(new(worldId, 1001, 1, CreatedAt));

        Assert.Equal(10, cast.Characters.Select(item => item.Id).Distinct().Count());
        Assert.Equal(10, cast.Characters.Select(item => item.Handle).Distinct().Count());
        Assert.Equal(10, cast.Actors.Select(item => item.CharacterId).Distinct().Count());
        Assert.All(cast.Actors, actor =>
        {
            Assert.Equal(worldId, actor.WorldId);
            Assert.Equal(Domain.Worlds.ActorType.Character, actor.ActorType);
            Assert.Null(actor.PlayerProfileId);
            Assert.NotNull(actor.CharacterId);
        });
        Assert.All(cast.Traits, trait => Assert.All(
            new[]
            {
                trait.Humour, trait.Confidence, trait.Empathy, trait.Aggression,
                trait.Curiosity, trait.Honesty, trait.Sociability, trait.Ambition,
                trait.Patience, trait.Optimism, trait.Sensitivity, trait.RomanticOpenness,
            }, value => Assert.InRange(value, 0, 100)));
        Assert.All(cast.Interests, interest => Assert.InRange(interest.Strength, 0, 100));
        Assert.All(cast.Opinions, opinion =>
        {
            Assert.InRange(opinion.Position, -100, 100);
            Assert.InRange(opinion.Confidence, 0, 100);
            Assert.InRange(opinion.Intensity, 0, 100);
        });
    }

    [Fact]
    public void StructuredState_RejectsOutOfRangeValues()
    {
        var values = Enumerable.Repeat(50, 12).ToArray();
        values[7] = 101;

        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterTraits(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), values, CreatedAt));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterInterest(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "books", -1, CreatedAt));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterOpinion(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "books", -101, 50, 50, CreatedAt));
    }
}

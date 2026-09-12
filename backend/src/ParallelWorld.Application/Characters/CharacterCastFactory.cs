using System.Security.Cryptography;
using System.Text;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Application.Characters;

public static class CharacterCastFactory
{
    public const int CastSize = 10;

    public static CharacterCast Create(CharacterSeedContext context)
    {
        var characters = new List<Character>(CastSize);
        var actors = new List<Actor>(CastSize);
        var traits = new List<CharacterTraits>(CastSize);
        var interests = new List<CharacterInterest>(CastSize * 2);
        var opinions = new List<CharacterOpinion>(CastSize * 2);
        var schedules = new List<CharacterSchedule>(CastSize);

        for (var index = 0; index < Templates.Length; index++)
        {
            var template = Templates[index];
            var characterId = StableGuid(context, "character", index);
            characters.Add(new Character(
                characterId,
                context.WorldId,
                template.DisplayName,
                template.Handle,
                template.Bio,
                template.Age,
                template.Profession,
                template.Archetype,
                template.WritingStyle,
                template.ActivityLevel,
                template.Influence,
                template.Popularity,
                template.Mood,
                context.WorldCreatedAt));
            actors.Add(Actor.CreateCharacter(
                StableGuid(context, "actor", index),
                context.WorldId,
                characterId,
                context.WorldCreatedAt));
            traits.Add(new CharacterTraits(
                StableGuid(context, "traits", index),
                context.WorldId,
                characterId,
                template.Traits,
                context.WorldCreatedAt));

            for (var topicIndex = 0; topicIndex < template.Topics.Length; topicIndex++)
            {
                var topic = template.Topics[topicIndex];
                var ordinal = (index * 10) + topicIndex;
                interests.Add(new CharacterInterest(
                    StableGuid(context, "interest", ordinal),
                    context.WorldId,
                    characterId,
                    topic.Id,
                    topic.Strength,
                    context.WorldCreatedAt));
                opinions.Add(new CharacterOpinion(
                    StableGuid(context, "opinion", ordinal),
                    context.WorldId,
                    characterId,
                    topic.Id,
                    topic.Position,
                    topic.Confidence,
                    topic.Intensity,
                    context.WorldCreatedAt));
            }

            schedules.Add(new CharacterSchedule(
                StableGuid(context, "schedule", index),
                context.WorldId,
                characterId,
                template.DayOfWeek,
                template.Start,
                template.End,
                template.ScheduleActivity,
                context.WorldCreatedAt));
        }

        return new CharacterCast(characters, actors, traits, interests, opinions, schedules);
    }

    private static Guid StableGuid(CharacterSeedContext context, string category, int ordinal)
    {
        var input = Encoding.UTF8.GetBytes(FormattableString.Invariant(
            $"parallel-world:m05:{context.WorldId:N}:{context.WorldSeed}:{context.RuleVersion}:{category}:{ordinal}"));
        var bytes = SHA256.HashData(input)[..16];
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes);
    }

    private static readonly CharacterTemplate[] Templates =
    [
        new("Maya Chen", "maya", "A thoughtful designer who notices small details.", 29, "Designer", "Creative observer", "Warm and concise", 72, 48, 55, MoodType.Inspired, [68, 62, 80, 18, 88, 76, 65, 70, 72, 78, 74, 55], [new("design", 92, 74, 78, 70), new("technology", 66, 38, 62, 48)], 1, new(9, 0), new(17, 0), "Studio work"),
        new("Theo Brooks", "theo", "A patient teacher with a dry sense of humour.", 34, "Teacher", "Steady mentor", "Dry and encouraging", 58, 36, 43, MoodType.Calm, [82, 58, 86, 12, 72, 88, 74, 45, 91, 69, 52, 48], [new("education", 94, 86, 82, 76), new("books", 88, 70, 75, 65)], 1, new(8, 0), new(16, 0), "Teaching"),
        new("Amara Okafor", "amara", "An energetic chef who turns every meal into a gathering.", 31, "Chef", "Generous host", "Vivid and welcoming", 84, 51, 64, MoodType.Excited, [76, 78, 79, 24, 68, 70, 92, 73, 61, 86, 66, 63], [new("food", 98, 90, 88, 82), new("community", 85, 84, 72, 70)], 5, new(14, 0), new(22, 0), "Restaurant shift"),
        new("Luca Moretti", "luca", "A curious local journalist who always asks one more question.", 37, "Journalist", "Restless investigator", "Direct and inquisitive", 76, 63, 58, MoodType.Anxious, [55, 75, 61, 36, 96, 83, 78, 80, 47, 52, 71, 58], [new("culture", 83, 44, 80, 64), new("local-news", 96, 76, 88, 79)], 2, new(10, 0), new(18, 0), "Reporting"),
        new("Priya Shah", "priya", "A pragmatic engineer who enjoys making complicated things clear.", 28, "Engineer", "Practical builder", "Precise and upbeat", 64, 57, 52, MoodType.Happy, [49, 77, 68, 20, 91, 90, 60, 86, 83, 73, 44, 51], [new("technology", 97, 82, 90, 75), new("science", 90, 79, 84, 72)], 1, new(9, 0), new(17, 30), "Engineering work"),
        new("Elias Park", "elias", "A reflective musician who collects sounds and half-finished melodies.", 26, "Musician", "Quiet dreamer", "Lyrical and reflective", 61, 44, 60, MoodType.Tired, [71, 46, 73, 16, 84, 77, 48, 67, 75, 64, 89, 69], [new("music", 99, 91, 86, 83), new("nightlife", 64, 22, 52, 41)], 4, new(16, 0), new(23, 0), "Rehearsal"),
        new("Sofia Alvarez", "sofia", "A compassionate nurse who stays calm when the day gets difficult.", 33, "Nurse", "Dependable carer", "Gentle and practical", 55, 42, 49, MoodType.Calm, [52, 69, 96, 10, 62, 91, 76, 58, 88, 81, 78, 57], [new("health", 98, 88, 92, 86), new("community", 82, 77, 80, 67)], 3, new(7, 0), new(15, 0), "Hospital shift"),
        new("Noah Williams", "noah", "A sociable bookseller with recommendations for every mood.", 41, "Bookseller", "Friendly curator", "Conversational and witty", 69, 39, 56, MoodType.Happy, [88, 61, 84, 14, 79, 82, 90, 48, 80, 75, 59, 54], [new("books", 99, 92, 90, 84), new("culture", 79, 68, 70, 59)], 6, new(10, 0), new(18, 0), "Bookshop shift"),
        new("Hana Sato", "hana", "An ambitious architect balancing bold ideas with careful planning.", 35, "Architect", "Measured visionary", "Structured and thoughtful", 67, 59, 61, MoodType.Inspired, [43, 81, 64, 22, 87, 85, 57, 94, 78, 71, 63, 49], [new("architecture", 98, 89, 91, 80), new("design", 86, 73, 77, 68)], 1, new(8, 30), new(17, 30), "Architecture practice"),
        new("Jamal Reed", "jamal", "A confident photographer drawn to people, motion, and honest moments.", 30, "Photographer", "Social documentarian", "Bold and observant", 81, 54, 67, MoodType.Excited, [66, 84, 72, 31, 90, 74, 87, 76, 55, 79, 68, 65], [new("photography", 99, 94, 88, 85), new("travel", 83, 71, 73, 61)], 6, new(11, 0), new(19, 0), "Photo assignment"),
    ];

    private sealed record CharacterTemplate(
        string DisplayName,
        string Handle,
        string Bio,
        int Age,
        string Profession,
        string Archetype,
        string WritingStyle,
        int ActivityLevel,
        int Influence,
        int Popularity,
        MoodType Mood,
        int[] Traits,
        TopicTemplate[] Topics,
        int DayOfWeek,
        TimeOnly Start,
        TimeOnly End,
        string ScheduleActivity);

    private sealed record TopicTemplate(
        string Id,
        int Strength,
        int Position,
        int Confidence,
        int Intensity);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("characters", table =>
        {
            table.HasCheckConstraint("ck_characters_age", "age >= 0");
            table.HasCheckConstraint("ck_characters_activity_level", "activity_level BETWEEN 0 AND 100");
            table.HasCheckConstraint("ck_characters_influence", "influence BETWEEN 0 AND 100");
            table.HasCheckConstraint("ck_characters_popularity", "popularity BETWEEN 0 AND 100");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_characters");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(60);
        builder.Property(entity => entity.Handle).HasColumnName("handle").HasMaxLength(30);
        builder.Property(entity => entity.Bio).HasColumnName("bio").HasMaxLength(300);
        builder.Property(entity => entity.Age).HasColumnName("age");
        builder.Property(entity => entity.Profession).HasColumnName("profession").HasMaxLength(80);
        builder.Property(entity => entity.Archetype).HasColumnName("archetype").HasMaxLength(80);
        builder.Property(entity => entity.WritingStyle).HasColumnName("writing_style").HasMaxLength(120);
        builder.Property(entity => entity.ActivityLevel).HasColumnName("activity_level");
        builder.Property(entity => entity.Influence).HasColumnName("influence");
        builder.Property(entity => entity.Popularity).HasColumnName("popularity");
        builder.Property(entity => entity.CurrentMoodType)
            .HasColumnName("current_mood_type")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<MoodType>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<CharacterStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_characters_world_id_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.Handle })
            .IsUnique()
            .HasDatabaseName("ux_characters_world_handle");
        builder.HasIndex(entity => new { entity.WorldId, entity.Status, entity.Id })
            .HasDatabaseName("ix_characters_world_status_id");
        builder.HasOne<GameWorld>()
            .WithMany()
            .HasForeignKey(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_characters_game_worlds_world_id");
    }
}

public sealed class CharacterTraitsConfiguration : IEntityTypeConfiguration<CharacterTraits>
{
    public void Configure(EntityTypeBuilder<CharacterTraits> builder)
    {
        builder.ToTable("character_traits", table =>
        {
            foreach (var column in TraitColumns)
            {
                table.HasCheckConstraint($"ck_character_traits_{column}", $"{column} BETWEEN 0 AND 100");
            }
        });
        builder.HasKey(entity => entity.Id).HasName("pk_character_traits");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.CharacterId).HasColumnName("character_id");
        builder.Property(entity => entity.Humour).HasColumnName("humour");
        builder.Property(entity => entity.Confidence).HasColumnName("confidence");
        builder.Property(entity => entity.Empathy).HasColumnName("empathy");
        builder.Property(entity => entity.Aggression).HasColumnName("aggression");
        builder.Property(entity => entity.Curiosity).HasColumnName("curiosity");
        builder.Property(entity => entity.Honesty).HasColumnName("honesty");
        builder.Property(entity => entity.Sociability).HasColumnName("sociability");
        builder.Property(entity => entity.Ambition).HasColumnName("ambition");
        builder.Property(entity => entity.Patience).HasColumnName("patience");
        builder.Property(entity => entity.Optimism).HasColumnName("optimism");
        builder.Property(entity => entity.Sensitivity).HasColumnName("sensitivity");
        builder.Property(entity => entity.RomanticOpenness).HasColumnName("romantic_openness");
        ConfigureCharacterDetail(builder, "character_traits", true);
    }

    private static readonly string[] TraitColumns =
    [
        "humour", "confidence", "empathy", "aggression", "curiosity", "honesty",
        "sociability", "ambition", "patience", "optimism", "sensitivity", "romantic_openness",
    ];

    private static void ConfigureCharacterDetail(
        EntityTypeBuilder<CharacterTraits> builder,
        string tableName,
        bool unique)
    {
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.WorldId, entity.CharacterId })
            .IsUnique(unique)
            .HasDatabaseName($"ux_{tableName}_world_character");
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.CharacterId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName($"fk_{tableName}_characters_world_character");
    }
}

public sealed class CharacterInterestConfiguration : IEntityTypeConfiguration<CharacterInterest>
{
    public void Configure(EntityTypeBuilder<CharacterInterest> builder)
    {
        builder.ToTable("character_interests", table =>
            table.HasCheckConstraint("ck_character_interests_strength", "strength BETWEEN 0 AND 100"));
        builder.HasKey(entity => entity.Id).HasName("pk_character_interests");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.CharacterId).HasColumnName("character_id");
        builder.Property(entity => entity.TopicId).HasColumnName("topic_id").HasMaxLength(60);
        builder.Property(entity => entity.Strength).HasColumnName("strength");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.WorldId, entity.CharacterId, entity.TopicId })
            .IsUnique()
            .HasDatabaseName("ux_character_interests_world_character_topic");
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.CharacterId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_character_interests_characters_world_character");
    }
}

public sealed class CharacterOpinionConfiguration : IEntityTypeConfiguration<CharacterOpinion>
{
    public void Configure(EntityTypeBuilder<CharacterOpinion> builder)
    {
        builder.ToTable("character_opinions", table =>
        {
            table.HasCheckConstraint("ck_character_opinions_position", "position BETWEEN -100 AND 100");
            table.HasCheckConstraint("ck_character_opinions_confidence", "confidence BETWEEN 0 AND 100");
            table.HasCheckConstraint("ck_character_opinions_intensity", "intensity BETWEEN 0 AND 100");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_character_opinions");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.CharacterId).HasColumnName("character_id");
        builder.Property(entity => entity.TopicId).HasColumnName("topic_id").HasMaxLength(60);
        builder.Property(entity => entity.Position).HasColumnName("position");
        builder.Property(entity => entity.Confidence).HasColumnName("confidence");
        builder.Property(entity => entity.Intensity).HasColumnName("intensity");
        builder.Property(entity => entity.LastChangedAt).HasColumnName("last_changed_at");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.WorldId, entity.CharacterId, entity.TopicId })
            .IsUnique()
            .HasDatabaseName("ux_character_opinions_world_character_topic");
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.CharacterId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_character_opinions_characters_world_character");
    }
}

public sealed class CharacterScheduleConfiguration : IEntityTypeConfiguration<CharacterSchedule>
{
    public void Configure(EntityTypeBuilder<CharacterSchedule> builder)
    {
        builder.ToTable("character_schedules", table =>
        {
            table.HasCheckConstraint("ck_character_schedules_day", "day_of_week BETWEEN 0 AND 6");
            table.HasCheckConstraint("ck_character_schedules_time", "end_local_time > start_local_time");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_character_schedules");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.CharacterId).HasColumnName("character_id");
        builder.Property(entity => entity.DayOfWeek).HasColumnName("day_of_week");
        builder.Property(entity => entity.StartLocalTime).HasColumnName("start_local_time").HasColumnType("time without time zone");
        builder.Property(entity => entity.EndLocalTime).HasColumnName("end_local_time").HasColumnType("time without time zone");
        builder.Property(entity => entity.Activity).HasColumnName("activity").HasMaxLength(100);
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<CharacterScheduleStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.WorldId, entity.CharacterId, entity.DayOfWeek, entity.StartLocalTime })
            .HasDatabaseName("ix_character_schedules_world_character_day_start");
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.CharacterId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_character_schedules_characters_world_character");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Accounts;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class GameWorldConfiguration : IEntityTypeConfiguration<GameWorld>
{
    public void Configure(EntityTypeBuilder<GameWorld> builder)
    {
        builder.ToTable("game_worlds");
        builder.HasKey(entity => entity.Id).HasName("pk_game_worlds");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(80);
        builder.Property(entity => entity.Seed).HasColumnName("seed");
        builder.Property(entity => entity.CurrentWorldTime).HasColumnName("current_world_time");
        builder.Property(entity => entity.LastSimulatedAt).HasColumnName("last_simulated_at");
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<WorldStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.OwnerUserId, entity.Id })
            .HasName("ak_game_worlds_owner_user_id_id");
        builder.HasIndex(entity => new { entity.OwnerUserId, entity.CreatedAt, entity.Id })
            .IsDescending(false, true, true)
            .HasDatabaseName("ix_game_worlds_owner_created");
        builder.HasIndex(entity => new { entity.OwnerUserId, entity.Status })
            .HasDatabaseName("ix_game_worlds_owner_status");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_game_worlds_users_owner_user_id");
    }
}

public sealed class WorldSettingsConfiguration : IEntityTypeConfiguration<WorldSettings>
{
    public void Configure(EntityTypeBuilder<WorldSettings> builder)
    {
        builder.ToTable("world_settings", table =>
        {
            table.HasCheckConstraint("ck_world_settings_time_scale", "time_scale > 0");
            table.HasCheckConstraint("ck_world_settings_action_limit", "action_limit >= 0");
            table.HasCheckConstraint("ck_world_settings_ai_budget", "ai_budget_tokens >= 0");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_world_settings");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.TimeScale).HasColumnName("time_scale").HasPrecision(8, 4);
        builder.Property(entity => entity.ActionLimit).HasColumnName("action_limit");
        builder.Property(entity => entity.AiBudgetTokens).HasColumnName("ai_budget_tokens");
        builder.Property(entity => entity.ContentSettingsJson).HasColumnName("content_settings").HasColumnType("jsonb");
        builder.Property(entity => entity.RuleVersion).HasColumnName("rule_version");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => entity.WorldId).IsUnique().HasDatabaseName("ux_world_settings_world_id");
        builder.HasOne<GameWorld>()
            .WithOne()
            .HasForeignKey<WorldSettings>(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_world_settings_game_worlds_world_id");
    }
}

public sealed class WorldSimulationStateConfiguration : IEntityTypeConfiguration<WorldSimulationState>
{
    public void Configure(EntityTypeBuilder<WorldSimulationState> builder)
    {
        builder.ToTable("world_simulation_states", table =>
            table.HasCheckConstraint("ck_world_simulation_states_sequence", "deterministic_sequence >= 0"));
        builder.HasKey(entity => entity.Id).HasName("pk_world_simulation_states");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.NextDueAt).HasColumnName("next_due_at");
        builder.Property(entity => entity.LastCompletedIntervalEnd).HasColumnName("last_completed_interval_end");
        builder.Property(entity => entity.DeterministicSequence).HasColumnName("deterministic_sequence");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => entity.WorldId)
            .IsUnique()
            .HasDatabaseName("ux_world_simulation_states_world_id");
        builder.HasOne<GameWorld>()
            .WithOne()
            .HasForeignKey<WorldSimulationState>(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_world_simulation_states_game_worlds_world_id");
    }
}

public sealed class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("player_profiles", table =>
        {
            table.HasCheckConstraint("ck_player_profiles_reputation", "reputation BETWEEN 0 AND 100");
            table.HasCheckConstraint("ck_player_profiles_influence", "influence BETWEEN 0 AND 100");
            table.HasCheckConstraint("ck_player_profiles_followers", "followers_count >= 0");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_player_profiles");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(60);
        builder.Property(entity => entity.Handle).HasColumnName("handle").HasMaxLength(30);
        builder.Property(entity => entity.Bio).HasColumnName("bio").HasMaxLength(300);
        builder.Property(entity => entity.Reputation).HasColumnName("reputation");
        builder.Property(entity => entity.Influence).HasColumnName("influence");
        builder.Property(entity => entity.FollowersCount).HasColumnName("followers_count");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_player_profiles_world_id_id");
        builder.HasIndex(entity => entity.WorldId).IsUnique().HasDatabaseName("ux_player_profiles_world_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.Handle })
            .IsUnique()
            .HasDatabaseName("ux_player_profiles_world_handle");
        builder.HasOne<GameWorld>()
            .WithOne()
            .HasForeignKey<PlayerProfile>(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_player_profiles_game_worlds_world_id");
    }
}

public sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.ToTable("actors", table => table.HasCheckConstraint(
            "ck_actors_detail_shape",
            "(actor_type = 'player' AND player_profile_id IS NOT NULL AND character_id IS NULL) "
                + "OR (actor_type = 'character' AND player_profile_id IS NULL AND character_id IS NOT NULL) "
                + "OR (actor_type = 'system' AND player_profile_id IS NULL AND character_id IS NULL)"));
        builder.HasKey(entity => entity.Id).HasName("pk_actors");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.ActorType)
            .HasColumnName("actor_type")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<ActorType>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.PlayerProfileId).HasColumnName("player_profile_id");
        builder.Property(entity => entity.CharacterId).HasColumnName("character_id");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<ActorStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_actors_world_id_id");
        builder.HasIndex(entity => entity.WorldId)
            .IsUnique()
            .HasFilter("actor_type = 'player'")
            .HasDatabaseName("ux_actors_one_player_per_world");
        builder.HasIndex(entity => entity.PlayerProfileId)
            .IsUnique()
            .HasFilter("player_profile_id IS NOT NULL")
            .HasDatabaseName("ux_actors_player_profile_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.PlayerProfileId })
            .IsUnique()
            .HasDatabaseName("ux_actors_world_player_profile");
        builder.HasIndex(entity => entity.CharacterId)
            .IsUnique()
            .HasFilter("character_id IS NOT NULL")
            .HasDatabaseName("ux_actors_character_id");
        builder.HasOne<GameWorld>()
            .WithMany()
            .HasForeignKey(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_actors_game_worlds_world_id");
        builder.HasOne<PlayerProfile>()
            .WithOne()
            .HasForeignKey<Actor>(entity => new { entity.WorldId, entity.PlayerProfileId })
            .HasPrincipalKey<PlayerProfile>(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_actors_player_profiles_world_profile");
    }
}

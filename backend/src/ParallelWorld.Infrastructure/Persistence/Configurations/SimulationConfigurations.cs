using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class SimulationRunConfiguration : IEntityTypeConfiguration<SimulationRun>
{
    public void Configure(EntityTypeBuilder<SimulationRun> builder)
    {
        builder.ToTable("simulation_runs", table =>
        {
            table.HasCheckConstraint("ck_simulation_runs_interval", "interval_end > interval_start");
            table.HasCheckConstraint(
                "ck_simulation_runs_active_interval",
                "run_type <> 'activetick' OR interval_end = interval_start + interval '15 minutes'");
            table.HasCheckConstraint(
                "ck_simulation_runs_type",
                "run_type IN ('activetick', 'catchup')");
            table.HasCheckConstraint(
                "ck_simulation_runs_status",
                "status IN ('pending', 'running', 'partial', 'completed', 'failedretryable', 'failedterminal')");
            table.HasCheckConstraint(
                "ck_simulation_runs_processed_through",
                "processed_through >= interval_start AND processed_through <= interval_end");
            table.HasCheckConstraint(
                "ck_simulation_runs_effective_time_scale",
                "effective_time_scale BETWEEN 0.0001 AND 9999.9999");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_simulation_runs");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.RunType)
            .HasColumnName("run_type")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<SimulationRunType>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.IntervalStart).HasColumnName("interval_start");
        builder.Property(entity => entity.IntervalEnd).HasColumnName("interval_end");
        builder.Property(entity => entity.ProcessedThrough).HasColumnName("processed_through");
        builder.Property(entity => entity.EffectiveTimeScale)
            .HasColumnName("effective_time_scale")
            .HasPrecision(8, 4);
        builder.Property(entity => entity.Seed).HasColumnName("seed");
        builder.Property(entity => entity.RuleVersion).HasColumnName("rule_version");
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<SimulationRunStatus>(value, true))
            .HasMaxLength(30);
        builder.Property(entity => entity.StartedAt).HasColumnName("started_at");
        builder.Property(entity => entity.CompletedAt).HasColumnName("completed_at");
        builder.Property(entity => entity.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(entity => entity.ErrorCode).HasColumnName("error_code").HasMaxLength(80);
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_simulation_runs_world_id_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_simulation_runs_world_idempotency_key");
        builder.HasIndex(entity => new
        {
            entity.WorldId,
            entity.RuleVersion,
            entity.IntervalStart,
            entity.IntervalEnd,
        })
            .IsUnique()
            .HasDatabaseName("ux_simulation_runs_world_rule_interval");
        builder.HasIndex(entity => new { entity.WorldId, entity.Status, entity.IntervalStart })
            .HasDatabaseName("ix_simulation_runs_world_status_interval");
        builder.HasIndex(entity => new { entity.WorldId, entity.CompletedAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_simulation_runs_world_completed");
        builder.HasOne<GameWorld>()
            .WithMany()
            .HasForeignKey(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_runs_game_worlds_world_id");
    }
}

public sealed class SimulationRunCheckpointConfiguration
    : IEntityTypeConfiguration<SimulationRunCheckpoint>
{
    public void Configure(EntityTypeBuilder<SimulationRunCheckpoint> builder)
    {
        builder.ToTable("simulation_run_checkpoints", table =>
        {
            table.HasCheckConstraint("ck_simulation_run_checkpoints_bucket", "bucket_end > bucket_start");
            table.HasCheckConstraint("ck_simulation_run_checkpoints_ordinal", "stable_ordinal >= 0");
            table.HasCheckConstraint(
                "ck_simulation_run_checkpoints_status",
                "status IN ('pending', 'completed')");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_simulation_run_checkpoints");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.SimulationRunId).HasColumnName("simulation_run_id");
        builder.Property(entity => entity.BucketStart).HasColumnName("bucket_start");
        builder.Property(entity => entity.BucketEnd).HasColumnName("bucket_end");
        builder.Property(entity => entity.StableOrdinal).HasColumnName("stable_ordinal");
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<SimulationCheckpointStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.CommittedAt).HasColumnName("committed_at");
        builder.Property(entity => entity.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.WorldId, entity.SimulationRunId, entity.StableOrdinal })
            .IsUnique()
            .HasDatabaseName("ux_simulation_run_checkpoints_world_run_ordinal");
        builder.HasIndex(entity => new
        {
            entity.WorldId,
            entity.SimulationRunId,
            entity.BucketStart,
            entity.BucketEnd,
        })
            .IsUnique()
            .HasDatabaseName("ux_simulation_run_checkpoints_world_run_bucket");
        builder.HasIndex(entity => new { entity.WorldId, entity.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_simulation_run_checkpoints_world_idempotency_key");
        builder.HasIndex(entity => new { entity.WorldId, entity.SimulationRunId, entity.BucketStart })
            .HasDatabaseName("ix_simulation_run_checkpoints_world_run_bucket");
        builder.HasOne<SimulationRun>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.SimulationRunId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_run_checkpoints_runs_world_run");
    }
}

public sealed class SimulationRuleEvaluationConfiguration
    : IEntityTypeConfiguration<SimulationRuleEvaluation>
{
    public void Configure(EntityTypeBuilder<SimulationRuleEvaluation> builder)
    {
        builder.ToTable("simulation_rule_evaluations", table => table.HasCheckConstraint(
            "ck_simulation_rule_evaluations_outcome",
            "outcome IN ('unavailable', 'ineligible', 'eligible', 'executed')"));
        builder.HasKey(entity => entity.Id).HasName("pk_simulation_rule_evaluations");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.SimulationRunId).HasColumnName("simulation_run_id");
        builder.Property(entity => entity.RuleCode).HasColumnName("rule_code").HasMaxLength(30);
        builder.Property(entity => entity.Outcome)
            .HasColumnName("outcome")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<SimulationRuleOutcome>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.ReasonCode).HasColumnName("reason_code").HasMaxLength(80);
        builder.Property(entity => entity.RuleVersion).HasColumnName("rule_version");
        builder.Property(entity => entity.EvaluatedAtUtc).HasColumnName("evaluated_at_utc");
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_simulation_rule_evaluations_world_id_id");
        builder.HasIndex(entity => new { entity.SimulationRunId, entity.RuleCode })
            .IsUnique()
            .HasDatabaseName("ux_simulation_rule_evaluations_run_rule");
        builder.HasIndex(entity => new { entity.WorldId, entity.SimulationRunId, entity.RuleCode })
            .HasDatabaseName("ix_simulation_rule_evaluations_world_run_rule");
        builder.HasOne<SimulationRun>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.SimulationRunId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_rule_evaluations_runs_world_run");
    }
}

public sealed class SimulationActionConfiguration : IEntityTypeConfiguration<SimulationAction>
{
    public void Configure(EntityTypeBuilder<SimulationAction> builder)
    {
        builder.ToTable("simulation_actions", table =>
        {
            table.HasCheckConstraint("ck_simulation_actions_ordinal", "stable_ordinal >= 0");
            table.HasCheckConstraint(
                "ck_simulation_actions_distinct_actors",
                "target_actor_id IS NULL OR actor_id <> target_actor_id");
            table.HasCheckConstraint(
                "ck_simulation_actions_status",
                "status IN ('pending', 'executed', 'cancelled')");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_simulation_actions");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.SimulationRunId).HasColumnName("simulation_run_id");
        builder.Property(entity => entity.StableOrdinal).HasColumnName("stable_ordinal");
        builder.Property(entity => entity.ActorId).HasColumnName("actor_id");
        builder.Property(entity => entity.ActionType).HasColumnName("action_type").HasMaxLength(50);
        builder.Property(entity => entity.TargetActorId).HasColumnName("target_actor_id");
        builder.Property(entity => entity.TargetPostId).HasColumnName("target_post_id");
        builder.Property(entity => entity.TopicId).HasColumnName("topic_id");
        builder.Property(entity => entity.Stance).HasColumnName("stance").HasMaxLength(40);
        builder.Property(entity => entity.Tone).HasColumnName("tone").HasMaxLength(40);
        builder.Property(entity => entity.ReasonCode).HasColumnName("reason_code").HasMaxLength(80);
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<SimulationActionStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(entity => entity.ExecutedAt).HasColumnName("executed_at");
        builder.Property(entity => entity.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_simulation_actions_world_id_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_simulation_actions_world_idempotency_key");
        builder.HasIndex(entity => new { entity.WorldId, entity.SimulationRunId, entity.StableOrdinal })
            .IsUnique()
            .HasDatabaseName("ux_simulation_actions_world_run_ordinal");
        builder.HasIndex(entity => new { entity.WorldId, entity.Status, entity.ScheduledAt, entity.Id })
            .HasDatabaseName("ix_simulation_actions_world_status_scheduled_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.ActorId })
            .HasDatabaseName("ix_simulation_actions_world_actor");
        builder.HasIndex(entity => new { entity.WorldId, entity.TargetActorId })
            .HasDatabaseName("ix_simulation_actions_world_target_actor");
        builder.HasIndex(entity => new { entity.WorldId, entity.TargetPostId })
            .HasDatabaseName("ix_simulation_actions_world_target_post");
        builder.HasOne<SimulationRun>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.SimulationRunId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_actions_runs_world_run");
        builder.HasOne<Actor>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.ActorId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_actions_actors_world_actor");
        builder.HasOne<Actor>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.TargetActorId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_actions_actors_world_target");
        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.TargetPostId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_simulation_actions_posts_world_target");
    }
}

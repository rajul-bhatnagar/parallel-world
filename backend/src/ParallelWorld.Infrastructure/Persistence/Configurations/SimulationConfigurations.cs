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
            table.HasCheckConstraint(
                "ck_simulation_runs_interval_counts",
                "requested_interval_count > 0 AND processed_interval_count >= 0 "
                    + "AND remaining_interval_count >= 0 "
                    + "AND processed_interval_count + remaining_interval_count = requested_interval_count");
            table.HasCheckConstraint(
                "ck_simulation_runs_interval_count_boundaries",
                "interval_end = interval_start + requested_interval_count * interval '15 minutes' "
                    + "AND processed_through = interval_start + processed_interval_count * interval '15 minutes'");
            table.HasCheckConstraint(
                "ck_simulation_runs_lease",
                "(lease_owner_id IS NULL) = (lease_expires_at IS NULL)");
            table.HasCheckConstraint("ck_simulation_runs_attempt_count", "attempt_count >= 0");
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
        builder.Property(entity => entity.RequestedIntervalCount).HasColumnName("requested_interval_count");
        builder.Property(entity => entity.ProcessedIntervalCount).HasColumnName("processed_interval_count");
        builder.Property(entity => entity.RemainingIntervalCount).HasColumnName("remaining_interval_count");
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
        builder.Property(entity => entity.LeaseOwnerId).HasColumnName("lease_owner_id");
        builder.Property(entity => entity.LeaseExpiresAt).HasColumnName("lease_expires_at");
        builder.Property(entity => entity.AttemptCount).HasColumnName("attempt_count");
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
        builder.HasIndex(entity => new { entity.RunType, entity.Status, entity.LeaseExpiresAt })
            .HasDatabaseName("ix_simulation_runs_catchup_lease")
            .HasFilter("run_type = 'catchup' AND status IN ('running', 'partial', 'failedretryable')");
        builder.HasIndex(entity => entity.WorldId)
            .IsUnique()
            .HasDatabaseName("ux_simulation_runs_world_open_catchup")
            .HasFilter("run_type = 'catchup' AND status IN ('running', 'partial', 'failedretryable')");
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

public sealed class CatchUpSummaryConfiguration : IEntityTypeConfiguration<CatchUpSummary>
{
    public void Configure(EntityTypeBuilder<CatchUpSummary> builder)
    {
        builder.ToTable("catch_up_summaries", table =>
        {
            table.HasCheckConstraint("ck_catch_up_summaries_time", "to_game_time >= from_game_time");
            table.HasCheckConstraint("ck_catch_up_summaries_status", "status IN ('processing', 'partial', 'completed', 'failedretryable')");
        });
        builder.HasKey(x => x.Id).HasName("pk_catch_up_summaries");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorldId).HasColumnName("world_id");
        builder.Property(x => x.SimulationRunId).HasColumnName("simulation_run_id");
        builder.Property(x => x.FromGameTime).HasColumnName("from_game_time");
        builder.Property(x => x.ToGameTime).HasColumnName("to_game_time");
        builder.Property(x => x.Status).HasColumnName("status")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<CatchUpSummaryStatus>(v, true))
            .HasMaxLength(30);
        builder.Property(x => x.GeneratedAt).HasColumnName("generated_at");
        builder.Property(x => x.Text).HasColumnName("text").HasMaxLength(500);
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_catch_up_summaries_world_id_id");
        builder.HasIndex(x => new { x.WorldId, x.SimulationRunId }).IsUnique().HasDatabaseName("ux_catch_up_summaries_world_run");
        builder.HasIndex(x => new { x.WorldId, x.IdempotencyKey }).IsUnique().HasDatabaseName("ux_catch_up_summaries_world_idempotency");
        builder.HasIndex(x => new { x.WorldId, x.GeneratedAt, x.Id }).IsDescending(false, true, true).HasDatabaseName("ix_catch_up_summaries_world_generated_id");
        builder.HasOne<SimulationRun>().WithMany().HasForeignKey(x => new { x.WorldId, x.SimulationRunId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_catch_up_summaries_runs_world_run");
    }
}

public sealed class CatchUpSummaryItemConfiguration : IEntityTypeConfiguration<CatchUpSummaryItem>
{
    public void Configure(EntityTypeBuilder<CatchUpSummaryItem> builder)
    {
        builder.ToTable("catch_up_summary_items", table =>
            table.HasCheckConstraint("ck_catch_up_summary_items_ordinal", "stable_ordinal >= 0"));
        builder.HasKey(x => x.Id).HasName("pk_catch_up_summary_items");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorldId).HasColumnName("world_id");
        builder.Property(x => x.CatchUpSummaryId).HasColumnName("catch_up_summary_id");
        builder.Property(x => x.GameplayEventId).HasColumnName("gameplay_event_id");
        builder.Property(x => x.ItemType).HasColumnName("item_type").HasMaxLength(50);
        builder.Property(x => x.StableOrdinal).HasColumnName("stable_ordinal");
        builder.Property(x => x.FactCode).HasColumnName("fact_code").HasMaxLength(80);
        builder.Property(x => x.ActorId).HasColumnName("actor_id");
        builder.Property(x => x.TargetActorId).HasColumnName("target_actor_id");
        builder.Property(x => x.GameDate).HasColumnName("game_date");
        builder.Property(x => x.Wording).HasColumnName("wording").HasMaxLength(300);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.WorldId, x.CatchUpSummaryId, x.StableOrdinal }).IsUnique().HasDatabaseName("ux_catch_up_summary_items_world_summary_ordinal");
        builder.HasIndex(x => new { x.WorldId, x.CatchUpSummaryId, x.GameplayEventId, x.ItemType }).IsUnique().HasDatabaseName("ux_catch_up_summary_items_world_summary_event_type");
        builder.HasIndex(x => new { x.WorldId, x.CatchUpSummaryId, x.ItemType, x.GameDate }).IsUnique().HasDatabaseName("ux_catch_up_summary_items_world_summary_family_day");
        builder.HasOne<CatchUpSummary>().WithMany().HasForeignKey(x => new { x.WorldId, x.CatchUpSummaryId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_catch_up_summary_items_summaries_world_summary");
        builder.HasOne<GameplayEvent>().WithMany().HasForeignKey(x => new { x.WorldId, x.GameplayEventId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_catch_up_summary_items_events_world_event");
        builder.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.ActorId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_catch_up_summary_items_actors_world_actor");
        builder.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.TargetActorId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_catch_up_summary_items_actors_world_target");
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

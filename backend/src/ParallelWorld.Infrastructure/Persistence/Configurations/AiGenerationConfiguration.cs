using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Simulation;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class AiGenerationRequestConfiguration : IEntityTypeConfiguration<AiGenerationRequest>
{
    public void Configure(EntityTypeBuilder<AiGenerationRequest> builder)
    {
        builder.ToTable("ai_generation_requests", table =>
        {
            table.HasCheckConstraint(
                "ck_ai_generation_requests_status",
                "status = 'completed'");
            table.HasCheckConstraint(
                "ck_ai_generation_requests_attempt_count",
                "attempt_count BETWEEN 0 AND 2");
            table.HasCheckConstraint(
                "ck_ai_generation_requests_token_counts",
                "(prompt_token_count IS NULL OR prompt_token_count >= 0) AND "
                    + "(output_token_count IS NULL OR output_token_count >= 0)");
            table.HasCheckConstraint(
                "ck_ai_generation_requests_latency",
                "latency_milliseconds >= 0");
            table.HasCheckConstraint(
                "ck_ai_generation_requests_time",
                "completed_at_utc >= started_at_utc");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_ai_generation_requests");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.SimulationActionId).HasColumnName("simulation_action_id");
        builder.Property(entity => entity.Provider).HasColumnName("provider").HasMaxLength(40);
        builder.Property(entity => entity.Model).HasColumnName("model").HasMaxLength(100);
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<AiGenerationStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.AttemptCount).HasColumnName("attempt_count");
        builder.Property(entity => entity.InputHash).HasColumnName("input_hash").HasMaxLength(64).IsFixedLength();
        builder.Property(entity => entity.OutputHash).HasColumnName("output_hash").HasMaxLength(64).IsFixedLength();
        builder.Property(entity => entity.PromptTokenCount).HasColumnName("prompt_token_count");
        builder.Property(entity => entity.OutputTokenCount).HasColumnName("output_token_count");
        builder.Property(entity => entity.LatencyMilliseconds).HasColumnName("latency_milliseconds");
        builder.Property(entity => entity.FailureCode).HasColumnName("failure_code").HasMaxLength(80);
        builder.Property(entity => entity.FallbackUsed).HasColumnName("fallback_used");
        builder.Property(entity => entity.PromptTemplateVersion)
            .HasColumnName("prompt_template_version")
            .HasMaxLength(40);
        builder.Property(entity => entity.FinalizedText).HasColumnName("finalized_text").HasColumnType("text");
        builder.Property(entity => entity.StartedAtUtc).HasColumnName("started_at_utc");
        builder.Property(entity => entity.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(entity => entity.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.WorldId, entity.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_ai_generation_requests_world_idempotency_key");
        builder.HasIndex(entity => new
        {
            entity.WorldId,
            entity.SimulationActionId,
            entity.InputHash,
        })
            .IsUnique()
            .HasDatabaseName("ux_ai_generation_requests_world_action_input");
        builder.HasIndex(entity => new { entity.WorldId, entity.Status, entity.CompletedAtUtc })
            .HasDatabaseName("ix_ai_generation_requests_world_status_completed");
        builder.HasOne<SimulationAction>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.SimulationActionId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ai_generation_requests_actions_world_action");
    }
}

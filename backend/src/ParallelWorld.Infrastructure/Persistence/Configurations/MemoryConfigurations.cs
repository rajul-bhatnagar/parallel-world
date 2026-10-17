using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Characters;
using ParallelWorld.Domain.Memory;
using ParallelWorld.Domain.Messaging;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class CharacterMemoryConfiguration : IEntityTypeConfiguration<CharacterMemory>
{
    public void Configure(EntityTypeBuilder<CharacterMemory> b)
    {
        b.ToTable("character_memories", t =>
        {
            t.HasCheckConstraint("ck_character_memories_type", "memory_type IN ('fact','preference','event','secret','promise')");
            t.HasCheckConstraint("ck_character_memories_authority", "authority_type IN ('structuredgameplayfact','structuredplayerstatement','structuredpreference','gameplayevent','structuredsecret','structuredpromise')");
            t.HasCheckConstraint("ck_character_memories_subject", "(subject_type = 'actor' AND subject_actor_id IS NOT NULL AND subject_topic_id IS NULL) OR (subject_type = 'topic' AND subject_actor_id IS NULL AND subject_topic_id IS NOT NULL)");
            t.HasCheckConstraint("ck_character_memories_source", "(source_type = 'gameplayevent' AND gameplay_event_id = source_id AND message_id IS NULL) OR (source_type = 'message' AND message_id = source_id AND gameplay_event_id IS NULL)");
            t.HasCheckConstraint("ck_character_memories_importance", "(memory_type IN ('fact','preference') AND importance = 60) OR (memory_type = 'event' AND importance = 50) OR (memory_type IN ('secret','promise') AND importance = 90)");
            t.HasCheckConstraint("ck_character_memories_confidence", "(authority_type = 'structuredplayerstatement' AND confidence = 90) OR (authority_type = 'gameplayevent' AND confidence = 90) OR (authority_type IN ('structuredgameplayfact','structuredpreference','structuredsecret','structuredpromise') AND confidence = 100)");
            t.HasCheckConstraint("ck_character_memories_visibility", "visibility = 'characterprivate'");
            t.HasCheckConstraint("ck_character_memories_lifecycle", "(lifecycle_status = 'active' AND evicted_at_utc IS NULL) OR (lifecycle_status = 'evicted' AND evicted_at_utc IS NOT NULL)");
        });
        b.HasKey(x => x.Id).HasName("pk_character_memories");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.WorldId).HasColumnName("world_id");
        b.Property(x => x.OwnerCharacterId).HasColumnName("owner_character_id");
        EnumProperty(b.Property(x => x.MemoryType).HasColumnName("memory_type"));
        EnumProperty(b.Property(x => x.AuthorityType).HasColumnName("authority_type"));
        EnumProperty(b.Property(x => x.SubjectType).HasColumnName("subject_type"));
        b.Property(x => x.SubjectActorId).HasColumnName("subject_actor_id");
        b.Property(x => x.SubjectTopicId).HasColumnName("subject_topic_id").HasMaxLength(80);
        b.Property(x => x.TopicId).HasColumnName("topic_id").HasMaxLength(80);
        b.Property(x => x.StructuredContent).HasColumnName("structured_content").HasMaxLength(500);
        b.Property(x => x.Confidence).HasColumnName("confidence");
        b.Property(x => x.Importance).HasColumnName("importance");
        EnumProperty(b.Property(x => x.Visibility).HasColumnName("visibility"));
        EnumProperty(b.Property(x => x.LifecycleStatus).HasColumnName("lifecycle_status"));
        EnumProperty(b.Property(x => x.SourceType).HasColumnName("source_type"));
        b.Property(x => x.SourceId).HasColumnName("source_id");
        b.Property(x => x.GameplayEventId).HasColumnName("gameplay_event_id");
        b.Property(x => x.MessageId).HasColumnName("message_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.EvictedAtUtc).HasColumnName("evicted_at_utc");
        b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_character_memories_world_id_id");
        b.HasIndex(x => new { x.WorldId, x.OwnerCharacterId, x.SourceType, x.SourceId, x.MemoryType }).IsUnique().HasDatabaseName("ux_character_memories_world_owner_source_type");
        b.HasIndex(x => new { x.WorldId, x.OwnerCharacterId, x.LifecycleStatus, x.CreatedAtUtc, x.Id }).IsDescending(false, false, false, true, true).HasDatabaseName("ix_character_memories_recall");
        b.HasIndex(x => new { x.WorldId, x.OwnerCharacterId, x.LifecycleStatus, x.Importance, x.CreatedAtUtc, x.Id }).HasDatabaseName("ix_character_memories_retention");
        b.HasOne<Character>().WithMany().HasForeignKey(x => new { x.WorldId, x.OwnerCharacterId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_character_memories_characters_world_owner");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.SubjectActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_character_memories_actors_world_subject");
        b.HasOne<GameplayEvent>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.GameplayEventId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_character_memories_gameplay_events_world_source");
        b.HasOne<Message>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.MessageId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_character_memories_messages_world_source");
    }

    internal static PropertyBuilder<TEnum> EnumProperty<TEnum>(PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum =>
        property.HasConversion(
            value => value.ToString().ToLowerInvariant(),
            value => Enum.Parse<TEnum>(value, true)).HasMaxLength(40);
}

public sealed class MemoryCreationOutcomeConfiguration : IEntityTypeConfiguration<MemoryCreationOutcome>
{
    public void Configure(EntityTypeBuilder<MemoryCreationOutcome> b)
    {
        b.ToTable("memory_creation_outcomes", t =>
        {
            t.HasCheckConstraint("ck_memory_creation_outcomes_source", "(source_type = 'gameplayevent' AND gameplay_event_id = source_id AND message_id IS NULL) OR (source_type = 'message' AND message_id = source_id AND gameplay_event_id IS NULL)");
            t.HasCheckConstraint("ck_memory_creation_outcomes_shape", "(outcome = 'created' AND memory_id IS NOT NULL AND reason_code IS NULL) OR (outcome = 'rejected' AND memory_id IS NULL AND reason_code = 'memory_capacity_protected')");
        });
        b.HasKey(x => x.Id).HasName("pk_memory_creation_outcomes");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id");
        b.Property(x => x.OwnerCharacterId).HasColumnName("owner_character_id");
        CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.MemoryType).HasColumnName("memory_type"));
        CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.SourceType).HasColumnName("source_type"));
        b.Property(x => x.SourceId).HasColumnName("source_id"); b.Property(x => x.GameplayEventId).HasColumnName("gameplay_event_id"); b.Property(x => x.MessageId).HasColumnName("message_id");
        CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.Outcome).HasColumnName("outcome"));
        b.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(80); b.Property(x => x.MemoryId).HasColumnName("memory_id"); b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasIndex(x => new { x.WorldId, x.OwnerCharacterId, x.SourceType, x.SourceId, x.MemoryType }).IsUnique().HasDatabaseName("ux_memory_creation_outcomes_provenance");
        b.HasOne<Character>().WithMany().HasForeignKey(x => new { x.WorldId, x.OwnerCharacterId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_creation_outcomes_characters_world_owner");
        b.HasOne<CharacterMemory>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.MemoryId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_creation_outcomes_memories_world_memory");
        b.HasOne<GameplayEvent>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.GameplayEventId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_creation_outcomes_events_world_source");
        b.HasOne<Message>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.MessageId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_creation_outcomes_messages_world_source");
    }
}

public sealed class SecretConfiguration : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> b)
    {
        b.ToTable("secrets", t => t.HasCheckConstraint("ck_secrets_status", "status = 'active'"));
        b.HasKey(x => x.Id).HasName("pk_secrets"); b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.MemoryId).HasColumnName("memory_id"); CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.Status).HasColumnName("status")); b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_secrets_world_id_id");
        b.HasIndex(x => new { x.WorldId, x.MemoryId }).IsUnique().HasDatabaseName("ux_secrets_world_memory");
        b.HasOne<CharacterMemory>().WithMany().HasForeignKey(x => new { x.WorldId, x.MemoryId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_secrets_memories_world_memory");
    }
}

public sealed class SecretKnowerConfiguration : IEntityTypeConfiguration<SecretKnower>
{
    public void Configure(EntityTypeBuilder<SecretKnower> b)
    {
        b.ToTable("secret_knowers", t => t.HasCheckConstraint("ck_secret_knowers_status", "status = 'active'"));
        b.HasKey(x => new { x.WorldId, x.SecretId, x.CharacterId }).HasName("pk_secret_knowers"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.SecretId).HasColumnName("secret_id"); b.Property(x => x.CharacterId).HasColumnName("character_id"); CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.Status).HasColumnName("status")); b.Property(x => x.LearnedAtUtc).HasColumnName("learned_at_utc");
        b.HasIndex(x => new { x.WorldId, x.CharacterId, x.Status }).HasDatabaseName("ix_secret_knowers_world_character_status");
        b.HasOne<Secret>().WithMany().HasForeignKey(x => new { x.WorldId, x.SecretId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_secret_knowers_secrets_world_secret");
        b.HasOne<Character>().WithMany().HasForeignKey(x => new { x.WorldId, x.CharacterId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_secret_knowers_characters_world_character");
    }
}

public sealed class PromiseConfiguration : IEntityTypeConfiguration<Promise>
{
    public void Configure(EntityTypeBuilder<Promise> b)
    {
        b.ToTable("promises", t =>
        {
            t.HasCheckConstraint("ck_promises_status", "status IN ('active','fulfilled','cancelled','expired')");
            t.HasCheckConstraint("ck_promises_due_condition", "(due_condition_type = 'worldtime' AND due_at_world_time IS NOT NULL AND due_gameplay_event_id IS NULL AND due_event_type IS NULL) OR (due_condition_type = 'gameplayevent' AND due_at_world_time IS NULL AND (due_gameplay_event_id IS NOT NULL OR due_event_type IS NOT NULL))");
            t.HasCheckConstraint("ck_promises_distinct_actors", "target_actor_id IS NULL OR source_actor_id <> target_actor_id");
            t.HasCheckConstraint("ck_promises_resolution", "(status = 'active' AND resolved_at_utc IS NULL AND resolution_gameplay_event_id IS NULL) OR (status = 'fulfilled' AND resolved_at_utc IS NOT NULL AND resolution_gameplay_event_id IS NOT NULL) OR (status = 'cancelled' AND resolved_at_utc IS NOT NULL AND resolution_gameplay_event_id IS NOT NULL) OR (status = 'expired' AND resolved_at_utc IS NOT NULL AND resolution_gameplay_event_id IS NULL)");
        });
        b.HasKey(x => x.Id).HasName("pk_promises"); b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.MemoryId).HasColumnName("memory_id"); b.Property(x => x.PromiseType).HasColumnName("promise_type").HasMaxLength(80); b.Property(x => x.SourceActorId).HasColumnName("source_actor_id"); b.Property(x => x.TargetActorId).HasColumnName("target_actor_id"); CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.DueConditionType).HasColumnName("due_condition_type")); b.Property(x => x.DueAtWorldTime).HasColumnName("due_at_world_time"); b.Property(x => x.DueGameplayEventId).HasColumnName("due_gameplay_event_id"); b.Property(x => x.DueEventType).HasColumnName("due_event_type").HasMaxLength(80); CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.Status).HasColumnName("status")); b.Property(x => x.ResolutionGameplayEventId).HasColumnName("resolution_gameplay_event_id"); b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x => x.ResolvedAtUtc).HasColumnName("resolved_at_utc"); b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_promises_world_id_id"); b.HasIndex(x => new { x.WorldId, x.MemoryId }).IsUnique().HasDatabaseName("ux_promises_world_memory"); b.HasIndex(x => new { x.WorldId, x.Status, x.DueAtWorldTime }).HasFilter("status = 'active'").HasDatabaseName("ix_promises_active_due");
        b.HasOne<CharacterMemory>().WithMany().HasForeignKey(x => new { x.WorldId, x.MemoryId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_promises_memories_world_memory");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.SourceActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_promises_actors_world_source");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.TargetActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_promises_actors_world_target");
        b.HasOne<GameplayEvent>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.ResolutionGameplayEventId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_promises_events_world_resolution");
    }
}

public sealed class MemoryRecallRequestConfiguration : IEntityTypeConfiguration<MemoryRecallRequest>
{
    public void Configure(EntityTypeBuilder<MemoryRecallRequest> b)
    {
        b.ToTable("memory_recall_requests", t => t.HasCheckConstraint("ck_memory_recall_requests_subject", "(subject_type = 'actor' AND subject_actor_id IS NOT NULL AND subject_topic_id IS NULL) OR (subject_type = 'topic' AND subject_actor_id IS NULL AND subject_topic_id IS NOT NULL)"));
        b.HasKey(x => x.Id).HasName("pk_memory_recall_requests"); b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.CharacterId).HasColumnName("character_id"); CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.Purpose).HasColumnName("purpose")); CharacterMemoryConfiguration.EnumProperty(b.Property(x => x.SubjectType).HasColumnName("subject_type")); b.Property(x => x.SubjectActorId).HasColumnName("subject_actor_id"); b.Property(x => x.SubjectTopicId).HasColumnName("subject_topic_id").HasMaxLength(80); b.Property(x => x.TopicId).HasColumnName("topic_id").HasMaxLength(80); b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200); b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_memory_recall_requests_world_id_id"); b.HasIndex(x => new { x.WorldId, x.CharacterId, x.IdempotencyKey }).IsUnique().HasDatabaseName("ux_memory_recall_requests_world_character_key");
        b.HasOne<Character>().WithMany().HasForeignKey(x => new { x.WorldId, x.CharacterId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_recall_requests_characters_world_character");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.SubjectActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_recall_requests_actors_world_subject");
    }
}

public sealed class MemoryRecallSelectionConfiguration : IEntityTypeConfiguration<MemoryRecallSelection>
{
    public void Configure(EntityTypeBuilder<MemoryRecallSelection> b)
    {
        b.ToTable("memory_recall_selections", t => { t.HasCheckConstraint("ck_memory_recall_selections_rank", "rank BETWEEN 1 AND 8"); t.HasCheckConstraint("ck_memory_recall_selections_score", "score BETWEEN 0 AND 100"); });
        b.HasKey(x => new { x.WorldId, x.RequestId, x.MemoryId }).HasName("pk_memory_recall_selections"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.RequestId).HasColumnName("request_id"); b.Property(x => x.MemoryId).HasColumnName("memory_id"); b.Property(x => x.Rank).HasColumnName("rank"); b.Property(x => x.Score).HasColumnName("score"); b.Property(x => x.UsedAtUtc).HasColumnName("used_at_utc");
        b.HasIndex(x => new { x.WorldId, x.RequestId, x.Rank }).IsUnique().HasDatabaseName("ux_memory_recall_selections_request_rank");
        b.HasOne<MemoryRecallRequest>().WithMany().HasForeignKey(x => new { x.WorldId, x.RequestId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_recall_selections_requests_world_request");
        b.HasOne<CharacterMemory>().WithMany().HasForeignKey(x => new { x.WorldId, Id = x.MemoryId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_memory_recall_selections_memories_world_memory");
    }
}

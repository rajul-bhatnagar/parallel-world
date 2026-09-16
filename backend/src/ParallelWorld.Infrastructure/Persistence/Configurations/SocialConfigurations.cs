using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Accounts;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class GameplayEventConfiguration : IEntityTypeConfiguration<GameplayEvent>
{
    public void Configure(EntityTypeBuilder<GameplayEvent> builder)
    {
        builder.ToTable("gameplay_events", table =>
        {
            table.HasCheckConstraint("ck_gameplay_events_importance", "importance BETWEEN 0 AND 100");
            table.HasCheckConstraint(
                "ck_gameplay_events_emotional_impact",
                "emotional_impact BETWEEN -100 AND 100");
            table.HasCheckConstraint(
                "ck_gameplay_events_actor_target",
                "actor_id IS NULL OR target_actor_id IS NULL OR actor_id <> target_actor_id");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_gameplay_events");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.EventType).HasColumnName("event_type").HasMaxLength(50);
        builder.Property(entity => entity.ActorId).HasColumnName("actor_id");
        builder.Property(entity => entity.TargetActorId).HasColumnName("target_actor_id");
        builder.Property(entity => entity.OccurredAt).HasColumnName("occurred_at");
        builder.Property(entity => entity.Importance).HasColumnName("importance");
        builder.Property(entity => entity.EmotionalImpact).HasColumnName("emotional_impact");
        builder.Property(entity => entity.ReasonCode).HasColumnName("reason_code").HasMaxLength(80);
        builder.Property(entity => entity.RuleVersion).HasColumnName("rule_version");
        builder.Property(entity => entity.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_gameplay_events_world_id_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_gameplay_events_world_idempotency_key");
        builder.HasIndex(entity => new { entity.WorldId, entity.ActorId })
            .HasDatabaseName("ix_gameplay_events_world_actor");
        builder.HasIndex(entity => new { entity.WorldId, entity.TargetActorId })
            .HasDatabaseName("ix_gameplay_events_world_target");
        builder.HasIndex(entity => new { entity.WorldId, entity.OccurredAt, entity.Id })
            .IsDescending(false, true, true)
            .HasDatabaseName("ix_gameplay_events_world_occurred_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.EventType, entity.OccurredAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("ix_gameplay_events_world_type_occurred");
        builder.HasOne<GameWorld>()
            .WithMany()
            .HasForeignKey(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_gameplay_events_game_worlds_world_id");
        builder.HasOne<Actor>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.ActorId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_gameplay_events_actors_world_actor");
        builder.HasOne<Actor>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.TargetActorId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_gameplay_events_actors_world_target");
    }
}

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts", table =>
        {
            table.HasCheckConstraint("ck_posts_not_self_parent", "parent_post_id IS NULL OR parent_post_id <> id");
            table.HasCheckConstraint("ck_posts_like_count", "like_count >= 0");
            table.HasCheckConstraint("ck_posts_reply_count", "reply_count >= 0");
        });
        builder.HasKey(entity => entity.Id).HasName("pk_posts");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.AuthorActorId).HasColumnName("author_actor_id");
        builder.Property(entity => entity.ParentPostId).HasColumnName("parent_post_id");
        builder.Property(entity => entity.GameplayEventId).HasColumnName("gameplay_event_id");
        builder.Property(entity => entity.Content).HasColumnName("content").HasColumnType("text");
        builder.Property(entity => entity.Visibility)
            .HasColumnName("visibility")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<PostVisibility>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.LikeCount).HasColumnName("like_count");
        builder.Property(entity => entity.ReplyCount).HasColumnName("reply_count");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.DeletedAt).HasColumnName("deleted_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(entity => new { entity.WorldId, entity.Id })
            .HasName("ak_posts_world_id_id");
        builder.HasIndex(entity => new { entity.WorldId, entity.CreatedAt, entity.Id })
            .IsDescending(false, true, true)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_posts_world_created_id_active");
        builder.HasIndex(entity => new { entity.WorldId, entity.AuthorActorId, entity.CreatedAt, entity.Id })
            .IsDescending(false, false, true, true)
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_posts_world_author_created_id_active");
        builder.HasIndex(entity => new { entity.WorldId, entity.ParentPostId, entity.CreatedAt, entity.Id })
            .HasFilter("parent_post_id IS NOT NULL AND deleted_at IS NULL")
            .HasDatabaseName("ix_posts_world_parent_created_id_active");
        builder.HasIndex(entity => new { entity.WorldId, entity.GameplayEventId })
            .HasDatabaseName("ix_posts_world_gameplay_event");
        builder.HasOne<GameWorld>()
            .WithMany()
            .HasForeignKey(entity => entity.WorldId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_posts_game_worlds_world_id");
        builder.HasOne<Actor>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.AuthorActorId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_posts_actors_world_author");
        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.ParentPostId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_posts_posts_world_parent");
        builder.HasOne<GameplayEvent>()
            .WithMany()
            .HasForeignKey(entity => new { entity.WorldId, entity.GameplayEventId })
            .HasPrincipalKey(entity => new { entity.WorldId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_posts_gameplay_events_world_event");
    }
}

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records", table =>
            table.HasCheckConstraint("ck_idempotency_records_response_code", "response_code >= 100"));
        builder.HasKey(entity => entity.Id).HasName("pk_idempotency_records");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.UserId).HasColumnName("user_id");
        builder.Property(entity => entity.WorldId).HasColumnName("world_id");
        builder.Property(entity => entity.Operation).HasColumnName("operation").HasMaxLength(80);
        builder.Property(entity => entity.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
        builder.Property(entity => entity.RequestHash).HasColumnName("request_hash").HasMaxLength(64).IsFixedLength();
        builder.Property(entity => entity.Status).HasColumnName("status").HasMaxLength(20);
        builder.Property(entity => entity.ResponseCode).HasColumnName("response_code");
        builder.Property(entity => entity.ResponseResourceId).HasColumnName("response_resource_id");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => new { entity.UserId, entity.Operation, entity.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_idempotency_records_user_operation_key");
        builder.HasIndex(entity => new { entity.Status, entity.ExpiresAt })
            .HasDatabaseName("ix_idempotency_records_status_expires");
        builder.HasIndex(entity => new { entity.UserId, entity.WorldId })
            .HasDatabaseName("ix_idempotency_records_user_world");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_idempotency_records_users_user_id");
        builder.HasOne<GameWorld>()
            .WithMany()
            .HasForeignKey(entity => new { entity.UserId, entity.WorldId })
            .HasPrincipalKey(entity => new { entity.OwnerUserId, entity.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_idempotency_records_game_worlds_user_world");
    }
}

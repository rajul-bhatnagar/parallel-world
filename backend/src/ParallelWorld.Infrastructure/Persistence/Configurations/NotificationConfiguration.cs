using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Accounts;
using ParallelWorld.Domain.Notifications;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
        {
            table.HasCheckConstraint("ck_notifications_source_shape",
                "(source_type = 'gameplayevent' AND gameplay_event_id = source_id) OR "
                + "(source_type = 'catchupsummary' AND gameplay_event_id IS NULL)");
            table.HasCheckConstraint("ck_notifications_category",
                "category IN ('reply', 'privatemessage', 'catchupsummary')");
            table.HasCheckConstraint("ck_notifications_category_source",
                "(category = 'catchupsummary' AND source_type = 'catchupsummary') OR "
                + "(category IN ('reply', 'privatemessage') AND source_type = 'gameplayevent')");
            table.HasCheckConstraint("ck_notifications_priority",
                "priority IN ('normal', 'high')");
            table.HasCheckConstraint("ck_notifications_expiry",
                "expires_at IS NULL OR expires_at > created_at");
        });
        builder.HasKey(x => x.Id).HasName("pk_notifications");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorldId).HasColumnName("world_id");
        builder.Property(x => x.RecipientUserId).HasColumnName("recipient_user_id");
        builder.Property(x => x.RecipientActorId).HasColumnName("recipient_actor_id");
        builder.Property(x => x.SourceType).HasColumnName("source_type")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<NotificationSourceType>(v, true))
            .HasMaxLength(30);
        builder.Property(x => x.SourceId).HasColumnName("source_id");
        builder.Property(x => x.GameplayEventId).HasColumnName("gameplay_event_id");
        builder.Property(x => x.Category).HasColumnName("category")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<NotificationCategory>(v, true))
            .HasMaxLength(30);
        builder.Property(x => x.Priority).HasColumnName("priority")
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<NotificationPriority>(v, true))
            .HasMaxLength(20);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.ReadAt).HasColumnName("read_at");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_notifications_world_id_id");
        builder.HasIndex(x => new { x.WorldId, x.IdempotencyKey }).IsUnique()
            .HasDatabaseName("ux_notifications_world_idempotency_key");
        builder.HasIndex(x => new { x.WorldId, x.RecipientUserId, x.SourceType, x.SourceId, x.Category })
            .IsUnique().HasDatabaseName("ux_notifications_world_recipient_source_category");
        builder.HasIndex(x => new { x.WorldId, x.RecipientUserId, x.CreatedAt, x.Id })
            .IsDescending(false, false, true, true)
            .HasDatabaseName("ix_notifications_recipient_cursor");
        builder.HasIndex(x => new { x.WorldId, x.RecipientUserId, x.CreatedAt })
            .IsDescending(false, false, true)
            .HasFilter("read_at IS NULL")
            .HasDatabaseName("ix_notifications_recipient_unread");
        builder.HasOne<GameWorld>().WithMany()
            .HasForeignKey(x => new { x.RecipientUserId, x.WorldId })
            .HasPrincipalKey(x => new { x.OwnerUserId, x.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notifications_worlds_recipient_world");
        builder.HasOne<Actor>().WithMany()
            .HasForeignKey(x => new { x.WorldId, Id = x.RecipientActorId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notifications_actors_world_recipient");
        builder.HasOne<GameplayEvent>().WithMany()
            .HasForeignKey(x => new { x.WorldId, Id = x.GameplayEventId })
            .HasPrincipalKey(x => new { x.WorldId, x.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notifications_events_world_event");
    }
}

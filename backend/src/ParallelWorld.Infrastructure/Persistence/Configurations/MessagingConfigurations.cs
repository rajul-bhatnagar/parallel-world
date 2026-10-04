using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Messaging;
using ParallelWorld.Domain.Simulation;
using ParallelWorld.Domain.Social;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("conversations", t =>
        {
            t.HasCheckConstraint("ck_conversations_type", "conversation_type = 'direct'");
            t.HasCheckConstraint("ck_conversations_distinct_actors", "player_actor_id <> character_actor_id");
        });
        b.HasKey(x => x.Id).HasName("pk_conversations");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id");
        b.Property(x => x.ConversationType).HasColumnName("conversation_type").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<ConversationType>(v, true)).HasMaxLength(20);
        b.Property(x => x.PlayerActorId).HasColumnName("player_actor_id"); b.Property(x => x.CharacterActorId).HasColumnName("character_actor_id");
        b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.LastMessageAt).HasColumnName("last_message_at");
        b.Property(x => x.IsActive).HasColumnName("is_active"); b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_conversations_world_id_id");
        b.HasIndex(x => new { x.WorldId, x.PlayerActorId, x.CharacterActorId }).IsUnique().HasFilter("is_active = TRUE").HasDatabaseName("ux_conversations_world_player_character_active");
        b.HasIndex(x => new { x.WorldId, x.LastMessageAt, x.Id }).IsDescending(false, true, true).HasDatabaseName("ix_conversations_world_last_message_id");
        b.HasOne<GameWorld>().WithMany().HasForeignKey(x => x.WorldId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_conversations_game_worlds_world_id");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.PlayerActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_conversations_actors_world_player");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.CharacterActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_conversations_actors_world_character");
    }
}

public sealed class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> b)
    {
        b.ToTable("conversation_participants");
        b.HasKey(x => new { x.WorldId, x.ConversationId, x.ActorId }).HasName("pk_conversation_participants");
        b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.ConversationId).HasColumnName("conversation_id"); b.Property(x => x.ActorId).HasColumnName("actor_id");
        b.Property(x => x.JoinedAt).HasColumnName("joined_at"); b.Property(x => x.LeftAt).HasColumnName("left_at"); b.Property(x => x.LastReadMessageId).HasColumnName("last_read_message_id"); b.Property(x => x.LastReadAt).HasColumnName("last_read_at");
        b.HasOne<Conversation>().WithMany().HasForeignKey(x => new { x.WorldId, x.ConversationId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_conversation_participants_conversations_world_conversation");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.ActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_conversation_participants_actors_world_actor");
        b.HasOne<Message>().WithMany().HasForeignKey(x => new { x.WorldId, x.ConversationId, Id = x.LastReadMessageId }).HasPrincipalKey(x => new { x.WorldId, x.ConversationId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_conversation_participants_messages_read_cursor");
    }
}

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("messages", t => t.HasCheckConstraint("ck_messages_delivery_status", "delivery_status = 'delivered'"));
        b.HasKey(x => x.Id).HasName("pk_messages");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.ConversationId).HasColumnName("conversation_id"); b.Property(x => x.SenderActorId).HasColumnName("sender_actor_id");
        b.Property(x => x.GameplayEventId).HasColumnName("gameplay_event_id"); b.Property(x => x.SimulationActionId).HasColumnName("simulation_action_id"); b.Property(x => x.ClientOperationId).HasColumnName("client_operation_id");
        b.Property(x => x.Content).HasColumnName("content").HasColumnType("text"); b.Property(x => x.DeliveryStatus).HasColumnName("delivery_status").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<MessageDeliveryStatus>(v, true)).HasMaxLength(20);
        b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.EditedAt).HasColumnName("edited_at"); b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_messages_world_id_id");
        b.HasAlternateKey(x => new { x.WorldId, x.ConversationId, x.Id }).HasName("ak_messages_world_conversation_id");
        b.HasIndex(x => new { x.WorldId, x.SenderActorId, x.ClientOperationId }).IsUnique().HasFilter("client_operation_id IS NOT NULL").HasDatabaseName("ux_messages_world_sender_client_operation");
        b.HasIndex(x => new { x.WorldId, x.ConversationId, x.CreatedAt, x.Id }).IsDescending(false, false, true, true).HasDatabaseName("ix_messages_world_conversation_created_id");
        b.HasOne<Conversation>().WithMany().HasForeignKey(x => new { x.WorldId, x.ConversationId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_messages_conversations_world_conversation");
        b.HasOne<ConversationParticipant>().WithMany().HasForeignKey(x => new { x.WorldId, x.ConversationId, x.SenderActorId }).HasPrincipalKey(x => new { x.WorldId, x.ConversationId, x.ActorId }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_messages_participants_world_conversation_sender");
        b.HasOne<GameplayEvent>().WithMany().HasForeignKey(x => new { x.WorldId, x.GameplayEventId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_messages_gameplay_events_world_event");
        b.HasOne<SimulationAction>().WithMany().HasForeignKey(x => new { x.WorldId, x.SimulationActionId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_messages_simulation_actions_world_action");
    }
}

public sealed class PlannedReplyConfiguration : IEntityTypeConfiguration<PlannedReply>
{
    public void Configure(EntityTypeBuilder<PlannedReply> b)
    {
        b.ToTable("planned_replies", t =>
        {
            t.HasCheckConstraint("ck_planned_replies_urgency", "urgency BETWEEN 0 AND 100");
            t.HasCheckConstraint("ck_planned_replies_conflict_penalty", "conflict_avoidance_penalty >= 0");
            t.HasCheckConstraint("ck_planned_replies_score", "reply_score BETWEEN 0 AND 100");
            t.HasCheckConstraint("ck_planned_replies_roll", "eligibility_roll BETWEEN 0 AND 99");
            t.HasCheckConstraint("ck_planned_replies_status", "status IN ('planned','completed','fallbackcompleted','noresponse')");
        });
        b.HasKey(x => x.Id).HasName("pk_planned_replies");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id"); b.Property(x => x.ConversationId).HasColumnName("conversation_id"); b.Property(x => x.SourceMessageId).HasColumnName("source_message_id"); b.Property(x => x.RecipientActorId).HasColumnName("recipient_actor_id");
        b.Property(x => x.Urgency).HasColumnName("urgency"); b.Property(x => x.ConflictAvoidancePenalty).HasColumnName("conflict_avoidance_penalty"); b.Property(x => x.ReplyScore).HasColumnName("reply_score"); b.Property(x => x.EligibilityRoll).HasColumnName("eligibility_roll");
        b.Property(x => x.Status).HasColumnName("status").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<PlannedReplyStatus>(v, true)).HasMaxLength(30); b.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(80);
        b.Property(x => x.DueAt).HasColumnName("due_at"); b.Property(x => x.CreatedAt).HasColumnName("created_at"); b.Property(x => x.CompletedAt).HasColumnName("completed_at"); b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_planned_replies_world_id_id");
        b.HasIndex(x => new { x.WorldId, x.SourceMessageId, x.RecipientActorId }).IsUnique().HasDatabaseName("ux_planned_replies_source_recipient");
        b.HasIndex(x => new { x.Status, x.DueAt, x.Id }).HasDatabaseName("ix_planned_replies_status_due_id");
        b.HasOne<Message>().WithMany().HasForeignKey(x => new { x.WorldId, x.ConversationId, Id = x.SourceMessageId }).HasPrincipalKey(x => new { x.WorldId, x.ConversationId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_planned_replies_messages_source");
        b.HasOne<ConversationParticipant>().WithMany().HasForeignKey(x => new { x.WorldId, x.ConversationId, ActorId = x.RecipientActorId }).HasPrincipalKey(x => new { x.WorldId, x.ConversationId, x.ActorId }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_planned_replies_participants_recipient");
    }
}

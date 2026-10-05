using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Relationships;
using ParallelWorld.Domain.Worlds;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class RomanticRelationshipConfiguration : IEntityTypeConfiguration<RomanticRelationship>
{
    public void Configure(EntityTypeBuilder<RomanticRelationship> b)
    {
        b.ToTable("romantic_relationships", t =>
        {
            t.HasCheckConstraint("ck_romantic_relationships_canonical_pair", "actor_a_id < actor_b_id");
            t.HasCheckConstraint("ck_romantic_relationships_m13_status", "status IN ('none', 'romanticinterest', 'invitationpending', 'dating')");
        });
        b.HasKey(x => x.Id).HasName("pk_romantic_relationships");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id");
        b.Property(x => x.ActorAId).HasColumnName("actor_a_id"); b.Property(x => x.ActorBId).HasColumnName("actor_b_id");
        b.Property(x => x.Status).HasColumnName("status").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<RomanticStatus>(v, true)).HasMaxLength(30);
        b.Property(x => x.StatusSinceWorldTime).HasColumnName("status_since_world_time");
        b.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc"); b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_romantic_relationships_world_id_id");
        b.HasIndex(x => new { x.WorldId, x.ActorAId, x.ActorBId }).IsUnique().HasDatabaseName("ux_romantic_relationships_world_pair");
        b.HasIndex(x => new { x.WorldId, x.Status, x.ActorAId }).HasDatabaseName("ix_romantic_relationships_world_status_actor_a");
        b.HasIndex(x => new { x.WorldId, x.Status, x.ActorBId }).HasDatabaseName("ix_romantic_relationships_world_status_actor_b");
        b.HasOne<GameWorld>().WithMany().HasForeignKey(x => x.WorldId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_relationships_worlds_world_id");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.ActorAId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_relationships_actors_world_actor_a");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.ActorBId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_relationships_actors_world_actor_b");
    }
}

public sealed class RomanticInvitationConfiguration : IEntityTypeConfiguration<RomanticInvitation>
{
    public void Configure(EntityTypeBuilder<RomanticInvitation> b)
    {
        b.ToTable("romantic_invitations", t =>
        {
            t.HasCheckConstraint("ck_romantic_invitations_distinct_actors", "initiator_actor_id <> target_actor_id");
            t.HasCheckConstraint("ck_romantic_invitations_date_type", "date_type = 'CasualDate'");
            t.HasCheckConstraint("ck_romantic_invitations_scores", "initiation_score BETWEEN 0 AND 100 AND (acceptance_score IS NULL OR acceptance_score BETWEEN 0 AND 100)");
            t.HasCheckConstraint("ck_romantic_invitations_offset", "seeded_offset IS NULL OR seeded_offset BETWEEN -5 AND 5");
            t.HasCheckConstraint("ck_romantic_invitations_expiry", "expires_at_world_time = created_at_world_time + interval '24 hours'");
            t.HasCheckConstraint("ck_romantic_invitations_status", "status IN ('pending', 'accepted', 'rejected', 'expired')");
        });
        b.HasKey(x => x.Id).HasName("pk_romantic_invitations");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id");
        b.Property(x => x.RomanticRelationshipId).HasColumnName("romantic_relationship_id"); b.Property(x => x.EpisodeId).HasColumnName("episode_id");
        b.Property(x => x.InitiatorActorId).HasColumnName("initiator_actor_id"); b.Property(x => x.TargetActorId).HasColumnName("target_actor_id");
        b.Property(x => x.DateType).HasColumnName("date_type").HasMaxLength(30);
        b.Property(x => x.Status).HasColumnName("status").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<RomanticInvitationStatus>(v, true)).HasMaxLength(20);
        b.Property(x => x.InitiationScore).HasColumnName("initiation_score"); b.Property(x => x.AcceptanceScore).HasColumnName("acceptance_score");
        b.Property(x => x.SeededOffset).HasColumnName("seeded_offset"); b.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(80);
        b.Property(x => x.CommitmentApplied).HasColumnName("commitment_applied"); b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedAtWorldTime).HasColumnName("created_at_world_time"); b.Property(x => x.ExpiresAtWorldTime).HasColumnName("expires_at_world_time");
        b.Property(x => x.ResolvedAtUtc).HasColumnName("resolved_at_utc"); b.Property(x => x.ResolvedAtWorldTime).HasColumnName("resolved_at_world_time");
        b.Property(x => x.RuleVersion).HasColumnName("rule_version"); b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        b.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        b.HasAlternateKey(x => new { x.WorldId, x.Id }).HasName("ak_romantic_invitations_world_id_id");
        b.HasIndex(x => new { x.WorldId, x.IdempotencyKey }).IsUnique().HasDatabaseName("ux_romantic_invitations_world_idempotency");
        b.HasIndex(x => new { x.WorldId, x.RomanticRelationshipId }).IsUnique().HasFilter("status = 'pending'").HasDatabaseName("ux_romantic_invitations_pending_pair");
        b.HasIndex(x => new { x.WorldId, x.Status, x.ExpiresAtWorldTime, x.Id }).HasDatabaseName("ix_romantic_invitations_expiry");
        b.HasOne<RomanticRelationship>().WithMany().HasForeignKey(x => new { x.WorldId, x.RomanticRelationshipId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_invitations_relationships_world_relationship");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.InitiatorActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_invitations_actors_world_initiator");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.TargetActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_invitations_actors_world_target");
    }
}

public sealed class RomanticStatusHistoryConfiguration : IEntityTypeConfiguration<RomanticStatusHistory>
{
    public void Configure(EntityTypeBuilder<RomanticStatusHistory> b)
    {
        b.ToTable("romantic_status_history", t =>
        {
            t.HasCheckConstraint("ck_romantic_status_history_from_status", "from_status IN ('none', 'romanticinterest', 'invitationpending', 'dating', 'formerpartner')");
            t.HasCheckConstraint("ck_romantic_status_history_to_status", "to_status IN ('none', 'romanticinterest', 'invitationpending', 'dating', 'formerpartner')");
        }); b.HasKey(x => x.Id).HasName("pk_romantic_status_history");
        b.Property(x => x.Id).HasColumnName("id"); b.Property(x => x.WorldId).HasColumnName("world_id");
        b.Property(x => x.RomanticRelationshipId).HasColumnName("romantic_relationship_id"); b.Property(x => x.EpisodeId).HasColumnName("episode_id");
        b.Property(x => x.RomanticInvitationId).HasColumnName("romantic_invitation_id");
        b.Property(x => x.FromStatus).HasColumnName("from_status").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<RomanticStatus>(v, true)).HasMaxLength(30);
        b.Property(x => x.ToStatus).HasColumnName("to_status").HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<RomanticStatus>(v, true)).HasMaxLength(30);
        b.Property(x => x.InitiatorActorId).HasColumnName("initiator_actor_id"); b.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(80);
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc"); b.Property(x => x.OccurredAtWorldTime).HasColumnName("occurred_at_world_time");
        b.Property(x => x.RuleVersion).HasColumnName("rule_version"); b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
        b.HasIndex(x => new { x.WorldId, x.IdempotencyKey }).IsUnique().HasDatabaseName("ux_romantic_status_history_world_idempotency");
        b.HasIndex(x => new { x.WorldId, x.RomanticRelationshipId, x.OccurredAtWorldTime, x.Id }).IsDescending(false, false, true, true).HasDatabaseName("ix_romantic_status_history_pair_time_id");
        b.HasOne<RomanticRelationship>().WithMany().HasForeignKey(x => new { x.WorldId, x.RomanticRelationshipId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_status_history_relationships_world_relationship");
        b.HasOne<RomanticInvitation>().WithMany().HasForeignKey(x => new { x.WorldId, x.RomanticInvitationId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_status_history_invitations_world_invitation");
        b.HasOne<Actor>().WithMany().HasForeignKey(x => new { x.WorldId, x.InitiatorActorId }).HasPrincipalKey(x => new { x.WorldId, x.Id }).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_romantic_status_history_actors_world_initiator");
    }
}

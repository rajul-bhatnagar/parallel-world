using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParallelWorld.Domain.Accounts;

namespace ParallelWorld.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(entity => entity.Id).HasName("pk_users");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.AccountType)
            .HasColumnName("account_type")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<AccountType>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(320);
        builder.Property(entity => entity.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320);
        builder.Property(entity => entity.PasswordHash).HasColumnName("password_hash").HasMaxLength(512);
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion(value => value.ToString().ToLowerInvariant(), value => Enum.Parse<AccountStatus>(value, true))
            .HasMaxLength(20);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");
        builder.Property(entity => entity.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(entity => entity.NormalizedEmail)
            .IsUnique()
            .HasFilter("normalized_email IS NOT NULL")
            .HasDatabaseName("ux_users_normalized_email");
    }
}

public sealed class DeviceInstallationConfiguration : IEntityTypeConfiguration<DeviceInstallation>
{
    public void Configure(EntityTypeBuilder<DeviceInstallation> builder)
    {
        builder.ToTable("device_installations");
        builder.HasKey(entity => entity.Id).HasName("pk_device_installations");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.UserId).HasColumnName("user_id");
        builder.Property(entity => entity.InstallationPublicId)
            .HasColumnName("installation_public_id")
            .HasMaxLength(64);
        builder.Property(entity => entity.Platform).HasColumnName("platform").HasMaxLength(32);
        builder.Property(entity => entity.AppVersion).HasColumnName("app_version").HasMaxLength(32);
        builder.Property(entity => entity.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.RevokedAt).HasColumnName("revoked_at");
        builder.HasIndex(entity => entity.InstallationPublicId)
            .IsUnique()
            .HasDatabaseName("ux_device_installations_public_id");
        builder.HasAlternateKey(entity => new { entity.UserId, entity.Id })
            .HasName("ak_device_installations_user_id_id");
        builder.HasIndex(entity => new { entity.UserId, entity.LastSeenAt })
            .IsDescending(false, true)
            .HasDatabaseName("ix_device_installations_user_last_seen");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_device_installations_users_user_id");
    }
}

public sealed class GuestBootstrapOperationConfiguration : IEntityTypeConfiguration<GuestBootstrapOperation>
{
    public void Configure(EntityTypeBuilder<GuestBootstrapOperation> builder)
    {
        builder.ToTable("guest_bootstrap_operations", table =>
            table.HasCheckConstraint(
                "ck_guest_bootstrap_operations_expiry",
                "expires_at > completed_at"));
        builder.HasKey(entity => entity.Id).HasName("pk_guest_bootstrap_operations");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.ProofHash).HasColumnName("proof_hash").HasMaxLength(64);
        builder.Property(entity => entity.UserId).HasColumnName("user_id");
        builder.Property(entity => entity.DeviceInstallationId).HasColumnName("device_installation_id");
        builder.Property(entity => entity.RefreshTokenFamilyId).HasColumnName("refresh_token_family_id");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        builder.Property(entity => entity.CompletedAt).HasColumnName("completed_at");
        builder.Property(entity => entity.RecoveryConsumedAt).HasColumnName("recovery_consumed_at");
        builder.HasIndex(entity => entity.ProofHash)
            .IsUnique()
            .HasDatabaseName("ux_guest_bootstrap_operations_proof_hash");
        builder.HasIndex(entity => entity.ExpiresAt)
            .HasDatabaseName("ix_guest_bootstrap_operations_expires_at");
        builder.HasIndex(entity => new { entity.UserId, entity.DeviceInstallationId })
            .HasDatabaseName("ix_guest_bootstrap_operations_user_device");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_guest_bootstrap_operations_users_user_id");
        builder.HasOne<DeviceInstallation>()
            .WithMany()
            .HasForeignKey(entity => new { entity.UserId, entity.DeviceInstallationId })
            .HasPrincipalKey(entity => new { entity.UserId, entity.Id })
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_guest_bootstrap_operations_installations_user_device");
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", table =>
            table.HasCheckConstraint("ck_refresh_tokens_expiry", "expires_at > created_at"));
        builder.HasKey(entity => entity.Id).HasName("pk_refresh_tokens");
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.UserId).HasColumnName("user_id");
        builder.Property(entity => entity.DeviceInstallationId).HasColumnName("device_installation_id");
        builder.Property(entity => entity.TokenHash).HasColumnName("token_hash").HasMaxLength(64);
        builder.Property(entity => entity.RotationFamilyId).HasColumnName("rotation_family_id");
        builder.Property(entity => entity.ExpiresAt).HasColumnName("expires_at");
        builder.Property(entity => entity.ConsumedAt).HasColumnName("consumed_at");
        builder.Property(entity => entity.RevokedAt).HasColumnName("revoked_at");
        builder.Property(entity => entity.ReplacedByTokenId).HasColumnName("replaced_by_token_id");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(entity => entity.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_token_hash");
        builder.HasIndex(entity => new { entity.UserId, entity.DeviceInstallationId, entity.ExpiresAt })
            .HasDatabaseName("ix_refresh_tokens_user_device_expiry");
        builder.HasIndex(entity => new { entity.RotationFamilyId, entity.ExpiresAt })
            .HasDatabaseName("ix_refresh_tokens_family_expiry");
        builder.HasIndex(entity => new
        {
            entity.UserId,
            entity.RotationFamilyId,
            entity.CreatedAt,
            entity.RevokedAt,
            entity.ConsumedAt,
            entity.ExpiresAt,
        })
            .HasDatabaseName("ix_refresh_tokens_user_family_created_state");
        builder.HasIndex(entity => entity.ReplacedByTokenId)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_replaced_by_token_id");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refresh_tokens_users_user_id");
        builder.HasOne<DeviceInstallation>()
            .WithMany()
            .HasForeignKey(entity => new { entity.UserId, entity.DeviceInstallationId })
            .HasPrincipalKey(entity => new { entity.UserId, entity.Id })
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refresh_tokens_installations_user_device");
        builder.HasOne<RefreshToken>()
            .WithOne()
            .HasForeignKey<RefreshToken>(entity => entity.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_refresh_tokens_replacement");
    }
}

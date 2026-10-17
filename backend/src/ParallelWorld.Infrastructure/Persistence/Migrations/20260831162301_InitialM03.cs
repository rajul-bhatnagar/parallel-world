using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialM03 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "device_installations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                installation_public_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                app_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_device_installations", x => x.id);
                table.UniqueConstraint("ak_device_installations_user_id_id", x => new { x.user_id, x.id });
                table.ForeignKey(
                    name: "fk_device_installations_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "game_worlds",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                seed = table.Column<long>(type: "bigint", nullable: false),
                current_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_simulated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_game_worlds", x => x.id);
                table.UniqueConstraint("ak_game_worlds_owner_user_id_id", x => new { x.owner_user_id, x.id });
                table.ForeignKey(
                    name: "fk_game_worlds_users_owner_user_id",
                    column: x => x.owner_user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "guest_bootstrap_operations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                proof_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                device_installation_id = table.Column<Guid>(type: "uuid", nullable: false),
                refresh_token_family_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                recovery_consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_guest_bootstrap_operations", x => x.id);
                table.CheckConstraint("ck_guest_bootstrap_operations_expiry", "expires_at > completed_at");
                table.ForeignKey(
                    name: "fk_guest_bootstrap_operations_installations_user_device",
                    columns: x => new { x.user_id, x.device_installation_id },
                    principalTable: "device_installations",
                    principalColumns: new[] { "user_id", "id" },
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_guest_bootstrap_operations_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                device_installation_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                rotation_family_id = table.Column<Guid>(type: "uuid", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.CheckConstraint("ck_refresh_tokens_expiry", "expires_at > created_at");
                table.ForeignKey(
                    name: "fk_refresh_tokens_installations_user_device",
                    columns: x => new { x.user_id, x.device_installation_id },
                    principalTable: "device_installations",
                    principalColumns: new[] { "user_id", "id" },
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_refresh_tokens_replacement",
                    column: x => x.replaced_by_token_id,
                    principalTable: "refresh_tokens",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "player_profiles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                display_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                handle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                bio = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                reputation = table.Column<int>(type: "integer", nullable: false),
                influence = table.Column<int>(type: "integer", nullable: false),
                followers_count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_player_profiles", x => x.id);
                table.UniqueConstraint("ak_player_profiles_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_player_profiles_followers", "followers_count >= 0");
                table.CheckConstraint("ck_player_profiles_influence", "influence BETWEEN 0 AND 100");
                table.CheckConstraint("ck_player_profiles_reputation", "reputation BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_player_profiles_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "world_settings",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                time_scale = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                action_limit = table.Column<int>(type: "integer", nullable: false),
                ai_budget_tokens = table.Column<int>(type: "integer", nullable: false),
                content_settings = table.Column<string>(type: "jsonb", nullable: false),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_world_settings", x => x.id);
                table.CheckConstraint("ck_world_settings_action_limit", "action_limit >= 0");
                table.CheckConstraint("ck_world_settings_ai_budget", "ai_budget_tokens >= 0");
                table.CheckConstraint("ck_world_settings_time_scale", "time_scale > 0");
                table.ForeignKey(
                    name: "fk_world_settings_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "world_simulation_states",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                next_due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_completed_interval_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deterministic_sequence = table.Column<long>(type: "bigint", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_world_simulation_states", x => x.id);
                table.CheckConstraint("ck_world_simulation_states_sequence", "deterministic_sequence >= 0");
                table.ForeignKey(
                    name: "fk_world_simulation_states_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "actors",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                player_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                character_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_actors", x => x.id);
                table.UniqueConstraint("ak_actors_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_actors_detail_shape", "(actor_type = 'player' AND player_profile_id IS NOT NULL AND character_id IS NULL) OR (actor_type = 'character' AND player_profile_id IS NULL AND character_id IS NOT NULL) OR (actor_type = 'system' AND player_profile_id IS NULL AND character_id IS NULL)");
                table.ForeignKey(
                    name: "fk_actors_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_actors_player_profiles_world_profile",
                    columns: x => new { x.world_id, x.player_profile_id },
                    principalTable: "player_profiles",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ux_actors_character_id",
            table: "actors",
            column: "character_id",
            unique: true,
            filter: "character_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_actors_one_player_per_world",
            table: "actors",
            column: "world_id",
            unique: true,
            filter: "actor_type = 'player'");

        migrationBuilder.CreateIndex(
            name: "ux_actors_player_profile_id",
            table: "actors",
            column: "player_profile_id",
            unique: true,
            filter: "player_profile_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_actors_world_player_profile",
            table: "actors",
            columns: new[] { "world_id", "player_profile_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_device_installations_user_last_seen",
            table: "device_installations",
            columns: new[] { "user_id", "last_seen_at" },
            descending: new[] { false, true });

        migrationBuilder.CreateIndex(
            name: "ux_device_installations_public_id",
            table: "device_installations",
            column: "installation_public_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_game_worlds_owner_created",
            table: "game_worlds",
            columns: new[] { "owner_user_id", "created_at", "id" },
            descending: new[] { false, true, true });

        migrationBuilder.CreateIndex(
            name: "ix_game_worlds_owner_status",
            table: "game_worlds",
            columns: new[] { "owner_user_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_guest_bootstrap_operations_expires_at",
            table: "guest_bootstrap_operations",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "ix_guest_bootstrap_operations_user_device",
            table: "guest_bootstrap_operations",
            columns: new[] { "user_id", "device_installation_id" });

        migrationBuilder.CreateIndex(
            name: "ux_guest_bootstrap_operations_proof_hash",
            table: "guest_bootstrap_operations",
            column: "proof_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_player_profiles_world_handle",
            table: "player_profiles",
            columns: new[] { "world_id", "handle" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_player_profiles_world_id",
            table: "player_profiles",
            column: "world_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_family_expiry",
            table: "refresh_tokens",
            columns: new[] { "rotation_family_id", "expires_at" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_device_expiry",
            table: "refresh_tokens",
            columns: new[] { "user_id", "device_installation_id", "expires_at" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_family_created_state",
            table: "refresh_tokens",
            columns: new[] { "user_id", "rotation_family_id", "created_at", "revoked_at", "consumed_at", "expires_at" });

        migrationBuilder.CreateIndex(
            name: "ux_refresh_tokens_replaced_by_token_id",
            table: "refresh_tokens",
            column: "replaced_by_token_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_refresh_tokens_token_hash",
            table: "refresh_tokens",
            column: "token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_users_normalized_email",
            table: "users",
            column: "normalized_email",
            unique: true,
            filter: "normalized_email IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_world_settings_world_id",
            table: "world_settings",
            column: "world_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_world_simulation_states_world_id",
            table: "world_simulation_states",
            column: "world_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "actors");

        migrationBuilder.DropTable(
            name: "guest_bootstrap_operations");

        migrationBuilder.DropTable(
            name: "refresh_tokens");

        migrationBuilder.DropTable(
            name: "world_settings");

        migrationBuilder.DropTable(
            name: "world_simulation_states");

        migrationBuilder.DropTable(
            name: "player_profiles");

        migrationBuilder.DropTable(
            name: "device_installations");

        migrationBuilder.DropTable(
            name: "game_worlds");

        migrationBuilder.DropTable(
            name: "users");
    }
}

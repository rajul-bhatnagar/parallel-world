using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM13Dating : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "romance_enabled",
            table: "world_settings",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<string>(
            name: "romance_preference_mode",
            table: "player_profiles",
            type: "character varying(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "anyeligibleactor");

        migrationBuilder.AddColumn<string>(
            name: "romance_preference_mode",
            table: "characters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "anyeligibleactor");

        migrationBuilder.CreateTable(
            name: "romantic_relationships",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_b_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                status_since_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_romantic_relationships", x => x.id);
                table.UniqueConstraint("ak_romantic_relationships_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_romantic_relationships_canonical_pair", "actor_a_id < actor_b_id");
                table.CheckConstraint("ck_romantic_relationships_m13_status", "status IN ('none', 'romanticinterest', 'invitationpending', 'dating')");
                table.ForeignKey(
                    name: "fk_romantic_relationships_actors_world_actor_a",
                    columns: x => new { x.world_id, x.actor_a_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_romantic_relationships_actors_world_actor_b",
                    columns: x => new { x.world_id, x.actor_b_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_romantic_relationships_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "romantic_invitations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                romantic_relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                episode_id = table.Column<Guid>(type: "uuid", nullable: false),
                initiator_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                date_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                initiation_score = table.Column<int>(type: "integer", nullable: false),
                acceptance_score = table.Column<int>(type: "integer", nullable: true),
                seeded_offset = table.Column<int>(type: "integer", nullable: true),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                commitment_applied = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                resolved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                resolved_at_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_romantic_invitations", x => x.id);
                table.UniqueConstraint("ak_romantic_invitations_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_romantic_invitations_date_type", "date_type = 'CasualDate'");
                table.CheckConstraint("ck_romantic_invitations_distinct_actors", "initiator_actor_id <> target_actor_id");
                table.CheckConstraint("ck_romantic_invitations_expiry", "expires_at_world_time = created_at_world_time + interval '24 hours'");
                table.CheckConstraint("ck_romantic_invitations_offset", "seeded_offset IS NULL OR seeded_offset BETWEEN -5 AND 5");
                table.CheckConstraint("ck_romantic_invitations_scores", "initiation_score BETWEEN 0 AND 100 AND (acceptance_score IS NULL OR acceptance_score BETWEEN 0 AND 100)");
                table.CheckConstraint("ck_romantic_invitations_status", "status IN ('pending', 'accepted', 'rejected', 'expired')");
                table.ForeignKey(
                    name: "fk_romantic_invitations_actors_world_initiator",
                    columns: x => new { x.world_id, x.initiator_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_romantic_invitations_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_romantic_invitations_relationships_world_relationship",
                    columns: x => new { x.world_id, x.romantic_relationship_id },
                    principalTable: "romantic_relationships",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "romantic_status_history",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                romantic_relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                episode_id = table.Column<Guid>(type: "uuid", nullable: false),
                romantic_invitation_id = table.Column<Guid>(type: "uuid", nullable: true),
                from_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                to_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                initiator_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                occurred_at_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_romantic_status_history", x => x.id);
                table.CheckConstraint("ck_romantic_status_history_from_status", "from_status IN ('none', 'romanticinterest', 'invitationpending', 'dating', 'formerpartner')");
                table.CheckConstraint("ck_romantic_status_history_to_status", "to_status IN ('none', 'romanticinterest', 'invitationpending', 'dating', 'formerpartner')");
                table.ForeignKey(
                    name: "fk_romantic_status_history_actors_world_initiator",
                    columns: x => new { x.world_id, x.initiator_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_romantic_status_history_invitations_world_invitation",
                    columns: x => new { x.world_id, x.romantic_invitation_id },
                    principalTable: "romantic_invitations",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_romantic_status_history_relationships_world_relationship",
                    columns: x => new { x.world_id, x.romantic_relationship_id },
                    principalTable: "romantic_relationships",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.AddCheckConstraint(
            name: "ck_player_profiles_romance_preference",
            table: "player_profiles",
            sql: "romance_preference_mode IN ('disabled', 'anyeligibleactor')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_characters_romance_preference",
            table: "characters",
            sql: "romance_preference_mode IN ('disabled', 'anyeligibleactor')");

        migrationBuilder.CreateIndex(
            name: "ix_romantic_invitations_expiry",
            table: "romantic_invitations",
            columns: new[] { "world_id", "status", "expires_at_world_time", "id" });

        migrationBuilder.CreateIndex(
            name: "IX_romantic_invitations_world_id_initiator_actor_id",
            table: "romantic_invitations",
            columns: new[] { "world_id", "initiator_actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_romantic_invitations_world_id_target_actor_id",
            table: "romantic_invitations",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ux_romantic_invitations_pending_pair",
            table: "romantic_invitations",
            columns: new[] { "world_id", "romantic_relationship_id" },
            unique: true,
            filter: "status = 'pending'");

        migrationBuilder.CreateIndex(
            name: "ux_romantic_invitations_world_idempotency",
            table: "romantic_invitations",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_romantic_relationships_world_id_actor_b_id",
            table: "romantic_relationships",
            columns: new[] { "world_id", "actor_b_id" });

        migrationBuilder.CreateIndex(
            name: "ix_romantic_relationships_world_status_actor_a",
            table: "romantic_relationships",
            columns: new[] { "world_id", "status", "actor_a_id" });

        migrationBuilder.CreateIndex(
            name: "ix_romantic_relationships_world_status_actor_b",
            table: "romantic_relationships",
            columns: new[] { "world_id", "status", "actor_b_id" });

        migrationBuilder.CreateIndex(
            name: "ux_romantic_relationships_world_pair",
            table: "romantic_relationships",
            columns: new[] { "world_id", "actor_a_id", "actor_b_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_romantic_status_history_pair_time_id",
            table: "romantic_status_history",
            columns: new[] { "world_id", "romantic_relationship_id", "occurred_at_world_time", "id" },
            descending: new[] { false, false, true, true });

        migrationBuilder.CreateIndex(
            name: "IX_romantic_status_history_world_id_initiator_actor_id",
            table: "romantic_status_history",
            columns: new[] { "world_id", "initiator_actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_romantic_status_history_world_id_romantic_invitation_id",
            table: "romantic_status_history",
            columns: new[] { "world_id", "romantic_invitation_id" });

        migrationBuilder.CreateIndex(
            name: "ux_romantic_status_history_world_idempotency",
            table: "romantic_status_history",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "romantic_status_history");

        migrationBuilder.DropTable(
            name: "romantic_invitations");

        migrationBuilder.DropTable(
            name: "romantic_relationships");

        migrationBuilder.DropCheckConstraint(
            name: "ck_player_profiles_romance_preference",
            table: "player_profiles");

        migrationBuilder.DropCheckConstraint(
            name: "ck_characters_romance_preference",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "romance_enabled",
            table: "world_settings");

        migrationBuilder.DropColumn(
            name: "romance_preference_mode",
            table: "player_profiles");

        migrationBuilder.DropColumn(
            name: "romance_preference_mode",
            table: "characters");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM10RelationshipEngine : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateOnly>(
            name: "ended_game_date",
            table: "follows",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ended_game_time",
            table: "follows",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateOnly>(
            name: "started_game_date",
            table: "follows",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "started_game_time",
            table: "follows",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE follows
            SET started_game_time = started_at,
                started_game_date = (started_at AT TIME ZONE 'UTC')::date,
                ended_game_time = ended_at,
                ended_game_date = CASE WHEN ended_at IS NULL THEN NULL ELSE (ended_at AT TIME ZONE 'UTC')::date END
            """);

        migrationBuilder.AlterColumn<DateOnly>(
            name: "started_game_date",
            table: "follows",
            type: "date",
            nullable: false,
            oldClrType: typeof(DateOnly),
            oldType: "date",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "started_game_time",
            table: "follows",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.AddColumn<int>(
            name: "reputation",
            table: "characters",
            type: "integer",
            nullable: false,
            defaultValue: 50);

        migrationBuilder.CreateTable(
            name: "relationship_daily_change_ledgers",
            columns: table => new
            {
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_date = table.Column<DateOnly>(type: "date", nullable: false),
                familiarity = table.Column<int>(type: "integer", nullable: false),
                trust = table.Column<int>(type: "integer", nullable: false),
                respect = table.Column<int>(type: "integer", nullable: false),
                affection = table.Column<int>(type: "integer", nullable: false),
                comfort = table.Column<int>(type: "integer", nullable: false),
                rivalry = table.Column<int>(type: "integer", nullable: false),
                jealousy = table.Column<int>(type: "integer", nullable: false),
                attraction = table.Column<int>(type: "integer", nullable: false),
                commitment = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false),
                severe_bypass_used = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_relationship_daily_change_ledgers", x => new { x.world_id, x.source_actor_id, x.target_actor_id, x.game_date });
                table.CheckConstraint("ck_relationship_daily_ledgers_affection", "affection BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_attraction", "attraction BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_comfort", "comfort BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_commitment", "commitment BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_distinct_actors", "source_actor_id <> target_actor_id");
                table.CheckConstraint("ck_relationship_daily_ledgers_familiarity", "familiarity BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_jealousy", "jealousy BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_respect", "respect BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_rivalry", "rivalry BETWEEN 0 AND 20");
                table.CheckConstraint("ck_relationship_daily_ledgers_trust", "trust BETWEEN 0 AND 20");
                table.ForeignKey(
                    name: "fk_relationship_daily_ledgers_actors_world_source",
                    columns: x => new { x.world_id, x.source_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_relationship_daily_ledgers_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "relationships",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                familiarity = table.Column<int>(type: "integer", nullable: false),
                trust = table.Column<int>(type: "integer", nullable: false),
                respect = table.Column<int>(type: "integer", nullable: false),
                affection = table.Column<int>(type: "integer", nullable: false),
                comfort = table.Column<int>(type: "integer", nullable: false),
                rivalry = table.Column<int>(type: "integer", nullable: false),
                jealousy = table.Column<int>(type: "integer", nullable: false),
                attraction = table.Column<int>(type: "integer", nullable: false),
                commitment = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_relationships", x => x.id);
                table.UniqueConstraint("ak_relationships_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_relationships_affection", "affection BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_attraction", "attraction BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_comfort", "comfort BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_commitment", "commitment BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_distinct_actors", "source_actor_id <> target_actor_id");
                table.CheckConstraint("ck_relationships_familiarity", "familiarity BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_jealousy", "jealousy BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_respect", "respect BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_rivalry", "rivalry BETWEEN 0 AND 100");
                table.CheckConstraint("ck_relationships_trust", "trust BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_relationships_actors_world_source",
                    columns: x => new { x.world_id, x.source_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_relationships_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_relationships_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "relationship_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                base_familiarity = table.Column<int>(type: "integer", nullable: false),
                base_trust = table.Column<int>(type: "integer", nullable: false),
                base_respect = table.Column<int>(type: "integer", nullable: false),
                base_affection = table.Column<int>(type: "integer", nullable: false),
                base_comfort = table.Column<int>(type: "integer", nullable: false),
                base_rivalry = table.Column<int>(type: "integer", nullable: false),
                base_jealousy = table.Column<int>(type: "integer", nullable: false),
                base_attraction = table.Column<int>(type: "integer", nullable: false),
                base_commitment = table.Column<int>(type: "integer", nullable: false),
                familiarity_delta = table.Column<int>(type: "integer", nullable: false),
                trust_delta = table.Column<int>(type: "integer", nullable: false),
                respect_delta = table.Column<int>(type: "integer", nullable: false),
                affection_delta = table.Column<int>(type: "integer", nullable: false),
                comfort_delta = table.Column<int>(type: "integer", nullable: false),
                rivalry_delta = table.Column<int>(type: "integer", nullable: false),
                jealousy_delta = table.Column<int>(type: "integer", nullable: false),
                attraction_delta = table.Column<int>(type: "integer", nullable: false),
                commitment_delta = table.Column<int>(type: "integer", nullable: false),
                before_values = table.Column<string>(type: "jsonb", nullable: false),
                after_values = table.Column<string>(type: "jsonb", nullable: false),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                occurred_game_date = table.Column<DateOnly>(type: "date", nullable: false),
                is_qualified_negative = table.Column<bool>(type: "boolean", nullable: false),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_relationship_events", x => x.id);
                table.CheckConstraint("ck_relationship_events_affection_delta", "affection_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_attraction_delta", "attraction_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_comfort_delta", "comfort_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_commitment_delta", "commitment_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_distinct_actors", "source_actor_id <> target_actor_id");
                table.CheckConstraint("ck_relationship_events_familiarity_delta", "familiarity_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_jealousy_delta", "jealousy_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_respect_delta", "respect_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_rivalry_delta", "rivalry_delta BETWEEN -30 AND 30");
                table.CheckConstraint("ck_relationship_events_trust_delta", "trust_delta BETWEEN -30 AND 30");
                table.ForeignKey(
                    name: "fk_relationship_events_actors_world_source",
                    columns: x => new { x.world_id, x.source_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_relationship_events_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_relationship_events_gameplay_events_world_event",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_relationship_events_relationships_world_relationship",
                    columns: x => new { x.world_id, x.relationship_id },
                    principalTable: "relationships",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.AddCheckConstraint(
            name: "ck_follows_end_game_fields",
            table: "follows",
            sql: "(ended_at IS NULL AND ended_game_time IS NULL AND ended_game_date IS NULL) OR (ended_at IS NOT NULL AND ended_game_time IS NOT NULL AND ended_game_date IS NOT NULL)");

        migrationBuilder.AddCheckConstraint(
            name: "ck_follows_game_time",
            table: "follows",
            sql: "ended_game_time IS NULL OR ended_game_time >= started_game_time");

        migrationBuilder.AddCheckConstraint(
            name: "ck_characters_reputation",
            table: "characters",
            sql: "reputation BETWEEN 0 AND 100");

        migrationBuilder.CreateIndex(
            name: "IX_relationship_daily_change_ledgers_world_id_target_actor_id",
            table: "relationship_daily_change_ledgers",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_relationship_events_game_date",
            table: "relationship_events",
            columns: new[] { "world_id", "source_actor_id", "target_actor_id", "occurred_game_date" });

        migrationBuilder.CreateIndex(
            name: "ix_relationship_events_history",
            table: "relationship_events",
            columns: new[] { "world_id", "source_actor_id", "target_actor_id", "occurred_at", "id" },
            descending: new[] { false, false, false, true, true });

        migrationBuilder.CreateIndex(
            name: "IX_relationship_events_world_id_relationship_id",
            table: "relationship_events",
            columns: new[] { "world_id", "relationship_id" });

        migrationBuilder.CreateIndex(
            name: "IX_relationship_events_world_id_target_actor_id",
            table: "relationship_events",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ux_relationship_events_source_rule_direction",
            table: "relationship_events",
            columns: new[] { "world_id", "gameplay_event_id", "source_actor_id", "target_actor_id", "rule_version" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_relationship_events_world_idempotency_key",
            table: "relationship_events",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_relationships_world_id_target_actor_id",
            table: "relationships",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_relationships_world_source_updated_id",
            table: "relationships",
            columns: new[] { "world_id", "source_actor_id", "updated_at", "id" },
            descending: new[] { false, false, true, true });

        migrationBuilder.CreateIndex(
            name: "ux_relationships_world_source_target",
            table: "relationships",
            columns: new[] { "world_id", "source_actor_id", "target_actor_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "relationship_daily_change_ledgers");

        migrationBuilder.DropTable(
            name: "relationship_events");

        migrationBuilder.DropTable(
            name: "relationships");

        migrationBuilder.DropCheckConstraint(
            name: "ck_follows_end_game_fields",
            table: "follows");

        migrationBuilder.DropCheckConstraint(
            name: "ck_follows_game_time",
            table: "follows");

        migrationBuilder.DropCheckConstraint(
            name: "ck_characters_reputation",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "ended_game_date",
            table: "follows");

        migrationBuilder.DropColumn(
            name: "ended_game_time",
            table: "follows");

        migrationBuilder.DropColumn(
            name: "started_game_date",
            table: "follows");

        migrationBuilder.DropColumn(
            name: "started_game_time",
            table: "follows");

        migrationBuilder.DropColumn(
            name: "reputation",
            table: "characters");
    }
}

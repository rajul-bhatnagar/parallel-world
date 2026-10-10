using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM15CatchUp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "attempt_count",
            table: "simulation_runs",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "lease_expires_at",
            table: "simulation_runs",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "lease_owner_id",
            table: "simulation_runs",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "processed_interval_count",
            table: "simulation_runs",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "remaining_interval_count",
            table: "simulation_runs",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "requested_interval_count",
            table: "simulation_runs",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "committed_at",
            table: "simulation_run_checkpoints",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone");

        migrationBuilder.Sql(
            """
            WITH counts AS (
                SELECT id,
                       GREATEST(1, (EXTRACT(EPOCH FROM (interval_end - interval_start)) / 900)::integer) AS requested,
                       GREATEST(0, (EXTRACT(EPOCH FROM (processed_through - interval_start)) / 900)::integer) AS processed
                FROM simulation_runs
            )
            UPDATE simulation_runs AS runs
            SET requested_interval_count = counts.requested,
                processed_interval_count = LEAST(counts.requested, counts.processed),
                remaining_interval_count = counts.requested - LEAST(counts.requested, counts.processed)
            FROM counts
            WHERE runs.id = counts.id;
            """);

        migrationBuilder.CreateTable(
            name: "catch_up_summaries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                simulation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                from_game_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                to_game_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_catch_up_summaries", x => x.id);
                table.UniqueConstraint("ak_catch_up_summaries_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_catch_up_summaries_status", "status IN ('processing', 'partial', 'completed', 'failedretryable')");
                table.CheckConstraint("ck_catch_up_summaries_time", "to_game_time >= from_game_time");
                table.ForeignKey(
                    name: "fk_catch_up_summaries_runs_world_run",
                    columns: x => new { x.world_id, x.simulation_run_id },
                    principalTable: "simulation_runs",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "catch_up_summary_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                catch_up_summary_id = table.Column<Guid>(type: "uuid", nullable: false),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                item_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                stable_ordinal = table.Column<int>(type: "integer", nullable: false),
                fact_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                game_date = table.Column<DateOnly>(type: "date", nullable: false),
                wording = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_catch_up_summary_items", x => x.id);
                table.CheckConstraint("ck_catch_up_summary_items_ordinal", "stable_ordinal >= 0");
                table.ForeignKey(
                    name: "fk_catch_up_summary_items_actors_world_actor",
                    columns: x => new { x.world_id, x.actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_catch_up_summary_items_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_catch_up_summary_items_events_world_event",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_catch_up_summary_items_summaries_world_summary",
                    columns: x => new { x.world_id, x.catch_up_summary_id },
                    principalTable: "catch_up_summaries",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_simulation_runs_catchup_lease",
            table: "simulation_runs",
            columns: new[] { "run_type", "status", "lease_expires_at" },
            filter: "run_type = 'catchup' AND status IN ('running', 'partial', 'failedretryable')");

        migrationBuilder.CreateIndex(
            name: "ux_simulation_runs_world_open_catchup",
            table: "simulation_runs",
            column: "world_id",
            unique: true,
            filter: "run_type = 'catchup' AND status IN ('running', 'partial', 'failedretryable')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_simulation_runs_attempt_count",
            table: "simulation_runs",
            sql: "attempt_count >= 0");

        migrationBuilder.AddCheckConstraint(
            name: "ck_simulation_runs_interval_counts",
            table: "simulation_runs",
            sql: "requested_interval_count > 0 AND processed_interval_count >= 0 AND remaining_interval_count >= 0 AND processed_interval_count + remaining_interval_count = requested_interval_count");

        migrationBuilder.AddCheckConstraint(
            name: "ck_simulation_runs_interval_count_boundaries",
            table: "simulation_runs",
            sql: "interval_end = interval_start + requested_interval_count * interval '15 minutes' AND processed_through = interval_start + processed_interval_count * interval '15 minutes'");

        migrationBuilder.AddCheckConstraint(
            name: "ck_simulation_runs_lease",
            table: "simulation_runs",
            sql: "(lease_owner_id IS NULL) = (lease_expires_at IS NULL)");

        migrationBuilder.CreateIndex(
            name: "ix_catch_up_summaries_world_generated_id",
            table: "catch_up_summaries",
            columns: new[] { "world_id", "generated_at", "id" },
            descending: new[] { false, true, true });

        migrationBuilder.CreateIndex(
            name: "ux_catch_up_summaries_world_idempotency",
            table: "catch_up_summaries",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_catch_up_summaries_world_run",
            table: "catch_up_summaries",
            columns: new[] { "world_id", "simulation_run_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_catch_up_summary_items_world_id_actor_id",
            table: "catch_up_summary_items",
            columns: new[] { "world_id", "actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_catch_up_summary_items_world_id_gameplay_event_id",
            table: "catch_up_summary_items",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "IX_catch_up_summary_items_world_id_target_actor_id",
            table: "catch_up_summary_items",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ux_catch_up_summary_items_world_summary_event_type",
            table: "catch_up_summary_items",
            columns: new[] { "world_id", "catch_up_summary_id", "gameplay_event_id", "item_type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_catch_up_summary_items_world_summary_family_day",
            table: "catch_up_summary_items",
            columns: new[] { "world_id", "catch_up_summary_id", "item_type", "game_date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_catch_up_summary_items_world_summary_ordinal",
            table: "catch_up_summary_items",
            columns: new[] { "world_id", "catch_up_summary_id", "stable_ordinal" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "catch_up_summary_items");

        migrationBuilder.DropTable(
            name: "catch_up_summaries");

        migrationBuilder.DropIndex(
            name: "ix_simulation_runs_catchup_lease",
            table: "simulation_runs");

        migrationBuilder.DropIndex(
            name: "ux_simulation_runs_world_open_catchup",
            table: "simulation_runs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_simulation_runs_attempt_count",
            table: "simulation_runs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_simulation_runs_interval_counts",
            table: "simulation_runs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_simulation_runs_interval_count_boundaries",
            table: "simulation_runs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_simulation_runs_lease",
            table: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "attempt_count",
            table: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "lease_expires_at",
            table: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "lease_owner_id",
            table: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "processed_interval_count",
            table: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "remaining_interval_count",
            table: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "requested_interval_count",
            table: "simulation_runs");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "committed_at",
            table: "simulation_run_checkpoints",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone",
            oldNullable: true);
    }
}

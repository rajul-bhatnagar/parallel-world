using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM08RuleBasedSimulation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "display_time_zone_id",
            table: "world_settings",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "UTC");

        migrationBuilder.Sql(
            """
            UPDATE world_simulation_states AS state
            SET next_due_at = world.created_at + interval '15 minutes'
            FROM game_worlds AS world
            WHERE state.world_id = world.id
              AND state.last_completed_interval_end IS NULL
              AND state.next_due_at = world.created_at;
            """);

        migrationBuilder.CreateTable(
            name: "simulation_runs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                run_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                interval_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                interval_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                processed_through = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                effective_time_scale = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                seed = table.Column<long>(type: "bigint", nullable: false),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                error_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_simulation_runs", x => x.id);
                table.UniqueConstraint("ak_simulation_runs_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_simulation_runs_active_interval", "run_type <> 'activetick' OR interval_end = interval_start + interval '15 minutes'");
                table.CheckConstraint("ck_simulation_runs_effective_time_scale", "effective_time_scale BETWEEN 0.0001 AND 9999.9999");
                table.CheckConstraint("ck_simulation_runs_interval", "interval_end > interval_start");
                table.CheckConstraint("ck_simulation_runs_processed_through", "processed_through >= interval_start AND processed_through <= interval_end");
                table.CheckConstraint("ck_simulation_runs_status", "status IN ('pending', 'running', 'partial', 'completed', 'failedretryable', 'failedterminal')");
                table.CheckConstraint("ck_simulation_runs_type", "run_type IN ('activetick', 'catchup')");
                table.ForeignKey(
                    name: "fk_simulation_runs_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "simulation_actions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                simulation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                stable_ordinal = table.Column<int>(type: "integer", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                action_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                target_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                topic_id = table.Column<Guid>(type: "uuid", nullable: true),
                stance = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                tone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_simulation_actions", x => x.id);
                table.UniqueConstraint("ak_simulation_actions_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_simulation_actions_distinct_actors", "target_actor_id IS NULL OR actor_id <> target_actor_id");
                table.CheckConstraint("ck_simulation_actions_ordinal", "stable_ordinal >= 0");
                table.CheckConstraint("ck_simulation_actions_status", "status IN ('pending', 'executed', 'cancelled')");
                table.ForeignKey(
                    name: "fk_simulation_actions_actors_world_actor",
                    columns: x => new { x.world_id, x.actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_simulation_actions_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_simulation_actions_posts_world_target",
                    columns: x => new { x.world_id, x.target_post_id },
                    principalTable: "posts",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_simulation_actions_runs_world_run",
                    columns: x => new { x.world_id, x.simulation_run_id },
                    principalTable: "simulation_runs",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "simulation_rule_evaluations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                simulation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                rule_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                evaluated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_simulation_rule_evaluations", x => x.id);
                table.UniqueConstraint("ak_simulation_rule_evaluations_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_simulation_rule_evaluations_outcome", "outcome IN ('unavailable', 'ineligible', 'eligible', 'executed')");
                table.ForeignKey(
                    name: "fk_simulation_rule_evaluations_runs_world_run",
                    columns: x => new { x.world_id, x.simulation_run_id },
                    principalTable: "simulation_runs",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "simulation_run_checkpoints",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                simulation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                bucket_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                stable_ordinal = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                committed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_simulation_run_checkpoints", x => x.id);
                table.CheckConstraint("ck_simulation_run_checkpoints_bucket", "bucket_end > bucket_start");
                table.CheckConstraint("ck_simulation_run_checkpoints_ordinal", "stable_ordinal >= 0");
                table.CheckConstraint("ck_simulation_run_checkpoints_status", "status IN ('pending', 'completed')");
                table.ForeignKey(
                    name: "fk_simulation_run_checkpoints_runs_world_run",
                    columns: x => new { x.world_id, x.simulation_run_id },
                    principalTable: "simulation_runs",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_simulation_actions_world_actor",
            table: "simulation_actions",
            columns: new[] { "world_id", "actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_simulation_actions_world_status_scheduled_id",
            table: "simulation_actions",
            columns: new[] { "world_id", "status", "scheduled_at", "id" });

        migrationBuilder.CreateIndex(
            name: "ix_simulation_actions_world_target_actor",
            table: "simulation_actions",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_simulation_actions_world_target_post",
            table: "simulation_actions",
            columns: new[] { "world_id", "target_post_id" });

        migrationBuilder.CreateIndex(
            name: "ux_simulation_actions_world_idempotency_key",
            table: "simulation_actions",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_simulation_actions_world_run_ordinal",
            table: "simulation_actions",
            columns: new[] { "world_id", "simulation_run_id", "stable_ordinal" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_simulation_rule_evaluations_world_run_rule",
            table: "simulation_rule_evaluations",
            columns: new[] { "world_id", "simulation_run_id", "rule_code" });

        migrationBuilder.CreateIndex(
            name: "ux_simulation_rule_evaluations_run_rule",
            table: "simulation_rule_evaluations",
            columns: new[] { "simulation_run_id", "rule_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_simulation_run_checkpoints_world_run_bucket",
            table: "simulation_run_checkpoints",
            columns: new[] { "world_id", "simulation_run_id", "bucket_start" });

        migrationBuilder.CreateIndex(
            name: "ux_simulation_run_checkpoints_world_idempotency_key",
            table: "simulation_run_checkpoints",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_simulation_run_checkpoints_world_run_bucket",
            table: "simulation_run_checkpoints",
            columns: new[] { "world_id", "simulation_run_id", "bucket_start", "bucket_end" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_simulation_run_checkpoints_world_run_ordinal",
            table: "simulation_run_checkpoints",
            columns: new[] { "world_id", "simulation_run_id", "stable_ordinal" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_simulation_runs_world_completed",
            table: "simulation_runs",
            columns: new[] { "world_id", "completed_at" },
            descending: new[] { false, true });

        migrationBuilder.CreateIndex(
            name: "ix_simulation_runs_world_status_interval",
            table: "simulation_runs",
            columns: new[] { "world_id", "status", "interval_start" });

        migrationBuilder.CreateIndex(
            name: "ux_simulation_runs_world_idempotency_key",
            table: "simulation_runs",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_simulation_runs_world_rule_interval",
            table: "simulation_runs",
            columns: new[] { "world_id", "rule_version", "interval_start", "interval_end" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "simulation_actions");

        migrationBuilder.DropTable(
            name: "simulation_rule_evaluations");

        migrationBuilder.DropTable(
            name: "simulation_run_checkpoints");

        migrationBuilder.DropTable(
            name: "simulation_runs");

        migrationBuilder.DropColumn(
            name: "display_time_zone_id",
            table: "world_settings");
    }
}

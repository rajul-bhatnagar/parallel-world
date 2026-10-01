using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM09AiTextGeneration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ai_generation_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                simulation_action_id = table.Column<Guid>(type: "uuid", nullable: false),
                provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                attempt_count = table.Column<int>(type: "integer", nullable: false),
                input_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                output_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                prompt_token_count = table.Column<int>(type: "integer", nullable: true),
                output_token_count = table.Column<int>(type: "integer", nullable: true),
                latency_milliseconds = table.Column<long>(type: "bigint", nullable: false),
                failure_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                fallback_used = table.Column<bool>(type: "boolean", nullable: false),
                prompt_template_version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                finalized_text = table.Column<string>(type: "text", nullable: false),
                started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ai_generation_requests", x => x.id);
                table.CheckConstraint("ck_ai_generation_requests_attempt_count", "attempt_count BETWEEN 0 AND 2");
                table.CheckConstraint("ck_ai_generation_requests_latency", "latency_milliseconds >= 0");
                table.CheckConstraint("ck_ai_generation_requests_status", "status = 'completed'");
                table.CheckConstraint("ck_ai_generation_requests_time", "completed_at_utc >= started_at_utc");
                table.CheckConstraint("ck_ai_generation_requests_token_counts", "(prompt_token_count IS NULL OR prompt_token_count >= 0) AND (output_token_count IS NULL OR output_token_count >= 0)");
                table.ForeignKey(
                    name: "fk_ai_generation_requests_actions_world_action",
                    columns: x => new { x.world_id, x.simulation_action_id },
                    principalTable: "simulation_actions",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_ai_generation_requests_world_status_completed",
            table: "ai_generation_requests",
            columns: new[] { "world_id", "status", "completed_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ux_ai_generation_requests_world_action_input",
            table: "ai_generation_requests",
            columns: new[] { "world_id", "simulation_action_id", "input_hash" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_ai_generation_requests_world_idempotency_key",
            table: "ai_generation_requests",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ai_generation_requests");
    }
}

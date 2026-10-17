using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM12LongTermMemory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "character_memories",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_character_id = table.Column<Guid>(type: "uuid", nullable: false),
                memory_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                authority_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                subject_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                subject_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                subject_topic_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                topic_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                structured_content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                confidence = table.Column<int>(type: "integer", nullable: false),
                importance = table.Column<int>(type: "integer", nullable: false),
                visibility = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                lifecycle_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                source_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                source_id = table.Column<Guid>(type: "uuid", nullable: false),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                message_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                evicted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_character_memories", x => x.id);
                table.UniqueConstraint("ak_character_memories_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_character_memories_authority", "authority_type IN ('structuredgameplayfact','structuredplayerstatement','structuredpreference','gameplayevent','structuredsecret','structuredpromise')");
                table.CheckConstraint("ck_character_memories_confidence", "(authority_type = 'structuredplayerstatement' AND confidence = 90) OR (authority_type = 'gameplayevent' AND confidence = 90) OR (authority_type IN ('structuredgameplayfact','structuredpreference','structuredsecret','structuredpromise') AND confidence = 100)");
                table.CheckConstraint("ck_character_memories_importance", "(memory_type IN ('fact','preference') AND importance = 60) OR (memory_type = 'event' AND importance = 50) OR (memory_type IN ('secret','promise') AND importance = 90)");
                table.CheckConstraint("ck_character_memories_lifecycle", "(lifecycle_status = 'active' AND evicted_at_utc IS NULL) OR (lifecycle_status = 'evicted' AND evicted_at_utc IS NOT NULL)");
                table.CheckConstraint("ck_character_memories_source", "(source_type = 'gameplayevent' AND gameplay_event_id = source_id AND message_id IS NULL) OR (source_type = 'message' AND message_id = source_id AND gameplay_event_id IS NULL)");
                table.CheckConstraint("ck_character_memories_subject", "(subject_type = 'actor' AND subject_actor_id IS NOT NULL AND subject_topic_id IS NULL) OR (subject_type = 'topic' AND subject_actor_id IS NULL AND subject_topic_id IS NOT NULL)");
                table.CheckConstraint("ck_character_memories_type", "memory_type IN ('fact','preference','event','secret','promise')");
                table.CheckConstraint("ck_character_memories_visibility", "visibility = 'characterprivate'");
                table.ForeignKey(
                    name: "fk_character_memories_actors_world_subject",
                    columns: x => new { x.world_id, x.subject_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_character_memories_characters_world_owner",
                    columns: x => new { x.world_id, x.owner_character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_character_memories_gameplay_events_world_source",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_character_memories_messages_world_source",
                    columns: x => new { x.world_id, x.message_id },
                    principalTable: "messages",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "memory_recall_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_id = table.Column<Guid>(type: "uuid", nullable: false),
                purpose = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                subject_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                subject_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                subject_topic_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                topic_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_memory_recall_requests", x => x.id);
                table.UniqueConstraint("ak_memory_recall_requests_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_memory_recall_requests_subject", "(subject_type = 'actor' AND subject_actor_id IS NOT NULL AND subject_topic_id IS NULL) OR (subject_type = 'topic' AND subject_actor_id IS NULL AND subject_topic_id IS NOT NULL)");
                table.ForeignKey(
                    name: "fk_memory_recall_requests_actors_world_subject",
                    columns: x => new { x.world_id, x.subject_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_memory_recall_requests_characters_world_character",
                    columns: x => new { x.world_id, x.character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "memory_creation_outcomes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_character_id = table.Column<Guid>(type: "uuid", nullable: false),
                memory_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                source_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                source_id = table.Column<Guid>(type: "uuid", nullable: false),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                message_id = table.Column<Guid>(type: "uuid", nullable: true),
                outcome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                memory_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_memory_creation_outcomes", x => x.id);
                table.CheckConstraint("ck_memory_creation_outcomes_shape", "(outcome = 'created' AND memory_id IS NOT NULL AND reason_code IS NULL) OR (outcome = 'rejected' AND memory_id IS NULL AND reason_code = 'memory_capacity_protected')");
                table.CheckConstraint("ck_memory_creation_outcomes_source", "(source_type = 'gameplayevent' AND gameplay_event_id = source_id AND message_id IS NULL) OR (source_type = 'message' AND message_id = source_id AND gameplay_event_id IS NULL)");
                table.ForeignKey(
                    name: "fk_memory_creation_outcomes_characters_world_owner",
                    columns: x => new { x.world_id, x.owner_character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_memory_creation_outcomes_events_world_source",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_memory_creation_outcomes_memories_world_memory",
                    columns: x => new { x.world_id, x.memory_id },
                    principalTable: "character_memories",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_memory_creation_outcomes_messages_world_source",
                    columns: x => new { x.world_id, x.message_id },
                    principalTable: "messages",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "promises",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                memory_id = table.Column<Guid>(type: "uuid", nullable: false),
                promise_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                source_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                due_condition_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                due_at_world_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                due_gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                due_event_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                resolution_gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                resolved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_promises", x => x.id);
                table.UniqueConstraint("ak_promises_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_promises_distinct_actors", "target_actor_id IS NULL OR source_actor_id <> target_actor_id");
                table.CheckConstraint("ck_promises_due_condition", "(due_condition_type = 'worldtime' AND due_at_world_time IS NOT NULL AND due_gameplay_event_id IS NULL AND due_event_type IS NULL) OR (due_condition_type = 'gameplayevent' AND due_at_world_time IS NULL AND (due_gameplay_event_id IS NOT NULL OR due_event_type IS NOT NULL))");
                table.CheckConstraint("ck_promises_resolution", "(status = 'active' AND resolved_at_utc IS NULL AND resolution_gameplay_event_id IS NULL) OR (status = 'fulfilled' AND resolved_at_utc IS NOT NULL AND resolution_gameplay_event_id IS NOT NULL) OR (status = 'cancelled' AND resolved_at_utc IS NOT NULL AND resolution_gameplay_event_id IS NOT NULL) OR (status = 'expired' AND resolved_at_utc IS NOT NULL AND resolution_gameplay_event_id IS NULL)");
                table.CheckConstraint("ck_promises_status", "status IN ('active','fulfilled','cancelled','expired')");
                table.ForeignKey(
                    name: "fk_promises_actors_world_source",
                    columns: x => new { x.world_id, x.source_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_promises_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_promises_events_world_resolution",
                    columns: x => new { x.world_id, x.resolution_gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_promises_memories_world_memory",
                    columns: x => new { x.world_id, x.memory_id },
                    principalTable: "character_memories",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "secrets",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                memory_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_secrets", x => x.id);
                table.UniqueConstraint("ak_secrets_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_secrets_status", "status = 'active'");
                table.ForeignKey(
                    name: "fk_secrets_memories_world_memory",
                    columns: x => new { x.world_id, x.memory_id },
                    principalTable: "character_memories",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "memory_recall_selections",
            columns: table => new
            {
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                request_id = table.Column<Guid>(type: "uuid", nullable: false),
                memory_id = table.Column<Guid>(type: "uuid", nullable: false),
                rank = table.Column<int>(type: "integer", nullable: false),
                score = table.Column<int>(type: "integer", nullable: false),
                used_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_memory_recall_selections", x => new { x.world_id, x.request_id, x.memory_id });
                table.CheckConstraint("ck_memory_recall_selections_rank", "rank BETWEEN 1 AND 8");
                table.CheckConstraint("ck_memory_recall_selections_score", "score BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_memory_recall_selections_memories_world_memory",
                    columns: x => new { x.world_id, x.memory_id },
                    principalTable: "character_memories",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_memory_recall_selections_requests_world_request",
                    columns: x => new { x.world_id, x.request_id },
                    principalTable: "memory_recall_requests",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "secret_knowers",
            columns: table => new
            {
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                secret_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                learned_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_secret_knowers", x => new { x.world_id, x.secret_id, x.character_id });
                table.CheckConstraint("ck_secret_knowers_status", "status = 'active'");
                table.ForeignKey(
                    name: "fk_secret_knowers_characters_world_character",
                    columns: x => new { x.world_id, x.character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_secret_knowers_secrets_world_secret",
                    columns: x => new { x.world_id, x.secret_id },
                    principalTable: "secrets",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_character_memories_recall",
            table: "character_memories",
            columns: new[] { "world_id", "owner_character_id", "lifecycle_status", "created_at_utc", "id" },
            descending: new[] { false, false, false, true, true });

        migrationBuilder.CreateIndex(
            name: "ix_character_memories_retention",
            table: "character_memories",
            columns: new[] { "world_id", "owner_character_id", "lifecycle_status", "importance", "created_at_utc", "id" });

        migrationBuilder.CreateIndex(
            name: "IX_character_memories_world_id_gameplay_event_id",
            table: "character_memories",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "IX_character_memories_world_id_message_id",
            table: "character_memories",
            columns: new[] { "world_id", "message_id" });

        migrationBuilder.CreateIndex(
            name: "IX_character_memories_world_id_subject_actor_id",
            table: "character_memories",
            columns: new[] { "world_id", "subject_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ux_character_memories_world_owner_source_type",
            table: "character_memories",
            columns: new[] { "world_id", "owner_character_id", "source_type", "source_id", "memory_type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_memory_creation_outcomes_world_id_gameplay_event_id",
            table: "memory_creation_outcomes",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "IX_memory_creation_outcomes_world_id_memory_id",
            table: "memory_creation_outcomes",
            columns: new[] { "world_id", "memory_id" });

        migrationBuilder.CreateIndex(
            name: "IX_memory_creation_outcomes_world_id_message_id",
            table: "memory_creation_outcomes",
            columns: new[] { "world_id", "message_id" });

        migrationBuilder.CreateIndex(
            name: "ux_memory_creation_outcomes_provenance",
            table: "memory_creation_outcomes",
            columns: new[] { "world_id", "owner_character_id", "source_type", "source_id", "memory_type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_memory_recall_requests_world_id_subject_actor_id",
            table: "memory_recall_requests",
            columns: new[] { "world_id", "subject_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ux_memory_recall_requests_world_character_key",
            table: "memory_recall_requests",
            columns: new[] { "world_id", "character_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_memory_recall_selections_world_id_memory_id",
            table: "memory_recall_selections",
            columns: new[] { "world_id", "memory_id" });

        migrationBuilder.CreateIndex(
            name: "ux_memory_recall_selections_request_rank",
            table: "memory_recall_selections",
            columns: new[] { "world_id", "request_id", "rank" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_promises_active_due",
            table: "promises",
            columns: new[] { "world_id", "status", "due_at_world_time" },
            filter: "status = 'active'");

        migrationBuilder.CreateIndex(
            name: "IX_promises_world_id_resolution_gameplay_event_id",
            table: "promises",
            columns: new[] { "world_id", "resolution_gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "IX_promises_world_id_source_actor_id",
            table: "promises",
            columns: new[] { "world_id", "source_actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_promises_world_id_target_actor_id",
            table: "promises",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ux_promises_world_memory",
            table: "promises",
            columns: new[] { "world_id", "memory_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_secret_knowers_world_character_status",
            table: "secret_knowers",
            columns: new[] { "world_id", "character_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ux_secrets_world_memory",
            table: "secrets",
            columns: new[] { "world_id", "memory_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "memory_creation_outcomes");

        migrationBuilder.DropTable(
            name: "memory_recall_selections");

        migrationBuilder.DropTable(
            name: "promises");

        migrationBuilder.DropTable(
            name: "secret_knowers");

        migrationBuilder.DropTable(
            name: "memory_recall_requests");

        migrationBuilder.DropTable(
            name: "secrets");

        migrationBuilder.DropTable(
            name: "character_memories");
    }
}

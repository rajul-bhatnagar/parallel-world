using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM06SocialFeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "gameplay_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                target_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                importance = table.Column<int>(type: "integer", nullable: false),
                emotional_impact = table.Column<int>(type: "integer", nullable: false),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                rule_version = table.Column<int>(type: "integer", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_gameplay_events", x => x.id);
                table.UniqueConstraint("ak_gameplay_events_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_gameplay_events_actor_target", "actor_id IS NULL OR target_actor_id IS NULL OR actor_id <> target_actor_id");
                table.CheckConstraint("ck_gameplay_events_emotional_impact", "emotional_impact BETWEEN -100 AND 100");
                table.CheckConstraint("ck_gameplay_events_importance", "importance BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_gameplay_events_actors_world_actor",
                    columns: x => new { x.world_id, x.actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_gameplay_events_actors_world_target",
                    columns: x => new { x.world_id, x.target_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_gameplay_events_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "idempotency_records",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: true),
                operation = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                request_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                response_code = table.Column<int>(type: "integer", nullable: false),
                response_resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_idempotency_records", x => x.id);
                table.CheckConstraint("ck_idempotency_records_response_code", "response_code >= 100");
                table.ForeignKey(
                    name: "fk_idempotency_records_game_worlds_user_world",
                    columns: x => new { x.user_id, x.world_id },
                    principalTable: "game_worlds",
                    principalColumns: new[] { "owner_user_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_idempotency_records_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "posts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                author_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                parent_post_id = table.Column<Guid>(type: "uuid", nullable: true),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                like_count = table.Column<int>(type: "integer", nullable: false),
                reply_count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_posts", x => x.id);
                table.UniqueConstraint("ak_posts_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_posts_like_count", "like_count >= 0");
                table.CheckConstraint("ck_posts_not_self_parent", "parent_post_id IS NULL OR parent_post_id <> id");
                table.CheckConstraint("ck_posts_reply_count", "reply_count >= 0");
                table.ForeignKey(
                    name: "fk_posts_actors_world_author",
                    columns: x => new { x.world_id, x.author_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_posts_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_posts_gameplay_events_world_event",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_posts_posts_world_parent",
                    columns: x => new { x.world_id, x.parent_post_id },
                    principalTable: "posts",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_gameplay_events_world_actor",
            table: "gameplay_events",
            columns: new[] { "world_id", "actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_gameplay_events_world_occurred_id",
            table: "gameplay_events",
            columns: new[] { "world_id", "occurred_at", "id" },
            descending: new[] { false, true, true });

        migrationBuilder.CreateIndex(
            name: "ix_gameplay_events_world_target",
            table: "gameplay_events",
            columns: new[] { "world_id", "target_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_gameplay_events_world_type_occurred",
            table: "gameplay_events",
            columns: new[] { "world_id", "event_type", "occurred_at" },
            descending: new[] { false, false, true });

        migrationBuilder.CreateIndex(
            name: "ux_gameplay_events_world_idempotency_key",
            table: "gameplay_events",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_idempotency_records_status_expires",
            table: "idempotency_records",
            columns: new[] { "status", "expires_at" });

        migrationBuilder.CreateIndex(
            name: "ix_idempotency_records_user_world",
            table: "idempotency_records",
            columns: new[] { "user_id", "world_id" });

        migrationBuilder.CreateIndex(
            name: "ux_idempotency_records_user_operation_key",
            table: "idempotency_records",
            columns: new[] { "user_id", "operation", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_posts_world_author_created_id_active",
            table: "posts",
            columns: new[] { "world_id", "author_actor_id", "created_at", "id" },
            descending: new[] { false, false, true, true },
            filter: "deleted_at IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_posts_world_created_id_active",
            table: "posts",
            columns: new[] { "world_id", "created_at", "id" },
            descending: new[] { false, true, true },
            filter: "deleted_at IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_posts_world_gameplay_event",
            table: "posts",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_posts_world_parent_created_id_active",
            table: "posts",
            columns: new[] { "world_id", "parent_post_id", "created_at", "id" },
            filter: "parent_post_id IS NOT NULL AND deleted_at IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "idempotency_records");

        migrationBuilder.DropTable(
            name: "posts");

        migrationBuilder.DropTable(
            name: "gameplay_events");
    }
}

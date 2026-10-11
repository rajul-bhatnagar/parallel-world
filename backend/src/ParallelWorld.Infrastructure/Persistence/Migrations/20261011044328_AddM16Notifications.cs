using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM16Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_actor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.UniqueConstraint("ak_notifications_world_id_id", x => new { x.world_id, x.id });
                    table.CheckConstraint("ck_notifications_category", "category IN ('reply', 'privatemessage', 'catchupsummary')");
                    table.CheckConstraint("ck_notifications_category_source", "(category = 'catchupsummary' AND source_type = 'catchupsummary') OR (category IN ('reply', 'privatemessage') AND source_type = 'gameplayevent')");
                    table.CheckConstraint("ck_notifications_expiry", "expires_at IS NULL OR expires_at > created_at");
                    table.CheckConstraint("ck_notifications_priority", "priority IN ('normal', 'high')");
                    table.CheckConstraint("ck_notifications_source_shape", "(source_type = 'gameplayevent' AND gameplay_event_id = source_id) OR (source_type = 'catchupsummary' AND gameplay_event_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_notifications_actors_world_recipient",
                        columns: x => new { x.world_id, x.recipient_actor_id },
                        principalTable: "actors",
                        principalColumns: new[] { "world_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_events_world_event",
                        columns: x => new { x.world_id, x.gameplay_event_id },
                        principalTable: "gameplay_events",
                        principalColumns: new[] { "world_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_worlds_recipient_world",
                        columns: x => new { x.recipient_user_id, x.world_id },
                        principalTable: "game_worlds",
                        principalColumns: new[] { "owner_user_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_recipient_cursor",
                table: "notifications",
                columns: new[] { "world_id", "recipient_user_id", "created_at", "id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_recipient_unread",
                table: "notifications",
                columns: new[] { "world_id", "recipient_user_id", "created_at" },
                descending: new[] { false, false, true },
                filter: "read_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id_world_id",
                table: "notifications",
                columns: new[] { "recipient_user_id", "world_id" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_world_id_gameplay_event_id",
                table: "notifications",
                columns: new[] { "world_id", "gameplay_event_id" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_world_id_recipient_actor_id",
                table: "notifications",
                columns: new[] { "world_id", "recipient_actor_id" });

            migrationBuilder.CreateIndex(
                name: "ux_notifications_world_idempotency_key",
                table: "notifications",
                columns: new[] { "world_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_notifications_world_recipient_source_category",
                table: "notifications",
                columns: new[] { "world_id", "recipient_user_id", "source_type", "source_id", "category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}

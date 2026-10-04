using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM11PrivateMessaging : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "conversations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                conversation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                player_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_message_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_conversations", x => x.id);
                table.UniqueConstraint("ak_conversations_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_conversations_distinct_actors", "player_actor_id <> character_actor_id");
                table.CheckConstraint("ck_conversations_type", "conversation_type = 'direct'");
                table.ForeignKey(
                    name: "fk_conversations_actors_world_character",
                    columns: x => new { x.world_id, x.character_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_conversations_actors_world_player",
                    columns: x => new { x.world_id, x.player_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_conversations_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "conversation_participants",
            columns: table => new
            {
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                left_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_read_message_id = table.Column<Guid>(type: "uuid", nullable: true),
                last_read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_conversation_participants", x => new { x.world_id, x.conversation_id, x.actor_id });
                table.ForeignKey(
                    name: "fk_conversation_participants_actors_world_actor",
                    columns: x => new { x.world_id, x.actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_conversation_participants_conversations_world_conversation",
                    columns: x => new { x.world_id, x.conversation_id },
                    principalTable: "conversations",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                sender_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                simulation_action_id = table.Column<Guid>(type: "uuid", nullable: true),
                client_operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                content = table.Column<string>(type: "text", nullable: false),
                delivery_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                edited_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_messages", x => x.id);
                table.UniqueConstraint("ak_messages_world_conversation_id", x => new { x.world_id, x.conversation_id, x.id });
                table.UniqueConstraint("ak_messages_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_messages_delivery_status", "delivery_status = 'delivered'");
                table.ForeignKey(
                    name: "fk_messages_conversations_world_conversation",
                    columns: x => new { x.world_id, x.conversation_id },
                    principalTable: "conversations",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_messages_gameplay_events_world_event",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_messages_participants_world_conversation_sender",
                    columns: x => new { x.world_id, x.conversation_id, x.sender_actor_id },
                    principalTable: "conversation_participants",
                    principalColumns: new[] { "world_id", "conversation_id", "actor_id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_messages_simulation_actions_world_action",
                    columns: x => new { x.world_id, x.simulation_action_id },
                    principalTable: "simulation_actions",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "planned_replies",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                recipient_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                urgency = table.Column<int>(type: "integer", nullable: false),
                conflict_avoidance_penalty = table.Column<int>(type: "integer", nullable: false),
                reply_score = table.Column<int>(type: "integer", nullable: false),
                eligibility_roll = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                reason_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_planned_replies", x => x.id);
                table.UniqueConstraint("ak_planned_replies_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_planned_replies_conflict_penalty", "conflict_avoidance_penalty >= 0");
                table.CheckConstraint("ck_planned_replies_roll", "eligibility_roll BETWEEN 0 AND 99");
                table.CheckConstraint("ck_planned_replies_score", "reply_score BETWEEN 0 AND 100");
                table.CheckConstraint("ck_planned_replies_status", "status IN ('planned','completed','fallbackcompleted','noresponse')");
                table.CheckConstraint("ck_planned_replies_urgency", "urgency BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_planned_replies_messages_source",
                    columns: x => new { x.world_id, x.conversation_id, x.source_message_id },
                    principalTable: "messages",
                    principalColumns: new[] { "world_id", "conversation_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_planned_replies_participants_recipient",
                    columns: x => new { x.world_id, x.conversation_id, x.recipient_actor_id },
                    principalTable: "conversation_participants",
                    principalColumns: new[] { "world_id", "conversation_id", "actor_id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_conversation_participants_world_id_actor_id",
            table: "conversation_participants",
            columns: new[] { "world_id", "actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_conversation_participants_world_id_conversation_id_last_rea~",
            table: "conversation_participants",
            columns: new[] { "world_id", "conversation_id", "last_read_message_id" });

        migrationBuilder.CreateIndex(
            name: "IX_conversations_world_id_character_actor_id",
            table: "conversations",
            columns: new[] { "world_id", "character_actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_conversations_world_last_message_id",
            table: "conversations",
            columns: new[] { "world_id", "last_message_at", "id" },
            descending: new[] { false, true, true });

        migrationBuilder.CreateIndex(
            name: "ux_conversations_world_player_character_active",
            table: "conversations",
            columns: new[] { "world_id", "player_actor_id", "character_actor_id" },
            unique: true,
            filter: "is_active = TRUE");

        migrationBuilder.CreateIndex(
            name: "ix_messages_world_conversation_created_id",
            table: "messages",
            columns: new[] { "world_id", "conversation_id", "created_at", "id" },
            descending: new[] { false, false, true, true });

        migrationBuilder.CreateIndex(
            name: "IX_messages_world_id_conversation_id_sender_actor_id",
            table: "messages",
            columns: new[] { "world_id", "conversation_id", "sender_actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_messages_world_id_gameplay_event_id",
            table: "messages",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "IX_messages_world_id_simulation_action_id",
            table: "messages",
            columns: new[] { "world_id", "simulation_action_id" });

        migrationBuilder.CreateIndex(
            name: "ux_messages_world_sender_client_operation",
            table: "messages",
            columns: new[] { "world_id", "sender_actor_id", "client_operation_id" },
            unique: true,
            filter: "client_operation_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_planned_replies_status_due_id",
            table: "planned_replies",
            columns: new[] { "status", "due_at", "id" });

        migrationBuilder.CreateIndex(
            name: "IX_planned_replies_world_id_conversation_id_recipient_actor_id",
            table: "planned_replies",
            columns: new[] { "world_id", "conversation_id", "recipient_actor_id" });

        migrationBuilder.CreateIndex(
            name: "IX_planned_replies_world_id_conversation_id_source_message_id",
            table: "planned_replies",
            columns: new[] { "world_id", "conversation_id", "source_message_id" });

        migrationBuilder.CreateIndex(
            name: "ux_planned_replies_source_recipient",
            table: "planned_replies",
            columns: new[] { "world_id", "source_message_id", "recipient_actor_id" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_conversation_participants_messages_read_cursor",
            table: "conversation_participants",
            columns: new[] { "world_id", "conversation_id", "last_read_message_id" },
            principalTable: "messages",
            principalColumns: new[] { "world_id", "conversation_id", "id" },
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_conversation_participants_conversations_world_conversation",
            table: "conversation_participants");

        migrationBuilder.DropForeignKey(
            name: "fk_messages_conversations_world_conversation",
            table: "messages");

        migrationBuilder.DropForeignKey(
            name: "fk_conversation_participants_messages_read_cursor",
            table: "conversation_participants");

        migrationBuilder.DropTable(
            name: "planned_replies");

        migrationBuilder.DropTable(
            name: "conversations");

        migrationBuilder.DropTable(
            name: "messages");

        migrationBuilder.DropTable(
            name: "conversation_participants");
    }
}

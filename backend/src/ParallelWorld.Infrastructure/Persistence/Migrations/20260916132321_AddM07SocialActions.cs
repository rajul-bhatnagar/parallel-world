using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM07SocialActions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "follows",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                follower_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                followed_actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_follows", x => x.id);
                table.CheckConstraint("ck_follows_distinct_actors", "follower_actor_id <> followed_actor_id");
                table.CheckConstraint("ck_follows_time", "ended_at IS NULL OR ended_at >= started_at");
                table.ForeignKey(
                    name: "fk_follows_actors_world_followed",
                    columns: x => new { x.world_id, x.followed_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_follows_actors_world_follower",
                    columns: x => new { x.world_id, x.follower_actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_follows_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_follows_gameplay_events_world_event",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "post_reactions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                post_id = table.Column<Guid>(type: "uuid", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                reaction_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                gameplay_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_post_reactions", x => x.id);
                table.CheckConstraint("ck_post_reactions_type", "reaction_type = 'like'");
                table.ForeignKey(
                    name: "fk_post_reactions_actors_world_actor",
                    columns: x => new { x.world_id, x.actor_id },
                    principalTable: "actors",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_post_reactions_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_post_reactions_gameplay_events_world_event",
                    columns: x => new { x.world_id, x.gameplay_event_id },
                    principalTable: "gameplay_events",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_post_reactions_posts_world_post",
                    columns: x => new { x.world_id, x.post_id },
                    principalTable: "posts",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_follows_world_followed_ended",
            table: "follows",
            columns: new[] { "world_id", "followed_actor_id", "ended_at" });

        migrationBuilder.CreateIndex(
            name: "ix_follows_world_follower_ended",
            table: "follows",
            columns: new[] { "world_id", "follower_actor_id", "ended_at" });

        migrationBuilder.CreateIndex(
            name: "ix_follows_world_gameplay_event",
            table: "follows",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "ux_follows_world_follower_followed_active",
            table: "follows",
            columns: new[] { "world_id", "follower_actor_id", "followed_actor_id" },
            unique: true,
            filter: "ended_at IS NULL");

        migrationBuilder.CreateIndex(
            name: "ux_follows_world_idempotency_key",
            table: "follows",
            columns: new[] { "world_id", "idempotency_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_post_reactions_world_actor",
            table: "post_reactions",
            columns: new[] { "world_id", "actor_id" });

        migrationBuilder.CreateIndex(
            name: "ix_post_reactions_world_gameplay_event",
            table: "post_reactions",
            columns: new[] { "world_id", "gameplay_event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_post_reactions_world_post_type_created_id",
            table: "post_reactions",
            columns: new[] { "world_id", "post_id", "reaction_type", "created_at", "id" });

        migrationBuilder.CreateIndex(
            name: "ux_post_reactions_world_post_actor_type",
            table: "post_reactions",
            columns: new[] { "world_id", "post_id", "actor_id", "reaction_type" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "follows");

        migrationBuilder.DropTable(
            name: "post_reactions");
    }
}

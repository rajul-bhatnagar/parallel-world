using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParallelWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddM05CharacterCatalogue : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "characters",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                display_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                handle = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                bio = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                age = table.Column<int>(type: "integer", nullable: false),
                profession = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                archetype = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                writing_style = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                activity_level = table.Column<int>(type: "integer", nullable: false),
                influence = table.Column<int>(type: "integer", nullable: false),
                popularity = table.Column<int>(type: "integer", nullable: false),
                current_mood_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_characters", x => x.id);
                table.UniqueConstraint("ak_characters_world_id_id", x => new { x.world_id, x.id });
                table.CheckConstraint("ck_characters_activity_level", "activity_level BETWEEN 0 AND 100");
                table.CheckConstraint("ck_characters_age", "age >= 0");
                table.CheckConstraint("ck_characters_influence", "influence BETWEEN 0 AND 100");
                table.CheckConstraint("ck_characters_popularity", "popularity BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_characters_game_worlds_world_id",
                    column: x => x.world_id,
                    principalTable: "game_worlds",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "character_interests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_id = table.Column<Guid>(type: "uuid", nullable: false),
                topic_id = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                strength = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_character_interests", x => x.id);
                table.CheckConstraint("ck_character_interests_strength", "strength BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_character_interests_characters_world_character",
                    columns: x => new { x.world_id, x.character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "character_opinions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_id = table.Column<Guid>(type: "uuid", nullable: false),
                topic_id = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                position = table.Column<int>(type: "integer", nullable: false),
                confidence = table.Column<int>(type: "integer", nullable: false),
                intensity = table.Column<int>(type: "integer", nullable: false),
                last_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_character_opinions", x => x.id);
                table.CheckConstraint("ck_character_opinions_confidence", "confidence BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_opinions_intensity", "intensity BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_opinions_position", "position BETWEEN -100 AND 100");
                table.ForeignKey(
                    name: "fk_character_opinions_characters_world_character",
                    columns: x => new { x.world_id, x.character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "character_schedules",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_id = table.Column<Guid>(type: "uuid", nullable: false),
                day_of_week = table.Column<int>(type: "integer", nullable: false),
                start_local_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                end_local_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                activity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_character_schedules", x => x.id);
                table.CheckConstraint("ck_character_schedules_day", "day_of_week BETWEEN 0 AND 6");
                table.CheckConstraint("ck_character_schedules_time", "end_local_time > start_local_time");
                table.ForeignKey(
                    name: "fk_character_schedules_characters_world_character",
                    columns: x => new { x.world_id, x.character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "character_traits",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                world_id = table.Column<Guid>(type: "uuid", nullable: false),
                character_id = table.Column<Guid>(type: "uuid", nullable: false),
                humour = table.Column<int>(type: "integer", nullable: false),
                confidence = table.Column<int>(type: "integer", nullable: false),
                empathy = table.Column<int>(type: "integer", nullable: false),
                aggression = table.Column<int>(type: "integer", nullable: false),
                curiosity = table.Column<int>(type: "integer", nullable: false),
                honesty = table.Column<int>(type: "integer", nullable: false),
                sociability = table.Column<int>(type: "integer", nullable: false),
                ambition = table.Column<int>(type: "integer", nullable: false),
                patience = table.Column<int>(type: "integer", nullable: false),
                optimism = table.Column<int>(type: "integer", nullable: false),
                sensitivity = table.Column<int>(type: "integer", nullable: false),
                romantic_openness = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_character_traits", x => x.id);
                table.CheckConstraint("ck_character_traits_aggression", "aggression BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_ambition", "ambition BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_confidence", "confidence BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_curiosity", "curiosity BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_empathy", "empathy BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_honesty", "honesty BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_humour", "humour BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_optimism", "optimism BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_patience", "patience BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_romantic_openness", "romantic_openness BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_sensitivity", "sensitivity BETWEEN 0 AND 100");
                table.CheckConstraint("ck_character_traits_sociability", "sociability BETWEEN 0 AND 100");
                table.ForeignKey(
                    name: "fk_character_traits_characters_world_character",
                    columns: x => new { x.world_id, x.character_id },
                    principalTable: "characters",
                    principalColumns: new[] { "world_id", "id" },
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ux_actors_world_character",
            table: "actors",
            columns: new[] { "world_id", "character_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_character_interests_world_character_topic",
            table: "character_interests",
            columns: new[] { "world_id", "character_id", "topic_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_character_opinions_world_character_topic",
            table: "character_opinions",
            columns: new[] { "world_id", "character_id", "topic_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_character_schedules_world_character_day_start",
            table: "character_schedules",
            columns: new[] { "world_id", "character_id", "day_of_week", "start_local_time" });

        migrationBuilder.CreateIndex(
            name: "ux_character_traits_world_character",
            table: "character_traits",
            columns: new[] { "world_id", "character_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_characters_world_status_id",
            table: "characters",
            columns: new[] { "world_id", "status", "id" });

        migrationBuilder.CreateIndex(
            name: "ux_characters_world_handle",
            table: "characters",
            columns: new[] { "world_id", "handle" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_actors_characters_world_character",
            table: "actors",
            columns: new[] { "world_id", "character_id" },
            principalTable: "characters",
            principalColumns: new[] { "world_id", "id" },
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_actors_characters_world_character",
            table: "actors");

        migrationBuilder.DropTable(
            name: "character_interests");

        migrationBuilder.DropTable(
            name: "character_opinions");

        migrationBuilder.DropTable(
            name: "character_schedules");

        migrationBuilder.DropTable(
            name: "character_traits");

        migrationBuilder.DropTable(
            name: "characters");

        migrationBuilder.DropIndex(
            name: "ux_actors_world_character",
            table: "actors");
    }
}

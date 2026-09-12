using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class M05SchemaCatalogTests
{
    [Fact]
    public async Task MigratedSchema_HasExactM05TablesConstraintsAndIndexes()
    {
        await using var factory = await CreateFactoryAsync();
        TestDatabaseGuard.EnsureSafe(factory.DatabaseName);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();

        var tables = await ReadNamesAsync(db, """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE'
              AND table_name <> '__EFMigrationsHistory'
            ORDER BY table_name
            """);
        Assert.Equal(new[]
        {
            "actors",
            "character_interests",
            "character_opinions",
            "character_schedules",
            "character_traits",
            "characters",
            "device_installations",
            "game_worlds",
            "guest_bootstrap_operations",
            "player_profiles",
            "refresh_tokens",
            "users",
            "world_settings",
            "world_simulation_states",
        }, tables);

        var constraints = await ReadNamesAsync(db, """
            SELECT c.conname
            FROM pg_constraint AS c
            JOIN pg_class AS relation ON relation.oid = c.conrelid
            JOIN pg_namespace AS namespace ON namespace.oid = relation.relnamespace
            WHERE namespace.nspname = 'public'
              AND relation.relname <> '__EFMigrationsHistory'
              AND c.contype <> 'n'
            ORDER BY c.conname
            """);
        Assert.Equal(ExpectedConstraints, constraints);

        var indexes = await ReadNamesAsync(db, """
            SELECT indexname
            FROM pg_indexes
            WHERE schemaname = 'public'
              AND tablename <> '__EFMigrationsHistory'
            ORDER BY indexname
            """);
        Assert.Equal(ExpectedIndexes, indexes);
    }

    private static readonly string[] ExpectedConstraints =
    [
        "ak_actors_world_id_id",
        "ak_characters_world_id_id",
        "ak_device_installations_user_id_id",
        "ak_game_worlds_owner_user_id_id",
        "ak_player_profiles_world_id_id",
        "ck_actors_detail_shape",
        "ck_character_interests_strength",
        "ck_character_opinions_confidence",
        "ck_character_opinions_intensity",
        "ck_character_opinions_position",
        "ck_character_schedules_day",
        "ck_character_schedules_time",
        "ck_character_traits_aggression",
        "ck_character_traits_ambition",
        "ck_character_traits_confidence",
        "ck_character_traits_curiosity",
        "ck_character_traits_empathy",
        "ck_character_traits_honesty",
        "ck_character_traits_humour",
        "ck_character_traits_optimism",
        "ck_character_traits_patience",
        "ck_character_traits_romantic_openness",
        "ck_character_traits_sensitivity",
        "ck_character_traits_sociability",
        "ck_characters_activity_level",
        "ck_characters_age",
        "ck_characters_influence",
        "ck_characters_popularity",
        "ck_guest_bootstrap_operations_expiry",
        "ck_player_profiles_followers",
        "ck_player_profiles_influence",
        "ck_player_profiles_reputation",
        "ck_refresh_tokens_expiry",
        "ck_world_settings_action_limit",
        "ck_world_settings_ai_budget",
        "ck_world_settings_time_scale",
        "ck_world_simulation_states_sequence",
        "fk_actors_characters_world_character",
        "fk_actors_game_worlds_world_id",
        "fk_actors_player_profiles_world_profile",
        "fk_character_interests_characters_world_character",
        "fk_character_opinions_characters_world_character",
        "fk_character_schedules_characters_world_character",
        "fk_character_traits_characters_world_character",
        "fk_characters_game_worlds_world_id",
        "fk_device_installations_users_user_id",
        "fk_game_worlds_users_owner_user_id",
        "fk_guest_bootstrap_operations_installations_user_device",
        "fk_guest_bootstrap_operations_users_user_id",
        "fk_player_profiles_game_worlds_world_id",
        "fk_refresh_tokens_installations_user_device",
        "fk_refresh_tokens_replacement",
        "fk_refresh_tokens_users_user_id",
        "fk_world_settings_game_worlds_world_id",
        "fk_world_simulation_states_game_worlds_world_id",
        "pk_actors",
        "pk_character_interests",
        "pk_character_opinions",
        "pk_character_schedules",
        "pk_character_traits",
        "pk_characters",
        "pk_device_installations",
        "pk_game_worlds",
        "pk_guest_bootstrap_operations",
        "pk_player_profiles",
        "pk_refresh_tokens",
        "pk_users",
        "pk_world_settings",
        "pk_world_simulation_states",
    ];

    private static readonly string[] ExpectedIndexes =
    [
        "ak_actors_world_id_id",
        "ak_characters_world_id_id",
        "ak_device_installations_user_id_id",
        "ak_game_worlds_owner_user_id_id",
        "ak_player_profiles_world_id_id",
        "ix_character_schedules_world_character_day_start",
        "ix_characters_world_status_id",
        "ix_device_installations_user_last_seen",
        "ix_game_worlds_owner_created",
        "ix_game_worlds_owner_status",
        "ix_guest_bootstrap_operations_expires_at",
        "ix_guest_bootstrap_operations_user_device",
        "ix_refresh_tokens_family_expiry",
        "ix_refresh_tokens_user_device_expiry",
        "ix_refresh_tokens_user_family_created_state",
        "pk_actors",
        "pk_character_interests",
        "pk_character_opinions",
        "pk_character_schedules",
        "pk_character_traits",
        "pk_characters",
        "pk_device_installations",
        "pk_game_worlds",
        "pk_guest_bootstrap_operations",
        "pk_player_profiles",
        "pk_refresh_tokens",
        "pk_users",
        "pk_world_settings",
        "pk_world_simulation_states",
        "ux_actors_character_id",
        "ux_actors_one_player_per_world",
        "ux_actors_player_profile_id",
        "ux_actors_world_character",
        "ux_actors_world_player_profile",
        "ux_character_interests_world_character_topic",
        "ux_character_opinions_world_character_topic",
        "ux_character_traits_world_character",
        "ux_characters_world_handle",
        "ux_device_installations_public_id",
        "ux_guest_bootstrap_operations_proof_hash",
        "ux_player_profiles_world_handle",
        "ux_player_profiles_world_id",
        "ux_refresh_tokens_replaced_by_token_id",
        "ux_refresh_tokens_token_hash",
        "ux_users_normalized_email",
        "ux_world_settings_world_id",
        "ux_world_simulation_states_world_id",
    ];

    private static async Task<string[]> ReadNamesAsync(ParallelWorldDbContext db, string sql)
    {
        var connection = db.Database.GetDbConnection();
        var closeConnection = connection.State == ConnectionState.Closed;
        if (closeConnection)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await using var reader = await command.ExecuteReaderAsync();
            var names = new List<string>();
            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }

            return names.ToArray();
        }
        finally
        {
            if (closeConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<M03ApiFactory> CreateFactoryAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "ConnectionStrings__Default must identify the PostgreSQL administrative base connection.");
        return await M03ApiFactory.CreateAsync(connectionString);
    }
}

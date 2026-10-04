using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelWorld.Infrastructure.Persistence;

namespace ParallelWorld.IntegrationTests;

[Trait("Category", "PostgreSql")]
public sealed class M12SchemaCatalogTests
{
    [Fact]
    public async Task MigratedSchema_HasExactM12TablesConstraintsAndIndexes()
    {
        await using var factory = await CreateFactoryAsync();
        TestDatabaseGuard.EnsureSafe(factory.DatabaseName);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ParallelWorldDbContext>();

        var migrations = await ReadNamesAsync(db, """
            SELECT "MigrationId"
            FROM "__EFMigrationsHistory"
            ORDER BY "MigrationId"
            """);
        Assert.Equal(new[]
        {
            "20260831162301_InitialM03",
            "20260905120855_AddM05CharacterCatalogue",
            "20260912160326_AddM06SocialFeed",
            "20260916132321_AddM07SocialActions",
            "20260920133535_AddM08RuleBasedSimulation",
            "20260921173919_AddM09AiTextGeneration",
            "20261003064625_AddM10RelationshipEngine",
            "20261003145439_AddM11PrivateMessaging",
            "20261004123030_AddM12LongTermMemory",
        }, migrations);

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
            "ai_generation_requests",
            "character_interests",
            "character_memories",
            "character_opinions",
            "character_schedules",
            "character_traits",
            "characters",
            "conversation_participants",
            "conversations",
            "device_installations",
            "follows",
            "game_worlds",
            "gameplay_events",
            "guest_bootstrap_operations",
            "idempotency_records",
            "memory_creation_outcomes",
            "memory_recall_requests",
            "memory_recall_selections",
            "messages",
            "planned_replies",
            "player_profiles",
            "post_reactions",
            "posts",
            "promises",
            "refresh_tokens",
            "relationship_daily_change_ledgers",
            "relationship_events",
            "relationships",
            "secret_knowers",
            "secrets",
            "simulation_actions",
            "simulation_rule_evaluations",
            "simulation_run_checkpoints",
            "simulation_runs",
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
        Assert.Equal(ExpectedConstraints.Concat(M10Constraints).Concat(M11Constraints).Concat(M12Constraints).Order(StringComparer.Ordinal), constraints);

        var indexes = await ReadNamesAsync(db, """
            SELECT indexname
            FROM pg_indexes
            WHERE schemaname = 'public'
              AND tablename <> '__EFMigrationsHistory'
            ORDER BY indexname
            """);
        Assert.Equal(ExpectedIndexes.Concat(M10Indexes).Concat(M11Indexes).Concat(M12Indexes).Order(StringComparer.Ordinal), indexes);
    }

    private static readonly string[] ExpectedConstraints =
    [
        "ak_actors_world_id_id",
        "ak_characters_world_id_id",
        "ak_device_installations_user_id_id",
        "ak_game_worlds_owner_user_id_id",
        "ak_gameplay_events_world_id_id",
        "ak_player_profiles_world_id_id",
        "ak_posts_world_id_id",
        "ak_simulation_actions_world_id_id",
        "ak_simulation_rule_evaluations_world_id_id",
        "ak_simulation_runs_world_id_id",
        "ck_actors_detail_shape",
        "ck_ai_generation_requests_attempt_count",
        "ck_ai_generation_requests_latency",
        "ck_ai_generation_requests_status",
        "ck_ai_generation_requests_time",
        "ck_ai_generation_requests_token_counts",
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
        "ck_follows_distinct_actors",
        "ck_follows_time",
        "ck_gameplay_events_actor_target",
        "ck_gameplay_events_emotional_impact",
        "ck_gameplay_events_importance",
        "ck_guest_bootstrap_operations_expiry",
        "ck_idempotency_records_response_code",
        "ck_player_profiles_followers",
        "ck_player_profiles_influence",
        "ck_player_profiles_reputation",
        "ck_post_reactions_type",
        "ck_posts_like_count",
        "ck_posts_not_self_parent",
        "ck_posts_reply_count",
        "ck_refresh_tokens_expiry",
        "ck_simulation_actions_distinct_actors",
        "ck_simulation_actions_ordinal",
        "ck_simulation_actions_status",
        "ck_simulation_rule_evaluations_outcome",
        "ck_simulation_run_checkpoints_bucket",
        "ck_simulation_run_checkpoints_ordinal",
        "ck_simulation_run_checkpoints_status",
        "ck_simulation_runs_active_interval",
        "ck_simulation_runs_effective_time_scale",
        "ck_simulation_runs_interval",
        "ck_simulation_runs_processed_through",
        "ck_simulation_runs_status",
        "ck_simulation_runs_type",
        "ck_world_settings_action_limit",
        "ck_world_settings_ai_budget",
        "ck_world_settings_time_scale",
        "ck_world_simulation_states_sequence",
        "fk_actors_characters_world_character",
        "fk_actors_game_worlds_world_id",
        "fk_actors_player_profiles_world_profile",
        "fk_ai_generation_requests_actions_world_action",
        "fk_character_interests_characters_world_character",
        "fk_character_opinions_characters_world_character",
        "fk_character_schedules_characters_world_character",
        "fk_character_traits_characters_world_character",
        "fk_characters_game_worlds_world_id",
        "fk_device_installations_users_user_id",
        "fk_follows_actors_world_followed",
        "fk_follows_actors_world_follower",
        "fk_follows_game_worlds_world_id",
        "fk_follows_gameplay_events_world_event",
        "fk_game_worlds_users_owner_user_id",
        "fk_gameplay_events_actors_world_actor",
        "fk_gameplay_events_actors_world_target",
        "fk_gameplay_events_game_worlds_world_id",
        "fk_guest_bootstrap_operations_installations_user_device",
        "fk_guest_bootstrap_operations_users_user_id",
        "fk_idempotency_records_game_worlds_user_world",
        "fk_idempotency_records_users_user_id",
        "fk_player_profiles_game_worlds_world_id",
        "fk_post_reactions_actors_world_actor",
        "fk_post_reactions_game_worlds_world_id",
        "fk_post_reactions_gameplay_events_world_event",
        "fk_post_reactions_posts_world_post",
        "fk_posts_actors_world_author",
        "fk_posts_game_worlds_world_id",
        "fk_posts_gameplay_events_world_event",
        "fk_posts_posts_world_parent",
        "fk_refresh_tokens_installations_user_device",
        "fk_refresh_tokens_replacement",
        "fk_refresh_tokens_users_user_id",
        "fk_simulation_actions_actors_world_actor",
        "fk_simulation_actions_actors_world_target",
        "fk_simulation_actions_posts_world_target",
        "fk_simulation_actions_runs_world_run",
        "fk_simulation_rule_evaluations_runs_world_run",
        "fk_simulation_run_checkpoints_runs_world_run",
        "fk_simulation_runs_game_worlds_world_id",
        "fk_world_settings_game_worlds_world_id",
        "fk_world_simulation_states_game_worlds_world_id",
        "pk_actors",
        "pk_ai_generation_requests",
        "pk_character_interests",
        "pk_character_opinions",
        "pk_character_schedules",
        "pk_character_traits",
        "pk_characters",
        "pk_device_installations",
        "pk_follows",
        "pk_game_worlds",
        "pk_gameplay_events",
        "pk_guest_bootstrap_operations",
        "pk_idempotency_records",
        "pk_player_profiles",
        "pk_post_reactions",
        "pk_posts",
        "pk_refresh_tokens",
        "pk_simulation_actions",
        "pk_simulation_rule_evaluations",
        "pk_simulation_run_checkpoints",
        "pk_simulation_runs",
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
        "ak_gameplay_events_world_id_id",
        "ak_player_profiles_world_id_id",
        "ak_posts_world_id_id",
        "ak_simulation_actions_world_id_id",
        "ak_simulation_rule_evaluations_world_id_id",
        "ak_simulation_runs_world_id_id",
        "ix_ai_generation_requests_world_status_completed",
        "ix_character_schedules_world_character_day_start",
        "ix_characters_world_status_id",
        "ix_device_installations_user_last_seen",
        "ix_follows_world_followed_ended",
        "ix_follows_world_follower_ended",
        "ix_follows_world_gameplay_event",
        "ix_game_worlds_owner_created",
        "ix_game_worlds_owner_status",
        "ix_gameplay_events_world_actor",
        "ix_gameplay_events_world_occurred_id",
        "ix_gameplay_events_world_target",
        "ix_gameplay_events_world_type_occurred",
        "ix_guest_bootstrap_operations_expires_at",
        "ix_guest_bootstrap_operations_user_device",
        "ix_idempotency_records_status_expires",
        "ix_idempotency_records_user_world",
        "ix_post_reactions_world_actor",
        "ix_post_reactions_world_gameplay_event",
        "ix_post_reactions_world_post_type_created_id",
        "ix_posts_world_author_created_id_active",
        "ix_posts_world_created_id_active",
        "ix_posts_world_gameplay_event",
        "ix_posts_world_parent_created_id_active",
        "ix_refresh_tokens_family_expiry",
        "ix_refresh_tokens_user_device_expiry",
        "ix_refresh_tokens_user_family_created_state",
        "ix_simulation_actions_world_actor",
        "ix_simulation_actions_world_status_scheduled_id",
        "ix_simulation_actions_world_target_actor",
        "ix_simulation_actions_world_target_post",
        "ix_simulation_rule_evaluations_world_run_rule",
        "ix_simulation_run_checkpoints_world_run_bucket",
        "ix_simulation_runs_world_completed",
        "ix_simulation_runs_world_status_interval",
        "pk_actors",
        "pk_ai_generation_requests",
        "pk_character_interests",
        "pk_character_opinions",
        "pk_character_schedules",
        "pk_character_traits",
        "pk_characters",
        "pk_device_installations",
        "pk_follows",
        "pk_game_worlds",
        "pk_gameplay_events",
        "pk_guest_bootstrap_operations",
        "pk_idempotency_records",
        "pk_player_profiles",
        "pk_post_reactions",
        "pk_posts",
        "pk_refresh_tokens",
        "pk_simulation_actions",
        "pk_simulation_rule_evaluations",
        "pk_simulation_run_checkpoints",
        "pk_simulation_runs",
        "pk_users",
        "pk_world_settings",
        "pk_world_simulation_states",
        "ux_actors_character_id",
        "ux_actors_one_player_per_world",
        "ux_actors_player_profile_id",
        "ux_actors_world_character",
        "ux_actors_world_player_profile",
        "ux_ai_generation_requests_world_action_input",
        "ux_ai_generation_requests_world_idempotency_key",
        "ux_character_interests_world_character_topic",
        "ux_character_opinions_world_character_topic",
        "ux_character_traits_world_character",
        "ux_characters_world_handle",
        "ux_device_installations_public_id",
        "ux_follows_world_follower_followed_active",
        "ux_follows_world_idempotency_key",
        "ux_gameplay_events_world_idempotency_key",
        "ux_guest_bootstrap_operations_proof_hash",
        "ux_idempotency_records_user_operation_key",
        "ux_player_profiles_world_handle",
        "ux_player_profiles_world_id",
        "ux_post_reactions_world_post_actor_type",
        "ux_refresh_tokens_replaced_by_token_id",
        "ux_refresh_tokens_token_hash",
        "ux_simulation_actions_world_idempotency_key",
        "ux_simulation_actions_world_run_ordinal",
        "ux_simulation_rule_evaluations_run_rule",
        "ux_simulation_run_checkpoints_world_idempotency_key",
        "ux_simulation_run_checkpoints_world_run_bucket",
        "ux_simulation_run_checkpoints_world_run_ordinal",
        "ux_simulation_runs_world_idempotency_key",
        "ux_simulation_runs_world_rule_interval",
        "ux_users_normalized_email",
        "ux_world_settings_world_id",
        "ux_world_simulation_states_world_id",
    ];

    private static readonly string[] M10Constraints =
    [
        "ak_relationships_world_id_id",
        "ck_characters_reputation",
        "ck_follows_end_game_fields", "ck_follows_game_time",
        "ck_relationship_daily_ledgers_affection", "ck_relationship_daily_ledgers_attraction", "ck_relationship_daily_ledgers_comfort", "ck_relationship_daily_ledgers_commitment", "ck_relationship_daily_ledgers_distinct_actors", "ck_relationship_daily_ledgers_familiarity", "ck_relationship_daily_ledgers_jealousy", "ck_relationship_daily_ledgers_respect", "ck_relationship_daily_ledgers_rivalry", "ck_relationship_daily_ledgers_trust",
        "ck_relationship_events_affection_delta", "ck_relationship_events_attraction_delta", "ck_relationship_events_comfort_delta", "ck_relationship_events_commitment_delta", "ck_relationship_events_distinct_actors", "ck_relationship_events_familiarity_delta", "ck_relationship_events_jealousy_delta", "ck_relationship_events_respect_delta", "ck_relationship_events_rivalry_delta", "ck_relationship_events_trust_delta",
        "ck_relationships_affection", "ck_relationships_attraction", "ck_relationships_comfort", "ck_relationships_commitment", "ck_relationships_distinct_actors", "ck_relationships_familiarity", "ck_relationships_jealousy", "ck_relationships_respect", "ck_relationships_rivalry", "ck_relationships_trust",
        "fk_relationship_daily_ledgers_actors_world_source", "fk_relationship_daily_ledgers_actors_world_target",
        "fk_relationship_events_actors_world_source", "fk_relationship_events_actors_world_target", "fk_relationship_events_gameplay_events_world_event", "fk_relationship_events_relationships_world_relationship",
        "fk_relationships_actors_world_source", "fk_relationships_actors_world_target", "fk_relationships_game_worlds_world_id",
        "pk_relationship_daily_change_ledgers", "pk_relationship_events", "pk_relationships",
    ];

    private static readonly string[] M10Indexes =
    [
        "IX_relationship_daily_change_ledgers_world_id_target_actor_id", "IX_relationship_events_world_id_relationship_id", "IX_relationship_events_world_id_target_actor_id", "IX_relationships_world_id_target_actor_id",
        "ak_relationships_world_id_id", "ix_relationship_events_game_date", "ix_relationship_events_history", "ix_relationships_world_source_updated_id",
        "pk_relationship_daily_change_ledgers", "pk_relationship_events", "pk_relationships",
        "ux_relationship_events_source_rule_direction", "ux_relationship_events_world_idempotency_key", "ux_relationships_world_source_target",
    ];

    private static readonly string[] M11Constraints =
    [
        "ak_conversations_world_id_id", "ak_messages_world_conversation_id", "ak_messages_world_id_id", "ak_planned_replies_world_id_id",
        "ck_conversations_distinct_actors", "ck_conversations_type", "ck_messages_delivery_status",
        "ck_planned_replies_conflict_penalty", "ck_planned_replies_roll", "ck_planned_replies_score", "ck_planned_replies_status", "ck_planned_replies_urgency",
        "fk_conversation_participants_actors_world_actor", "fk_conversation_participants_conversations_world_conversation", "fk_conversation_participants_messages_read_cursor",
        "fk_conversations_actors_world_character", "fk_conversations_actors_world_player", "fk_conversations_game_worlds_world_id",
        "fk_messages_conversations_world_conversation", "fk_messages_gameplay_events_world_event", "fk_messages_participants_world_conversation_sender", "fk_messages_simulation_actions_world_action",
        "fk_planned_replies_messages_source", "fk_planned_replies_participants_recipient",
        "pk_conversation_participants", "pk_conversations", "pk_messages", "pk_planned_replies",
    ];

    private static readonly string[] M11Indexes =
    [
        "IX_conversation_participants_world_id_actor_id", "IX_conversation_participants_world_id_conversation_id_last_rea~",
        "IX_conversations_world_id_character_actor_id", "IX_messages_world_id_conversation_id_sender_actor_id",
        "IX_messages_world_id_gameplay_event_id", "IX_messages_world_id_simulation_action_id",
        "IX_planned_replies_world_id_conversation_id_recipient_actor_id", "IX_planned_replies_world_id_conversation_id_source_message_id",
        "ak_conversations_world_id_id", "ak_messages_world_conversation_id", "ak_messages_world_id_id", "ak_planned_replies_world_id_id",
        "ix_conversations_world_last_message_id", "ix_messages_world_conversation_created_id", "ix_planned_replies_status_due_id",
        "pk_conversation_participants", "pk_conversations", "pk_messages", "pk_planned_replies",
        "ux_conversations_world_player_character_active", "ux_messages_world_sender_client_operation", "ux_planned_replies_source_recipient",
    ];

    private static readonly string[] M12Constraints =
    [
        "ak_character_memories_world_id_id", "ak_memory_recall_requests_world_id_id", "ak_promises_world_id_id", "ak_secrets_world_id_id",
        "ck_character_memories_authority", "ck_character_memories_confidence", "ck_character_memories_importance", "ck_character_memories_lifecycle", "ck_character_memories_source", "ck_character_memories_subject", "ck_character_memories_type", "ck_character_memories_visibility",
        "ck_memory_creation_outcomes_shape", "ck_memory_creation_outcomes_source", "ck_memory_recall_requests_subject", "ck_memory_recall_selections_rank", "ck_memory_recall_selections_score",
        "ck_promises_distinct_actors", "ck_promises_due_condition", "ck_promises_resolution", "ck_promises_status", "ck_secret_knowers_status", "ck_secrets_status",
        "fk_character_memories_actors_world_subject", "fk_character_memories_characters_world_owner", "fk_character_memories_gameplay_events_world_source", "fk_character_memories_messages_world_source",
        "fk_memory_creation_outcomes_characters_world_owner", "fk_memory_creation_outcomes_events_world_source", "fk_memory_creation_outcomes_memories_world_memory", "fk_memory_creation_outcomes_messages_world_source",
        "fk_memory_recall_requests_actors_world_subject", "fk_memory_recall_requests_characters_world_character", "fk_memory_recall_selections_memories_world_memory", "fk_memory_recall_selections_requests_world_request",
        "fk_promises_actors_world_source", "fk_promises_actors_world_target", "fk_promises_events_world_resolution", "fk_promises_memories_world_memory",
        "fk_secret_knowers_characters_world_character", "fk_secret_knowers_secrets_world_secret", "fk_secrets_memories_world_memory",
        "pk_character_memories", "pk_memory_creation_outcomes", "pk_memory_recall_requests", "pk_memory_recall_selections", "pk_promises", "pk_secret_knowers", "pk_secrets",
    ];

    private static readonly string[] M12Indexes =
    [
        "IX_character_memories_world_id_gameplay_event_id", "IX_character_memories_world_id_message_id", "IX_character_memories_world_id_subject_actor_id",
        "IX_memory_creation_outcomes_world_id_gameplay_event_id", "IX_memory_creation_outcomes_world_id_memory_id", "IX_memory_creation_outcomes_world_id_message_id",
        "IX_memory_recall_requests_world_id_subject_actor_id", "IX_memory_recall_selections_world_id_memory_id",
        "IX_promises_world_id_resolution_gameplay_event_id", "IX_promises_world_id_source_actor_id", "IX_promises_world_id_target_actor_id",
        "ak_character_memories_world_id_id", "ak_memory_recall_requests_world_id_id", "ak_promises_world_id_id", "ak_secrets_world_id_id",
        "ix_character_memories_recall", "ix_character_memories_retention", "ix_promises_active_due", "ix_secret_knowers_world_character_status",
        "pk_character_memories", "pk_memory_creation_outcomes", "pk_memory_recall_requests", "pk_memory_recall_selections", "pk_promises", "pk_secret_knowers", "pk_secrets",
        "ux_character_memories_world_owner_source_type", "ux_memory_creation_outcomes_provenance", "ux_memory_recall_requests_world_character_key", "ux_memory_recall_selections_request_rank", "ux_promises_world_memory", "ux_secrets_world_memory",
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

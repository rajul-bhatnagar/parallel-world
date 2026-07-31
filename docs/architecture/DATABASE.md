# Database Source of Truth

PostgreSQL is authoritative. All world-owned data is isolated by `WorldId`; worlds are isolated by `OwnerUserId`.

## Common conventions

- Primary keys: UUID/GUID.
- Persistent timestamps: UTC.
- Mutable rows generally include `CreatedAt`, `UpdatedAt`, and optionally a concurrency token.
- Soft deletion is used only where product behaviour needs recovery/history.
- All foreign-key delete behaviours must be explicit.
- Cursor indexes must match query ordering.

## Accounts

### Users

- Id
- AccountType: Guest or Registered
- Email nullable until registration
- PasswordHash nullable for external-only auth
- Status
- CreatedAt
- UpdatedAt

Unique email when non-null.

### DeviceInstallations

- Id
- UserId
- InstallationPublicId
- Platform
- LastSeenAt
- CreatedAt

Unique `InstallationPublicId`.

### RefreshTokens

Store only a secure token hash, rotation family, expiry, revocation state, and device relation.

## Worlds

### GameWorlds

- Id
- OwnerUserId
- Name
- CurrentWorldTime
- LastSimulatedAt
- Status
- CreatedAt
- UpdatedAt

Index `(OwnerUserId, CreatedAt)`.

### PlayerProfiles

- Id
- WorldId
- DisplayName
- Handle
- Bio
- Reputation
- Influence
- FollowersCount cache

Unique `WorldId`; one human player per world.

### WorldSettings

One row per world containing time scale, action limits, AI budget, content settings, and rule version.

### SimulationStates

One row per world containing next due time, last completed interval, and deterministic sequence state.

## Characters

### Characters

- Id
- WorldId
- DisplayName
- Handle
- Bio
- Age
- Profession
- Archetype
- WritingStyle
- ActivityLevel
- Influence
- Popularity
- CurrentMoodType
- Status
- CreatedAt

Unique `(WorldId, Handle)`.

### CharacterTraits

Prefer one row per character with explicit numeric trait columns for core stable traits. Flexible experimental traits may use a secondary table only when required.

### CharacterInterests

Unique `(CharacterId, TopicId)` with strength.

### CharacterOpinions

Unique `(CharacterId, TopicId)` with position, strength, confidence, and last changed time.

### CharacterGoals, CharacterSchedules, Careers

World ownership must be derivable and validated through character relation; include `WorldId` where it materially improves isolation and indexes.

## Actor identity

Use an `Actors` table to unify authorship and relationship endpoints within a world:

- Id
- WorldId
- ActorType: Player, Character, System
- PlayerProfileId nullable
- CharacterId nullable

Check constraint requires exactly one relevant reference for Player/Character and neither for System. Unique references prevent duplicate actor rows.

## Social

### Posts

- Id
- WorldId
- AuthorActorId
- ParentPostId nullable
- QuotePostId nullable
- Content
- Visibility fixed to private-world scope initially
- LikeCount cache
- ReplyCount cache
- RepostCount cache
- CreatedAt
- DeletedAt nullable

Indexes:

- `(WorldId, CreatedAt DESC, Id DESC)`
- `(WorldId, AuthorActorId, CreatedAt DESC, Id DESC)`
- `(ParentPostId, CreatedAt, Id)`

### PostReactions

Unique `(PostId, ActorId, ReactionType)`.

### Follows

Directional actor follows within the same world. Unique `(WorldId, FollowerActorId, FollowedActorId)`. Prevent self-follow.

### Reposts, Hashtags, PostHashtags, Mentions, Bookmarks

Add only when their milestone is implemented. All must enforce same-world ownership.

## Messaging

### Conversations

- Id
- WorldId
- ConversationType
- CreatedAt
- LastMessageAt
- IsActive

### ConversationParticipants

- ConversationId
- ActorId
- JoinedAt
- LeftAt nullable

For active direct player-character conversations, enforce uniqueness through a canonical direct-conversation key or dedicated columns/index so the pair has at most one active conversation.

### Messages

- Id
- WorldId
- ConversationId
- SenderActorId
- Content
- DeliveryStatus
- CreatedAt
- EditedAt nullable

Index `(ConversationId, CreatedAt DESC, Id DESC)`.

### MessageReadStates

Unique `(MessageId, ActorId)` or participant-level last-read cursor, selected based on implementation needs. Prefer participant `LastReadMessageId/At` for initial scale.

## Relationships

### Relationships

Directional row:

- Id
- WorldId
- SourceActorId
- TargetActorId
- Trust
- Familiarity
- Respect
- Affection
- Comfort
- Attraction
- Commitment
- Rivalry
- Jealousy
- RomanticStatus
- UpdatedAt

Unique `(WorldId, SourceActorId, TargetActorId)`. Prevent self relationship.

### RelationshipEvents

Immutable event history with source type/id, reason code, score deltas, old/new romantic status, occurred time, and idempotency key.

### RelationshipStatusHistory

Optional dedicated history if transition querying warrants it; avoid duplicating identical facts with `RelationshipEvents` unless there is a clear query need.

## Memories

### CharacterMemories

- Id
- WorldId
- CharacterId
- SubjectActorId nullable
- MemoryType
- Summary
- Importance
- EmotionalValue
- Confidence
- SourceType
- SourceId nullable
- CreatedAt
- LastRecalledAt nullable
- ExpiresAt nullable

Indexes for `(CharacterId, SubjectActorId, Importance DESC)` and active recall filtering.

## Simulation

### SimulationRuns

- Id
- WorldId
- IntervalStart
- IntervalEnd
- Seed
- RuleVersion
- Status
- StartedAt
- CompletedAt
- IdempotencyKey
- ErrorSummary nullable

Unique `(WorldId, IdempotencyKey)`.

### SimulationActions

- Id
- SimulationRunId
- WorldId
- ActorId
- ActionType
- TargetActorId nullable
- TargetPostId nullable
- TopicId nullable
- Stance nullable
- Tone nullable
- ReasonCode
- Status
- ScheduledAt
- ExecutedAt nullable
- IdempotencyKey

Unique `(WorldId, IdempotencyKey)`.

### WorldEvents, Trends, TrendSnapshots, Notifications

All contain `WorldId`; notifications additionally target the owning user/player.

## Concurrency and idempotency

- Use unique idempotency keys for simulation runs/actions and client create requests where retries are possible.
- Use transactions for mechanics that persist an event and update derived scores/counts.
- Cache counts are updated atomically and can be rebuilt from source rows.
- Use optimistic concurrency for mutable aggregate roots where simultaneous updates are plausible.

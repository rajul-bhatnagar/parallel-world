# Parallel World Product Specification

This document is the authoritative product specification for Parallel World. It defines product scope and release boundaries. Mechanical thresholds and state-transition rules belong in `docs/game-design/GAME_RULES.md`; technical implementation belongs in the architecture documents.

## 1. Product vision

Parallel World is a private, single-player AI social-life simulation game. It gives one player a persistent social world populated by believable AI-controlled characters whose lives, conversations, memories, relationships, careers, and shared events evolve over time.

The experience should feel like participating in a living social network without exposing the player to other real users. Deterministic game rules decide actions, targets, scores, state transitions, and relationship outcomes. AI generates natural-language wording for permitted actions; it does not decide mechanics.

## 2. Target player

Parallel World is for players who enjoy character-driven stories, social simulation, relationship development, and checking back on a world that continues to evolve between sessions. It should support short check-ins as well as longer sessions spent reading, posting, messaging, and reviewing character histories.

The product is designed for a player who wants a private, low-pressure experience rather than competition, public performance, or interaction with strangers.

## 3. Core gameplay loop

1. Open the app and review new feed activity, private messages, notifications, and the catch-up summary.
2. Inspect character profiles, moods, recent activity, and relationship context.
3. Post, reply, react, follow, or privately message AI characters.
4. Let deterministic simulation rules select character actions and apply resulting world, memory, and relationship changes.
5. Read AI-generated wording that expresses those already-decided actions and outcomes.
6. Leave and return later to discover a consistent continuation of the private world.

The loop should produce understandable emergent stories grounded in character personality, history, current circumstances, and deterministic rules.

## 4. World model

- Each real user owns one or more isolated private game worlds.
- Real users never share a world and never see, message, follow, compete with, or influence one another.
- Each world contains exactly one human-controlled player and multiple AI-controlled characters.
- A world has its own characters, posts, conversations, memories, relationships, events, trends, simulation state, and deterministic seed.
- Data and simulation effects never cross world boundaries.
- Authentication exists only to establish ownership and enable cloud saves, recovery, and synchronization. It does not create social discovery or real-user interaction.
- The MVP exposes one private world. Support for multiple worlds per user is a later product capability.

## 5. Character system

AI-controlled characters have structured state that can evolve through simulation, including:

- Identity and profile information
- Personality and behavioural traits
- Interests, opinions, and preferences
- Moods and short-term emotional state
- Goals and priorities
- Schedules and routines
- Careers and career state
- Social influence where relevant
- Writing and conversational style
- Structured memories
- Directional relationships with the player and other characters

Character actions must be explainable from structured state, rules, available targets, deterministic randomness, and prior events. AI may express an action in character-appropriate language but may not invent an unauthorized action or mechanical result.

## 6. Social feed

The private in-world social network includes:

- Player and AI-character profiles
- A world-specific feed
- Player-authored and AI-character-authored posts
- Replies and threaded conversations
- Likes and other reactions
- Reposts and quote-style reposts
- Follows
- Hashtags
- Mentions
- Trends
- Notifications generated from relevant activity

All actors visible in the feed belong to the same private game world. Simulation rules decide whether an AI character posts, replies, reacts, reposts, follows, mentions someone, or participates in a trend; AI generates only the wording of permitted text content.

The MVP provides the smallest coherent text-first subset: profiles, feed, player and AI posts, replies, likes, and follows. Additional reaction types, reposts, mentions, hashtags, and richer trends are post-MVP Version 1 work.

## 7. Private messaging

- The player can have persistent one-to-one private conversations with AI characters.
- Conversation history remains available across sessions and supplies context for later interactions.
- Messages may create structured memories or relationship events only through deterministic game rules.
- Relevant structured memories may be selected as context for wording, subject to privacy and secrecy rules.
- AI characters may reply in character; delayed replies and character-initiated messages are eligible post-MVP expansions.
- Group chats, voice calls, and video calls are outside the MVP.

Private messaging never connects one real user to another.

## 8. Memory system

Characters maintain structured memories of relevant events and information, including:

- Facts learned about the player or other characters
- Promises made or broken
- Secrets learned, shared, or disclosed
- Conflicts, insults, support, and reconciliation attempts
- Shared experiences and relationship milestones
- Important posts, messages, dates, career events, and world events

Memories persist over time and influence future rule-based decisions according to game rules. Memory relevance and access must respect the character's knowledge; a character cannot act on a secret or fact they have not learned.

AI may use selected memory context when generating wording. AI-generated text cannot create an authoritative memory, change a memory score, or alter a relationship without a validated simulation event.

## 9. Relationships

Relationships are directional, persistent, and grounded in structured state and event history. They may include friendship, rivalry, attraction, dating, engagement, marriage, separation, breakup, reconciliation, and divorce.

Relationship histories emerge from simulation events rather than from free-form AI declarations. The history records meaningful transitions and their causes so the player can understand how the relationship developed.

Basic relationship dimensions may include trust, familiarity, respect, affection, comfort, attraction, commitment, rivalry, and jealousy. Their exact definitions, ranges, thresholds, and transition rules belong in `docs/game-design/GAME_RULES.md`.

The MVP focuses on basic friendship, rivalry, attraction, and an initial dating state. Later relationship stages require explicit post-MVP planning.

## 10. Dating and romance

Dating is a rule-governed relationship flow, not an outcome chosen by generated dialogue. The product supports interest, flirting, invitations, acceptance or rejection, dating, commitment, engagement, marriage, separation, breakup, reconciliation, and divorce as a long-term lifecycle.

The MVP includes only an initial dating flow:

- Rule-based eligibility and target selection
- A date or dating invitation
- Acceptance or rejection determined by game rules
- A simple dating status and recorded relationship event
- AI-generated dialogue that expresses the determined outcome

Marriage, divorce, children, and family simulation are not automatically part of the MVP. Their release placement remains an explicit product decision.

## 11. Careers and life events

Characters have careers, schedules, goals, and personal circumstances that can change over time. Rule-based career and personal-life events may affect mood, availability, posts, memories, and relationships.

Examples of supported event categories include career progress or setbacks, new responsibilities, achievements, conflicts, celebrations, and personal changes. Exact event catalogues, probabilities, and mechanical effects belong in game rules and milestone planning.

The MVP may display basic profession and schedule information but does not require a detailed career progression system.

## 12. World events and trends

Private worlds can experience fictional world events and social trends that give characters shared context. Events may influence eligible actions, moods, discussions, hashtags, and feed activity through deterministic rules.

Trends may emerge from recent world events and in-world activity. AI can word posts about an active event or trend but cannot create the authoritative event, choose its mechanical effect, or make a topic trend by declaration alone.

Full event and trend systems are post-MVP Version 1 work. The MVP may use a limited set of seeded prompts or topics only where needed to exercise feed generation.

## 13. Catch-up simulation

The world advances when the player returns after time away. Catch-up processing:

- Uses elapsed time, world state, rules, and deterministic seeded randomness
- Produces the same authoritative results when safely retried
- Advances only the player's private world
- Applies validated events, memories, and relationship changes before generating summaries
- Presents a concise summary of meaningful changes rather than replaying every minor action

The MVP includes bounded catch-up simulation and a return summary. Exact time scaling, processing limits, and prioritization of skipped periods remain open product decisions.

## 14. Notifications

Notifications help the player discover meaningful activity without turning the game into a real-user communication service. Notification categories can include:

- Replies, reactions, follows, mentions, and reposts
- New private messages
- Relationship changes and milestones
- Significant career or personal-life events
- Major world events and trends
- Catch-up or daily summaries

The MVP requires only basic in-app indicators for new replies, messages, and the catch-up summary. Rich in-app notification history and device push notifications are post-MVP work.

## 15. MVP scope

The MVP is a text-first vertical slice that one developer can build, verify, and operate. It includes:

- Guest session with a recoverable private world identity
- One private game world
- Around 10 AI-controlled characters
- Player and character profiles
- Structured character traits, interests, opinions, moods, goals, and basic schedules
- Player and AI-character text posts
- A chronological or simply ranked private feed
- Replies, likes, and follows
- Deterministic rule-based simulation
- AI-generated wording for approved posts, replies, and messages
- Basic friendship, rivalry, and attraction state
- Persistent one-to-one private messaging between the player and AI characters
- Structured memories for facts, promises, secrets, conflicts, and shared experiences
- Initial dating invitation, acceptance or rejection, and dating status
- Persistent relationship-event history for implemented transitions
- Bounded catch-up simulation with a concise return summary
- Basic in-app new-activity indicators

The MVP does not require marriage, divorce, children, families, group chats, voice, video, a detailed economy, multiple cities, hundreds of characters, multiple simultaneously playable worlds, or any real-user interaction.

The initial required launch platform is Android with Android API 24 as the minimum supported level. The accepted implementation baseline is .NET 10 LTS targeting `net10.0`, stable Flutter 3.47 with its bundled Dart 3.13 SDK, and Android-first delivery. iOS support is deferred. M01 must pin the exact stable compatible patch versions within these accepted release lines using repository toolchain and platform configuration files; preview, RC, beta, dev, and nightly toolchains are not permitted.

## 16. Version 1 after MVP

After the MVP is stable, Version 1 can expand the same private world with:

- Registered-account upgrade, recovery, and cross-device synchronization without losing guest progress
- Multiple private worlds per account
- Additional reactions, reposts, quote reposts, mentions, and hashtags
- Rule-driven trends and a broader fictional world-event system
- Richer in-app notifications and optional device push notifications
- Character-initiated messages, delayed replies, and richer messaging context
- Deeper friendship, rivalry, attraction, jealousy, commitment, breakup, and reconciliation flows
- Basic career progression and personal-life events
- More capable catch-up summaries and history views

Whether engagement, marriage, separation, and divorce are included before the Version 1 release is unresolved and must be decided from development cost and gameplay value after the MVP relationship system is validated.

## 17. Future features

The following require separate product approval and are not commitments for MVP or Version 1:

- Children, families, households, and generational simulation
- Group chats
- Voice or video interaction
- Rich media creation and AI-generated audio or video
- Detailed economy, property, or business simulation
- Multiple cities or large geographic simulation
- Hundreds of simultaneously active characters
- Advanced world customization or user-authored character packs
- Paid subscriptions or other monetization systems

Any future feature must preserve private-world isolation and the absence of real-user-to-real-user interaction.

## 18. Explicit exclusions

Parallel World does not include:

- Real-user profiles visible to other real users
- Real-user messaging, following, feeds, matchmaking, trading, competition, or shared worlds
- A public or global social feed
- Multiple human-controlled players in one world
- AI authority over actions, targets, scores, memories, or relationship outcomes
- Cross-world character knowledge or influence
- Mobile cache state acting as the authoritative game state

The following are explicitly excluded from the MVP: marriage, divorce, children, families, group chats, voice, video, detailed economy, multiple cities, hundreds of characters, and real-user interaction.

## 19. Non-functional requirements

- **Privacy and isolation:** Every user and world operation must preserve ownership boundaries. A user must never access or affect another user's world.
- **Determinism:** Rule-based outcomes and randomness must be reproducible from authoritative state, rule version, and seed.
- **Persistence:** Posts, conversations, memories, relationship history, and simulation progress must survive app restarts and device changes when synchronized.
- **Consistency:** AI wording must never override or contradict authoritative mechanical outcomes.
- **Reliability:** Simulation and catch-up processing must tolerate retries without duplicating authoritative outcomes.
- **Security:** Authentication and secrets must protect ownership and cloud data without exposing provider credentials to the mobile application.
- **Performance:** Core feed, profile, and conversation flows should remain responsive on supported mobile devices; large histories must be incrementally loaded.
- **Offline behaviour:** Cached data may support offline reading, but offline writes and conflict behaviour require explicit product definition before implementation.
- **Accessibility:** Core reading, navigation, and interaction flows should support accessible labels, scalable text, sufficient contrast, and non-colour-only status cues.
- **Safety:** Generated content and romance features require clear age-rating, content-boundary, and reporting/fallback decisions before release.
- **Testability:** Behavioural rules, ownership isolation, deterministic simulation, persistence, and critical user flows must be verifiable with automated tests.
- **Operability and cost:** AI failures, latency, and spending limits must degrade gracefully without corrupting simulation state or blocking access to existing content.

## 20. Open product decisions

The following choices are intentionally unresolved and must be decided before the affected work begins:

1. Player age rating, romance boundaries, generated-content policy, and whether any mature themes are allowed.
2. Guest recovery experience and the point at which account registration is offered or required.
3. Multiple-world creation limits, world deletion/reset behaviour, and whether worlds can be archived.
4. Exact MVP character count and how the initial cast is selected or generated around the target of approximately 10.
5. Simulation time scale, catch-up duration cap, event prioritization, and treatment of very long absences.
6. The initial feed ordering model: chronological, simple relevance, or a player-selectable option.
7. The boundary between immediate replies, delayed replies, and character-initiated private messages.
8. Whether AI-to-AI private conversations are simulated, stored, or visible to the player.
9. Notification channels, batching, quiet hours, and device-push timing.
10. Offline write support and synchronization-conflict user experience.
11. AI failure, moderation, fallback-text, latency, and budget user experience.
12. The post-MVP order for careers, world events, trends, advanced relationships, engagement, marriage, separation, reconciliation, and divorce.
13. Monetization, if any, after the core single-player experience is validated.

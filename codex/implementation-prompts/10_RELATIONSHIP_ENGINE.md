# Implementation Prompt — Relationship Engine

```text
Read AGENTS.md, docs/milestones/M10_RELATIONSHIPS.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Relationship Engine

Scope:
Implement directional Relationship and immutable RelationshipEvent with exactly nine dimensions: Familiarity, Trust, Respect, Affection, Comfort, Rivalry, Jealousy, Attraction, and Commitment. Implement clamping, idempotent deltas, derived friendship/rival/enemy states, APIs and Flutter summary/timeline basics, with deterministic tests. Exclude romance transitions and any tenth hidden score.

Use ADR-025/026 for deterministic Jaccard `InterestOverlap`, weighted directional `RelationshipRelevance`, neutral M10 v1 REL-01 multiplier defaults with the two existing exact exceptions, persisted Character Reputation (0-100, default/backfill 50), action-to-event mapping, and autonomous-rule availability. Do not infer mechanics from natural-language content. FOLLOW-01 is fully evaluable. Initial Follow, Like, Unlike, and generic Reply intentionally create no REL-01 delta. REPLY-01 remains gated by `mood_activation_unavailable`; REACT-01 remains gated by `positive_mood_unavailable`, then GoalRelevance, then repetition semantics. ACT-01 and POST-01 remain gated by ADR-020.

Explicit exclusions:
- No romance transitions, dating, secrets/promises, or AI-decided relationship changes.

Tests:
- Test all nine directional values, clamping/caps, immutable events, idempotency, derived states, same-world constraints, safe API projection, InterestOverlap, RelationshipRelevance, neutral multiplier defaults and approved exceptions, authoritative action mapping, autonomous-rule gates, and UI.

Before editing:
1. List relevant existing files.
2. Provide a short plan.
3. State assumptions and risks.

Requirements:
- Implement only this task.
- Do not implement future milestones.
- Enforce user/world ownership where applicable.
- Add migrations only for schema changes introduced by this milestone; otherwise report Not applicable.
- Add or update automated tests.
- Do not add secrets.
- Explain any package added.
- Update documentation when behaviour changes.

Verification:
- Run only milestone-applicable formatting, builds/analyzers, tests, migration checks, and manual checks.
- Do not invent paths, projects, tools, or checks that an earlier milestone has not created.
- Report every command/check as Passed, Failed, Unavailable, or Not applicable — with reason.

Completion report:
- Summary
- Changed files
- Important decisions
- Tests and results
- Manual verification
- Remaining risks
- Suggested commit message
```

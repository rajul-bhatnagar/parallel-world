# Implementation Prompt — Rule-Based Simulation

```text
Read AGENTS.md, docs/milestones/M08_SIMULATION.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Rule-Based Simulation

Scope:
Implement SimulationRun, SimulationAction, deterministic random provider, world clock, activity selector, decision creation/execution separation, template text generator, create-post/reply/like actions, deterministic FOLLOW-01 unavailable/ineligible evaluation, development trigger, idempotency and transaction tests. No external AI, messaging, memory, or dating.

Autonomous follow phase boundary:
- M08 must not synthesize, infer, persist, or proxy Familiarity, Trust, Affection, or Rivalry.
- Without M10 relationship state, autonomous Character FOLLOW-01 evaluation is deterministically unavailable/ineligible and creates no SimulationAction, Follow row, or follow-change event. Do not use fallback randomness.
- Existing M07 Player follow/unfollow behavior remains operational and unchanged.
- M10 activates autonomous FOLLOW-01 eligibility using real relationship state; do not implement or design that state in M08.

World display timezone contract:
- Add required `WorldSettings.DisplayTimeZoneId`, persisted as `display_time_zone_id`, containing an IANA timezone identifier.
- The migration and new-world creation default to the exact identifier `UTC`; backfill existing worlds to `UTC` before enforcing non-nullability.
- Persist canonical simulation/event/action timestamps in UTC. Convert the relevant simulation UTC instant through the world timezone only to derive schedule and quiet-hour local date/time.
- Use timezone database DST rules. Do not accept an ambiguous/invalid local timestamp as engine input, because evaluation starts from UTC.
- Do not infer the timezone from the server, device, locale, IP address, or operating system. Do not add per-character timezone state.
- Reject an explicitly supplied invalid/non-IANA timezone using the standard validation/ProblemDetails contract; do not silently fall back to UTC.

Explicit exclusions:
- No external AI, messaging, memory, romance, catch-up, or deferred events/trends.

Tests:
- Test deterministic seeds/order, interval uniqueness, idempotency, concurrency, transaction boundaries, fallback wording, and cross-world rejection.
- Verify autonomous follow ineligibility without relationship state, no autonomous follow rows, unaffected M07 Player follows, no temporary relationship persistence, deterministic repeated follow eligibility results, and no M10 tables/entities/fields.
- Verify existing/new-world `UTC` defaults, UTC and non-UTC IANA schedule evaluation, quiet-hour conversion boundaries, different-zone eligibility for one UTC instant, deterministic repeat behavior, host-timezone independence, DST conversion from UTC, invalid timezone rejection, and no per-character timezone state.

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

# Implementation Prompt — Catch-Up Simulation

```text
Read AGENTS.md, docs/milestones/M15_CATCHUP.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, docs/development/FLUTTER_GUIDELINES.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Catch-Up Simulation

Scope:
Implement bounded compressed simulation by reusing SimulationRun with RunType CatchUp; persist requested/processed/remaining intervals, relational bucket checkpoints, Partial/Completed/retryable status, retry/resume state, selected/aggregate mechanics, idempotency/concurrency ownership, timeout/work budgets, CatchUpSummary and committed-fact items, the authoritative latest-summary route/UI, indexes/retention behavior, and short/long/duplicate/failure tests.

Run granularity:
- Normal M08 processing remains one fixed 15-minute interval per SimulationRun and at most one interval per normal trigger.
- One bounded M15 CatchUp operation is one SimulationRun with RunType CatchUp. Its six-hour/daily buckets, compression, aggregate mechanics, and partial checkpoints are internal to that run.
- Partial/retry state belongs to that CatchUp run and its relational checkpoints. Do not add a second batch-run abstraction and do not create one SimulationRun per historical 15-minute interval.
- CatchUp determinism is measured against identical CatchUp inputs, seed, bucket/checkpoint state, and compressed gameplay outcome. Exact SimulationRun counts, SimulationAction counts, and PRNG draw sequences from repeated M08 triggers are not an equivalence requirement.

Explicit exclusions:
- No minute-by-minute simulation, unbounded work, full M14 events/trends, duplicate run infrastructure, or AI-invented summary facts.

Tests:
- Test CatchUp SimulationRun identification and one-run ownership, processed/remaining intervals, relational checkpoints, Partial/completion/retry/resume, deterministic compressed replay, unchanged normal M08 granularity, summary/item persistence, idempotency/concurrency, ownership, retention behavior, fallback wording, and UI states. Do not require CatchUp to match the run/action counts or PRNG draw sequence of repeated M08 triggers.

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

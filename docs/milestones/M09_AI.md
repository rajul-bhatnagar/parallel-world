# M09 — AI text generation

## Goal
Add provider wording to already-decided actions without changing mechanics.

## User-visible result
Character text is more natural; failures still show deterministic fallback.

## Dependencies
M08 persisted decisions; approved provider/config choice.

## Scope
- **Backend:** Provider-neutral interface/adapter, minimized context, request/result metadata, timeouts/budgets/retry classification, output validation/duplicate detection/moderation hook, fallback.
- **Database:** Generation request/result/work metadata without full sensitive prompts/raw responses; idempotency and migration.
- **Flutter:** Display persisted wording/fallback normally; no provider call/key or mechanical assumption.
- **Infrastructure:** Runtime secret injection and staging budget; fake provider by default locally/CI.

## Explicit exclusions
AI-selected actions/targets/scores/memories, full chat history, mobile provider access, real AI in automated tests.

## Test scope
Success, failure, timeout, invalid/empty/excessive/duplicate, hostile prompt as data, context access, no mechanical mutation, sanitized logs.

## Security and ownership considerations
Mechanical capability boundary, privacy, provider secrets, retries/cost. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Mechanical records are identical with fake success/failure; fallback completes safely; secrets/private context do not leak.

## Required verification
Unit/stub integration/security/architecture tests and budget/fallback manual check. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Staging wording under strict budget, forced timeout/invalid output, inspect logs/context metadata.

## Exit criteria
AI affects wording only and provider failure cannot break mechanics.

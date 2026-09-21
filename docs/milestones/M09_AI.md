# M09 — AI text generation

## Goal
Add provider wording to already-decided actions without changing mechanics.

## User-visible result
Character text is more natural; failures still show deterministic fallback.

## Dependencies
M08 persisted decisions; ADR-024 local-first provider/configuration contract.

## Scope
- **Backend:** Provider-neutral interface, one Ollama adapter, minimized structured context, request/result metadata, configurable 10-second default timeout, one transient retry, output/resource budgets, output validation/duplicate detection, application-level content-safety hook, and deterministic fallback.
- **Database:** Generation request/result/work metadata without full sensitive prompts/raw responses; idempotency and migration.
- **Flutter:** Display persisted wording/fallback normally; no provider call/key or mechanical assumption.
- **Infrastructure:** Optional local Ollama configured server-side with preferred model `qwen3:4b`; no paid API dependency or automatic paid fallback; deterministic fake provider and local HTTP stub for automated tests/CI.

## Explicit exclusions
AI-selected actions/targets/scores/memories, full chat history, mobile provider access, real AI in automated tests, mandatory Ollama startup/model download, hosted-provider routing, paid API/moderation dependencies, and automatic cloud fallback.

## Test scope
Fake/stub success, disabled/unavailable provider, transient retry, timeout/cancellation, invalid/empty/excessive/duplicate output, hostile prompt as data, context access, deterministic fallback, no mechanical mutation, sanitized logs, and no external network/model requirement.

## Security and ownership considerations
Mechanical capability boundary, privacy, backend-only provider configuration, local resource budgets, bounded retry, and future-provider secret isolation. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Mechanical records are identical with fake success/failure; Ollama absence never blocks startup or gameplay; fallback completes safely; automated tests require no AI network/model; secrets/private context do not leak; no paid provider is called.

## Required verification
Unit/stub integration/security/architecture tests and budget/fallback manual check. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Optional local Ollama wording with fictional data and strict resource limits; disabled/unavailable provider, forced timeout/invalid output, deterministic fallback, and sanitized logs/context metadata.

## Exit criteria
AI affects wording only and provider failure cannot break mechanics.

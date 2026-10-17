# Implementation Prompt — AI Text Generation

```text
Read AGENTS.md, docs/milestones/M09_AI.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: AI Text Generation

Scope:
Implement IAiTextGenerator, request/result models, prompt builder, deterministic template fallback, one local Ollama adapter behind the provider-neutral AI boundary, configurable provider/model/10-second default timeout/output limit/template version, one transient connection/timeout retry, output validation/duplicate detection, resource-budget controls, safe logging, generation diagnostics, deterministic fake-provider tests, and process-local HTTP stub integration tests. Prefer configurable `qwen3:4b`; Ollama/model availability is optional and AI may write wording only.

Explicit exclusions:
- ADR-024 resolves the M09 provider contract. Do not add paid API dependencies, automatic cloud fallback, multiple-provider routing, a mandatory paid moderation service, real Ollama/model downloads in CI, AI-selected/mutated mechanics, or future AI features.
- The default flow is `Ollama -> deterministic fallback`; backend startup and gameplay must remain functional when Ollama is disabled, absent, unreachable, timed out, missing the configured model, or returns invalid output.

Tests:
- Use a deterministic fake provider and process-local HTTP stub for automated tests covering success, disabled/unavailable provider, exactly one transient retry, timeout/cancellation, validation, fallback, resource budgets, duplicate detection, hostile text as delimited data, redaction, and mechanical invariance.
- Automated tests require no external network, running Ollama server, model download, paid API, provider credential, or billing account. A real-Ollama check is optional local manual verification only.

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

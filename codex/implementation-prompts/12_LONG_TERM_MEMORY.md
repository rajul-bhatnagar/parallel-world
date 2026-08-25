# Implementation Prompt — Long-Term Memory

```text
Read AGENTS.md, docs/milestones/M12_MEMORY.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Long-Term Memory

Scope:
Implement structured CharacterMemory; Secret, SecretKnower, disclosure/provenance and confidentiality boundaries; Promise with Active/Kept/Broken/Cancelled lifecycle; source references; importance/emotion/confidence/expiry; deterministic creation/resolution from explicit game events; same-world and idempotency rules; bounded recall; authorized AI context for posts/messages; privacy tests; and internal application contracts. Do not expose raw public memory APIs or use AI to invent/extract authoritative state.

Explicit exclusions:
- No raw public memory/secret/promise APIs, AI-invented authoritative memories, cross-world knowledge, or deferred memory features.

Tests:
- Test CharacterMemory, secrets/knowers/disclosure provenance/confidentiality, Promise Active/Kept/Broken/Cancelled transitions, deterministic creation/recall, privacy, same-world constraints, and idempotency.

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

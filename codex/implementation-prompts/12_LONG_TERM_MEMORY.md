# Implementation Prompt — Long-Term Memory

```text
Read AGENTS.md, docs/milestones/M12_MEMORY.md, docs/product/PRODUCT.md, docs/game-design/GAME_RULES.md, docs/architecture/ARCHITECTURE.md, docs/architecture/DATABASE.md, docs/architecture/API_CONVENTIONS.md, docs/architecture/SECURITY.md, and docs/development/TEST_STRATEGY.md. Inspect the repository, current branch, status, and relevant diffs before editing.

Task: Long-Term Memory

Scope:
Implement world/owner-scoped CharacterMemory with exactly Fact, Preference, Event, Secret, and Promise; structured authoritative source/subject/topic provenance; exact confidence/importance/CharacterPrivate visibility; provenance-keyed durable creation outcomes and source replay uniqueness; the exact MEM-02 formula, rounding, ordering, and top-eight bound; serialized per-Character 100-active-memory retention with deterministic non-protected eviction and `memory_capacity_protected` rejection when all 100 are protected; Secret storage with disclosure inactive; Promise Active/Fulfilled/Cancelled/Expired lifecycle driven only by structured authoritative conditions; same-world constraints; recall audit records; authorized memory context for eligible M11 replies; privacy tests; and internal application contracts. Do not expose raw public memory APIs or use text/AI to invent, extract, classify, or change authoritative state.

Explicit exclusions:
- No raw public memory/secret/promise APIs, AI/text-invented authoritative memories, arbitrary topic strings, embeddings/vector search, semantic classification, Secret disclosure pressure/leakage, cross-world/cross-Character knowledge, full-history context, M13 behavior, or other deferred memory features.

Tests:
- Test approved structured-source mapping and prose rejection; exact confidence/importance/visibility; subject/topic/relationship matching; exact decimal recall score/rounding/order/top-eight bound; ordinary no-expiry; 99 protected plus one non-protected eviction; 100-protected `memory_capacity_protected` rejection; unchanged protected rows; active count never above 100; deterministic rejected replay/concurrency; no AI/provider capacity choice; Secret non-disclosure; Promise Active/Fulfilled/Cancelled/Expired transitions; replay uniqueness; privacy/world/owner constraints; and M11/M09 mechanical separation.

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

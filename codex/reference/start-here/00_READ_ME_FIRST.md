# Parallel World — Codex Development Kit

This kit is an ordered instruction system for developing a private, single-player AI social-life simulation game using Codex.

## Product model

Each real account owns one or more completely private game worlds. Inside each world there is exactly one human-controlled player and many AI-controlled characters. Real users never meet, message, follow, or influence one another.

## Core engineering principle

> Simulation rules decide what happens. The database records what happened. AI decides how characters express it.

## How to use this kit

1. Open the reorganized repository root. Do not extract another copy of the development kit into it.
2. Read `codex/reference/start-here/01_PROMPT_ORDER.md`.
3. Give Codex prompts in the exact numbered sequence.
4. Do not skip the product, rules, architecture, and database stages.
5. Review every generated document before proceeding.
6. Implement one milestone and one vertical slice at a time.
7. Keep repository instructions in `AGENTS.md` and accepted product, game, architecture, security, and quality rules in `docs/`.
8. Do not allow implementation prompts to silently rewrite source-of-truth files.

## Folder purpose

- `codex/reference/start-here/`: usage instructions and ordered workflow.
- `AGENTS.md` and `docs/`: authoritative product, game, architecture, database, API, security, and quality rules.
- `codex/planning-prompts/`: prompts that ask Codex to finalize planning documents.
- `codex/implementation-prompts/`: ordered implementation prompts.
- `codex/review-prompts/`: independent architecture, security, database, backend, Flutter, simulation, and release reviews.
- `codex/templates/`: reusable templates for future work.
- `codex/reference/`: naming, branching, commands, environment variables, and definition-of-done references.
- `docs/milestones/`: milestone acceptance criteria and checklists.

## Important warning

Do not give Codex one prompt asking it to build the whole game. Large prompts encourage architectural drift, incomplete tests, inconsistent entities, and accidental implementation of future features.

Working prompts never override `AGENTS.md` or accepted documents under `docs/`. Stop and report any conflict before editing implementation or source-of-truth files.

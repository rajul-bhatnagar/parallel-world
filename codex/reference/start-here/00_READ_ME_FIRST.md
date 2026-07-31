# Parallel World — Codex Development Kit

This kit is an ordered instruction system for developing a private, single-player AI social-life simulation game using Codex.

## Product model

Each real account owns one or more completely private game worlds. Inside each world there is exactly one human-controlled player and many AI-controlled characters. Real users never meet, message, follow, or influence one another.

## Core engineering principle

> Simulation rules decide what happens. The database records what happened. AI decides how characters express it.

## How to use this kit

1. Extract the entire folder into the root of a new Git repository.
2. Read `codex/reference/start-here/01_PROMPT_ORDER.md`.
3. Give Codex prompts in the exact numbered sequence.
4. Do not skip the product, rules, architecture, and database stages.
5. Review every generated document before proceeding.
6. Implement one milestone and one vertical slice at a time.
7. Keep accepted product and game rules in `AGENTS.md` and `docs/`.
8. Do not allow implementation prompts to silently rewrite source-of-truth files.

## Folder purpose

- `codex/reference/start-here`: usage instructions and ordered workflow.
- `AGENTS.md` and `docs/`: authoritative product, game, architecture, database, API, security, and quality rules.
- `codex/planning-prompts`: prompts that ask Codex to finalize planning documents.
- `codex/implementation-prompts`: ordered implementation prompts.
- `codex/review-prompts`: independent architecture, security, database, backend, Flutter, simulation, and release reviews.
- `codex/templates`: reusable templates for future work.
- `codex/reference`: naming, branching, commands, environment variables, and definition-of-done references.
- `docs/milestones`: milestone acceptance criteria and checklists.

## Important warning

Do not give Codex one prompt asking it to build the whole game. Large prompts encourage architectural drift, incomplete tests, inconsistent entities, and accidental implementation of future features.

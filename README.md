# Parallel World

Parallel World is a private, single-player AI social-life simulation game. Each account owns isolated game worlds containing one human-controlled player and AI-controlled characters; real users never interact with or influence one another.

## Engineering principle

Simulation rules decide what happens, PostgreSQL records what happened, and AI decides how characters express it.

## Repository governance

Implementation is governed by the authoritative documents in this repository:

- `AGENTS.md` defines project-wide engineering instructions.
- `docs/product/PRODUCT.md` defines product scope and MVP boundaries.
- `docs/game-design/GAME_RULES.md` defines simulation behaviour.
- `docs/architecture/` defines architecture, data, API, and security rules.
- `docs/development/` defines testing and records accepted decisions.
- `docs/milestones/` defines milestone acceptance criteria.

The files under `codex/` are ordered working prompts, templates, and references. They instruct development work but do not replace the authoritative documents above.

If repository guidance conflicts with an accepted source-of-truth document, stop and resolve the conflict before implementation. Do not silently invent a product rule, game mechanic, or architecture decision.

## Application areas

- `backend/` — ASP.NET Core modular-monolith backend
- `mobile/` — Flutter mobile application
- `infrastructure/` — local and production infrastructure

Application code will be added one milestone at a time after the planning documents have been reviewed and accepted.

## Development workflow

1. Read `AGENTS.md` and the relevant source-of-truth documents.
2. Follow `codex/reference/start-here/01_PROMPT_ORDER.md` in sequence.
3. Review and approve planning outputs before implementation.
4. Update the persistent `dev` branch from the approved repository state, confirm a clean working tree, and perform milestone work on `dev`.
5. Implement only the current milestone or explicitly requested task.
6. Run all applicable checks in `codex/reference/VERIFICATION_COMMANDS.md` and the current milestone checklist.
7. Inspect the diff, record unresolved risks, and commit a self-contained change.
8. Push `dev`. A pull request is not required after every milestone.
9. When `dev` reaches an approved stable checkpoint, open a pull request from `dev` into `main`, require all applicable CI checks to succeed, and review the pull request, including the required independent Codex diff review.
10. Merge the reviewed `dev`-to-`main` pull request only after the applicable CI and review gates pass.

A milestone is complete on `dev` when its acceptance criteria and verification pass, its diff is reviewed, and its milestone-specific commit is pushed to `dev`. Promotion to stable/release-ready `main` is a separate checkpoint: it requires a reviewed `dev`-to-`main` pull request with applicable CI. A reviewed local merge is not an accepted substitute for that promotion pull request. Direct feature development on `main` is not allowed. Branch and commit naming rules are documented in `codex/reference/NAMING_CONVENTIONS.md`.

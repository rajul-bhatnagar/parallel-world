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
4. Start from an updated `main`, confirm a clean working tree, and create the required milestone branch defined by `docs/development/DEVELOPMENT_PLAN.md` and `codex/reference/NAMING_CONVENTIONS.md`.
5. Implement only the current milestone or explicitly requested task.
6. Run all applicable checks in `codex/reference/VERIFICATION_COMMANDS.md` and the current milestone checklist.
7. Inspect the diff, record unresolved risks, and commit a self-contained change.
8. Push the milestone branch and open a pull request into `main`.
9. Require all applicable CI checks to succeed and review the pull request, including the required independent Codex diff review.
10. Merge the reviewed pull request into `main` only after the applicable CI and review gates pass.

A local commit or reviewed local merge does not complete an implementation milestone. Milestone completion requires the push, pull-request, applicable-CI, review, and merge workflow above. Branch and commit naming rules are documented in `codex/reference/NAMING_CONVENTIONS.md`.

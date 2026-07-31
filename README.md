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

## Application areas

- `backend/` — ASP.NET Core modular-monolith backend
- `mobile/` — Flutter mobile application
- `infrastructure/` — local and production infrastructure

Application code will be added one milestone at a time after the planning documents have been reviewed and accepted.

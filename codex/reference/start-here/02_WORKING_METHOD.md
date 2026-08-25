# Working Method with Codex

## One thread per responsibility

Use separate Codex threads for:

- Product and architecture planning
- Backend implementation
- Flutter implementation
- Simulation and game-rule implementation
- Independent code review

Do not let two agents modify the same contracts, migrations, or shared models at the same time.

Before assigning or starting work, inspect `git status`, relevant diffs, and existing code. Preserve unrelated changes and stop if another task is modifying overlapping files or contracts.

## Persistent dev branch

Branch roles and optional examples:

```text
main
dev
feature/private-messaging
fix/message-pagination
```

`main` is stable/release-ready. `dev` is the persistent active development and integration branch, and M01 through later milestones are implemented sequentially on `dev`. Direct feature development on `main` is not allowed. Keep milestone-specific commits self-contained, do not mix unrelated work, and do not force-push or rewrite shared history.

After each milestone, run applicable verification, inspect the diff, create a milestone-specific commit, and push `dev`. A pull request is not required after every milestone. When `dev` reaches an approved stable checkpoint, open a pull request from `dev` into `main`, require successful applicable CI checks, review it, and merge through that pull request. A reviewed local merge is not an accepted alternative to stable promotion. Short-lived `feature/...` branches are optional for isolated or risky work and integrate into `dev`; they are not required by the milestone workflow. Codex must not perform push, pull-request, or merge operations unless the user explicitly authorizes them.

## One vertical slice at a time

A vertical slice should include, where relevant:

- Domain model
- Persistence mapping and migration
- Application behaviour
- API endpoint
- Flutter repository/state/UI
- Automated tests
- Documentation update

## Definition of a safe Codex task

A task is small enough when:

- It has one measurable user-visible or engineering result.
- Its acceptance criteria fit on one page.
- It can be tested independently.
- It can be committed without unfinished future features.
- It does not require Codex to redesign multiple modules silently.

## Never accept these behaviours

- Codex claims tests passed without showing commands or results.
- AI output directly changes trust, dating state, reputation, or other mechanics.
- World-specific rows are created without `WorldId` ownership.
- Mobile application secrets contain server API keys.
- Database schema changes have no migration.
- Random simulation behaviour cannot be reproduced with a seed.
- A free-hosting restart can duplicate simulation runs.
- Verification failures or skipped checks are hidden behind a claim that work is complete.

## Review handling

Critical and High findings must be resolved before merge. Medium and Low findings must be recorded and assessed against acceptance criteria, security, data integrity, and user-visible risk; they are not silently discarded.

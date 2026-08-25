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

## One branch per milestone

Required milestone branch pattern and examples:

```text
feature/m01-repository-bootstrap
feature/m02-backend-foundation
feature/m05-character-catalogue
feature/m11-private-messaging
fix/message-pagination
```

Create milestone branches from an up-to-date `main`. Keep one milestone per branch, do not mix unrelated work, and do not force-push, rewrite shared history, or commit directly to `main` unless explicitly authorized.

For M01, the branch is exactly `feature/m01-repository-bootstrap`; it is not optional or merely recommended. After applicable verification and diff review, commit and push the milestone branch, open a pull request into `main`, require successful applicable CI checks, review the pull request, and merge through that pull request. A local commit or reviewed local merge is not an accepted alternative. Codex must not perform push, pull-request, or merge operations unless the user explicitly authorizes them.

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

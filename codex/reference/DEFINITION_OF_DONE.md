# Definition of Done

A task is done only when:

- Acceptance criteria are met.
- Ownership and security checks are present.
- Every world-owned persistent record has an explicit `WorldId` boundary and server-side ownership enforcement.
- Schema changes include migrations.
- Unit/integration/widget tests cover behaviour.
- Applicable formatting checks and builds pass.
- Relevant tests pass.
- Any failed or unavailable check is reported with its command and reason; the task is not described as fully verified until required checks pass or the exception is explicitly accepted.
- No secrets are introduced.
- Error/loading/empty states are handled where applicable.
- Documentation is updated.
- Diff was reviewed.
- Remaining risks are recorded.
- A commit can stand independently without hidden future work.
- No future-milestone or unrelated work is included.

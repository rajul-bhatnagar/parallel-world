# Definition of Done

A task is done only when:

- Acceptance criteria are met.
- Ownership and security checks are present.
- Every world-owned persistent record has an explicit `WorldId` boundary and server-side ownership enforcement.
- Schema changes include migrations; when no schema changes exist, migration verification is Not applicable with reason.
- Applicable unit/integration/widget tests cover behaviour; checks for projects or surfaces not yet created are Not applicable or Unavailable with reason.
- Applicable formatting checks and builds pass.
- Relevant tests pass.
- Every check is reported as Passed, Failed, Unavailable, or Not applicable — with the exact command/check and reason for non-pass results. The task is not described as fully verified until required applicable checks pass or an exception is explicitly accepted.
- No secrets are introduced.
- Error/loading/empty states are handled where applicable.
- Documentation is updated.
- Diff was reviewed.
- Remaining risks are recorded.
- A commit can stand independently without hidden future work.
- No future-milestone or unrelated work is included.
- An implementation milestone is complete on `dev` when its acceptance criteria and applicable verification pass, its diff is reviewed, and its milestone-specific commit is pushed to `dev`. This does not make it stable/release-ready on `main`.
- Promotion from `dev` to stable/release-ready `main` occurs only at an approved checkpoint through a pull request with applicable CI and review. A reviewed local merge is not an accepted substitute.

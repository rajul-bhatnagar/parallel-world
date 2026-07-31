# Exact Prompt Order

Use the following sequence. Do not move forward until the output of the current stage is reviewed and accepted. A later prompt does not authorize silently changing an accepted output from an earlier stage.

## Planning sequence

1. `codex/planning-prompts/01_CREATE_REPOSITORY_GOVERNANCE.md`
2. `codex/planning-prompts/02_FINALIZE_PRODUCT_SPEC.md`
3. `codex/planning-prompts/03_FINALIZE_GAME_RULES.md`
4. `codex/planning-prompts/04_FINALIZE_ARCHITECTURE.md`
5. `codex/planning-prompts/05_FINALIZE_DATABASE_DESIGN.md`
6. `codex/review-prompts/01_REVIEW_DATABASE_AND_OWNERSHIP.md`
7. `codex/planning-prompts/06_FINALIZE_API_CONVENTIONS.md`
8. `codex/planning-prompts/07_FINALIZE_FLUTTER_GUIDELINES.md`
9. `codex/planning-prompts/08_FINALIZE_SECURITY_AND_AUTH_EVOLUTION.md`
10. `codex/planning-prompts/09_FINALIZE_TEST_STRATEGY.md`
11. `codex/planning-prompts/10_CREATE_DEVELOPMENT_PLAN.md`

## Implementation sequence

12. `codex/implementation-prompts/01_BOOTSTRAP_REPOSITORY.md`
13. `codex/implementation-prompts/02_BACKEND_FOUNDATION.md`
14. `codex/implementation-prompts/03_GUEST_SESSION_AND_WORLD.md`
15. `codex/implementation-prompts/04_FLUTTER_FOUNDATION.md`
16. `codex/implementation-prompts/05_CHARACTER_CATALOGUE.md`
17. `codex/implementation-prompts/06_SOCIAL_FEED.md`
18. `codex/implementation-prompts/07_REACTIONS_REPLIES_FOLLOWS.md`
19. `codex/implementation-prompts/08_RULE_BASED_SIMULATION.md`
20. `codex/implementation-prompts/09_AI_TEXT_GENERATION.md`
21. `codex/implementation-prompts/10_RELATIONSHIP_ENGINE.md`
22. `codex/implementation-prompts/11_PRIVATE_MESSAGING.md`
23. `codex/implementation-prompts/12_LONG_TERM_MEMORY.md`
24. `codex/implementation-prompts/13_DATING_AND_RELATIONSHIP_HISTORY.md`
25. `codex/implementation-prompts/14_WORLD_EVENTS_AND_TRENDS.md`
26. `codex/implementation-prompts/15_CATCH_UP_SIMULATION.md`
27. `codex/implementation-prompts/16_NOTIFICATIONS_AND_REALTIME.md`
28. `codex/implementation-prompts/17_REGISTRATION_AND_LOGIN.md`
29. `codex/implementation-prompts/18_PRODUCTION_HARDENING.md`

## Review rhythm

After each implementation prompt:

1. Run the relevant milestone checklist in `docs/milestones`.
2. Use `codex/review-prompts/00_GENERAL_DIFF_REVIEW.md` in a fresh Codex thread.
3. Fix Critical and High findings before merge. Record Medium and Low findings and address them when required by acceptance criteria or risk.
4. Re-run tests.
5. Commit.
6. Update `docs/development/DECISIONS.md` when architecture or behaviour changed.

## Prompt execution rule

When giving any implementation prompt to Codex, include this sentence at the end:

> Implement only this task. Read AGENTS.md and relevant source-of-truth documents, inspect the existing code and Git changes first, and do not implement future features. Add tests, run applicable formatting, builds, and relevant tests, and report each command as passed, failed, or not run with a reason.

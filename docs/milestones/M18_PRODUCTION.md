# M18 — Production hardening

## Goal
Produce a staging/beta-ready, operable, secure release candidate.

## User-visible result
Stable core journey with controlled failure behavior and recoverable service operations.

## Dependencies
Every milestone selected for the release; M14/M17 only if that release includes them.

## Scope
- **Backend:** Configuration validation, limits/budgets, observability/redaction, health/readiness, recovery, performance fixes supported by evidence—no new feature.
- **Database:** Empty/previous migration validation, query-plan/index review, backup/restore procedure, retention implementation only after decisions.
- **Flutter:** Release build, production configuration, error/crash reporting only if approved, accessibility/performance regression.
- **Infrastructure:** Complete CI, controlled deployment/migrations, protected secrets, monitoring/alerts, backups, staging smoke, rollback runbook.

## Explicit exclusions
New gameplay, speculative scale stack, Redis/Kafka/Kubernetes/microservices, unresolved future features.

## Test scope
Complete backend/Flutter suites, auth/ownership/simulation regressions, migration, provider failure, performance sanity, staging/release acceptance.

## Security and ownership considerations
Release readiness, operations/security, data loss, cost/performance, honest evidence. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
No unresolved Critical/High; staging deploy/migration/core journey pass; secrets absent; backup/rollback plans verified; required gates green.

## Required verification
Exact CI/release commands, security/dependency/secret scans, migration/restore, staging smoke, release build, safe post-deploy checks. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Full player journey, offline/recovery, ownership negative, log redaction, provider outage, rollback rehearsal.

## Exit criteria
Signed release checklist and approved versioned artifact; no future work hidden in hardening.

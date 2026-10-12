# M18 — Production hardening

## Goal
Produce a staging/beta-ready, operable, secure release candidate.

## User-visible result
Stable core journey with controlled failure behavior and recoverable service operations.

## Dependencies
Every milestone selected for the release; M14/M17 only if that release includes them.

ADR-034 is the authoritative M18 production-operations contract. M14 remains deferred; M17 is included in the current release.

## Scope
- **Backend:** Configuration validation, limits/budgets, observability/redaction, health/readiness, recovery, performance fixes supported by evidence—no new feature.
- **Database:** Empty/previous migration validation, query-plan/index review, backup/restore procedure, retention implementation only after decisions.
- **Flutter:** Release build, production configuration, error/crash reporting only if approved, accessibility/performance regression.
- **Infrastructure:** Complete CI, controlled deployment/migrations, protected secrets, monitoring/alerts, backups, staging smoke, rollback runbook.

## Accepted production baseline

- Development is local. Staging and Production are isolated Render Web Services using separate Neon projects/databases, configuration, secrets, and JWT keys.
- Production uses a non-Free Render plan, Render-managed TLS, an explicit Host/CORS allowlist, Render runtime environment variables, and a Render Secret File for the RSA private key.
- Production has at least seven days of Neon restore history where supported, plus an encrypted weekly logical backup with four-copy retention. Targets are RPO <= 24 hours outside PITR and RTO <= 4 hours.
- Operational visibility uses sanitized structured logs/correlation IDs, Render metrics, health/readiness, and Render/Neon notifications. Ollama is optional and excluded from readiness.
- CI adds free/repository-native dependency, secret, single-SAST, SBOM, and single-container scanning. Critical risk blocks release; High risk requires written project-owner acceptance.
- Representative load is 25 concurrent active users and 100 one-world registered accounts. Targets are non-AI CRUD/read p95 <= 500 ms, synchronous simulation/catch-up p95 <= 5 seconds, unexpected 5xx < 1%, and readiness <= 60 seconds.
- Releases flow from `dev` through PR to `main`, Staging, smoke/migration/security checks, project-owner approval, Production, and post-deploy checks. Migrations are an explicit pre-traffic deployment step; automatic downgrade is prohibited.

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

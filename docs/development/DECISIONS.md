# Architecture Decision Log

Do not rewrite accepted entries. Add a new record and mark an older decision superseded when necessary.

## ADR-001 — Private isolated worlds

**Status:** Accepted

Each real user owns isolated single-player worlds. Real users never interact.

## ADR-002 — Modular monolith

**Status:** Accepted

Use one deployable ASP.NET Core backend divided into modules. Avoid microservices initially.

## ADR-003 — PostgreSQL authority

**Status:** Accepted

PostgreSQL is the source of truth. Flutter local storage is a cache/offline layer.

## ADR-004 — Rules before language generation

**Status:** Accepted

The deterministic engine decides mechanics; AI generates wording only.

## ADR-005 — Guest-first authentication

**Status:** Accepted

Start through automatic guest sessions and allow later account upgrade without progress loss.

## ADR-006 — Catch-up simulation

**Status:** Accepted

The world uses compressed catch-up processing because free/low-cost hosting may sleep and mobile users may remain away for long periods.

## ADR-007 — M01 technology baseline

**Date:** 2026-08-25
**Status:** Accepted

Baseline checked against official Flutter stable-release and supported-platform documentation on 2026-08-25. Use the stable .NET 10 LTS SDK and `net10.0`; stable Flutter 3.47 with its bundled compatible Dart 3.13 SDK; Android API 24 as the minimum supported Android level; and Android as the initial required launch platform. iOS is deferred. M01 must pin this accepted toolchain, including the latest stable compatible patch available within the accepted lines, in repository toolchain/configuration files. Patch updates may be adopted after applicable CI passes; any future minor or major Flutter/Dart upgrade requires explicit review. Preview, RC, beta, dev, main/master, and nightly releases are prohibited.

## ADR-008 — Project and namespace prefix

**Date:** 2026-08-25
**Status:** Accepted

Use `ParallelWorld` consistently: projects are `ParallelWorld.Api`, `ParallelWorld.Application`, `ParallelWorld.Domain`, `ParallelWorld.Infrastructure`, `ParallelWorld.Simulation`, and `ParallelWorld.AI`; namespaces are `ParallelWorld.<Project>` and `ParallelWorld.<Project>.<Area>`. The Flutter package remains `parallel_world_app`.

## ADR-009 — PostgreSQL identifier naming

**Date:** 2026-08-25
**Status:** Accepted

Use unquoted `snake_case` PostgreSQL identifiers and PascalCase C# entity/property names. EF Core naming conventions or explicit mappings translate between them. Do not create quoted PascalCase database identifiers.

## ADR-010 — Initial CI baseline

**Date:** 2026-08-25
**Status:** Accepted

M01 creates a working initial GitHub Actions workflow containing all applicable M01 checks for only the projects that exist: backend restore, configured format check, build, and the unit/empty test suite; Flutter `pub get`, format check, analyze, and tests. PostgreSQL integration jobs begin only in the milestone that introduces PostgreSQL-dependent tests. A check that is not yet applicable must be reported as such rather than simulated.

## ADR-011 — Merge workflow

**Date:** 2026-08-25
**Status:** Accepted

Use a milestone feature branch, push it, open a pull request into `main`, require all available CI checks to succeed, and review before merge. For a one-developer project self-review is acceptable, but the independent Codex diff review required by `DEVELOPMENT_PLAN.md` remains mandatory.

## ADR-012 — Persistent development branch and stable promotion

**Date:** 2026-08-25
**Status:** Accepted; supersedes ADR-011

`main` is the stable/release-ready branch and `dev` is the persistent active development and integration branch. Implement M01 and later milestones sequentially on `dev`. After each milestone, run its verification, inspect the diff, create a milestone-specific commit, and push `dev`; a pull request is not required after every milestone. When `dev` reaches an approved stable checkpoint, promote it through a pull request from `dev` into `main`, require all applicable CI checks to succeed, review the pull request, and merge through that pull request. A reviewed local merge is not an accepted substitute for this promotion. Direct feature development on `main` is prohibited. Short-lived `feature/...` branches are optional for isolated or risky work and are not required by the milestone workflow. For a one-developer project self-review is acceptable, but the independent Codex diff review required by `DEVELOPMENT_PLAN.md` remains mandatory.

## New ADR template

### ADR-XXX — Title

**Date:** YYYY-MM-DD  
**Status:** Proposed / Accepted / Superseded

**Context**

**Decision**

**Alternatives considered**

**Consequences**

**Revisit when**

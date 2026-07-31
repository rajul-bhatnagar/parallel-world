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

## New ADR template

### ADR-XXX — Title

**Date:** YYYY-MM-DD  
**Status:** Proposed / Accepted / Superseded

**Context**

**Decision**

**Alternatives considered**

**Consequences**

**Revisit when**

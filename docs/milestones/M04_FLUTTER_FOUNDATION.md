# M04 — Flutter foundation

## Goal
Securely bootstrap the mobile app and route by session/world state.

## User-visible result
First launch creates guest online; returning launch restores; missing world routes to creation; existing world routes home.

## Dependencies
Stable M03 contracts.

## Scope
- **Backend:** Only fixes needed to meet approved M03 contract; no new feature endpoint.
- **Database:** No EF schema change.
- **Flutter:** Bootstrap, public environment config, Dio, secure token/installation storage, Riverpod session controller, single-flight refresh, GoRouter guards, splash/retry/world-create/home placeholders, Drift initialization, cache/session clearing.
- **Infrastructure:** Safe local/test/staging public configuration mechanism.

## Explicit exclusions
Feed/characters, new offline writes, simulation, AI, registration.

## Test scope
Startup states, guest repository, secure-store abstraction, refresh races, routes, first-launch offline, cached offline state, missing world, logout clearing.

## Security and ownership considerations
Secure storage, interceptor loops, cache authority, navigation races, privacy. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
All bootstrap paths terminate in explicit usable/recovery states; tokens never enter Drift/logs; routing matches server state.

## Required verification
Flutter pub/format/analyze/unit/provider/widget tests and backend contract regression. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
First/returning/offline/expired-session/missing-world/logout flows.

## Exit criteria
Reliable shell against M03 API with explicit loading/error/offline states.

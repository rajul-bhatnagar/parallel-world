# M17 — Registration and login

## Goal
Version 1 upgrade of the existing guest user without progress loss.

## User-visible result
Guest registers/logs in/recovers and uses multiple approved devices while retaining the same worlds/history.

## Dependencies
Stable guest MVP, ADR-013/ADR-014 session semantics, and accepted ADR-033 email/password and recovery-code contracts.

## Scope
- **Backend:** ADR-033 email/password registration and login; no email verification; hash-only single-use recovery-code lifecycle; same-user transactional upgrade; password change; login, logout-all, and minimal refresh-family session management. Magic links/providers remain deferred.
- **Database:** Existing User credential fields plus hash-only recovery-code lifecycle, normalized-email uniqueness, account/credential consistency, and migration preserving `UserId`.
- **Flutter:** Register/login/upgrade/recovery/code-save/password-change/session management, same-account cache preservation, account-switch clearing, and no persistent password/recovery-code storage.
- **Infrastructure:** Existing auth rate limiter, RS256/token configuration, ASP.NET Core password hasher, and CSPRNG only; no delivery/provider dependency.

## Explicit exclusions
MVP requirement, social discovery, account/world merging, real-user interaction, unapproved providers/MFA.

## Test scope
Email normalization/uniqueness and password bounds/hash-only persistence; same `UserId`/world/data; atomic upgrade rollback and replay; generic login/recovery failures; recovery-code one-time/rotation/hash-only behavior; all-family revocation after recovery/password change; owned-family listing/revoke/logout-all; cache behavior and ownership regression.

## Security and ownership considerations
Identity proof, session rotation/reuse, enumeration, cache/data preservation. Repository-wide ownership, privacy, and secret-handling rules remain mandatory where applicable.

## Acceptance criteria
Upgrade mutates the same user transactionally; failure leaves guest usable; no multiplayer path exists.

## Required verification
Full auth/security/PostgreSQL/API/Flutter regression and log/secret checks. Record every result as Passed, Failed, Unavailable, or Not applicable — with reason.

## Manual checks
Upgrade populated guest, relaunch/login, recovery, second device, duplicate registered identifier, account switch.

## Exit criteria
Version 1 auth decisions are accepted and flows pass independent security review.

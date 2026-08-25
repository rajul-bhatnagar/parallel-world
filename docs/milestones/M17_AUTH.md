# M17 — Registration and login

## Goal
Version 1 upgrade of the existing guest user without progress loss.

## User-visible result
Guest registers/logs in/recovers and uses multiple approved devices while retaining the same worlds/history.

## Dependencies
Stable guest MVP and accepted authentication/recovery decisions.

## Scope
- **Backend:** The registered-authentication and recovery approach accepted in `docs/development/DECISIONS.md`, including its required verification/recovery flow; same-user transactional upgrade; login, logout-all, and session/device management; external identity linking only if the accepted approach requires it or it is separately approved.
- **Database:** Registered credential/identity state and session evolution; uniqueness and integrity rules required by the accepted authentication method; migration preserving `UserId`.
- **Flutter:** Register/login/upgrade/recovery/session devices, same-account cache preservation, account-switch clearing.
- **Infrastructure:** Method-specific provider or recovery configuration and protected secrets only for the selected approach.

## Explicit exclusions
MVP requirement, social discovery, account/world merging, real-user interaction, unapproved providers/MFA.

## Test scope
Same `UserId`/world/data, atomic rollback, duplicate/parallel request, login/recovery/generic responses, revocation/devices, cache behavior, ownership regression.

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
